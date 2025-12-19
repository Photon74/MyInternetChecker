#nullable enable
using System.Windows;
using System.Windows.Input;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для выполнения команд при MouseEnter/MouseLeave без code-behind</summary>
public static class MouseOverBehavior
{
    public static readonly DependencyProperty MouseEnterCommandProperty = DependencyProperty.RegisterAttached(
        "MouseEnterCommand",
        typeof(ICommand),
        typeof(MouseOverBehavior),
        new PropertyMetadata(null, OnMouseEnterCommandChanged));

    public static void SetMouseEnterCommand(UIElement Element, ICommand? Value) => Element.SetValue(MouseEnterCommandProperty, Value);

    public static ICommand? GetMouseEnterCommand(UIElement Element) => (ICommand?)Element.GetValue(MouseEnterCommandProperty);

    private static void OnMouseEnterCommandChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not UIElement element)
            return;

        element.MouseEnter -= Element_MouseEnter;
        if (E.NewValue is not null)
            element.MouseEnter += Element_MouseEnter;
    }

    private static void Element_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        var cmd = GetMouseEnterCommand(element);
        if (cmd?.CanExecute(null) ?? false)
            cmd.Execute(null);
    }

    public static readonly DependencyProperty MouseLeaveCommandProperty = DependencyProperty.RegisterAttached(
        "MouseLeaveCommand",
        typeof(ICommand),
        typeof(MouseOverBehavior),
        new PropertyMetadata(null, OnMouseLeaveCommandChanged));

    public static void SetMouseLeaveCommand(UIElement Element, ICommand? Value) => Element.SetValue(MouseLeaveCommandProperty, Value);

    public static ICommand? GetMouseLeaveCommand(UIElement Element) => (ICommand?)Element.GetValue(MouseLeaveCommandProperty);

    private static void OnMouseLeaveCommandChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not UIElement element)
            return;

        element.MouseLeave -= Element_MouseLeave;
        if (E.NewValue is not null)
            element.MouseLeave += Element_MouseLeave;
    }

    private static void Element_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        var cmd = GetMouseLeaveCommand(element);
        if (cmd?.CanExecute(null) ?? false)
            cmd.Execute(null);
    }
}
