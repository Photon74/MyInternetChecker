using Microsoft.Win32;
using System;

namespace MyInternetChecker;

public static class AutoStartManager
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "MyInternetChecker";

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public static void EnableAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key != null)
            {
                // Получаем путь к текущему исполняемому файлу
                var executablePath = Environment.ProcessPath;

                // Если путь содержит пробелы, заключаем в кавычки
                if (executablePath.Contains(" "))
                {
                    executablePath = $"\"{executablePath}\"";
                }

                key.SetValue(AppName, executablePath);
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не паникуем
            System.Diagnostics.Debug.WriteLine($"Ошибка при включении автозапуска: {ex.Message}");
        }
    }

    public static void DisableAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при отключении автозапуска: {ex.Message}");
        }
    }

    public static void ToggleAutoStart()
    {
        if (IsAutoStartEnabled())
        {
            DisableAutoStart();
        }
        else
        {
            EnableAutoStart();
        }
    }
}