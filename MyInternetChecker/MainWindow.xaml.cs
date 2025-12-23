using MyInternetChecker.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
    private CancellationTokenSource _cts;

    // Переменные для анимаций
    private Storyboard _onlineAnimation;
    private Storyboard _offlineAnimation;
    private bool _wasOnline = false; // Для отслеживания предыдущего состояния

    private bool _isDragging = false;
    private Point _dragStartPoint;

    /// <summary>Инициализирует окно и запускает таймер проверки</summary>
    public MainWindow()
    {
        InitializeComponent();

        _cts = new CancellationTokenSource();

        // Загружаем анимации из ресурсов
        _onlineAnimation = (Storyboard)FindResource("OnlineAnimation");
        _offlineAnimation = (Storyboard)FindResource("OfflineAnimation");

        Rect.MouseRightButtonDown += (s, e) =>
        {
            RectContextMenu.IsOpen = true;
        };

        Loaded += (s, e) =>
        {
            LoadWindowPosition();
            UpdateAutoStartMenuItem();

            // Устанавливаем начальный цвет (серый, пока не проверили)
            Rect.Fill = Brushes.Gray;
        };

        TimerStart();
    }

    private void LoadWindowPosition()
    {
        var iniPosition = ConfigManager.Window;
        if (iniPosition.Left != 0 || iniPosition.Top != 0)
        {
            // Проверяем, что окно будет в пределах видимой области
            var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
            var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
            var virtualScreenLeft = SystemParameters.VirtualScreenLeft;
            var virtualScreenTop = SystemParameters.VirtualScreenTop;

            // Корректируем позицию
            double left = iniPosition.Left;
            double top = iniPosition.Top;

            if (left < virtualScreenLeft)
                left = virtualScreenLeft;
            else if (left + Width > virtualScreenLeft + virtualScreenWidth)
                left = virtualScreenLeft + virtualScreenWidth - Width;

            if (top < virtualScreenTop)
                top = virtualScreenTop;
            else if (top + Height > virtualScreenTop + virtualScreenHeight)
                top = virtualScreenTop + virtualScreenHeight - Height;

            Left = left;
            Top = top;
            return;
        }
    }

    private void SaveWindowPosition()
    {
        ConfigManager.SaveWindowPosition(this.Left, this.Top);
    }

    private void TimerStart()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += TimerTick;
        _timer.Interval = ConfigManager.CheckInterval;
        _timer.Start();
    }

    private async void TimerTick(object sender, EventArgs e)
    {
        if (_isChecking) return;

        _isChecking = true;
        try
        {
            var internetAvailable = await CheckInternetConnectionAsync();
            if (internetAvailable != _wasOnline)
            {
                // Останавливаем текущую анимацию
                StopAllAnimations();

                if (internetAvailable)
                {
                    StartOnlineAnimation();
                }
                else
                {
                    StartOfflineAnimation();
                }

                _wasOnline = internetAvailable;
            }
            //Rect.Fill = internetAvailable
            //    ? (_count == 0) ? Brushes.DarkGreen : Brushes.SlateGray
            //    : (_count == 0) ? Brushes.DarkRed : Brushes.SlateGray;
            //_count = (_count + 1) % 2; // Чередуем 0 и 1

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
        var hosts = ConfigManager.Hosts.ToArray();

        var tasks = hosts
            .Select(host => PingIt.PingHostAsync(host, _cts.Token))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var any = false;
        for (var i = 0; i < hosts.Length; i++)
        {
            _pingResults[hosts[i]] = results[i];
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

        var anyOnline = false;
        foreach (var host in ConfigManager.Hosts)
        {
            var pingTime = _pingResults.TryGetValue(host, out var time) ? time : -1;

            AppendPingResult(sb, host, pingTime);

            if (pingTime >= 0)
                anyOnline = true;
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
        // Отменяем текущие операции
        _cts?.Cancel();

        try
        {
            var settingsWindow = new HostsSettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (settingsWindow.ShowDialog() == true)
            {
                // Создаем новый токен после отмены
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                ConfigManager.Reload();
                //UpdatePingResultsDictionary();

                // Немедленная проверка
                _timer?.Stop();
                await CheckInternetConnectionAsync();
                UpdateToolTip();

                var internetAvailable = _pingResults.Values.Any(v => v >= 0);
                Rect.Fill = internetAvailable ? Brushes.DarkGreen : Brushes.DarkRed;

                _timer?.Start();
            }
        }
        finally
        {
            // Восстанавливаем токен, если был отменен
            if (_cts?.IsCancellationRequested == true)
            {
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
            }
        }
    }

    private void RectContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        UpdateAutoStartMenuItem();
    }

    private void AutoStartMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AutoStartManager.IsAutoStartEnabled = !AutoStartManager.IsAutoStartEnabled;
        UpdateAutoStartMenuItem();
    }

    private void UpdateAutoStartMenuItem()
    {
        AutoStartMenuItem.IsChecked = AutoStartManager.IsAutoStartEnabled;
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
        if (StatusToolTip.IsOpen) return;

        StatusToolTip.PlacementTarget = Rect;
        StatusToolTip.IsOpen = true;
        UpdateToolTip();
    }

    private void HideToolTip()
    {
        if (!StatusToolTip.IsOpen) return;

        StatusToolTip.IsOpen = false;
    }

    // Обработчики для перемещения окна
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            CaptureMouse();  // Захватываем мышь для отслеживания вне окна
        }
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging && e.ChangedButton == MouseButton.Left)
        {
            _isDragging = false;
            ReleaseMouseCapture();  // Освобождаем захват мыши

            // Сохраняем позицию при отпускании ЛКМ
            SaveWindowPosition();
        }
    }

    // Также обрабатываем перемещение мыши для drag&drop
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _dragStartPoint;

            Left += delta.X;
            Top += delta.Y;
        }
    }

    /// <summary>Запускает анимацию для онлайн состояния</summary>
    private void StartOnlineAnimation()
    {
        if (_onlineAnimation != null)
        {
            // Убедимся, что цвет установлен в начальное состояние
            Rect.Fill = new SolidColorBrush(Colors.DarkGreen);
            _onlineAnimation.Begin(Rect, true);
        }
    }

    /// <summary>Запускает анимацию для оффлайн состояния</summary>
    private void StartOfflineAnimation()
    {
        if (_offlineAnimation != null)
        {
            // Убедимся, что цвет установлен в начальное состояние
            Rect.Fill = new SolidColorBrush(Colors.DarkRed);
            _offlineAnimation.Begin(Rect, true);
        }
    }

    /// <summary>Останавливает все анимации</summary>
    private void StopAllAnimations()
    {
        _onlineAnimation?.Stop(Rect);
        _offlineAnimation?.Stop(Rect);

        // Сбрасываем анимационные свойства
        Rect.BeginAnimation(Rectangle.FillProperty, null);
    }

    /// <summary>Освобождает ресурсы при закрытии окна</summary>
    /// <param name="e">Аргументы события закрытия</param>
    protected override void OnClosed(EventArgs e)
    {
        SaveWindowPosition();

        StopAllAnimations();
        _cts?.Cancel();
        _timer?.Stop();
        base.OnClosed(e);
    }
}
