using System;
using System.Collections.Generic;
using System.Windows;

using AGSUnpacker.UI.Core;

namespace AGSUnpacker.UI.Service
{
  internal class WindowService
  {
    private IDictionary<Type, Type> Mappings { get; }

    public WindowService()
    {
      Mappings = new Dictionary<Type, Type>();
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

      window.DataContext = viewModel;
      window.Show();
    }
  }
}
