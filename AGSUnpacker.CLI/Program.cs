using AGSUnpacker.Assets;

namespace AGSUnpacker.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      AssetsManager manager = AssetsManager.Create(args[0]);
      manager.Extract(args[1]);
    }
  }
}
