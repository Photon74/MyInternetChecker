namespace MyInternetChecker.Config.Models;

public class PingSettings
{
    public int Timeout { get; set; } = 1000;
    public int Attempts { get; set; } = 3;
}