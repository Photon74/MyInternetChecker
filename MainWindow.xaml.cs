using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        public MainWindow()
        {
            InitializeComponent();
            Top = (_screenHeight - 10);
            Left = 2;
            timerStart();
        }
        private void timerStart()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(timerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            
            bool isPing = PingIt.PingHost("google.com");
            if (isPing)
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
        }
    }
}
