using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyInternetChecker;

/// <summary>Главное окно приложения</summary>
public partial class MainWindow
{
    private DispatcherTimer _timer = null;
    private readonly double _screenHeight = SystemParameters.FullPrimaryScreenHeight;
    private readonly double _screenWidth = SystemParameters.FullPrimaryScreenWidth;
    private int _count = 0;
    private bool _isMouseOver = false;
    private bool _isChecking = false;
    private Dictionary<string, long> _pingResults = new();
    private readonly CancellationTokenSource _cts;

    /// <summary>Инициализирует окно и запускает таймер проверки</summary>
    public MainWindow()
    {
        InitializeComponent();

        _cts = new CancellationTokenSource();

        Rect.MouseRightButtonDown += (s, e) =>
        {
            RectContextMenu.IsOpen = true;
        };

        Loaded += (s, e) =>
        {
            Top = _screenHeight;
            Left = _screenWidth - _screenWidth;

            UpdateAutoStartMenuItem();
        };

        UpdatePingResultsDictionary();

        TimerStart();
    }



    private void TimerStart()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += new EventHandler(TimerTick);
        _timer.Interval = Config.CheckInterval;
        _timer.Start();
    }

    private async void TimerTick(object sender, EventArgs e)
    {
        if (_isChecking) return;

        _isChecking = true;
        try
        {
            var internetAvailable = await CheckInternetConnectionAsync();
            Rect.Fill = internetAvailable
                ? (_count == 0) ? Brushes.DarkGreen : Brushes.SlateGray
                : (_count == 0) ? Brushes.DarkRed : Brushes.SlateGray;
            _count = (_count + 1) % 2; // Чередуем 0 и 1

            if (_isMouseOver && StatusToolTip.IsOpen)
            {
                UpdateToolTip();
            }
        }
        finally
        {
            _isChecking = false;
        }
    }

    private async Task<bool> CheckInternetConnectionAsync()
    {
        var hosts = Config.HostsToCheck.ToArray();

        var tasks = hosts
            .Select(host => PingIt.PingHostAsync(host, _cts.Token))
            .ToArray();

        await Task.WhenAll(tasks);

        for (var i = 0; i < hosts.Length; i++)
        {
            _pingResults[hosts[i]] = tasks[i].Result;
        }

        return tasks.Any(t => t.Result >= 0);
    }

    private void UpdateToolTip()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🌐 СТАТУС ИНТЕРНЕТА");
        sb.AppendLine();

        var anyOnline = false;
        foreach (var host in Config.HostsToCheck)
        {
            var pingTime = _pingResults.TryGetValue(host, out var time) ? time : -1;
            AppendPingResult(sb, host, pingTime);
            if (pingTime >= 0) anyOnline = true;
        }

        sb.AppendLine();
        sb.AppendLine($"📊 ОБЩИЙ СТАТУС: {(anyOnline ? "Онлайн" : "Оффлайн")}");
        sb.Append($"⏰ {DateTime.Now:HH:mm:ss}");

        ToolTipText.Text = sb.ToString();
    }

    private void AppendPingResult(StringBuilder sb, string hostName, long pingTime)
    {
        var result = new PingResult(pingTime >= 0, pingTime, hostName);
        sb.AppendLine($"• {hostName}: {result}");
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ShowSettingsWindow()
    {
        // Блокируем проверку во время изменения настроек
        _isChecking = true;

        try
        {
            var settingsWindow = new HostsSettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (settingsWindow.ShowDialog() == true)
            {
                // Если настройки сохранены, перезагружаем хосты
                Config.ReloadHosts();
                UpdatePingResultsDictionary();

                // Обновляем ToolTip, если он открыт
                if (_isMouseOver && StatusToolTip.IsOpen)
                {
                    UpdateToolTip();
                }

                // Сбрасываем счетчик для немедленного обновления цвета
                _count = 0;

                // Выполняем немедленную проверку с новыми хостами
                await CheckInternetConnectionAsync();

                // Обновляем цвет индикатора
                var internetAvailable = _pingResults.Values.Any(v => v >= 0);
                Rect.Fill = internetAvailable ? Brushes.DarkGreen : Brushes.DarkRed;
            }
        }
        finally
        {
            _isChecking = false;
        }
    }

    private void UpdatePingResultsDictionary()
    {
        // Сохраняем старые результаты для тех хостов, которые остались
        var newResults = new Dictionary<string, long>();

        foreach (var host in Config.HostsToCheck)
        {
            newResults[host] = _pingResults.TryGetValue(host, out _) ? _pingResults[host] : -1;
        }

        _pingResults = newResults;
    }

    private void RectContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        UpdateAutoStartMenuItem();
    }

    private void AutoStartMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AutoStartManager.ToggleAutoStart();
        UpdateAutoStartMenuItem();
    }

    private void UpdateAutoStartMenuItem()
    {
        AutoStartMenuItem.IsChecked = AutoStartManager.IsAutoStartEnabled();
    }

    private void Rect_MouseEnter(object sender, MouseEventArgs e)
    {
        _isMouseOver = true;
        ShowToolTip();
    }

    private void Rect_MouseLeave(object sender, MouseEventArgs e)
    {
        _isMouseOver = false;
        HideToolTip();
    }

    private void ShowToolTip()
    {
        if (!StatusToolTip.IsOpen)
        {
            StatusToolTip.PlacementTarget = Rect;
            StatusToolTip.IsOpen = true;
            UpdateToolTip();
        }
    }

    private void HideToolTip()
    {
        if (StatusToolTip.IsOpen)
        {
            StatusToolTip.IsOpen = false;
        }
    }

    /// <summary>Освобождает ресурсы при закрытии окна</summary>
    /// <param name="e">Аргументы события закрытия</param>
    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        _timer?.Stop();
        base.OnClosed(e);
    }
}
