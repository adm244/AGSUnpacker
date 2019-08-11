using System;
using AGSUnpackerSharp.Graphics;
using AGSUnpackerSharp.Room;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using AGSUnpackerSharp.Utils;
using System.Text;
using AGSUnpackerSharp.Game;
using AGSUnpackerSharp.Extensions;
using AGSUnpackerSharp.Translation;

namespace AGSUnpackerSharp
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];
        string filename = Path.GetFileNameWithoutExtension(filepath);
        string folderpath = Path.GetDirectoryName(filepath);
        string translationpath = Path.Combine(folderpath, filename + ".trs");

        AGSTranslation translation = new AGSTranslation();
        translation.Decompile(filepath);
        //TextExtractor.WriteTranslationFile(translationpath, translation.OriginalLines);

        StreamWriter writer = new StreamWriter(translationpath, false, Encoding.GetEncoding(1252));
        for (int i = 0; i < translation.OriginalLines.Count; ++i)
        {
          writer.WriteLine(translation.OriginalLines[i]);
          writer.WriteLine(translation.TranslatedLines[i]);
        }

        /*int a = 0x422E020C; // 43.502
        float b = (float)a;
        float c = BitConverter.ToSingle(BitConverter.GetBytes(a), 0);*/

        /*string[] roomFiles = Directory.GetFiles(filepath, "room*.crm", SearchOption.TopDirectoryOnly);
        string foldername = "resaved";
        //NOTE(adm244): funny how Path.GetDirectoryName just finds last '/' slash symbol
        // and cuts everything that is following after, essentially introducing a bug
        // where you're trying to find a directory name of a filepath to a directory (not a file)
        //string test = Path.GetDirectoryName(filepath);
        string folderpath = Path.Combine(filepath, foldername);

        if (!Directory.Exists(folderpath))
        {
          Directory.CreateDirectory(folderpath);
        }

        for (int i = 0; i < roomFiles.Length; ++i)
        {
          string filename = Path.GetFileNameWithoutExtension(roomFiles[i]);
          string imagepath = filename + ".png";
          AGSRoom room = new AGSRoom();
          room.LoadFromFile(roomFiles[i]);

          room.backgrounds[0] = (Bitmap)Bitmap.FromFile(imagepath);

          string newfilepath = Path.Combine(folderpath, filename + ".crm");
          room.SaveToFile(newfilepath, room.version);

          room = new AGSRoom();
          room.LoadFromFile(newfilepath);

          room.backgrounds[0].Save(imagepath, ImageFormat.Png);
        }*/

        //string[] files = AGSClibUtils.UnpackAGSAssetFiles(filepath);
        /*for (int i = 0; i < files.Length; ++i)
        {
          string filename = files[i].Substring(files[i].LastIndexOf('/') + 1);
          string extension = filename.Substring(filename.LastIndexOf('.') + 1);

          if (extension == "crm")
          {
            AGSRoom room = new AGSRoom();
            room.LoadFromFile(files[i]);
          }
        }*/
        //AGSSpritesCache.ExtractSprites(filepath);
        /*string[] files = Directory.GetFiles(filepath, "spr*");
        AGSSpritesCache.PackSprites(files);*/
        /*AGSRoom room = new AGSRoom();
        room.LoadFromFile(filepath);

        using (StreamWriter writer = new StreamWriter(filepath + ".dump", false, Encoding.GetEncoding(1252)))
        {
          room.script.DumpInstructions(writer);
        }*/

        /*// dump stringsBlob section
        using (FileStream stream = new FileStream(filepath + ".strings", FileMode.Create))
        {
          using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
          {
            writer.Write(room.script.StringsBlob);
          }
        }

        // dump code section
        using (FileStream stream = new FileStream(filepath + ".code", FileMode.Create))
        {
          using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
          {
            writer.WriteArrayInt32(room.script.code);
          }
        }

        // dump exports section
        using (FileStream stream = new FileStream(filepath + ".exports", FileMode.Create))
        {
          using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
          {
            for (int i = 0; i < room.script.exports.Length; ++i)
            {
              writer.WriteNullTerminatedString(room.script.exports[i].name, 300);
              writer.Write((UInt32)room.script.exports[i].pointer);
            }
          }
        }

        // dump imports section
        using (FileStream stream = new FileStream(filepath + ".imports", FileMode.Create))
        {
          using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
          {
            for (int i = 0; i < room.script.imports.Length; ++i)
            {
              writer.WriteNullTerminatedString(room.script.imports[i], 300);
            }
          }
        }

        // dump fixups section
        using (FileStream stream = new FileStream(filepath + ".fixups", FileMode.Create))
        {
          using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
          {
            for (int i = 0; i < room.script.fixups.Length; ++i)
            {
              writer.Write((byte)room.script.fixups[i].type);
              writer.Write((UInt32)room.script.fixups[i].value);
            }
          }
        }*/

        /*string[] files = Directory.GetFiles(filepath);
        Convert16bitTo32bitImages(files);*/

        // open original file
        /*FileStream file = new FileStream(filepath, FileMode.Open);
        BinaryReader reader = new BinaryReader(file, Encoding.GetEncoding(1252));
        Bitmap image = AGSGraphicUtils.ParseLZ77Image(reader);
        reader.Close();

        // save original uncompressed image
        image.Save(filepath + ".bmp.original.bmp", ImageFormat.Bmp);

        // compress original image into a file
        FileStream output = new FileStream(filepath + ".compressed", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(output, Encoding.GetEncoding(1252));
        AGSGraphicUtils.WriteLZ77Image(writer, image);
        writer.Close();

        // decompress image again from a file
        file = new FileStream(filepath + ".compressed", FileMode.Open);
        reader = new BinaryReader(file, Encoding.GetEncoding(1252));
        image = AGSGraphicUtils.ParseLZ77Image(reader);
        reader.Close();

        // save decompressed image into a file
        image.Save(filepath + ".bmp.decompressed.bmp", ImageFormat.Bmp);*/

        /*AGSGameData gameData = new AGSGameData();
        gameData.LoadFromFile(filepath);*/

        /*public static Color[] DefaultPalette = new Color[] {
          Color.FromArgb(alpha, red, green, blue),
          Color.FromArgb(alpha, red, green, blue),
          Color.FromArgb(alpha, red, green, blue),
          Color.FromArgb(alpha, red, green, blue),
        };*/

        /*FileStream file = new FileStream("palette.txt", FileMode.Create);
        StreamWriter writer = new StreamWriter(file, Encoding.ASCII);

        writer.WriteLine("public static Color[] DefaultPalette = new Color[] {");
        for (int i = 0; i < gameData.setup.defaultPallete.Length; ++i)
        {
          byte alpha = 0xFF;
          byte blue = (byte)(gameData.setup.defaultPallete[i] >> 16);
          byte green = (byte)(gameData.setup.defaultPallete[i] >> 8);
          byte red = (byte)(gameData.setup.defaultPallete[i] >> 0);

          //NOTE(adm244): AGS is using only 6-bits per channel, so we have to convert it to full 8-bit range
          blue = (byte)((blue / 64f) * 256f);
          green = (byte)((green / 64f) * 256f);
          red = (byte)((red / 64f) * 256f);

          writer.Write("  Color.FromArgb({0,3}, {1,3}, {2,3}, {3,3}),", alpha, red, green, blue);
          if (((i + 1) % 3 == 0) || (i == gameData.setup.defaultPallete.Length - 1))
          {
            writer.WriteLine();
          }
        }
        writer.WriteLine("};");

        writer.Close();*/
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }

    public static void Convert16bitTo32bitImages(params string[] files)
    {
      Color transparentColor = Color.FromArgb(255, 0, 255);
      for (int i = 0; i < files.Length; ++i)
      {
        Bitmap bitmap = new Bitmap(files[i]);
        bitmap.MakeTransparent(transparentColor);

        string filename = files[i].Split('.')[0];
        bitmap.Save(string.Format("{0}.png", filename), ImageFormat.Png);
      }
    }
  }
}
