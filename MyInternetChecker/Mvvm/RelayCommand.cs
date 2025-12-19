#nullable enable
using System;
using System.Windows.Input;

namespace MyInternetChecker.Mvvm;

/// <summary>Команда для привязки действий UI к ViewModel</summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _Execute;
    private readonly Func<bool>? _CanExecute;

    public RelayCommand(Action Execute, Func<bool>? CanExecute = null)
    {
        ArgumentNullException.ThrowIfNull(Execute);

        _Execute = Execute;
        _CanExecute = CanExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? Parameter) => _CanExecute?.Invoke() ?? true;

    public void Execute(object? Parameter) => _Execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>Команда для привязки действий UI с параметром к ViewModel</summary>
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _Execute;
    private readonly Predicate<T?>? _CanExecute;

    public RelayCommand(Action<T?> Execute, Predicate<T?>? CanExecute = null)
    {
        ArgumentNullException.ThrowIfNull(Execute);

        _Execute = Execute;
        _CanExecute = CanExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? Parameter)
    {
        if (_CanExecute is null)
            return true;

        if (Parameter is null)
            return _CanExecute(default);

        return Parameter is T typed && _CanExecute(typed);
    }

    public void Execute(object? Parameter)
    {
        if (Parameter is null)
        {
            _Execute(default);
            return;
        }

        if (Parameter is T typed)
            _Execute(typed);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
