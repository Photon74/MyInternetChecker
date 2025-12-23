using System;

namespace MyInternetChecker.Config.Models;

public class AppSettings
{
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(1);
    public ColorSettings Colors { get; set; } = new();
    public PingSettings Ping { get; set; } = new();
    public WindowPosition Window { get; set; } = new();
    public string[] Hosts { get; set; } = { "ya.ru", "google.com" };
}