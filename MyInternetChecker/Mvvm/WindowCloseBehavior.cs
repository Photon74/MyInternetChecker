#nullable enable
using System;
using System.Windows;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для закрытия окна по флагу из ViewModel</summary>
public static class WindowCloseBehavior
{
    public static readonly DependencyProperty CloseRequestedProperty = DependencyProperty.RegisterAttached(
        "CloseRequested",
        typeof(bool),
        typeof(WindowCloseBehavior),
        new PropertyMetadata(false, OnCloseRequestedChanged));

    public static void SetCloseRequested(Window Window, bool Value) => Window.SetValue(CloseRequestedProperty, Value);

    public static bool GetCloseRequested(Window Window) => (bool)Window.GetValue(CloseRequestedProperty);

    private static void OnCloseRequestedChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window window)
            return;

        if (E.NewValue is not true)
            return;

        window.Close();

        if (window.DataContext is ViewModels.MainWindowViewModel vm)
            vm.RequestClose = false;
    }
}
