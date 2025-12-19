#nullable enable
using System;
using System.Windows;

namespace MyInternetChecker.Mvvm;

/// <summary>Attached property для закрытия окна из ViewModel</summary>
public static class DialogCloser
{
    public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
        "DialogResult",
        typeof(bool?),
        typeof(DialogCloser),
        new PropertyMetadata(null, OnDialogResultChanged));

    public static void SetDialogResult(Window Window, bool? Value) => Window.SetValue(DialogResultProperty, Value);

    public static bool? GetDialogResult(Window Window) => (bool?)Window.GetValue(DialogResultProperty);

    private static void OnDialogResultChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
    {
        if (D is not Window window)
            return;

        if (E.NewValue is not bool result)
            return;

        window.DialogResult = result;
        window.Close();
    }
}
