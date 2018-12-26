using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Game;
using AGSUnpackerSharp.Room;

namespace AGSUnpackerSharp
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }
  }
}
