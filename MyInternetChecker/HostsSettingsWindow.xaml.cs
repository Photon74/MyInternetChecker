using System;
using System.Windows;
using System.Windows.Input;

using MyInternetChecker.ViewModels;

namespace MyInternetChecker;

/// <summary>Окно для редактирования списка хостов</summary>
public partial class HostsSettingsWindow
{
    private readonly HostsSettingsViewModel _vm;

    /// <summary>Инициализирует окно</summary>
    public HostsSettingsWindow()
    {
        InitializeComponent();

        _vm = new HostsSettingsViewModel();
        DataContext = _vm;

        _vm.ErrorOccurred += (_, message) => MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        _vm.SaveRequested += (_, _) =>
        {
            _vm.Save();
            DialogResult = true;
            Close();
        };
        _vm.CancelRequested += (_, _) =>
        {
            DialogResult = false;
            Close();
        };

        Loaded += (_, _) => NewHostTextBox.Focus();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
}