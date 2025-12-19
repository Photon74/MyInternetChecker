#nullable enable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для выполнения команды при открытии ContextMenu без code-behind</summary>
public static class ContextMenuEventBehavior
{
    public static readonly DependencyProperty OpenedCommandProperty = DependencyProperty.RegisterAttached(
        "OpenedCommand",
        typeof(ICommand),
        typeof(ContextMenuEventBehavior),
        new PropertyMetadata(null, OnOpenedCommandChanged));

    public static void SetOpenedCommand(ContextMenu Menu, ICommand? Value) => Menu.SetValue(OpenedCommandProperty, Value);

    public static ICommand? GetOpenedCommand(ContextMenu Menu) => (ICommand?)Menu.GetValue(OpenedCommandProperty);

    private static void OnOpenedCommandChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not ContextMenu menu)
            return;

        menu.Opened -= Menu_Opened;
        if (E.NewValue is not null)
            menu.Opened += Menu_Opened;
    }

    private static void Menu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu)
            return;

        var cmd = GetOpenedCommand(menu);
        if (cmd?.CanExecute(null) ?? false)
            cmd.Execute(null);
    }
}
