using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Graphics;
using AGSUnpacker.Lib.Translation;
using AGSUnpacker.Lib.Utils;
using AGSUnpacker.UI.Core;
using AGSUnpacker.UI.Core.Commands;
using AGSUnpacker.UI.Service;

using Microsoft.Win32;

namespace AGSUnpacker.UI.Views.Windows
{
  internal class MainWindowViewModel : ViewModel
  {
    private readonly WindowService _windowService;

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


    private int _tasksRunning;
    public int TasksRunning
    {
      get => _tasksRunning;
      set => SetProperty(ref _tasksRunning, value);
    }
    #endregion

    #region Commands
    #region UnpackAssetsCommand
    private IAsyncCommand _unpackAssetsCommand;
    public IAsyncCommand UnpackAssetsCommand
    {
      get => _unpackAssetsCommand;
      set => SetProperty(ref _unpackAssetsCommand, value);
    }

    private Task OnUnpackAssetsCommandExecuteAsync(object parameter)
    {
      return UnpackAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, targetFolder) =>
        {
          AssetsManager assetsManager = AssetsManager.Create(filepath);
          assetsManager.Extract(targetFolder);
        }
      );
    }
    #endregion

    #region UnpackSpritesCommand
    private IAsyncCommand _unpackSpritesCommand;
    public IAsyncCommand UnpackSpritesCommand
    {
      get => _unpackSpritesCommand;
      set => SetProperty(ref _unpackSpritesCommand, value);
    }

    private Task OnUnpackSpritesCommandExecuteAsync(object parameter)
    {
      return UnpackAsync("Select acsprset.spr file", "AGS sprite set|*.spr",
        (filepath, targetFolder) => AGSSpriteSet.UnpackSprites(filepath, targetFolder)
      );
    }
    #endregion

    #region RepackAssetsCommand
    private IAsyncCommand _repackAssetsCommand;
    public IAsyncCommand RepackAssetsCommand
    {
      get => _repackAssetsCommand;
      set => SetProperty(ref _repackAssetsCommand, value);
    }

    private Task OnRepackAssetsCommandExecuteAsync(object parameter)
    {
      // TODO(adm244): implement assets packing logic
      return Task.CompletedTask;
    }
    #endregion

    #region RepackSpritesCommand
    private IAsyncCommand _repackSpritesCommand;
    public IAsyncCommand RepackSpritesCommand
    {
      get => _repackSpritesCommand;
      set => SetProperty(ref _repackSpritesCommand, value);
    }

    private Task OnRepackSpritesCommandExecuteAsync(object parameter)
    {
      return RepackAsync("Select header.bin file", "Sprite set header|header.bin",
        (filepath, targetFolder) =>
        {
          string inputFolder = Path.GetDirectoryName(filepath);
          AGSSpriteSet.PackSprites(inputFolder, targetFolder);
        }
      );
    }
    #endregion

    #region ExtractTRSCommand
    private IAsyncCommand _extractTRSCommand;
    public IAsyncCommand ExtractTRSCommand
    {
      get => _extractTRSCommand;
      set => SetProperty(ref _extractTRSCommand, value);
    }

    private Task OnExtractTRSCommandExecuteAsync(object parameter)
    {
      return UnpackAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, targetFolder) =>
        {
          string targetFilepath = Path.Combine(targetFolder, "Extracted.trs");
          TextExtractor.Extract(filepath, targetFilepath);
        }
      );
    }
    #endregion

    #region DecompileTRACommand
    private IAsyncCommand _decompileTRACommand;
    public IAsyncCommand DecompileTRACommand
    {
      get => _decompileTRACommand;
      set => SetProperty(ref _decompileTRACommand, value);
    }

    private Task OnDecompileTRACommandExecuteAsync(object parameter)
    {
      return SelectFileAsync("Select TRA file", "AGS compiled translation|*.tra",
        (filepath) =>
        {
          string targetFilepath = Path.ChangeExtension(filepath, "trs");
          AGSTranslation translation = new AGSTranslation();

          translation.Decompile(filepath);
          translation.WriteSourceFile(targetFilepath);
        }
      );
    }
    #endregion

    #region CompileTRSCommand
    private IAsyncCommand _compileTRSCommand;
    public IAsyncCommand CompileTRSCommand
    {
      get => _compileTRSCommand;
      set => SetProperty(ref _compileTRSCommand, value);
    }

    private Task OnCompileTRSCommandExecuteAsync(object parameter)
    {
      return SelectFileAsync("Select TRS file", "AGS translation|*.trs",
        (filepath) =>
        {
          string targetFilepath = Path.ChangeExtension(filepath, "tra");
          AGSTranslation translation = AGSTranslation.ReadSourceFile(filepath);
          translation.Compile(targetFilepath);
        }
      );
    }
    #endregion

    #region ExtractGameIdCommand
    private IAsyncCommand _extractGameIdCommand;
    public IAsyncCommand ExtractGameIdCommand
    {
      get => _extractGameIdCommand;
      set => SetProperty(ref _extractGameIdCommand, value);
    }

    private Task OnExtractGameIdCommandExecuteAsync(object parameter)
    {
      return UnpackAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, targetFolder) => AGSIdentityExtractor.ExtractIdentity(filepath, targetFolder)
      );
    }
    #endregion

    #region ShowRoomManagerWindowCommand
    private ICommand _showRoomManagerWindowCommand;
    public ICommand ShowRoomManagerWindowCommand
    {
      get => _showRoomManagerWindowCommand;
      set => SetProperty(ref _showRoomManagerWindowCommand, value);
    }

    private void OnShowRoomManagerWindowCommandExecute(object parameter)
    {
      _windowService.Show(new RoomManagerWindowViewModel(_windowService));
    }
    #endregion
    #endregion

    // FIXME(adm244): code duplication; see RoomManagerWindowViewModel
    private static Task SelectFileAsync(string title, string filter, Action<string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(App.Current.MainWindow) != true)
        return Task.CompletedTask;

      return Task.Run(
        () => action(openDialog.FileName)
      );
    }

    private static Task UnpackAsync(string title, string filter, Action<string, string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(App.Current.MainWindow) != true)
        return Task.CompletedTask;

      string filepath = openDialog.FileName;
      string filename = Path.GetFileNameWithoutExtension(openDialog.SafeFileName);
      string folder = Path.GetDirectoryName(openDialog.FileName);
      string targetFolder = Path.Combine(folder, filename);

      if (!Directory.Exists(targetFolder))
        Directory.CreateDirectory(targetFolder);

      return Task.Run(
        () => action(filepath, targetFolder)
      );
    }

    private static Task RepackAsync(string title, string filter, Action<string, string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(App.Current.MainWindow) != true)
        return Task.CompletedTask;

      string filepath = openDialog.FileName;
      string folder = Path.GetDirectoryName(openDialog.FileName);
      string targetFolder = Path.Combine(folder, "packed");

      if (!Directory.Exists(targetFolder))
        Directory.CreateDirectory(targetFolder);

      return Task.Run(
        () => action(filepath, targetFolder)
      );
    }

    private void OnIsExecutingChanged(object sender, bool newValue)
    {
      TasksRunning += newValue ? 1 : -1;

      if (TasksRunning < 0)
        TasksRunning = 0;

      Status = TasksRunning > 0 ? AppStatus.Busy : AppStatus.Ready;
    }

    public MainWindowViewModel(WindowService windowService)
    {
      _windowService = windowService;
      Title = ProgramName;
      TasksRunning = 0;

      // TODO(adm244): automate this somehow (reflection maybe?)

      UnpackAssetsCommand = new AsyncExecuteCommand(OnUnpackAssetsCommandExecuteAsync);
      UnpackAssetsCommand.IsExecutingChanged += OnIsExecutingChanged;

      UnpackSpritesCommand = new AsyncExecuteCommand(OnUnpackSpritesCommandExecuteAsync);
      UnpackSpritesCommand.IsExecutingChanged += OnIsExecutingChanged;

      // NOTE(adm244): temporary disabled (no assets packing logic yet)
      RepackAssetsCommand = new AsyncExecuteCommand(OnRepackAssetsCommandExecuteAsync, (o) => false);
      RepackAssetsCommand.IsExecutingChanged += OnIsExecutingChanged;

      RepackSpritesCommand = new AsyncExecuteCommand(OnRepackSpritesCommandExecuteAsync);
      RepackSpritesCommand.IsExecutingChanged += OnIsExecutingChanged;

      ExtractTRSCommand = new AsyncExecuteCommand(OnExtractTRSCommandExecuteAsync);
      ExtractTRSCommand.IsExecutingChanged += OnIsExecutingChanged;

      DecompileTRACommand = new AsyncExecuteCommand(OnDecompileTRACommandExecuteAsync);
      DecompileTRACommand.IsExecutingChanged += OnIsExecutingChanged;

      CompileTRSCommand = new AsyncExecuteCommand(OnCompileTRSCommandExecuteAsync);
      CompileTRSCommand.IsExecutingChanged += OnIsExecutingChanged;

      ExtractGameIdCommand = new AsyncExecuteCommand(OnExtractGameIdCommandExecuteAsync);
      ExtractGameIdCommand.IsExecutingChanged += OnIsExecutingChanged;

      ShowRoomManagerWindowCommand = new ExecuteCommand(OnShowRoomManagerWindowCommandExecute);
    }
  }
}
