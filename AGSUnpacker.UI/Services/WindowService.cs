using System;
using System.Collections.Generic;
using System.Windows;

using AGSUnpacker.UI.Core;

namespace AGSUnpacker.UI.Services
{
  internal class WindowService
  {
    private IDictionary<Type, Type> Mappings { get; }
    private IDictionary<ViewModel, Window> CreatedWindows { get; }

    public WindowService()
    {
      Mappings = new Dictionary<Type, Type>();
      CreatedWindows = new Dictionary<ViewModel, Window>();
    }

    public void Register<TViewModel, TWindow>()
      where TViewModel : ViewModel
      where TWindow : Window
    {
      if (Mappings.ContainsKey(typeof(TViewModel)))
        throw new ArgumentException($"Type {typeof(TViewModel)} is already mapped to window type {typeof(TWindow)}");

      Mappings.Add(typeof(TViewModel), typeof(TWindow));
    }

    public void Show<TViewModel>(TViewModel viewModel)
      where TViewModel : ViewModel
    {
      Type windowType = Mappings[typeof(TViewModel)];
      Window window = (Window)Activator.CreateInstance(windowType);

      if (CreatedWindows.ContainsKey(viewModel))
        CreatedWindows[viewModel] = window;
      else
        CreatedWindows.Add(viewModel, window);

      window.DataContext = viewModel;
      window.Show();
    }

    public Window GetWindow(ViewModel viewModel)
    {
      if (!CreatedWindows.ContainsKey(viewModel))
        throw new ArgumentException($"Attempt to get {viewModel} window that doesn't exist");

      return CreatedWindows[viewModel];
    }

    public void Close(ViewModel viewModel)
    {
      GetWindow(viewModel).Close();
    }
  }
}
