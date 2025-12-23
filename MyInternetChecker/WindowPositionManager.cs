using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace MyInternetChecker;

/// <summary>Управляет сохранением и восстановлением позиции окна</summary>
public static class WindowPositionManager
{
    private static readonly string PositionFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyInternetChecker",
        "window_position.json"
    );

    /// <summary>Сохраняет текущую позицию окна</summary>
    /// <param name="window">Окно для сохранения</param>
    public static void SaveWindowPosition(Window window)
    {
        try
        {
            var position = new WindowPosition
            {
                Left = window.Left,
                Top = window.Top,
            };

            var directory = Path.GetDirectoryName(PositionFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(position, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PositionFilePath, json);
        }
        catch (Exception ex)
        {
            // Тихий сбой - не критично
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения позиции окна: {ex.Message}");
        }
    }

    /// <summary>Загружает сохраненную позицию окна</summary>
    /// <returns>Позиция окна или null, если сохранения нет</returns>
    public static WindowPosition LoadWindowPosition()
    {
        try
        {
            if (File.Exists(PositionFilePath))
            {
                var json = File.ReadAllText(PositionFilePath);
                return JsonSerializer.Deserialize<WindowPosition>(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки позиции окна: {ex.Message}");
        }

        return null;
    }
}

/// <summary>Позиция и размер окна</summary>
public class WindowPosition
{
    public double Left { get; set; }
    public double Top { get; set; }
}