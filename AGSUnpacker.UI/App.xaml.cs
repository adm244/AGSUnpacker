using System.Windows;

using AGSUnpacker.UI.Services;
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

      windowService.Register<MainWindowViewModel, MainWindowView>();
      windowService.Register<RoomManagerWindowViewModel, RoomManagerWindowView>();

      MainWindowViewModel mainViewModel = new MainWindowViewModel(windowService);
      windowService.Show(mainViewModel);
    }
  }
}
