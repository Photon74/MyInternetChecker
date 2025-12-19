using System;
using System.IO;
using System.Linq;

namespace MyInternetChecker;

internal class Config
{
    public static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyInternetChecker", // Имя папки для приложения
        "settings.txt"       // Имя файла с настройками
    );

    private static string[] _hostsToCheck;
    public static string[] HostsToCheck => GetHosts();

    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(1);

    // Добавляем метод для принудительной перезагрузки хостов
    public static void ReloadHosts()
    {
        _hostsToCheck = null;
        GetHosts(); // Загружаем заново
    }

    // Метод для переноса старых настроек из папки с программой в AppData
    private static void TryMigrateOldSettings()
    {
        // Старый путь: рядом с исполняемым файлом (.exe)
        string oldPath = Path.Combine(AppContext.BaseDirectory, "settings.txt");
        string newPath = SettingsFilePath; // Новый путь в AppData

        // Переносим только если старый файл ЕСТЬ, а нового еще НЕТ
        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            try
            {
                // Убедимся, что целевая папка существует
                string newDirectory = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }
                File.Copy(oldPath, newPath);
                Console.WriteLine($"Настройки перенесены в: {newPath}");
                // Файл в старой папке можно оставить или удалить
                // File.Delete(oldPath);
            }
            catch (Exception ex)
            {
                // Если не получилось - не страшно, просто пишем в лог
                Console.WriteLine($"Не удалось перенести настройки: {ex.Message}");
            }
        }
    }

    private static string[] GetHosts()
    {
        TryMigrateOldSettings();

        if (_hostsToCheck != null)
            return _hostsToCheck;

        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var lines = File.ReadAllLines(SettingsFilePath)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                    .ToArray();

                _hostsToCheck = lines.Length > 0 ? lines : GetDefaultHosts();
            }
            else
            {
                _hostsToCheck = GetDefaultHosts();

                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(SettingsFilePath, _hostsToCheck);
            }
        }
        catch (Exception)
        {
            // При ошибке используем настройки по умолчанию
            _hostsToCheck = GetDefaultHosts();
        }

        return _hostsToCheck;
    }

    private static string[] GetDefaultHosts() => new[] { "ya.ru", "google.com" };
}