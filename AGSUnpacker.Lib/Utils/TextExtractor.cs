using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Game;
using AGSUnpacker.Lib.Room;
using AGSUnpacker.Lib.Translation;

namespace AGSUnpacker.Lib.Utils
{
  public static class TextExtractor
  {
    private static AGSGameData gameData;
    private static List<AGSRoom> rooms;
    private static List<string> lines;

    // FIXME(adm244): doesn't support multilib files
    //public static bool Extract(string filepath, string targetFilepath)
    //{
    //  AssetsManager assetsManager = AssetsManager.Create(filepath);
    //  if (assetsManager == null)
    //    return false;
    //
    //  using (FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
    //  {
    //    for (int i = 0; i < assetsManager.Files.Length; ++i)
    //    {
    //      for (int k = 0; k < assetsManager.Files[i].Assets.Count; ++k)
    //      {
    //        string extension = Path.GetExtension(assetsManager.Files[i].Assets[k].Filename);
    //        if (extension == ".dta" || extension == ".crm")
    //        {
    //          long baseOffset = assetsManager.Files[i].Offset;
    //          long assetOffset = assetsManager.Files[i].Assets[k].Offset;
    //
    //          fileStream.Seek(baseOffset + assetOffset, SeekOrigin.Begin);
    //
    //          // TODO(adm244): create a substream instead; that way it's easy to handle large data
    //          byte[] buffer = new byte[assetsManager.Files[i].Assets[k].Size];
    //          int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
    //          Trace.Assert(bytesRead == buffer.Length);
    //
    //          // HACK(adm244): our API needs some serious attention; too much object allocation
    //          using (MemoryStream memoryStream = new MemoryStream(buffer))
    //          {
    //            using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.Latin1))
    //            {
    //              if (extension == ".dta")
    //              {
    //                gameData = new AGSGameData();
    //                gameData.LoadFromStream(binaryReader);
    //              }
    //              else
    //              {
    //                string name = Path.GetFileNameWithoutExtension(assetsManager.Files[i].Assets[k].Filename);
    //                AGSRoom room = new AGSRoom(name);
    //                room.ReadFromStream(binaryReader);
    //
    //                rooms.Add(room);
    //              }
    //            }
    //          }
    //        }
    //      }
    //    }
    //  }
    //
    //  //gameData = assetsManager.GameData;
    //  //rooms.AddRange(assetsManager.Rooms);
    //
    //  Console.Write("Extracting text lines...");
    //  PrepareTranslationLines();
    //  Console.WriteLine(" Done!");
    //
    //  Console.Write("Writing translation source file...");
    //  WriteTranslationFile(targetFilepath, lines);
    //  Console.WriteLine(" Done!");
    //
    //  return true;
    //}

    public static bool ExtractFromFolder(string sourceFolder, string targetFile)
    {
      // FIXME(adm244): exactly the reason to not make it static
      gameData = new AGSGameData();
      rooms = new List<AGSRoom>();
      lines = new List<string>();

      if (Directory.Exists(sourceFolder))
      {
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

        Console.Write("Extracting text lines...");
        PrepareTranslationLines();
        Console.WriteLine(" Done!");

        Console.Write("Writing translation source file...");
        WriteTranslationFile(targetFile, lines);
        Console.WriteLine(" Done!");

        return true;
      }
      else
      {
        Console.WriteLine(string.Format("ERROR: Folder {0} does not exist.", sourceFolder));
        return false;
      }
    }

    private static void PrepareTranslationLines()
    {
      // embed game id and name
      lines.Add($"{AGSTranslation.TRS_TAG_GAMEID}{gameData.setup.unique_id}");
      lines.Add($"{AGSTranslation.TRS_TAG_GAMENAME}{gameData.setup.name}");

      // extract translation lines from game data
      lines.Add("// [game28.dta]");
      ExtractTranslationLines(gameData);

      // extract translation lines from rooms
      for (int i = 0; i < rooms.Count; ++i)
      {
        string metadata = string.Empty;
        if (gameData.roomsDebugInfo.Length == 0)
        {
          metadata = string.Format("// [{0}.crm]", rooms[i].Name);
        }
        else
        {
          metadata = string.Format("// [{0}.crm - {1}]", rooms[i].Name, gameData.roomsDebugInfo[i].name);
        }

        lines.Add(metadata);
        ExtractTranslationLines(rooms[i]);
      }
    }

