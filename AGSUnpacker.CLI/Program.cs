using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Graphics;
using AGSUnpacker.Lib.Translation;

namespace AGSUnpacker.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      //AssetsManager manager = AssetsManager.Create(args[0]);
      //manager.Extract(args[1]);

      AGSSpriteSet.UnpackSprites(args[0], args[1]);

      //AGSTranslation translation = AGSTranslation.ReadSourceFile(args[0]);
      //translation.Compile(args[1], 1302728765, "A Golden Wake");
    }
  }
}
