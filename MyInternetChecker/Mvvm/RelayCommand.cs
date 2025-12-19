#nullable enable
using System;
using System.Threading.Tasks;
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

/// <summary>Команда для привязки действий UI к ViewModel с произвольным параметром</summary>
public sealed class RelayCommandObject : ICommand
{
    private readonly Action<object?> _Execute;
    private readonly Predicate<object?>? _CanExecute;

    public RelayCommandObject(Action<object?> Execute, Predicate<object?>? CanExecute = null)
    {
        ArgumentNullException.ThrowIfNull(Execute);

        _Execute = Execute;
        _CanExecute = CanExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? Parameter) => _CanExecute?.Invoke(Parameter) ?? true;

    public void Execute(object? Parameter) => _Execute(Parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>Команда для привязки асинхронных действий UI к ViewModel</summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _Execute;
    private readonly Func<bool>? _CanExecute;

    private bool _IsExecuting;

    public AsyncRelayCommand(Func<Task> Execute, Func<bool>? CanExecute = null)
    {
        ArgumentNullException.ThrowIfNull(Execute);

        _Execute = Execute;
        _CanExecute = CanExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? Parameter) => !_IsExecuting && (_CanExecute?.Invoke() ?? true);

    public async void Execute(object? Parameter)
    {
        if (!CanExecute(Parameter))
            return;

        _IsExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _Execute();
        }
        finally
        {
            _IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

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
