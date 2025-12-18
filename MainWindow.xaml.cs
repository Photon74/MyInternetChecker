using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyInternetChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer = null;
        readonly double _screenHeight = SystemParameters.FullPrimaryScreenHeight;
        readonly double _screenWidth = SystemParameters.FullPrimaryScreenWidth;
        private int _count = 0;
        private bool _isMouseOver = false;
        private bool _isChecking = false;
        private long _lastYaRuPing = -1;
        private long _lastGooglePing = -1;
        private Dictionary<string, long> _pingResults = [];

        public MainWindow()
        {
            InitializeComponent();
            Top = _screenHeight - this.ActualHeight;
            Left = _screenWidth - _screenWidth;

            foreach (var host in Config.HostsToCheck)
            {
                _pingResults[host] = -1;
            }

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
                bool internetAvailable = await CheckInternetConnectionAsync();
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
            var tasks = Config.HostsToCheck.Select(host => PingIt.PingHostAsync(host)).ToArray();

            await Task.WhenAll(tasks);

            for (int i = 0; i < Config.HostsToCheck.Length; i++)
            {
                _pingResults[Config.HostsToCheck[i]] = tasks[i].Result;
            }

            return tasks.Any(t => t.Result >= 0);
        }


        private void UpdateToolTip()
        {
            var sb = new StringBuilder();
            sb.AppendLine("🌐 СТАТУС ИНТЕРНЕТА");
            sb.AppendLine();

            bool anyOnline = false;
            foreach (var host in Config.HostsToCheck)
            {
                long pingTime = _pingResults.ContainsKey(host) ? _pingResults[host] : -1;
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

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
