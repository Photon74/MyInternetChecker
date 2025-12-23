using System;
using System.Collections.Generic;
using System.IO;

namespace MyInternetChecker.Config;

internal static class IniParser
{
    public static Dictionary<string, Dictionary<string, string>> Parse(string filePath)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = null;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();

            // Пропускаем пустые строки и комментарии
            if (string.IsNullOrEmpty(trimmedLine)
                || trimmedLine.StartsWith("#")
                || trimmedLine.StartsWith(";"))
                continue;

            // Секция [section]
            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine[1..^1];
                result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            // Ключ=значение
            var equalsIndex = trimmedLine.IndexOf('=');
            if (equalsIndex > 0 && currentSection != null)
            {
                var key = trimmedLine[..equalsIndex].Trim();
                var value = trimmedLine[(equalsIndex + 1)..].Trim();

                // Удаляем комментарии в конце строки
                var commentIndex = value.IndexOfAny(new[] { '#', ';' });
                if (commentIndex >= 0)
                    value = value[..commentIndex].Trim();

                if (!string.IsNullOrEmpty(key))
                    result[currentSection][key] = value;
            }
        }

        return result;
    }

    public static void Save(string filePath, Dictionary<string, Dictionary<string, string>> data)
    {
        var lines = new List<string>
        {
            "# Файл настроек MyInternetChecker",
            "# Версия формата: 1.0",
            ""
        };

        foreach (var section in data)
        {
            lines.Add($"[{section.Key}]");
            foreach (var kvp in section.Value)
                lines.Add($"{kvp.Key} = {kvp.Value}");
            lines.Add("");
        }

        File.WriteAllLines(filePath, lines);
    }
}