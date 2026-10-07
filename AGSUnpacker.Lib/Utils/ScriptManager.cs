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
          bool found = translation.Lines.TryGetValue(script.StringsReferenced[j].Text, out string translated);
          if (found && !string.IsNullOrEmpty(translated))
          {
            script.StringsReferenced[j].Text = translated;
            ++changedTexts;
          }
        }

        if (changedTexts > 0)
        {
          File.Copy(targetFiles[i], targetFiles[i] + ".backup", true);
          script.WriteToFile(targetFiles[i]);
        }
      }
    }

    public static void Inject(string targetFile, string[] scriptFiles)
    {
      // TODO(adm244): determine file by it's signature
      string extension = Path.GetExtension(targetFile).ToLower();
      if (extension == ".dta")
        InjectIntoData(targetFile, scriptFiles);
      else
      {
        // FIXME(adm244): silently inject first script, maybe show warning message
        InjectIntoRoom(targetFile, scriptFiles[0]);
      }
    }

    private static void InjectIntoData(AGSGameData gameData, string scriptFile)
    {
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
    }

    private static void InjectIntoData(string targetFile, string[] scriptFiles)
    {
      AGSGameData gameData = AGSGameData.ReadFromFile(targetFile);

      for (int i = 0; i < scriptFiles.Length; ++i)
      {
        InjectIntoData(gameData, scriptFiles[i]);
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
        string[] files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
          string fileName = Path.GetFileName(files[i]);
          string fileExtension = Path.GetExtension(files[i].ToLower());

          if (fileExtension == ".dta")
          {
            Console.Write($"Parsing {fileName} data file...");
            AGSGameData gameData = AGSGameData.ReadFromFile(files[i]);
            Console.WriteLine(" Done!");

            Console.Write("Extracting scripts from game data...");
            string globalScriptsFolder = Path.Combine(targetFolder, "global");
            ExtractGameDataScripts(gameData, globalScriptsFolder);
            Console.WriteLine(" Done!");
          }
          else if (fileExtension == ".crm")
          {
            Console.Write($"Parsing {fileName} room file...");
            AGSRoom room = AGSRoom.ReadFromFile(files[i], Path.GetFileNameWithoutExtension(files[i]));
            Console.WriteLine(" Done!");

            Console.Write($"Extracting {fileName} script...");
            ExtractRoomScript(room, targetFolder);
            Console.WriteLine(" Done!");
          }
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
      // NOTE(adm244): extracting dialog and global scripts into a separate folder
      //  because script modules with exact same name will overwrite them!
      string builtinFolder = Path.Combine(targetFolder, "built-in");
      Directory.CreateDirectory(builtinFolder);

      // FIXME(adm244): now that's just stupid, assign null if it's not initialized
      if (gameData.dialogScript.Code.Length > 0)
        DumpScript(gameData.dialogScript, builtinFolder, "DialogScript");

      if (gameData.globalScript.Code.Length > 0)
        DumpScript(gameData.globalScript, builtinFolder, "GlobalScript");

      for (int i = 0; i < gameData.scriptModules.Length; ++i)
      {
        string filename = null;
        if (gameData.scriptModules[i].Sections.Length <= 0)
          filename = string.Format($"globalscript{i}");

        DumpScript(gameData.scriptModules[i], targetFolder, filename);
      }
    }

    private static void ExtractRoomScript(AGSRoom room, string targetFolder)
    {
      if (!string.IsNullOrEmpty(room.Script.SourceCode))
      {
        string filename = Path.GetFileNameWithoutExtension(room.Name);
        if (string.IsNullOrEmpty(filename))
        {
          filename = Path.GetRandomFileName();
        }
        filename += ".txt";

        string targetFilepath = Path.Combine(targetFolder, filename);
        Directory.CreateDirectory(targetFolder);
        File.WriteAllText(targetFilepath, room.Script.SourceCode);
      }

      DumpScript(room.Script.SCOM3, targetFolder, room.Name);
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

    public static void ExtractModulesList(string targetFile, string outputFile)
    {
      AGSGameData data = new();
      data.LoadFromFile(targetFile);

      string[] modules = new string[data.scriptModules.Length];

      for (int i = 0; i < modules.Length; ++i)
      {
        // TODO(adm244): refactor this into AGSScript member function?
        string name = data.scriptModules[i].Name;
        if (string.IsNullOrWhiteSpace(name) && (data.scriptModules[i].Sections.Length > 0))
          name = Path.GetFileNameWithoutExtension(data.scriptModules[i].Sections[^1].Name);
        if (string.IsNullOrWhiteSpace(name))
          name = $"globalscript{i}";

        modules[i] = $"{name}.{ScriptFileExtension}";
      }

      File.WriteAllLines(outputFile, modules);
    }
  }
}
