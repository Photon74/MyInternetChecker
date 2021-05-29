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
        private DispatcherTimer timer = null;
        
        public MainWindow()
        {
            InitializeComponent();
            timerStart();
        }
        private void timerStart()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            bool isPing = PingIt.PingHost("google.com");
            if (isPing)
            {
                Rect.Fill = Brushes.DarkGreen;
                Thread.Sleep(500);
                Rect.Fill = Brushes.White;
            }
            else
            {
                Rect.Fill = Brushes.DarkRed;
                Thread.Sleep(500);
                Rect.Fill = Brushes.White;
            }
        }
    }
}
