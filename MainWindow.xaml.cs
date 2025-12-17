using System;
using System.Text;
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

        //private readonly string[] _hostsToCheck = { "ya.ru", "google.com" };

        public MainWindow()
        {
            InitializeComponent();
            Top = (_screenHeight - 20);
            Left = 2;
            TimerStart();
        }
        private void TimerStart()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {

            bool internetAvailable = CheckInternetConnection();
            if (internetAvailable)
            {
                if (_count == 0)
                {
                    Rect.Fill = Brushes.DarkGreen;
                    _count++;
                }
                else
                {
                    Rect.Fill = Brushes.SlateGray;
                    _count = 0;
                }
            }
            else
            {
                if (_count == 0)
                {
                    Rect.Fill = Brushes.DarkRed;
                    _count++;
                }
                else
                {
                    Rect.Fill = Brushes.SlateGray;
                    _count = 0;
                }
            }

            if (_isMouseOver && StatusToolTip.IsOpen)
            {
                UpdateToolTip();
            }
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

        private void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseOver && StatusToolTip.IsOpen)
            {
                UpdateToolTip();
            }
        }

        private void UpdateToolTip()
        {
            var yaRuResult = PingIt.PingHost("ya.ru");
            var googleResult = PingIt.PingHost("google.com");

            var sb = new StringBuilder();
            sb.AppendLine("🌐 СТАТУС ИНТЕРНЕТА");
            sb.AppendLine();

            AppendPingResult(sb, "ya.ru", yaRuResult);
            AppendPingResult(sb, "google.com", googleResult);

            sb.AppendLine();
            sb.AppendLine($"📊 ОБЩИЙ СТАТУС: {(yaRuResult.IsSuccess || googleResult.IsSuccess ? "Онлайн" : "Оффлайн")}");
            sb.Append($"⏰ {DateTime.Now:HH:mm:ss}");

            ToolTipText.Text = sb.ToString();
        }

        private void AppendPingResult(StringBuilder sb, string hostName, PingResult result)
        {
            if (result.IsSuccess)
            {
                string quality;
                switch (result.RoundtripTime)
                {
                    case long r when r < 50:
                        quality = "Отлично";
                        break;
                    case long r when r < 100:
                        quality = "Хорошо";
                        break;
                    case long r when r < 200:
                        quality = "Нормально";
                        break;
                    case long r when r < 500:
                        quality = "Медленно";
                        break;
                    default:
                        quality = "Очень медленно";
                        break;
                }

                sb.AppendLine($"• {hostName}: {result.RoundtripTime} мс ({quality})");
            }
            else
            {
                sb.AppendLine($"• {hostName}: ❌ недоступен");
            }
        }

        private bool CheckInternetConnection()
        {
            return PingIt.PingHost("ya.ru").IsSuccess || 
                PingIt.PingHost("google.com").IsSuccess;
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
