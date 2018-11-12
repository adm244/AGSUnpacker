using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Room;

namespace AGSUnpackerSharp
{
  class Program
  {
    public static List<AGSRoom> rooms = new List<AGSRoom>();
    public static List<string> lines = new List<string>();

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
            string fileExtension = filenames[i].Substring(index + 1);

            /*if (fileExtension == "dta")
            {
              parser.ParseDTAText(filenames[i]);
            }
            else */
            if (fileExtension == "crm")
            {
              AGSRoom room = new AGSRoom();
              room.LoadFromFile(filenames[i]);
              rooms.Add(room);
            }
          }

          PrepareTranslationLines();
          WriteTranslationFile("text.trs");
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

    public static void PrepareTranslationLines()
    {
      //FIX(adm244): quick and dirty
      for (int room_index = 0; room_index < rooms.Count; ++room_index)
      {
        // hotspots
        for (int i = 0; i < rooms[room_index].hotspots.Length; ++i)
        {
          if (string.IsNullOrEmpty(rooms[room_index].hotspots[i].name)) continue;
          if (lines.IndexOf(rooms[room_index].hotspots[i].name) < 0)
          {
            lines.Add(rooms[room_index].hotspots[i].name);
          }
        }

        // messages
        for (int i = 0; i < rooms[room_index].messages.Length; ++i)
        {
          if (string.IsNullOrEmpty(rooms[room_index].messages[i].text)) continue;
          if (lines.IndexOf(rooms[room_index].messages[i].text) < 0)
          {
            lines.Add(rooms[room_index].messages[i].text);
          }
        }

        // objects
        for (int i = 0; i < rooms[room_index].objects.Length; ++i)
        {
          if (string.IsNullOrEmpty(rooms[room_index].objects[i].name)) continue;
          if (lines.IndexOf(rooms[room_index].objects[i].name) < 0)
          {
            lines.Add(rooms[room_index].objects[i].name);
          }
        }

        // script strings
        for (int i = 0; i < rooms[room_index].script.strings.Length; ++i)
        {
          if (string.IsNullOrEmpty(rooms[room_index].script.strings[i])) continue;
          if (lines.IndexOf(rooms[room_index].script.strings[i]) < 0)
          {
            lines.Add(rooms[room_index].script.strings[i]);
          }
        }
      }
    }

    public static void WriteTranslationFile(string filename)
    {
      string filepath = Path.Combine(Environment.CurrentDirectory, filename);
      FileStream f = new FileStream(filepath, FileMode.Create);
      StreamWriter w = new StreamWriter(f, Encoding.GetEncoding("windows-1252"));

      for (int i = 0; i < lines.Count; ++i)
      {
        w.WriteLine(lines[i]);
        w.WriteLine();
      }

      w.Close();
    }
  }
}
