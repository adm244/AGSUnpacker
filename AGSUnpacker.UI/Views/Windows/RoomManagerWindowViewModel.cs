using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using AGSUnpacker.UI.Core;
using AGSUnpacker.UI.Models.Room;
using AGSUnpacker.UI.Services;

using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AGSUnpacker.UI.Views.Windows
{
  internal class RoomManagerWindowViewModel : ViewModel
  {
    private readonly WindowService _windowService;

    #region Properties
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

    private Room _room;
    public Room Room
    {
      get => _room;
      set
      {
        SetProperty(ref _room, value);
        SaveRoomCommand?.NotifyCanExecuteChanged();
        CloseRoomCommand?.NotifyCanExecuteChanged();
      }
    }

    private RoomFrame _selectedFrame;
    public RoomFrame SelectedFrame
    {
      get => _selectedFrame;
      set
      {
        SetProperty(ref _selectedFrame, value);
        SaveImageCommand?.NotifyCanExecuteChanged();
        ReplaceImageCommand?.NotifyCanExecuteChanged();
      }
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
      get => _selectedIndex;
      set => SetProperty(ref _selectedIndex, value);
    }
    #endregion

    #region Commands
    #region LoadRoomCommand
    private IRelayCommand _loadRoomCommand;
    public IRelayCommand LoadRoomCommand
    {
      get => _loadRoomCommand;
      set => SetProperty(ref _loadRoomCommand, value);
    }

    private async void OnLoadRoomExecute()
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = "Select room file",
        Filter = "AGS room file|*.crm",
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return;

      Status = AppStatus.Loading;

      Room = await ModelService.LoadRoomAsync(openDialog.FileName);

      Title = openDialog.SafeFileName;
      SelectedIndex = 0;

      Status = AppStatus.Ready;
    }
    #endregion

    #region SaveRoomCommand
    private IRelayCommand _saveRoomCommand;
    public IRelayCommand SaveRoomCommand
    {
      get => _saveRoomCommand;
      set => SetProperty(ref _saveRoomCommand, value);
    }

    private async void OnSaveRoomExecute()
    {
      SaveFileDialog saveDialog = new SaveFileDialog()
      {
        Title = "Save room file",
        Filter = "AGS room file|*.crm",
        CreatePrompt = false,
        OverwritePrompt = true,
        // FIXME(adm244): room filename stored in Title, really?
        FileName = Title
      };

      if (saveDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return;

      Status = AppStatus.Busy;

      await ModelService.SaveRoomAsync(saveDialog.FileName, Room);

      Status = AppStatus.Ready;
    }

    private bool OnCanSaveRoomExecute()
    {
      return Room != null;
    }
    #endregion

    #region CloseRoomCommand
    private IRelayCommand _closeRoomCommand;
    public IRelayCommand CloseRoomCommand
    {
      get => _closeRoomCommand;
      set => SetProperty(ref _closeRoomCommand, value);
    }

    private void OnCloseRoomExecute()
    {
      Room = null;
      Title = null;
    }

    private bool OnCanCloseRoomExecute()
    {
      return Room != null;
    }
    #endregion

    #region QuitCommand
    private IRelayCommand _quitCommand;
    public IRelayCommand QuitCommand
    {
      get => _quitCommand;
      set => SetProperty(ref _quitCommand, value);
    }

    private void OnQuitExecute()
    {
      _windowService.Close(this);
    }
    #endregion

    #region SaveImageCommand
    private IAsyncRelayCommand _saveImageCommand;
    public IAsyncRelayCommand SaveImageCommand
    {
      get => _saveImageCommand;
      set => SetProperty(ref _saveImageCommand, value);
    }

    private Task OnSaveImageExecute()
    {
      SaveFileDialog saveDialog = new SaveFileDialog()
      {
        Title = "Save image",
        Filter = "Image file|*.png;*.bmp",
        CreatePrompt = false,
        OverwritePrompt = true,
        // FIXME(adm244): room filename stored in Title, really?
        FileName = Path.GetFileNameWithoutExtension(Title)
      };

      if (saveDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return Task.CompletedTask;

      return Task.Run(
        () =>
        {
          Graphics.Bitmap bitmap = Room.BaseRoom.Background.Frames[SelectedIndex];

          Graphics.Formats.ImageFormat format = Graphics.Formats.ImageFormat.Png;
          if (bitmap.Format == Graphics.Formats.PixelFormat.Rgb565)
            format = Graphics.Formats.ImageFormat.Bmp;

          string filename = Path.GetFileNameWithoutExtension(saveDialog.FileName);
          string targetFolder = Path.GetDirectoryName(saveDialog.FileName);
          string targetFilepath = Path.Combine(targetFolder, filename);
          bitmap.Save(targetFilepath, format);
        }
      );
    }

    private bool OnCanSaveImageExecute()
    {
      return !SaveImageCommand.IsRunning && !ReplaceImageCommand.IsRunning && SelectedFrame != null;
    }
    #endregion

    #region ReplaceImageCommand
    private IAsyncRelayCommand _replaceImageCommand;
    public IAsyncRelayCommand ReplaceImageCommand
    {
      get => _replaceImageCommand;
      set => SetProperty(ref _replaceImageCommand, value);
    }

    private async Task OnReplaceImageExecute()
    {
      OpenFileDialog openDialog = new OpenFileDialog()
      {
        Title = "Select image file",
        Filter = "Image file|*.png;*.bmp",
        Multiselect = false,
        CheckFileExists = true,
        CheckPathExists = true
      };

      if (openDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return;

      Graphics.Bitmap image = await Task.Run(
        () => new Graphics.Bitmap(openDialog.FileName)
      );

      Room.ChangeFrame(SelectedIndex, image);
    }

    private bool OnCanReplaceImageExecute()
    {
      return !ReplaceImageCommand.IsRunning && SelectedFrame != null;
    }
    #endregion
    #endregion

    // FIXME(adm244): code duplication; see MainWindowViewModel
    private void OnIsRunningChanged(IAsyncRelayCommand command)
    {
      TasksRunning += command.IsRunning ? 1 : -1;

      if (TasksRunning < 0)
        TasksRunning = 0;

      Status = TasksRunning > 0 ? AppStatus.Busy : AppStatus.Ready;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      IAsyncRelayCommand command = sender as IAsyncRelayCommand;
      Debug.Assert(command != null);

      switch (e.PropertyName)
      {
        case nameof(IAsyncRelayCommand.IsRunning):
          OnIsRunningChanged(command);
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

    private void OnUncaughtException(AggregateException ex)
    {
      MessageBox.Show(_windowService.GetWindow(this),
        $"A critical error occurred during operation:\n\n{ex.Message}\n\nPlease, report this issue.",
        "Critical error",
        MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnReplaceImagePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnPropertyChanged(sender, e);
      SaveImageCommand.NotifyCanExecuteChanged();
    }

    public RoomManagerWindowViewModel(WindowService windowService)
    {
      _windowService = windowService;

      Room = null;
      Title = null;
      Status = AppStatus.Ready;

      LoadRoomCommand = new RelayCommand(OnLoadRoomExecute);
      SaveRoomCommand = new RelayCommand(OnSaveRoomExecute, OnCanSaveRoomExecute);
      CloseRoomCommand = new RelayCommand(OnCloseRoomExecute, OnCanCloseRoomExecute);
      QuitCommand = new RelayCommand(OnQuitExecute);

      SaveImageCommand = new AsyncRelayCommand(OnSaveImageExecute, OnCanSaveImageExecute);
      SaveImageCommand.PropertyChanged += OnPropertyChanged;

      ReplaceImageCommand = new AsyncRelayCommand(OnReplaceImageExecute, OnCanReplaceImageExecute);
      ReplaceImageCommand.PropertyChanged += OnReplaceImagePropertyChanged;
    }
  }
}
