#nullable enable
using System;
using System.Windows;

namespace MyInternetChecker.Mvvm;

/// <summary>Сервис для открытия окон без участия code-behind</summary>
public static class WindowService
{
    public static readonly DependencyProperty HostsSettingsRequestedProperty = DependencyProperty.RegisterAttached(
        "HostsSettingsRequested",
        typeof(bool),
        typeof(WindowService),
        new PropertyMetadata(false, OnHostsSettingsRequestedChanged));

    public static void SetHostsSettingsRequested(Window Window, bool Value) => Window.SetValue(HostsSettingsRequestedProperty, Value);

    public static bool GetHostsSettingsRequested(Window Window) => (bool)Window.GetValue(HostsSettingsRequestedProperty);

    private static async void OnHostsSettingsRequestedChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window owner)
            return;

        if (E.NewValue is not true)
            return;

        if (owner.DataContext is not ViewModels.MainWindowViewModel vm)
            return;

        vm.RequestHostsSettings = false;

        var settings_window = new HostsSettingsWindow
        {
            Owner = owner,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (settings_window.ShowDialog() == true)
            await vm.ApplyHostsChangedAsync();
    }
}
