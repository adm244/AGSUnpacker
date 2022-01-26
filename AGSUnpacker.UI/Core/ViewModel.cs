using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AGSUnpacker.UI.Core
{
  internal class ViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
    {
      if (Equals(member, value))
        return;

      member = value;
      OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
