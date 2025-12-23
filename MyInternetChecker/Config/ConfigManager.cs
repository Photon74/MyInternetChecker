using MyInternetChecker.Config.Models;
using System;

namespace MyInternetChecker.Config;

public static class ConfigManager
{
    private static AppSettings _currentSettings;
    private static readonly object _lock = new();

    public static AppSettings Settings
    {
        get
        {
            lock (_lock)
            {
                _currentSettings ??= SettingsLoader.Load();
                return _currentSettings;
            }
        }
    }

    public static string[] Hosts => Settings.Hosts;
    public static TimeSpan CheckInterval => Settings.CheckInterval;
    public static ColorSettings Colors => Settings.Colors;
    public static PingSettings Ping => Settings.Ping;
    public static WindowPosition Window => Settings.Window;

    public static void Reload()
    {
        lock (_lock)
        {
            _currentSettings = SettingsLoader.Load();
        }
    }

    public static void SaveAllSettings(AppSettings settings)
    {
        lock (_lock)
        {
            _currentSettings = settings;
            SettingsSaver.Save(settings);
        }
    }

    public static void SaveHosts(string[] hosts) => SettingsSaver.SaveHosts(hosts);
    public static void SaveWindowPosition(double left, double top) => SettingsSaver.SaveWindowPosition(left, top);
}