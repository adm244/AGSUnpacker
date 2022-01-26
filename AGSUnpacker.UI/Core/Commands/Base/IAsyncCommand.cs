using System;
using System.Windows.Input;

namespace AGSUnpacker.UI.Core.Commands
{
  internal interface IAsyncCommand : ICommand
  {
    public event EventHandler<bool> IsExecutingChanged;

    public bool IsExecuting { get; }
  }
}
