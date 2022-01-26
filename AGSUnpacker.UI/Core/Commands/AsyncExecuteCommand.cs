using System;
using System.Threading.Tasks;

namespace AGSUnpacker.UI.Core.Commands
{
  internal class AsyncExecuteCommand : AsyncBaseCommand
  {
    private readonly Func<object, Task> _executeMethod;
    private readonly Func<object, bool> _canExecuteMethod;

    public AsyncExecuteCommand(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod = null)
    {
      _executeMethod = executeMethod;
      _canExecuteMethod = canExecuteMethod;
    }

    public override bool CanExecute(object parameter)
    {
      if (_canExecuteMethod != null)
        return _canExecuteMethod.Invoke(parameter);

      return base.CanExecute(parameter);
    }

    public override Task ExecuteAsync(object parameter)
    {
      return _executeMethod(parameter);
    }
  }
}
