using System.Windows.Input;

using AGSUnpacker.UI.Core;
using AGSUnpacker.UI.Core.Commands;
using AGSUnpacker.UI.Models.Room;
using AGSUnpacker.UI.Service;

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
      private set
      {
        SetProperty(ref _status, value);
        OnPropertyChanged(nameof(StatusText));
      }
    }

    // TODO(adm244): just use converters
    public string StatusText => Status.AsString();

    private Room _room;
    public Room Room
    {
      get => _room;
      set => SetProperty(ref _room, value);
    }

    private RoomFrame _selectedFrame;
    public RoomFrame SelectedFrame
    {
      get => _selectedFrame;
      set => SetProperty(ref _selectedFrame, value);
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
    private ICommand _loadRoomCommand;
    public ICommand LoadRoomCommand
    {
      get => _loadRoomCommand;
      set => SetProperty(ref _loadRoomCommand, value);
    }

    private async void OnLoadRoomCommandExecuted(object parameter)
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
    private ICommand _saveRoomCommand;
    public ICommand SaveRoomCommand
    {
      get => _saveRoomCommand;
      set => SetProperty(ref _saveRoomCommand, value);
    }

    private async void OnSaveRoomCommandExecuted(object parameter)
    {
      SaveFileDialog saveDialog = new SaveFileDialog()
      {
        Title = "Save room file",
        Filter = "AGS room file|*.crm",
        CreatePrompt = false,
        OverwritePrompt = true,
      };

      if (saveDialog.ShowDialog(_windowService.GetWindow(this)) != true)
        return;

      Status = AppStatus.Busy;

      await ModelService.SaveRoomAsync(saveDialog.FileName, Room);

      Status = AppStatus.Ready;
    }

    private bool OnCanSaveRoomCommandExecuted(object parameter)
    {
      return Room != null;
    }
    #endregion

    #region CloseRoomCommand
    private ICommand _closeRoomCommand;
    public ICommand CloseRoomCommand
    {
      get => _closeRoomCommand;
      set => SetProperty(ref _closeRoomCommand, value);
    }

    private void OnCloseRoomCommandExecuted(object parameter)
    {
      Room = null;
    }

    private bool OnCanCloseRoomCommandExecuted(object parameter)
    {
      return Room != null;
    }
    #endregion

    #region QuitCommand
    private ICommand _quitCommand;
    public ICommand QuitCommand
    {
      get => _quitCommand;
      set => SetProperty(ref _quitCommand, value);
    }

    private void OnQuitCommandExecuted(object parameter)
    {
      _windowService.Close(this);
    }
    #endregion
    #endregion

    public RoomManagerWindowViewModel(WindowService windowService)
    {
      _windowService = windowService;

      Room = null;
      Status = AppStatus.Ready;

      LoadRoomCommand = new ExecuteCommand(OnLoadRoomCommandExecuted);
      SaveRoomCommand = new ExecuteCommand(OnSaveRoomCommandExecuted, OnCanSaveRoomCommandExecuted);
      CloseRoomCommand = new ExecuteCommand(OnCloseRoomCommandExecuted, OnCanCloseRoomCommandExecuted);
      QuitCommand = new ExecuteCommand(OnQuitCommandExecuted);
    }
  }
}
