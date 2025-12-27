using MyInternetChecker.Config.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyInternetChecker.Config;

internal static class SettingsSaver
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyInternetChecker",
        "settings.ini"
    );

    public static void Save(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var iniData = new Dictionary<string, Dictionary<string, string>>
            {
                ["general"] = new()
                {
                    ["check_interval"] = ((int)settings.CheckInterval.TotalSeconds).ToString(),
                    ["color_online"] = settings.Colors.Online,
                    ["color_offline"] = settings.Colors.Offline,
                    ["window_left"] = settings.Window.Left.ToString(),
                    ["window_top"] = settings.Window.Top.ToString()
                },
                ["ping"] = new()
                {
                    ["timeout"] = settings.Ping.Timeout.ToString(),
                    ["attempts"] = settings.Ping.Attempts.ToString()
                },
                ["hosts"] = new()
            };

            for (int i = 0; i < settings.Hosts.Length; i++)
                iniData["hosts"][$"host{i + 1}"] = settings.Hosts[i];

            IniParser.Save(SettingsFilePath, iniData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
        }
    }

    public static void SaveHosts(string[] hosts)
    {
        var settings = SettingsLoader.Load();
        settings.Hosts = hosts;
        Save(settings);
    }

    public static void SaveWindowPosition(double left, double top)
    {
        var settings = SettingsLoader.Load();
        settings.Window.Left = left;
        settings.Window.Top = top;
        Save(settings);
    }
}