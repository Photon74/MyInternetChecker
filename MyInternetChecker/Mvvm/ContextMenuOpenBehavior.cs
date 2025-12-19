#nullable enable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для открытия ContextMenu по правому клику без code-behind</summary>
public static class ContextMenuOpenBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(ContextMenuOpenBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(UIElement Element, bool Value) => Element.SetValue(IsEnabledProperty, Value);

    public static bool GetIsEnabled(UIElement Element) => (bool)Element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not UIElement element)
            return;

        if (E.NewValue is true)
            element.PreviewMouseRightButtonDown += Element_PreviewMouseRightButtonDown;
        else
            element.PreviewMouseRightButtonDown -= Element_PreviewMouseRightButtonDown;
    }

    private static void Element_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.ContextMenu is null)
            return;

        element.ContextMenu.PlacementTarget = element;
        element.ContextMenu.IsOpen = true;
        e.Handled = true;
    }
}
