using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using MyInternetChecker.ViewModels;

namespace MyInternetChecker;

/// <summary>Главное окно приложения</summary>
public partial class MainWindow
{
    private readonly double _screenHeight = SystemParameters.FullPrimaryScreenHeight;
    private readonly double _screenWidth = SystemParameters.FullPrimaryScreenWidth;

    private readonly MainWindowViewModel _vm;

    /// <summary>Инициализирует окно</summary>
    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainWindowViewModel();
        DataContext = _vm;

        _vm.HostsSettingsRequested += async (_, _) => await ShowSettingsWindowAsync();
        _vm.ExitRequested += (_, _) => Close();

        Loaded += (_, _) =>
        {
            Top = _screenHeight;
            Left = _screenWidth - _screenWidth;
            _vm.OnContextMenuOpened();
        };
    }

    private void Rect_MouseEnter(object sender, MouseEventArgs e) => _vm.OnMouseEnter();

    private void Rect_MouseLeave(object sender, MouseEventArgs e) => _vm.OnMouseLeave();

    private void Rect_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        => RectContextMenu.IsOpen = true;

    private void RectContextMenu_Opened(object sender, RoutedEventArgs e) => _vm.OnContextMenuOpened();

    private async Task ShowSettingsWindowAsync()
    {
        var settings_window = new HostsSettingsWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (settings_window.ShowDialog() == true)
            await _vm.ApplyHostsChangedAsync();
    }

    /// <summary>Освобождает ресурсы при закрытии окна</summary>
    /// <param name="e">Аргументы события закрытия</param>
    protected override void OnClosed(EventArgs e)
    {
        _vm.Dispose();
        base.OnClosed(e);
    }
}
