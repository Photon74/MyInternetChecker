#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyInternetChecker.Mvvm;

/// <summary>Базовый класс для ViewModel с поддержкой INotifyPropertyChanged</summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? property_name = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(property_name);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? property_name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
}
