using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Graphics;
using AGSUnpacker.Lib.Translation;
using AGSUnpacker.Lib.Utils;
using AGSUnpacker.UI.Core;
using AGSUnpacker.UI.Services;

using CommunityToolkit.Mvvm.Input;

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
      private set => SetProperty(ref _status, value);
    }

    private int _tasksRunning;
    public int TasksRunning
    {
      get => _tasksRunning;
      set => SetProperty(ref _tasksRunning, value);
    }
    #endregion

    #region Commands
    #region UnpackAssetsCommand
    private IAsyncRelayCommand _unpackAssetsCommand;
    public IAsyncRelayCommand UnpackAssetsCommand
    {
      get => _unpackAssetsCommand;
      set => SetProperty(ref _unpackAssetsCommand, value);
    }

    private async Task OnUnpackAssetsExecute()
    {
      try
      {
        await UnpackAsync("Select AGS game archive", "AGS archive|*.ags;*.exe",
        (filepath, targetFolder) =>
        {
          AssetsManager assetsManager = AssetsManager.Create(filepath);

          if (assetsManager == null)
            throw new InvalidDataException(
              $"Could not find assets at \"{filepath}\"\n\nMake sure you've selected a valid AGS game file.");

          assetsManager.Extract(targetFolder);
        });
      }
      catch (InvalidDataException ex)
      {
        MessageBox.Show(_windowService.GetWindow(this),
          ex.Message,
          "Assets not found",
          MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private bool OnCanUnpackAssetsExecute()
    {
      return !UnpackAssetsCommand.IsRunning;
    }
    #endregion

    #region UnpackSpritesCommand
    private IAsyncRelayCommand _unpackSpritesCommand;
    public IAsyncRelayCommand UnpackSpritesCommand
    {
      get => _unpackSpritesCommand;
      set => SetProperty(ref _unpackSpritesCommand, value);
    }

    private async Task OnUnpackSpritesExecute()
    {
      try
      {
        await UnpackAsync("Select acsprset.spr file", "AGS sprite set|*.spr",
          (filepath, targetFolder) => AGSSpriteSet.UnpackSprites(filepath, targetFolder)
        );
      }
      catch (NotSupportedException ex)
      {
        MessageBox.Show(_windowService.GetWindow(this),
          ex.Message,
          "Not supported",
          MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private bool OnCanUnpackSpritesExecute()
    {
      return !UnpackSpritesCommand.IsRunning;
    }
    #endregion

    #region RepackAssetsCommand
    private IAsyncRelayCommand _repackAssetsCommand;
    public IAsyncRelayCommand RepackAssetsCommand
    {
      get => _repackAssetsCommand;
      set => SetProperty(ref _repackAssetsCommand, value);
    }

    private Task OnRepackAssetsExecute()
    {
      // TODO(adm244): implement assets packing logic
      return Task.CompletedTask;
    }

    private bool OnCanRepackAssetsExecute()
    {
      // NOTE(adm244): temporary disabled (no assets packing logic yet)
      return false;
    }
    #endregion

    #region RepackSpritesCommand
    private IAsyncRelayCommand _repackSpritesCommand;
    public IAsyncRelayCommand RepackSpritesCommand
    {
      get => _repackSpritesCommand;
      set => SetProperty(ref _repackSpritesCommand, value);
    }

    private Task OnRepackSpritesExecute()
    {
      return RepackAsync("Select header.bin file", "Sprite set header|header.bin",
        (filepath, targetFolder) =>
        {
          string inputFolder = Path.GetDirectoryName(filepath);
          AGSSpriteSet.PackSprites(inputFolder, targetFolder);
        }
      );
    }

    private bool OnCanRepackSpritesExecute()
    {
      return !RepackSpritesCommand.IsRunning;
    }
    #endregion

    #region ExtractTRSCommand
    private IAsyncRelayCommand _extractTRSCommand;
    public IAsyncRelayCommand ExtractTranslationCommand
    {
      get => _extractTRSCommand;
      set => SetProperty(ref _extractTRSCommand, value);
    }

    private Task OnExtractTranslationExecute()
    {
      return ExtractAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, targetFolder) =>
        {
          string targetFilepath = Path.Combine(targetFolder, "Extracted.trs");
          TextExtractor.ExtractFromFolder(targetFolder, targetFilepath);
        }
      );
    }

    private bool OnCanExtractTranslationExecute()
    {
      return !ExtractTranslationCommand.IsRunning;
    }
    #endregion

    #region DecompileTRACommand
    private IAsyncRelayCommand _decompileTRACommand;
    public IAsyncRelayCommand DecompileTranslationCommand
    {
      get => _decompileTRACommand;
      set => SetProperty(ref _decompileTRACommand, value);
    }

    private Task OnDecompileTranslationExecute()
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

    private bool OnCanDecompileTranslationExecute()
    {
      return !DecompileTranslationCommand.IsRunning;
    }
    #endregion

    #region CompileTRSCommand
    private IAsyncRelayCommand _compileTRSCommand;
    public IAsyncRelayCommand CompileTranslationCommand
    {
      get => _compileTRSCommand;
      set => SetProperty(ref _compileTRSCommand, value);
    }

    private Task OnCompileTraslationExecute()
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

    private bool OnCanCompileTranslationExecute()
    {
      return !CompileTranslationCommand.IsRunning;
    }
    #endregion

    #region ExtractGameIdCommand
    private IAsyncRelayCommand _extractGameIdCommand;
    public IAsyncRelayCommand ExtractGameIdCommand
    {
      get => _extractGameIdCommand;
      set => SetProperty(ref _extractGameIdCommand, value);
    }

    private Task OnExtractGameIdExecute()
    {
      return ExtractAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, targetFolder) =>
        {
          string targetFilepath = Path.Combine(targetFolder, "game_id.txt");
          AGSIdentityExtractor.ExtractFromFolder(targetFolder, targetFilepath);
        }
      );
    }

    private bool OnCanExtractGameIdExecute()
    {
      return !ExtractGameIdCommand.IsRunning;
    }
    #endregion

    #region ShowRoomManagerWindowCommand
    private IRelayCommand _showRoomManagerWindowCommand;
    public IRelayCommand ShowRoomManagerCommand
    {
      get => _showRoomManagerWindowCommand;
      set => SetProperty(ref _showRoomManagerWindowCommand, value);
    }

    private void OnShowRoomManagerExecute()
    {
      _windowService.Show(new RoomManagerWindowViewModel(_windowService));
    }
    #endregion

    #region ExtractScriptsCommand
    private IAsyncRelayCommand _extractScriptsCommand;
    public IAsyncRelayCommand ExtractScriptsCommand
    {
      get => _extractScriptsCommand;
      set => SetProperty(ref _extractScriptsCommand, value);
    }

    private Task OnExtractScriptsExecute()
    {
      return ExtractAsync("Select AGS game executable", "AGS game executable|*.exe",
        (filepath, sourceFolder) =>
        {
          string targetFolder = Path.Combine(sourceFolder, "Scripts");
          ScriptManager.ExtractFromFolder(sourceFolder, targetFolder);
        }
      );
    }

    private bool OnCanExtractScriptsExecute()
    {
      return !ExtractScriptsCommand.IsRunning;
    }
    #endregion

    #region InjectScriptCommand
    private IAsyncRelayCommand _injectScriptCommand;
    public IAsyncRelayCommand InjectScriptCommand
    {
      get => _injectScriptCommand;
      set => SetProperty(ref _injectScriptCommand, value);
    }

    private Task OnInjectScriptExecute()
    {
      return SelectFilesAsync(
        (filenames) =>
        {
          ScriptManager.Inject(filenames[0], filenames[1]);
        },
        new DialogOptions
        {
          Title = "Select asset file to inject into",
          Filter = "Game data or room file|*.dta;*.crm"
        },
        new DialogOptions
        {
          Title = "Select script object file to inject",
          Filter = "SCOM3 script file|*." + ScriptManager.ScriptFileExtension
        }
      );
    }

    private bool OnCanInjectScriptExecute()
    {
      return !InjectScriptCommand.IsRunning;
    }
    #endregion

    #region ReplaceScriptTextCommand
    private IAsyncRelayCommand _replaceScriptTextCommand;
    public IAsyncRelayCommand ReplaceScriptTextCommand
    {
      get => _replaceScriptTextCommand;
      set => SetProperty(ref _replaceScriptTextCommand, value);
    }

    private Task OnReplaceScriptTextCommand()
    {
      return SelectFilesAsync(
        (filenames) =>
        {
          ScriptManager.ReplaceText(filenames[0], filenames.AsSpan(1).ToArray());
        },
        new DialogOptions
        {
          Title = "Select trs file to get replacements from",
          Filter = "Translation source file|*.trs"
        },
        new DialogOptions
        {
          Title = "Select one or more script files",
          Filter = "SCOM3 script file|*." + ScriptManager.ScriptFileExtension,
          Multiselect = true
        }
      );
    }

    private bool OnCanReplaceScriptTextCommand()
    {
      return !ReplaceScriptTextCommand.IsRunning;
    }
    #endregion
    #endregion

    // FIXME(adm244): code duplication; see RoomManagerWindowViewModel
    private Task SelectFileAsync(string title, string filter, Action<string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return Task.CompletedTask;

      return Task.Run(
        () => action(openDialog.FileName)
      );
    }

    private Task SelectFilesAsync(Action<string[]> action, params DialogOptions[] options)
    {
      List<string> filenames = new List<string>();

      OpenFileDialog openDialog = new OpenFileDialog();
      for (int i = 0; i < options.Length; ++i)
      {
        openDialog.Title = options[i].Title;
        openDialog.Filter = options[i].Filter;
        openDialog.Multiselect = options[i].Multiselect;
        openDialog.CheckFileExists = true;
        openDialog.CheckPathExists = true;

        if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
          return Task.CompletedTask;

        filenames.AddRange(openDialog.FileNames);
      }

      return Task.Run(
        () => action(filenames.ToArray())
      );
    }

    private Task UnpackAsync(string title, string filter, Action<string, string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
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

    private Task RepackAsync(string title, string filter, Action<string, string> action)
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = title,
        Filter = filter,
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
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

    private Task ExtractAsync(string title, string filter, Action<string, string> action)
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
      {
        MessageBox.Show(_windowService.GetWindow(this),
          $"Could not found unpacked assets at \"{targetFolder}\"\n\nMake sure to unpack assets first.",
          "Assets folder not found",
          MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
      }

      return Task.Run(
        () => action(filepath, targetFolder)
      );
    }

    // FIXME(adm244): code duplication; see RoomManagerWindowViewModel
    private void OnIsRunningChanged(bool newValue)
    {
      TasksRunning += newValue ? 1 : -1;

      if (TasksRunning < 0)
        TasksRunning = 0;

      Status = TasksRunning > 0 ? AppStatus.Busy : AppStatus.Ready;
    }

    // FIXME(adm244): code duplication; see RoomManagerWindowViewModel
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      IAsyncRelayCommand command = sender as IAsyncRelayCommand;
      Debug.Assert(command != null);

      switch (e.PropertyName)
      {
        case nameof(IAsyncRelayCommand.IsRunning):
          OnIsRunningChanged(command.IsRunning);
          command.NotifyCanExecuteChanged();
          break;

        case nameof(IAsyncRelayCommand.ExecutionTask):
          if (command.ExecutionTask is Task task && task.Exception is AggregateException)
            OnUncaughtException(task.Exception);
          break;

        default:
          break;
      }
    }

    // FIXME(adm244): code duplication; see RoomManagerWindowViewModel
    private void OnUncaughtException(AggregateException ex)
    {
      MessageBox.Show(_windowService.GetWindow(this),
        $"A critical error occurred during operation:\n\n{ex.Message}\n\nPlease, report this issue.",
        "Critical error",
        MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public MainWindowViewModel(WindowService windowService)
    {
      _windowService = windowService;

      Title = ProgramName;
      Status = AppStatus.Ready;
      TasksRunning = 0;

      // TODO(adm244): automate this

      UnpackAssetsCommand = new AsyncRelayCommand(OnUnpackAssetsExecute, OnCanUnpackAssetsExecute);
      UnpackAssetsCommand.PropertyChanged += OnPropertyChanged;

      UnpackSpritesCommand = new AsyncRelayCommand(OnUnpackSpritesExecute, OnCanUnpackSpritesExecute);
      UnpackSpritesCommand.PropertyChanged += OnPropertyChanged;

      RepackAssetsCommand = new AsyncRelayCommand(OnRepackAssetsExecute, OnCanRepackAssetsExecute);
      RepackAssetsCommand.PropertyChanged += OnPropertyChanged;

      RepackSpritesCommand = new AsyncRelayCommand(OnRepackSpritesExecute, OnCanRepackSpritesExecute);
      RepackSpritesCommand.PropertyChanged += OnPropertyChanged;

      ExtractTranslationCommand = new AsyncRelayCommand(OnExtractTranslationExecute, OnCanExtractTranslationExecute);
      ExtractTranslationCommand.PropertyChanged += OnPropertyChanged;

      DecompileTranslationCommand = new AsyncRelayCommand(OnDecompileTranslationExecute, OnCanDecompileTranslationExecute);
      DecompileTranslationCommand.PropertyChanged += OnPropertyChanged;

      CompileTranslationCommand = new AsyncRelayCommand(OnCompileTraslationExecute, OnCanCompileTranslationExecute);
      CompileTranslationCommand.PropertyChanged += OnPropertyChanged;

      ExtractScriptsCommand = new AsyncRelayCommand(OnExtractScriptsExecute, OnCanExtractScriptsExecute);
      ExtractScriptsCommand.PropertyChanged += OnPropertyChanged;

      InjectScriptCommand = new AsyncRelayCommand(OnInjectScriptExecute, OnCanInjectScriptExecute);
      InjectScriptCommand.PropertyChanged += OnPropertyChanged;

      ReplaceScriptTextCommand = new AsyncRelayCommand(OnReplaceScriptTextCommand, OnCanReplaceScriptTextCommand);
      ReplaceScriptTextCommand.PropertyChanged += OnPropertyChanged;

      ExtractGameIdCommand = new AsyncRelayCommand(OnExtractGameIdExecute, OnCanExtractGameIdExecute);
      ExtractGameIdCommand.PropertyChanged += OnPropertyChanged;

      ShowRoomManagerCommand = new RelayCommand(OnShowRoomManagerExecute);
    }

    private struct DialogOptions
    {
      public string Title;
      public string Filter;
      public bool Multiselect;

      public DialogOptions()
      {
        Title = string.Empty;
        Filter = string.Empty;
        Multiselect = false;
      }
    }
  }
}
