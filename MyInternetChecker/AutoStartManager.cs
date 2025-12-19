#nullable enable
using System;

using Microsoft.Win32;

namespace MyInternetChecker;

/// <summary>Управляет автозапуском приложения через реестр текущего пользователя</summary>
public static class AutoStartManager
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "MyInternetChecker";

    /// <summary>Возвращает или задаёт состояние автозапуска приложения</summary>
    public static bool IsAutoStartEnabled
    {
        get
        {
            try
            {
                using var registry_key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var value = registry_key?.GetValue(AppName);
                return value is not null;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            try
            {
                if (value)
                {
                    using var registry_key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                    if (registry_key is not null)
                    {
                        var executable_path = Environment.ProcessPath;
                        if (executable_path is not null && executable_path.Contains(' '))
                            executable_path = $"\"{executable_path}\""; // окружить путь кавычками если есть пробелы
                        registry_key.SetValue(AppName, executable_path);
                    }
                }
                else
                {
                    using var registry_key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                    registry_key?.DeleteValue(AppName, false); // удалить значение без исключения если нет
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при изменении автозапуска: {ex.Message}");
            }
        }
    }
}