    private static void ExtractTranslationLines(AGSRoom room)
    {
      // hotspots
      lines.Add("//   [hotspots]");
      for (int i = 0; i < room.Markup.Hotspots.Length; ++i)
      {
        PushIntoLines(room.Markup.Hotspots[i].Name);
      }

      // messages
      lines.Add("//   [messages]");
      for (int i = 0; i < room.Messages.Length; ++i)
      {
        PushIntoLines(room.Messages[i].Text);
      }

      // objects
      lines.Add("//   [objects]");
      for (int i = 0; i < room.Markup.Objects.Length; ++i)
      {
        PushIntoLines(room.Markup.Objects[i].Name);
      }

      // script strings
      lines.Add("//   [script strings]");
      for (int i = 0; i < room.Script.SCOM3.StringsReferenced.Length; ++i)
      {
        PushIntoLines(room.Script.SCOM3.StringsReferenced[i].Text);
      }
    }

    private static void ExtractTranslationLines(AGSGameData data)
    {
      // dialogs
      for (int i = 0; i < data.dialogs.Length; ++i)
      {
        lines.Add(string.Format("//   [dialog topic {0}]", (i + 1)));
        for (int j = 0; j < data.dialogs[i].options_number; ++j)
        {
          PushIntoLines(data.dialogs[i].options[j]);
        }
      }

      // global script strings
      lines.Add("//   [global script strings]");
      for (int i = 0; i < data.globalScript.StringsReferenced.Length; ++i)
      {
        PushIntoLines(data.globalScript.StringsReferenced[i].Text);
      }

      // dialog script strings
      lines.Add("//   [dialog script strings]");
      for (int i = 0; i < data.dialogScript.StringsReferenced.Length; ++i)
      {
        PushIntoLines(data.dialogScript.StringsReferenced[i].Text);
      }

      // old dialog strings
      lines.Add("//   [old dialog strings]");
      for (int i = 0; i < data.oldDialogStrings.Count; ++i)
      {
        PushIntoLines(data.oldDialogStrings[i]);
      }

      // module scripts strings
      for (int script_index = 0; script_index < data.scriptModules.Length; ++script_index)
      {
        lines.Add(string.Format("//   [{0} strings]", data.scriptModules[script_index].Sections[0].Name));
        for (int i = 0; i < data.scriptModules[script_index].StringsReferenced.Length; ++i)
        {
          PushIntoLines(data.scriptModules[script_index].StringsReferenced[i].Text);
        }
      }

      // gui labels
      lines.Add("//   [labels]");
      for (int i = 0; i < data.labels.Length; ++i)
      {
        PushIntoLines(data.labels[i].text);
      }

      // gui buttons
      lines.Add("//   [buttons]");
      for (int i = 0; i < data.buttons.Length; ++i)
      {
        PushIntoLines(data.buttons[i].text);
      }

      // gui listboxes
      lines.Add("//   [listboxes]");
      for (int i = 0; i < data.listboxes.Length; ++i)
      {
        for (int j = 0; j < data.listboxes[i].items.Length; ++j)
        {
          PushIntoLines(data.listboxes[i].items[j]);
        }
      }

      // characters
      lines.Add("//   [characters]");
      for (int i = 0; i < data.characters.Length; ++i)
      {
        PushIntoLines(data.characters[i].name);
      }

      // inventory items
      lines.Add("//   [inventory items]");
      for (int i = 0; i < data.inventoryItems.Length; ++i)
      {
        PushIntoLines(data.inventoryItems[i].name);
      }

      // global messages
      lines.Add("//   [global messages]");
      for (int i = 0; i < data.globalMessages.Length; ++i)
      {
        if (data.setup.global_messages[i] == 0) continue;
        PushIntoLines(data.globalMessages[i]);
      }

      // dictionary
      lines.Add("//   [dictionary]");
      for (int i = 0; i < data.dictionary.words.Length; ++i)
      {
        PushIntoLines(data.dictionary.words[i].text);
      }
    }

    private static bool PushIntoLines(string line)
    {
      if (line == null)
        return false;

      // check if string is valid
      if (string.IsNullOrWhiteSpace(line))
        return false;

      // check if a duplicate
      if (lines.IndexOf(line) < 0)
        lines.Add(line);

      return true;
    }

    public static void WriteTranslationFile(string filename, List<string> lines)
    {
      string filepath = Path.Combine(Environment.CurrentDirectory, filename);
      using (FileStream stream = new FileStream(filepath, FileMode.Create))
      {
        using (StreamWriter writer = new StreamWriter(stream, Encoding.Latin1))
        {
          for (int i = 0; i < lines.Count; ++i)
          {
            writer.WriteLine(lines[i]);
            if (!lines[i].StartsWith("//"))
            {
              writer.WriteLine();
            }
          }
        }
      }
    }
  }
}
