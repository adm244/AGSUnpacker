using System.IO;
using System.Threading.Tasks;

using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Graphics;
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
    private IAsyncCommand _unpackAssetsCommand;
    public IAsyncCommand UnpackAssetsCommand
    {
      get => _unpackAssetsCommand;
      set => SetProperty(ref _unpackAssetsCommand, value);
    }

    private async Task OnUnpackAssetsCommandExecuteAsync(object parameter)
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

      await Task.Run(() =>
      {
        AssetsManager assetsManager = AssetsManager.Create(assetsFilepath);
        assetsManager.Extract(assetsTargetFolder);
      });
    }
    #endregion

    #region UnpackSpritesCommand
    private IAsyncCommand _unpackSpritesCommand;
    public IAsyncCommand UnpackSpritesCommand
    {
      get => _unpackSpritesCommand;
      set => SetProperty(ref _unpackSpritesCommand, value);
    }
    
    private async Task OnUnpackSpritesCommandExecuteAsync(object parameter)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = "Select acsprset.spr file",
        Filter = "AGS sprite set|*.spr",
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(App.Current.MainWindow) != true)
        return;

      string sprsetFilepath = openDialog.FileName;
      string sprsetFilename = Path.GetFileNameWithoutExtension(openDialog.SafeFileName);
      string sprsetFolder = Path.GetDirectoryName(openDialog.FileName);
      string sprsetTargetFolder = Path.Combine(sprsetFolder, sprsetFilename);

      // FIX(adm244): should caller create a folder or callee?
      if (!Directory.Exists(sprsetTargetFolder))
        Directory.CreateDirectory(sprsetTargetFolder);

      await Task.Run(() =>
      {
        AGSSpriteSet.UnpackSprites(sprsetFilepath, sprsetTargetFolder);
      });
    }
    #endregion
    #endregion

    private void OnIsExecutingChanged(object sender, bool newValue)
    {
      Status = newValue ? AppStatus.Busy : AppStatus.Ready;
    }

    public MainWindowViewModel()
    {
      Title = ProgramName;

      UnpackAssetsCommand = new AsyncExecuteCommand(OnUnpackAssetsCommandExecuteAsync);
      UnpackAssetsCommand.IsExecutingChanged += OnIsExecutingChanged;

      UnpackSpritesCommand = new AsyncExecuteCommand(OnUnpackSpritesCommandExecuteAsync);
      UnpackSpritesCommand.IsExecutingChanged += OnIsExecutingChanged;
    }
  }
}
