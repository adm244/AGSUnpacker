using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];
        if (File.Exists(filepath))
        {
          AGSTextParser parser = new AGSTextParser();
          string[] filenames = parser.UnpackAGSAssetFiles(filepath);

          for (int i = 0; i < filenames.Length; ++i)
          {
            int index = filenames[i].LastIndexOf('.');
            string filename = filenames[i].Substring(index + 1);

            if (filename == "dta")
            {
              parser.ParseDTAText(filenames[i]);
            }
          }
        }
        else
        {
          Console.WriteLine(string.Format("ERROR: File {0} does not exist.", filepath));
        }
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }
  }
}
