using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

using AGSUnpacker.Lib.Assets;
using AGSUnpacker.UI.Core;
using AGSUnpacker.UI.Core.Commands;

using Microsoft.Win32;

namespace AGSUnpacker.UI.Views.Windows
{
  internal class MainWindowViewModel : ViewModel
  {
    #region Properties
    public static string ProgramName => AppDescription.ProgramName;
    public static string ProgramVersion => AppDescription.ProgramVersion;

    private string _title;
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    private AppStatus _status;
    public AppStatus Status
    {
      get => _status;
      private set
      {
        SetProperty(ref _status, value);
        OnPropertyChanged(nameof(StatusText));
      }
    }

    public string StatusText => Status.AsString();
    #endregion

    #region Commands
    #region UnpackAssetsCommand
    private ICommand _unpackAssetsCommand;
    public ICommand UnpackAssetsCommand
    {
      get => _unpackAssetsCommand;
      set => SetProperty(ref _unpackAssetsCommand, value);
    }

    private bool OnUnpackAssetsCommandCanExecute(object parameter)
    {
      return Status == AppStatus.Ready;
    }

    private async void OnUnpackAssetsCommandExecute(object parameter)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = "Select AGS game executable",
        Filter = "AGS game executable|*.exe",
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(App.Current.MainWindow) != true)
        return;

      string assetsFilepath = openDialog.FileName;
      string assetsFilename = Path.GetFileNameWithoutExtension(openDialog.SafeFileName);
      string assetsFolder = Path.GetDirectoryName(openDialog.FileName);
      string assetsTargetFolder = Path.Combine(assetsFolder, assetsFilename);

      Status = AppStatus.Busy;

      await Task.Run(() =>
      {
        AssetsManager assetsManager = AssetsManager.Create(assetsFilepath);
        assetsManager.Extract(assetsTargetFolder);
      }).ConfigureAwait(true);

      Status = AppStatus.Ready;

      // HACK(adm244): force command bindings to update CanExecute value
      CommandManager.InvalidateRequerySuggested();
    }
    #endregion
    #endregion

    public MainWindowViewModel()
    {
      Title = ProgramName;

      UnpackAssetsCommand = new ExecuteCommand(OnUnpackAssetsCommandExecute, OnUnpackAssetsCommandCanExecute);
    }
  }
}
