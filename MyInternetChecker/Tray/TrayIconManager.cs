using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;

namespace MyInternetChecker
{
    /// <summary>
    /// Управление анимированной иконкой в трее
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _trayIcon;
        private MainWindow _mainWindow;
        private System.Windows.Threading.DispatcherTimer _animationTimer;
        private System.Drawing.Bitmap _trayBitmap;
        private System.Drawing.Graphics _graphics;
        private bool _isDisposed;

        public TrayIconManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            // Создаем иконку
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "MyInternetChecker - Монитор сети";
            _trayIcon.Visible = true;

            // Создаем растровое изображение для рисования (16x16 - стандартный размер иконки трея)
            _trayBitmap = new System.Drawing.Bitmap(16, 16);
            _graphics = System.Drawing.Graphics.FromImage(_trayBitmap);

            // Создаем контекстное меню
            CreateTrayContextMenu();

            // Настраиваем обработчики событий
            _trayIcon.DoubleClick += (sender, e) => ToggleMainWindow();
            _trayIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    // Обновляем состояние чекбокса автозапуска при каждом открытии меню
                    var autoStartItem = _trayIcon.ContextMenuStrip?.Items[3] as ToolStripMenuItem;
                    if (autoStartItem != null)
                        autoStartItem.Checked = AutoStartManager.IsAutoStartEnabled;
                }
            };

            // Запускаем таймер анимации (обновляем 10 раз в секунду)
            _animationTimer = new System.Windows.Threading.DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            _animationTimer.Tick += (sender, e) => UpdateTrayIcon();
            _animationTimer.Start();

            // Первоначальное обновление иконки
            UpdateTrayIcon();
        }

        /// <summary>
        /// Обновляет иконку в трее, рисуя текущий цвет из главного окна
        /// </summary>
        private void UpdateTrayIcon()
        {
            try
            {
                // Очищаем фон
                _graphics.Clear(Color.FromArgb(0, 0, 0, 0));

                // Получаем текущий цвет прямоугольника из главного окна
                var currentColor = GetCurrentColorFromMainWindow();

                // Рисуем квадрат 14x14 (с небольшим отступом)
                using (var brush = new SolidBrush(currentColor))
                {
                    _graphics.FillRectangle(brush, 1, 1, 14, 14);
                }

                // Добавляем тонкую рамку для видимости
                using (var pen = new System.Drawing.Pen(Color.FromArgb(100, Color.Gray), 1))
                {
                    _graphics.DrawRectangle(pen, 0, 0, 15, 15);
                }

                // Создаем иконку из bitmap
                var iconHandle = _trayBitmap.GetHicon();
                var icon = System.Drawing.Icon.FromHandle(iconHandle);

                // Меняем иконку
                if (_trayIcon.Icon != null)
                {
                    // Уничтожаем старую иконку (чтобы не было утечки памяти)
                    NativeMethods.DestroyIcon(_trayIcon.Icon.Handle);
                    _trayIcon.Icon.Dispose();
                }

                _trayIcon.Icon = icon;

                // Уничтожаем handle (иконка уже скопирована в системный кэш)
                NativeMethods.DestroyIcon(iconHandle);
                icon.Dispose();
            }
            catch (Exception ex)
            {
                // В случае ошибки просто игнорируем - лучше неработающая анимация, чем сломанная программа
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления иконки трея: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает текущий цвет прямоугольника из главного окна
        /// </summary>
        private Color GetCurrentColorFromMainWindow()
        {
            // Безопасно получаем цвет из главного окна
            Color fallbackColor = Color.Gray;

            try
            {
                // Вызываем в UI-потоке главного окна
                return _mainWindow.Dispatcher.Invoke(() =>
                {
                    if (_mainWindow.Rect.Fill is SolidColorBrush brush)
                    {
                        var mediaColor = brush.Color;
                        return Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
                    }
                    return fallbackColor;
                });
            }
            catch
            {
                return fallbackColor;
            }
        }

        /// <summary>
        /// Создает контекстное меню для иконки в трее
        /// </summary>
        private void CreateTrayContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            // Показать/скрыть окно
            var showHideItem = new ToolStripMenuItem("Показать/Скрыть окно");
            showHideItem.Click += (sender, e) => ToggleMainWindow();
            contextMenu.Items.Add(showHideItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Настройки хостов
            var settingsItem = new ToolStripMenuItem("Настройки хостов");
            settingsItem.Click += (sender, e) => _mainWindow.Dispatcher.Invoke(() => _mainWindow.ShowSettingsWindow());
            contextMenu.Items.Add(settingsItem);

            // Графики
            var graphsItem = new ToolStripMenuItem("Показать графики");
            graphsItem.Click += (sender, e) => _mainWindow.Dispatcher.Invoke(() => _mainWindow.ShowHistoryWindow());
            contextMenu.Items.Add(graphsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Автозапуск
            var autoStartItem = new ToolStripMenuItem("Автозапуск с Windows");
            autoStartItem.Click += (sender, e) =>
            {
                AutoStartManager.IsAutoStartEnabled = !AutoStartManager.IsAutoStartEnabled;
                autoStartItem.Checked = AutoStartManager.IsAutoStartEnabled;
            };
            autoStartItem.Checked = AutoStartManager.IsAutoStartEnabled;
            contextMenu.Items.Add(autoStartItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Выход
            var exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (sender, e) =>
            {
                if (MessageBox.Show("Завершить работу программы?", "Выход",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            };
            contextMenu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = contextMenu;
        }

        /// <summary>
        /// Показывает или скрывает главное окно
        /// </summary>
        private void ToggleMainWindow()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.Hide();
                }
                else
                {
                    _mainWindow.Show();
                    _mainWindow.Activate();
                }
            });
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _animationTimer?.Stop();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                if (_trayIcon.Icon != null)
                {
                    NativeMethods.DestroyIcon(_trayIcon.Icon.Handle);
                    _trayIcon.Icon.Dispose();
                }
                _trayIcon.Dispose();
            }

            _graphics?.Dispose();
            _trayBitmap?.Dispose();
        }
    }

    /// <summary>
    /// Вспомогательные методы WinAPI
    /// </summary>
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}