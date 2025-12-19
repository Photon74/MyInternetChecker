#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using MyInternetChecker.Mvvm;

namespace MyInternetChecker.ViewModels;

/// <summary>ViewModel главного окна</summary>
public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer _Timer;
    private readonly CancellationTokenSource _Cts = new();

    private int _BlinkCount;
    private bool _IsMouseOver;
    private bool _IsToolTipOpen;
    private bool _IsChecking;
    private Dictionary<string, long> _PingResults = new();

    private Brush _IndicatorFill = Brushes.DarkRed;
    private string _ToolTipText = "Проверка состояния...";
    private bool _IsAutoStartEnabled;
    private bool _RequestClose;
    private bool _RequestHostsSettings;

    public MainWindowViewModel()
    {
        UpdatePingResultsDictionary();

        _Timer = new DispatcherTimer { Interval = Config.CheckInterval };
        _Timer.Tick += async (_, _) => await TimerTickAsync();
        _Timer.Start();

        OpenHostsSettingsCommand = new RelayCommand(RequestOpenHostsSettings);
        ExitCommand = new RelayCommand(() => RequestClose = true);
        ToggleAutoStartCommand = new RelayCommand(ToggleAutoStart);

        MouseEnterCommand = new RelayCommand(OnMouseEnter);
        MouseLeaveCommand = new RelayCommand(OnMouseLeave);
        ContextMenuOpenedCommand = new RelayCommand(RefreshAutoStartState);

        OnLoaded = OnLoadedImpl;
        OnClosed = Dispose;

        RefreshAutoStartState();
    }

    /// <summary>Цвет индикатора статуса</summary>
    public Brush IndicatorFill { get => _IndicatorFill; set => Set(ref _IndicatorFill, value); }

    /// <summary>Текст подсказки со статусом</summary>
    public string ToolTipText { get => _ToolTipText; set => Set(ref _ToolTipText, value); }

    /// <summary>Открыта ли подсказка</summary>
    public bool IsToolTipOpen { get => _IsToolTipOpen; set => Set(ref _IsToolTipOpen, value); }

    /// <summary>Включён ли автозапуск</summary>
    public bool IsAutoStartEnabled { get => _IsAutoStartEnabled; set => Set(ref _IsAutoStartEnabled, value); }

    /// <summary>Запрос закрыть окно</summary>
    public bool RequestClose { get => _RequestClose; set => Set(ref _RequestClose, value); }

    /// <summary>Запрос открыть окно настроек хостов</summary>
    public bool RequestHostsSettings { get => _RequestHostsSettings; set => Set(ref _RequestHostsSettings, value); }

    /// <summary>Действие при загрузке окна</summary>
    public Action<Window>? OnLoaded { get; }

    /// <summary>Действие при закрытии окна</summary>
    public Action? OnClosed { get; }

    public RelayCommand OpenHostsSettingsCommand { get; }
    public RelayCommand ExitCommand { get; }
    public RelayCommand ToggleAutoStartCommand { get; }

    public RelayCommand MouseEnterCommand { get; }
    public RelayCommand MouseLeaveCommand { get; }
    public RelayCommand ContextMenuOpenedCommand { get; }

    private void OnLoadedImpl(Window Window)
    {
        Window.Top = SystemParameters.FullPrimaryScreenHeight;
        Window.Left = SystemParameters.FullPrimaryScreenWidth - SystemParameters.FullPrimaryScreenWidth;
    }

    private void RequestOpenHostsSettings() => RequestHostsSettings = true;

    public async Task ApplyHostsChangedAsync()
    {
        Config.ReloadHosts();
        UpdatePingResultsDictionary();
        _BlinkCount = 0;

        await CheckInternetConnectionAsync();
        var any_online = _PingResults.Values.Any(v => v >= 0);
        IndicatorFill = any_online ? Brushes.DarkGreen : Brushes.DarkRed;

        if (_IsMouseOver && IsToolTipOpen)
            UpdateToolTip();
    }

    private void ToggleAutoStart()
    {
        AutoStartManager.IsAutoStartEnabled = !AutoStartManager.IsAutoStartEnabled;
        RefreshAutoStartState();
    }

    private void RefreshAutoStartState() => IsAutoStartEnabled = AutoStartManager.IsAutoStartEnabled;

    private void OnMouseEnter()
    {
        _IsMouseOver = true;
        IsToolTipOpen = true;
        UpdateToolTip();
    }

    private void OnMouseLeave()
    {
        _IsMouseOver = false;
        IsToolTipOpen = false;
    }

    private async Task TimerTickAsync()
    {
        if (_IsChecking)
            return;

        _IsChecking = true;
        try
        {
            var internet_available = await CheckInternetConnectionAsync();
            IndicatorFill = internet_available
                ? (_BlinkCount == 0 ? Brushes.DarkGreen : Brushes.SlateGray)
                : (_BlinkCount == 0 ? Brushes.DarkRed : Brushes.SlateGray);

            _BlinkCount = (_BlinkCount + 1) % 2;

            if (_IsMouseOver && IsToolTipOpen)
                UpdateToolTip();
        }
        finally
        {
            _IsChecking = false;
        }
    }

    private async Task<bool> CheckInternetConnectionAsync()
    {
        var hosts = Config.HostsToCheck.ToArray();

        var tasks = hosts
            .Select(host => PingIt.PingHostAsync(host, _Cts.Token))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var any = false;
        for (var i = 0; i < hosts.Length; i++)
        {
            _PingResults[hosts[i]] = results[i];
            if (results[i] >= 0)
                any = true;
        }

        return any;
    }

    private void UpdateToolTip()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🌐 СТАТУС ИНТЕРНЕТА");
        sb.AppendLine();

        var any_online = false;
        foreach (var host in Config.HostsToCheck)
        {
            var ping_time = _PingResults.TryGetValue(host, out var time) ? time : -1;
            var result = new PingResult(ping_time >= 0, ping_time, host);
            sb.AppendLine($"• {host}: {result}");

            if (ping_time >= 0)
                any_online = true;
        }

        sb.AppendLine();
        sb.AppendLine($"📊 ОБЩИЙ СТАТУС: {(any_online ? "Онлайн" : "Оффлайн")}");
        sb.Append($"⏰ {DateTime.Now:HH:mm:ss}");

        ToolTipText = sb.ToString();
    }

    private void UpdatePingResultsDictionary()
    {
        var new_results = new Dictionary<string, long>();
        foreach (var host in Config.HostsToCheck)
            new_results[host] = _PingResults.TryGetValue(host, out _) ? _PingResults[host] : -1;

        _PingResults = new_results;
    }

    public void Dispose()
    {
        _Cts.Cancel();
        _Timer.Stop();
    }
}
