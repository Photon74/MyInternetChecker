using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyInternetChecker;

internal class Config
{
    public static readonly string[] HostsToCheck = { "ya.ru", "google.com" };
    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(1);

    static Config()
    {
        if (HostsToCheck.Length == 0)
            throw new InvalidOperationException("Не указаны хосты для проверки!");
    }
}
