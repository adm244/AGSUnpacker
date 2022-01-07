using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Graphics;

namespace AGSUnpacker.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      //AssetsManager manager = AssetsManager.Create(args[0]);
      //manager.Extract(args[1]);

      AGSSpriteSet.UnpackSprites(args[0], args[1]);
    }
  }
}
