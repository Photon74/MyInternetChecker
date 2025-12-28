using LiveCharts;
using LiveCharts.Wpf;
using MyInternetChecker.Chart;
using MyInternetChecker.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace MyInternetChecker
{
    public partial class HistoryWindow : Window
    {
        private DispatcherTimer _updateTimer;
        public SeriesCollection SeriesCollection { get; set; }
        private Dictionary<string, ChartValues<double>> _chartValuesDict;

        public HistoryWindow()
        {
            InitializeComponent();

            _chartValuesDict = new Dictionary<string, ChartValues<double>>();
            SeriesCollection = new SeriesCollection();

            // Инициализируем график для каждого хоста с начальными значениями
            foreach (var host in ConfigManager.Hosts)
            {
                var initialValues = HistoryManager.GetLatestHistoryForHost(host);
                var chartValues = new ChartValues<double>(initialValues);

                _chartValuesDict[host] = chartValues;

                SeriesCollection.Add(new LineSeries
                {
                    Title = host,
                    Values = chartValues,
                    PointGeometry = null
                });
            }

            DataContext = this;

            // Таймер обновления - добавляет новые точки и удаляет старые
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateChartData;
            _updateTimer.Start();
        }

        private void UpdateChartData(object sender, EventArgs e)
        {
            foreach (var host in ConfigManager.Hosts)
            {
                if (!_chartValuesDict.TryGetValue(host, out var chartValues))
                    continue;

                var latestHistory = HistoryManager.GetLatestHistoryForHost(host);

                // Если история пустая или не изменилась - пропускаем
                if (latestHistory.Length == 0) continue;

                // Получаем последнее значение из истории
                var latestValue = latestHistory.Last();

                // Добавляем новую точку справа
                chartValues.Add(latestValue);

                // Удаляем самую старую точку слева, если превысили лимит
                if (chartValues.Count > 100)
                {
                    chartValues.RemoveAt(0);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            base.OnClosed(e);
        }
    }
}