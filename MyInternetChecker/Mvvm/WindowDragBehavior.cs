#nullable enable
using System.Windows;
using System.Windows.Input;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для перетаскивания окна мышью без code-behind</summary>
public static class WindowDragBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(WindowDragBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(Window Window, bool Value) => Window.SetValue(IsEnabledProperty, Value);

    public static bool GetIsEnabled(Window Window) => (bool)Window.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window window)
            return;

        if (E.NewValue is true)
            window.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        else
            window.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
    }

    private static void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Window window && e.ButtonState == MouseButtonState.Pressed)
            window.DragMove();
    }
}
