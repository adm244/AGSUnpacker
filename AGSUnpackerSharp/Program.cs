using System;
using AGSUnpackerSharp.Graphics;

namespace AGSUnpackerSharp
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];
        AGSSpritesCache.ExtractSprites(filepath);
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }
  }
}
