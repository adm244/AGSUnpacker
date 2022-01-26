using System;

namespace AGSUnpacker.UI.Core.Commands
{
  internal class ExecuteCommand : BaseCommand
  {
    private readonly Action<object> _executeMethod;
    private readonly Func<object, bool> _canExecuteMethod;

    public ExecuteCommand(Action<object> executeMethod)
    {
      _executeMethod = executeMethod;
    }

    public ExecuteCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
    {
      _executeMethod = executeMethod;
      _canExecuteMethod = canExecuteMethod;
    }

    public override bool CanExecute(object parameter)
    {
      if (_canExecuteMethod != null)
        return _canExecuteMethod(parameter);

      return _executeMethod != null;
    }

    public override void Execute(object parameter)
    {
      _executeMethod?.Invoke(parameter);
    }
  }
}
