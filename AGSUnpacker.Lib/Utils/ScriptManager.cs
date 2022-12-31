using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Game;
using AGSUnpacker.Lib.Room;
using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Lib.Translation;

namespace AGSUnpacker.Lib.Utils
{
  public static class ScriptManager
  {
    public const string ScriptFileExtension = "scom3";

    public static void ReplaceText(string trsFile, string[] targetFiles)
    {
      AGSTranslation translation = AGSTranslation.ReadSourceFile(trsFile);

      // FIXME(adm244): this is messy, consider rewriting
      for (int i = 0; i < targetFiles.Length; ++i)
      {
        AGSScript script = AGSScript.ReadFromFile(targetFiles[i]);

        int changedTexts = 0;
        for (int j = 0; j < script.StringsReferenced.Length; ++j)
        {
          int index = translation.OriginalLines.IndexOf(script.StringsReferenced[j].Text);
          if (index >= 0)
          {
            string replacementString = translation.TranslatedLines[index];
            if (!string.IsNullOrEmpty(replacementString))
            {
              script.StringsReferenced[j].Text = replacementString;
              ++changedTexts;
            }
          }
        }

        if (changedTexts > 0)
        {
          File.Copy(targetFiles[i], targetFiles[i] + ".backup", true);
          script.WriteToFile(targetFiles[i]);
        }
      }
    }

    public static void Inject(string targetFile, string scriptFile)
    {
      // TODO(adm244): determine file by it's signature
      string extension = Path.GetExtension(targetFile).ToLower();
      if (extension == ".dta")
        InjectIntoData(targetFile, scriptFile);
      else
        InjectIntoRoom(targetFile, scriptFile);
    }

    private static void InjectIntoData(string targetFile, string scriptFile)
    {
      AGSGameData gameData = AGSGameData.ReadFromFile(targetFile);
      AGSScript injectee = AGSScript.ReadFromFile(scriptFile);

      if (injectee.Sections.Length > 0)
      {
        ((Action)(() => {
          if (TryInjectInto(ref gameData.globalScript, injectee))
            return;

          if (TryInjectInto(ref gameData.dialogScript, injectee))
            return;

          if (TryInjectInto(ref gameData.scriptModules, injectee))
            return;

          throw new InvalidDataException(
            $"Could not find script with name \"{injectee.Sections[0].Name}\" in game data file.");
        }))();
      }
      else
      {
        // NOTE(adm244): old script, should be globalScript

        if (gameData.globalScript.Sections.Length > 0)
          throw new InvalidDataException(
            "Sections length mismatch. Script you're trying to inject is too old.");

        gameData.globalScript = injectee;
      }

      File.Copy(targetFile, targetFile + ".backup", true);
      gameData.WriteToFile(targetFile);
    }

    private static void InjectIntoRoom(string targetFile, string scriptFile)
    {
      AGSRoom room = AGSRoom.ReadFromFile(targetFile);
      AGSScript injectee = AGSScript.ReadFromFile(scriptFile);

      if (!TryInjectInto(ref room.Script.SCOM3, injectee))
        throw new InvalidDataException("Could not inject room script.");

      File.Copy(targetFile, targetFile + ".backup", true);
      room.WriteToFile(targetFile, room.Version);
    }

    private static bool TryInjectInto(ref AGSScript[] scripts, AGSScript injectee)
    {
      for (int i = 0; i < scripts.Length; ++i)
      {
        if (TryInjectInto(ref scripts[i], injectee))
          return true;
      }

      return false;
    }

    private static bool TryInjectInto(ref AGSScript script, AGSScript injectee)
    {
      if (script.Sections.Length > 0)
      {
        if (script.Sections[0].Name == injectee.Sections[0].Name)
        {
          script = injectee;
          return true;
        }
      }

      return false;
    }

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
            room.ReadFromFileDeprecated(filenames[i]);
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
            filename += ".txt";

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
      filename += "." + ScriptFileExtension;

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
