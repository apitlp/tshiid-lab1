using System.Windows.Input;

namespace Game2048;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action execute = execute;
    private readonly Func<bool> canExecute = canExecute ?? (() => true);

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => canExecute();

    public void Execute(object? parameter) => execute();

    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}