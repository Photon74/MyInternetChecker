using MyInternetChecker.Config.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyInternetChecker.Config;

internal static class SettingsLoader
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyInternetChecker",
        "settings.ini"
    );

    public static AppSettings Load()
    {
        var settings = new AppSettings();

        if (!File.Exists(SettingsFilePath))
        {
            CreateDefaultSettings();
            return settings;
        }

        try
        {
            var iniData = IniParser.Parse(SettingsFilePath);
            LoadGeneralSettings(iniData, settings);
            LoadPingSettings(iniData, settings);
            LoadHosts(iniData, settings);
            LoadWindowPosition(iniData, settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
        }

        return settings;
    }

    private static void LoadGeneralSettings(
        Dictionary<string, Dictionary<string, string>> iniData,
        AppSettings settings)
    {
        if (!iniData.TryGetValue("general", out Dictionary<string, string> section)) return;
        if (section.TryGetValue("check_interval", out var intervalStr) &&
            int.TryParse(intervalStr, out int interval))
        {
            settings.CheckInterval = TimeSpan.FromSeconds(Math.Clamp(interval, 1, 60));
        }

        if (section.TryGetValue("color_online", out var onlineColor))
            settings.Colors.Online = onlineColor;

        if (section.TryGetValue("color_offline", out var offlineColor))
            settings.Colors.Offline = offlineColor;
    }

    private static void LoadPingSettings(
        Dictionary<string, Dictionary<string, string>> iniData,
        AppSettings settings)
    {
        if (!iniData.TryGetValue("ping", out Dictionary<string, string> section)) return;
        if (section.TryGetValue("timeout", out var timeoutStr) &&
            int.TryParse(timeoutStr, out int timeout))
            settings.Ping.Timeout = Math.Max(timeout, 100);

        if (section.TryGetValue("attempts", out var attemptsStr) &&
            int.TryParse(attemptsStr, out int attempts))
            settings.Ping.Attempts = Math.Clamp(attempts, 1, 10);
    }

    private static void LoadHosts(
        Dictionary<string, Dictionary<string, string>> iniData,
        AppSettings settings)
    {
        if (!iniData.TryGetValue("hosts", out Dictionary<string, string> value)) return;

        var hosts = value.Values
            .Where(host => !string.IsNullOrWhiteSpace(host) && !host.StartsWith("#"))
            .ToArray();

        if (hosts.Length > 0)
            settings.Hosts = hosts;
    }

    private static void LoadWindowPosition(
        Dictionary<string, Dictionary<string, string>> iniData,
        AppSettings settings)
    {
        if (!iniData.TryGetValue("general", out Dictionary<string, string> section)) return;
        if (section.TryGetValue("window_left", out var leftStr) &&
            double.TryParse(leftStr, out double left) &&
            section.TryGetValue("window_top", out var topStr) &&
            double.TryParse(topStr, out double top))
        {
            settings.Window.Left = left;
            settings.Window.Top = top;
        }
    }

    private static void CreateDefaultSettings()
    {
        var defaultSettings = new Dictionary<string, Dictionary<string, string>>
        {
            ["general"] = new()
            {
                ["check_interval"] = "1",
                ["color_online"] = "#32CD32",
                ["color_offline"] = "#DC143C",
                ["window_left"] = "0",
                ["window_top"] = "1013"
            },
            ["ping"] = new()
            {
                ["timeout"] = "1000",
                ["attempts"] = "3"
            },
            ["hosts"] = new()
            {
                ["host1"] = "ya.ru",
                ["host2"] = "google.com"
            }
        };

        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        IniParser.Save(SettingsFilePath, defaultSettings);
    }
}