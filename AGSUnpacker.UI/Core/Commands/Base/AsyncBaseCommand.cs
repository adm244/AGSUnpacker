using System;
using System.Threading.Tasks;

namespace AGSUnpacker.UI.Core.Commands
{
  internal abstract class AsyncBaseCommand : IAsyncCommand
  {
    public event EventHandler CanExecuteChanged;
    public event EventHandler<bool> IsExecutingChanged;

    private bool _isExecuting;
    public bool IsExecuting
    {
      get => _isExecuting;
      set
      {
        _isExecuting = value;
        IsExecutingChanged?.Invoke(this, value);
        OnIsExecutingChanged();
      }
    }

    protected virtual void OnIsExecutingChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public virtual bool CanExecute(object parameter)
    {
      return !IsExecuting;
    }

    public async void Execute(object parameter)
    {
      IsExecuting = true;

      await ExecuteAsync(parameter);

      IsExecuting = false;
    }

    public abstract Task ExecuteAsync(object parameter);
  }
}
