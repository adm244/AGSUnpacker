using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Game;
using AGSUnpackerSharp.Room;

namespace AGSUnpackerSharp.Utils
{
  public class TextExtractor
  {
    private static AGSGameData gameData = new AGSGameData();
    private static List<AGSRoom> rooms = new List<AGSRoom>();
    private static List<string> lines = new List<string>();

    public static bool Extract(string filepath)
    {
      if (File.Exists(filepath))
      {
        string[] filenames = AGSClibUtils.UnpackAGSAssetFiles(filepath);

        for (int i = 0; i < filenames.Length; ++i)
        {
          int index = filenames[i].LastIndexOf('.');
          string fileExtension = filenames[i].Substring(index + 1);

          if (fileExtension == "dta")
          {
            gameData.LoadFromFile(filenames[i]);
          }
          else if (fileExtension == "crm")
          {
            AGSRoom room = new AGSRoom();
            room.LoadFromFile(filenames[i]);
            rooms.Add(room);
          }
        }

        PrepareTranslationLines();
        WriteTranslationFile("text.trs");

        return true;
      }
      else
      {
        Console.WriteLine(string.Format("ERROR: File {0} does not exist.", filepath));
        return false;
      }
    }

    private static void PrepareTranslationLines()
    {
      // extract translation lines from game data
      lines.Add("// [game28.dta]");
      ExtractTranslationLines(gameData);

      // extract translation lines from rooms
      for (int i = 0; i < rooms.Count; ++i)
      {
        string metadata = string.Empty;
        if (gameData.roomsDebugInfo.Length == 0)
        {
          metadata = string.Format("// [room{0}.crm]", (i + 1));
        }
        else
        {
          metadata = string.Format("// [room{0}.crm - {1}]", (i + 1), gameData.roomsDebugInfo[i].name);
        }

        lines.Add(metadata);
        ExtractTranslationLines(rooms[i]);
      }
    }

    private static void ExtractTranslationLines(AGSRoom room)
    {
      // hotspots
      lines.Add("//   [hotspots]");
      for (int i = 0; i < room.hotspots.Length; ++i)
      {
        PushIntoLines(room.hotspots[i].name);
      }

      // messages
      lines.Add("//   [messages]");
      for (int i = 0; i < room.messages.Length; ++i)
      {
        PushIntoLines(room.messages[i].text);
      }

      // objects
      lines.Add("//   [objects]");
      for (int i = 0; i < room.objects.Length; ++i)
      {
        PushIntoLines(room.objects[i].name);
      }

      // script strings
      lines.Add("//   [script strings]");
      for (int i = 0; i < room.script.strings.Length; ++i)
      {
        PushIntoLines(room.script.strings[i]);
      }
    }

    private static void ExtractTranslationLines(AGSGameData data)
    {
      // dialogs
      for (int i = 0; i < data.dialogs.Length; ++i)
      {
        lines.Add(string.Format("//   [dialog topic {0}]", (i + 1)));
        for (int j = 0; j < data.dialogs[i].options.Length; ++j)
        {
          PushIntoLines(data.dialogs[i].options[j]);
        }
      }

      // global script strings
      lines.Add("//   [global script strings]");
      for (int i = 0; i < data.globalScript.strings.Length; ++i)
      {
        PushIntoLines(data.globalScript.strings[i]);
      }

      // dialog script strings
      lines.Add("//   [dialog script strings]");
      for (int i = 0; i < data.dialogScript.strings.Length; ++i)
      {
        PushIntoLines(data.dialogScript.strings[i]);
      }

      // module scritps strings
      for (int script_index = 0; script_index < data.scriptModules.Length; ++script_index)
      {
        lines.Add(string.Format("//   [{0} strings]", data.scriptModules[script_index].sections[0].name));
        for (int i = 0; i < data.scriptModules[script_index].strings.Length; ++i)
        {
          PushIntoLines(data.scriptModules[script_index].strings[i]);
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
    }

    private static bool PushIntoLines(string line)
    {
      if (line == null) return false;

      // cut non printable characters at the beginning
      int trim_index = 0;
      for (int i = 0; i < line.Length; ++i)
      {
        if (line[i] < 0x20) trim_index = i;
        else break;
      }
      if ((trim_index + 1) >= line.Length) return false;
      if (trim_index > 0) line = line.Substring(trim_index + 1);

      // check if string is valid
      if (string.IsNullOrEmpty(line)) return false;
      if (lines.IndexOf(line) < 0)
      {
        lines.Add(line);
      }

      return true;
    }

    private static void WriteTranslationFile(string filename)
    {
      string filepath = Path.Combine(Environment.CurrentDirectory, filename);
      FileStream f = new FileStream(filepath, FileMode.Create);
      StreamWriter w = new StreamWriter(f, Encoding.GetEncoding(1252));

      for (int i = 0; i < lines.Count; ++i)
      {
        if (lines[i].StartsWith("__")) continue;

        w.WriteLine(lines[i]);
        if (!lines[i].StartsWith("//"))
        {
          w.WriteLine();
        }
      }

      w.Close();
    }
  }
}
