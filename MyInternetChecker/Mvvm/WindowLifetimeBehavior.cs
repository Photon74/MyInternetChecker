#nullable enable
using System;
using System.Windows;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для вызова действий при событиях окна без code-behind</summary>
public static class WindowLifetimeBehavior
{
    public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached(
        "LoadedCommand",
        typeof(Action<Window>),
        typeof(WindowLifetimeBehavior),
        new PropertyMetadata(null, OnLoadedCommandChanged));

    public static void SetLoadedCommand(Window Window, Action<Window>? Value) => Window.SetValue(LoadedCommandProperty, Value);

    public static Action<Window>? GetLoadedCommand(Window Window) => (Action<Window>?)Window.GetValue(LoadedCommandProperty);

    private static void OnLoadedCommandChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window window)
            return;

        window.Loaded -= Window_Loaded;
        if (E.NewValue is not null)
            window.Loaded += Window_Loaded;
    }

    private static void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window)
            return;

        var action = GetLoadedCommand(window);
        action?.Invoke(window);
    }

    public static readonly DependencyProperty ClosedCommandProperty = DependencyProperty.RegisterAttached(
        "ClosedCommand",
        typeof(Action),
        typeof(WindowLifetimeBehavior),
        new PropertyMetadata(null, OnClosedCommandChanged));

    public static void SetClosedCommand(Window Window, Action? Value) => Window.SetValue(ClosedCommandProperty, Value);

    public static Action? GetClosedCommand(Window Window) => (Action?)Window.GetValue(ClosedCommandProperty);

    private static void OnClosedCommandChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window window)
            return;

        window.Closed -= Window_Closed;
        if (E.NewValue is not null)
            window.Closed += Window_Closed;
    }

    private static void Window_Closed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
            return;

        var action = GetClosedCommand(window);
        action?.Invoke();
    }
}
