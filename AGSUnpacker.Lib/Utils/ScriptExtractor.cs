using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGSUnpacker.Lib.Game;
using AGSUnpacker.Lib.Room;
using AGSUnpacker.Lib.Shared;

namespace AGSUnpacker.Lib.Utils
{
  public static class ScriptExtractor
  {
    public static bool ExtractFromFolder(string sourceFolder, string targetFolder)
    {
      if (Directory.Exists(sourceFolder))
      {
        AGSGameData gameData = new AGSGameData();
        List<AGSRoom> rooms = new List<AGSRoom>();

        string[] filenames = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

        Console.WriteLine("Searching asset files...");

        for (int i = 0; i < filenames.Length; ++i)
        {
          /*int index = filenames[i].LastIndexOf('.');
          string fileExtension = filenames[i].Substring(index + 1);*/
          string fileExtension = Path.GetExtension(filenames[i].ToLower());

          if (fileExtension == ".dta")
          {
            Console.Write("\tParsing {0} data file...", Path.GetFileName(filenames[i]));

            gameData.LoadFromFile(filenames[i]);

            Console.WriteLine(" Done!");
          }
          else if (fileExtension == ".crm")
          {
            Console.Write("\tParsing {0} room file...", Path.GetFileName(filenames[i]));

            AGSRoom room = new AGSRoom(Path.GetFileNameWithoutExtension(filenames[i]));
            room.ReadFromFile(filenames[i]);
            rooms.Add(room);

            Console.WriteLine(" Done!");
          }
        }

        Console.Write("Extracting scripts from game data...");
        string globalScriptsFolder = Path.Combine(targetFolder, "global");
        ExtractGameDataScripts(gameData, globalScriptsFolder);
        Console.WriteLine(" Done!");

        for (int i = 0; i < rooms.Count; ++i)
        {
          Console.Write($"Extracting room{i} script...");
          if (!string.IsNullOrEmpty(rooms[i].Script.SourceCode))
          {
            string filename = Path.GetFileNameWithoutExtension(rooms[i].Name);
            if (string.IsNullOrEmpty(filename))
            {
              filename = Path.GetRandomFileName();
            }
            filename += ".s";

            string targetFilepath = Path.Combine(targetFolder, filename);

            Directory.CreateDirectory(targetFolder);

            File.WriteAllText(targetFilepath, rooms[i].Script.SourceCode);
          }

          DumpScript(rooms[i].Script.SCOM3, targetFolder, rooms[i].Name);
          Console.WriteLine(" Done!");
        }

        return true;
      }
      else
      {
        Console.WriteLine(string.Format("ERROR: Folder {0} does not exist.", sourceFolder));
        return false;
      }
    }

    private static void ExtractGameDataScripts(AGSGameData gameData, string targetFolder)
    {
      // FIXME(adm244): now that's just stupid, assign null if it's not initialized
      if (gameData.dialogScript.Code.Length > 0)
        DumpScript(gameData.dialogScript, targetFolder, "DialogScripts");

      if (gameData.globalScript.Code.Length > 0)
      DumpScript(gameData.globalScript, targetFolder, "GlobalScript");

      for (int i = 0; i < gameData.scriptModules.Length; ++i)
      {
        string filename = null;
        if (gameData.scriptModules[i].Sections.Length <= 0)
          filename = string.Format($"globalscript{i}");

        DumpScript(gameData.scriptModules[i], targetFolder, filename);
      }
    }

    private static void DumpScript(AGSScript script, string targetFolder, string name = null)
    {
      string filename = name ?? Path.GetFileNameWithoutExtension(script.Sections[^1].Name);
      if (string.IsNullOrEmpty(filename))
      {
        filename = Path.GetRandomFileName();
      }
      filename += ".o";

      string targetFilepath = Path.Combine(targetFolder, filename);

      Directory.CreateDirectory(targetFolder);

      using (FileStream stream = new FileStream(targetFilepath, FileMode.Create, FileAccess.Write))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          script.WriteToStream(writer);
        }
      }
    }
  }
}
