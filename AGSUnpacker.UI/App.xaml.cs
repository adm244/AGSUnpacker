using System.Windows;

using AGSUnpacker.UI.Service;
using AGSUnpacker.UI.Views.Windows;

namespace AGSUnpacker.UI
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      WindowService windowService = new WindowService();

      windowService.Register<RoomManagerWindowViewModel, RoomManagerWindowView>();

      MainWindowViewModel mainViewModel = new MainWindowViewModel(windowService);
      MainWindowView mainView = new MainWindowView
      {
        DataContext = mainViewModel
      };

      mainView.Show();
    }
  }
}
