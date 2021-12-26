using System;
using System.Drawing;
using System.Drawing.Imaging;
using AGSUnpackerSharp.Translation;
using AGSUnpackerSharp.Utils;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Graphics;
using AGSUnpackerSharp.Assets;
using AGSUnpackerSharp.Game;
using AGSUnpackerSharp.Room;
using System.Collections.Generic;
using System.Diagnostics;

namespace AGSUnpackerSharp
{
  //struct Pixel
  //{
  //  public byte A;
  //  public byte R;
  //  public byte G;
  //  public byte B;

  //  public Pixel(byte a, byte r, byte g, byte b)
  //  {
  //    A = a;
  //    R = r;
  //    G = g;
  //    B = b;
  //  }
  //}

  class Program
  {
    private static string EncryptText(string text, string alphabet, string cypher)
    {
      List<char> skippedSymbols = new List<char>();

      char[] symbols = text.ToCharArray();

      for (int i = 0; i < symbols.Length; ++i)
      {
        char symbol = symbols[i];

        int index = alphabet.IndexOf(symbol);
        if (index == -1)
        {
          skippedSymbols.Add(symbol);
          continue;
        }

        symbols[i] = cypher[index];
      }

      return new string(symbols);
    }

    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];

        //byte[] symbols = Encoding.GetEncoding(1251).GetBytes("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя");
        //int[] lengths = { 7, 7, 7, 6, 9, 6, 6, 13, 6, 7, 7, 6, 7, 9, 7, 8, 7, 7, 8, 7, 7, 11, 7, 8, 7, 10, 11, 9, 10, 7, 8, 11, 7, 5, 5, 5, 5, 5, 6, 6, 9, 8, 6, 6, 5, 5, 7, 6, 6, 6, 5, 6, 5, 5, 9, 5, 7, 6, 9, 10, 7, 7, 5, 6, 9, 5 };
        //
        //Debug.Assert(symbols.Length == lengths.Length);
        //
        //StringBuilder sb = new StringBuilder();
        //for (int i = 0; i < symbols.Length; ++i)
        //{
        //  sb.AppendFormat("\tli ax, {0}\n", lengths[i] + 1);
        //  sb.AppendLine("\tpush ax");
        //  sb.AppendFormat("\tli ax, {0}\n", symbols[i]);
        //  sb.AppendLine("\tpush ax");
        //  sb.AppendLine("\tli ax, SimpleSetGlyph$2");
        //  sb.AppendLine("\tcall ax");
        //  sb.AppendLine("\tsubi sp, 8");
        //  sb.AppendLine();
        //}
        //string code = sb.ToString();
        //int foo = 0;

        //string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //StringBuilder sb = new StringBuilder();
        //for (int i = 1; i <= 26; ++i)
        //{
        //  sb.AppendLine("\tptrstack 16");
        //  sb.AppendLine("\tmemread4 ax");
        //  sb.AppendLine("\tmr bx, ax");
        //  sb.AppendLine(string.Format("\tli mar, {0}{1}", alphabet[i - 1], i));
        //  sb.AppendLine("\tmr ax, mar");
        //  sb.AppendLine("\tpush op");
        //  sb.AppendLine("\tcallobj ax");
        //  sb.AppendLine("\tfarpush bx");
        //  sb.AppendLine("\tsetfuncargs 1");
        //  sb.AppendLine("\tli ax, Button::set_Font");
        //  sb.AppendLine("\tfarcall ax");
        //  sb.AppendLine("\tfarsubsp 1");
        //  sb.AppendLine("\tpop op");
        //  sb.AppendLine();
        //}
        //string code = sb.ToString();

        //string alphabet = "абвгдежзийклмнопрстухфьыюя";
        //string cypher = "быстйнагяпзхлоьевуикрюджмф";
        //string cypher = "жарзьпыктдумюенйхвгсляобфи";

        //string alphabet = "жарзьпыктдумюенйхвгсляобфи";
        //string cypher = "абвгдежзийклмнопрстухфьыюя";

        //string text = "КЛЯНУСЬ Я БЕЗДНУЮ МОРСКОЙ,[К ТОСКЛИВЫМ БУДНЯМ НЕТ ВОЗВРАТА,[КОРАБЛЬ ВЕРНЫЙ, ХЛЕСТКИЙ КНУТ[СУДЬБА МОЯ И ЖИЗНЬ ПИРАТА.[ПОРУКОЙ СТАНУТ НЕБЕСА,[ДОБУДУ Я ЗАВЕТНЫЙ КЛАД,[НЕ БУДУ БРИТЬСЯ НИКОГДА[ОТНЫНЕ Я ПИРАТ!";
        //string text = "УМИЕСВО И АПКЬЕСФ ЮНХВУНД,[У ГНВУМТРБЮ АСЬЕИЮ ЕПГ РНКРХЖГЖ,[УНХЖАМО РПХЕБД, ЛМПВГУТД УЕСГ[ВСЬОАЖ ЮНИ Т ЫТКЕО ЙТХЖГЖ.[ЙНХСУНД ВГЖЕСГ ЕПАПВЖ,[ЬНАСЬС И КЖРПГЕБД УМЖЬ,[ЕП АСЬС АХТГОВИ ЕТУНЗЬЖ[НГЕБЕП И ЙТХЖГ!";

        //string cypherText = EncryptText(text.ToLower(), alphabet, cypher);

        //AGSTranslation original = AGSTranslation.ReadSourceFile(args[0]);
        //AGSTranslation translated = AGSTranslation.ReadSourceFile(args[1]);
        //
        //int translatedCount = 0;
        //for (int i = 0; i < original.OriginalLines.Count; ++i)
        //{
        //  int index = translated.OriginalLines.IndexOf(original.OriginalLines[i]);
        //  if (index >= 0)
        //  {
        //    original.TranslatedLines[i] = translated.TranslatedLines[index];
        //    ++translatedCount;
        //  }
        //}
        //
        //string outputFilename = args[0] + ".new";
        //original.WriteSourceFile(outputFilename);
        //
        //string statsText = string.Format("// {0} / {1} translated ({2:P1})", translatedCount, original.OriginalLines.Count,
        //  (double)translatedCount / original.OriginalLines.Count);
        //File.AppendAllText(outputFilename, statsText, Encoding.GetEncoding(1251));

        //AssetsManager assets = AssetsManager.Create(args[0]);
        //assets.Extract(args[1]);

        //FIXME(adm244): can't extract resources from multilib's
        //AGSClibUtils.UnpackAGSAssetFiles(filepath, args[1]);
        //TextExtractor.Extract(filepath, args[1]);

        //byte[] buffer = File.ReadAllBytes(filepath);
        //byte[] buffer = null;
        //using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
        //{
        //  using (BinaryReader reader = new BinaryReader(stream, Encoding.GetEncoding(1252)))
        //  {
        //    List<Pixel> pixels = new List<Pixel>();

        //    int column = 0;
        //    while (!reader.EOF())
        //    {
        //      byte a = reader.ReadByte();
        //      byte r = reader.ReadByte();
        //      byte g = reader.ReadByte();
        //      byte b = reader.ReadByte();

        //      ++column;

        //      if (column >= 640)
        //      {
        //        column = 0;
        //        byte pad = reader.ReadByte();
        //      }

        //      pixels.Add(new Pixel(a, r, g, b));
        //    }

        //    buffer = new byte[pixels.Count * 4];
        //    for (int i = 0; i < pixels.Count; ++i)
        //    {
        //      int index = i * 4;
        //      buffer[index + 3] = pixels[i].A;
        //      buffer[index + 0] = pixels[i].R;
        //      buffer[index + 1] = pixels[i].G;
        //      buffer[index + 2] = pixels[i].B;
        //    }
        //  }
        //}
        //Bitmap image = new Bitmap(640, 40, PixelFormat.Format32bppArgb);
        //image.SetPixels(buffer);
        //image.Save(args[1], ImageFormat.Png);

        //string[] traFiles = Directory.GetFiles(args[0], "*.tra", SearchOption.TopDirectoryOnly);
        //
        //AGSTranslation translation = new AGSTranslation();
        //for (int i = 0; i < traFiles.Length; ++i)
        //{
        //  translation.Decompile(traFiles[i]);
        //  translation.WriteSourceFile(Path.GetFileNameWithoutExtension(traFiles[i]) + ".trs");
        //}

        //AGSTranslation translation = new AGSTranslation();
        //translation.Decompile(filepath);
        //translation.WriteSourceFile(args[1]);

        //AGSTranslation translation = AGSTranslation.ReadSourceFile(filepath);
        //translation.Compile(args[1], 1302728765, "A Golden Wake");

        //AGSSpriteSet.UnpackSprites(filepath, args[1]);
        //AGSSpriteSet.PackSprites(filepath, args[1]);

        //AGSGameData dta = new AGSGameData();
        //dta.LoadFromFile(filepath);

        //using (BinaryWriter stream = new BinaryWriter(new FileStream("globalscript.o", FileMode.Create, FileAccess.Write)))
        //{
        //  dta.globalScript.WriteToStream(stream);
        //}

        //using (BinaryWriter stream = new BinaryWriter(new FileStream("dialogscript.o", FileMode.Create, FileAccess.Write)))
        //{
        //  dta.dialogScript.WriteToStream(stream);
        //}

        //for (int i = 0; i < dta.scriptModules.Length; ++i)
        //{
        //  string filename = string.Format("{0}.o", dta.scriptModules[i].Sections[0].Name);
        //  using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
        //  {
        //    using (BinaryWriter writer = new BinaryWriter(fileStream))
        //    {
        //      dta.scriptModules[i].WriteToStream(writer);
        //    }
        //  }
        //}

        //AGSTranslation translation = AGSTranslation.ReadSourceFile(filepath);
        //translation.Compile(filepath + ".tra", 5934168, "Kathy Rain");

        //SourceExtractor extractor = SourceExtractor.Create(AGSVersion.AGS262);
        //bool result = extractor.Extract(filepath);

        //string[] filepaths = Directory.GetFiles(args[0]);
        //for (int i = 0; i < filepaths.Length; ++i)
        //{
        //  if (Path.GetExtension(filepaths[i]) != ".crm")
        //    continue;
        //
        //  AGSRoom room = new AGSRoom();
        //  room.ReadFromFile(filepaths[i]);
        //
        //  string filename = Path.GetFileNameWithoutExtension(filepaths[i]);
        //
        //  using (FileStream stream = new FileStream(string.Format("{0}.o", filename), FileMode.Create, FileAccess.Write))
        //  {
        //    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
        //    {
        //      room.Script.SCOM3.WriteToStream(writer);
        //    }
        //  }
        //
        //  //for (int j = 0; j < room.Background.Frames.Length; ++j)
        //  //{
        //  //  if (room.Background.Frames[j] == null)
        //  //    continue;
        //  //
        //  //  room.Background.Frames[j].Save(string.Format("{0}_{1}.bmp", filename, j), ImageFormat.Bmp);
        //  //}
        //}

        //using (StreamWriter writer = new StreamWriter(new FileStream(filepath + ".asm", FileMode.Create)))
        //{
        //  room.script.DumpInstructions(writer);
        //}

        //AGSGameData dta = new AGSGameData();
        //dta.LoadFromFile(filepath);
        //for (int i = 0; i < dta.scriptModules.Length; ++i)
        //{
        //    string scriptName = dta.scriptModules[i].Sections[dta.scriptModules[i].Sections.Length - 1].Name;
        //    using (FileStream stream = new FileStream(scriptName, FileMode.Create, FileAccess.Write))
        //    {
        //        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
        //        {
        //            dta.scriptModules[i].WriteToStream(writer);
        //        }
        //    }
        //}

        //AGSRoom room = new AGSRoom();
        //room.ReadFromFile(filepath);

        //room.Script.SCOM3.ExtractReferencedStrings(filepath + ".script.strings.trs");

        /*string[][] exports = new string[dta.scriptModules.Length][];
        for (int i = 0; i < dta.scriptModules.Length; ++i)
        {
          exports[i] = new string[dta.scriptModules[i].Exports.Length];
          for (int j = 0; j < dta.scriptModules[i].Exports.Length; ++j)
          {
            exports[i][j] = dta.scriptModules[i].Exports[j].Name;
          }
        }

        int k = 0;*/

        /*string[] files = Directory.GetFiles(filepath);

        for (int i = 0; i < files.Length; ++i)
        {
          if (Path.GetExtension(files[i]) != ".crm")
            continue;

          AGSRoom room = new AGSRoom();
          room.LoadFromFile(files[i]);

          string filename = Path.GetFileNameWithoutExtension(files[i]);
          using (StreamWriter writer = new StreamWriter(new FileStream(filename + ".asm", FileMode.Create)))
          {
            room.script.DumpInstructions(writer);
          }
        }*/

        /*using (BinaryWriter writer = new BinaryWriter(new FileStream("Dialog.o", FileMode.Create)))
        {
          //dta.globalScript.DumpInstructions(writer);
          //dta.globalScript.WriteToStream(writer, dta.globalScript.Version);
          dta.scriptModules[5].WriteToStream(writer, dta.scriptModules[5].Version);
        }*/

        /*string filename = Path.GetFileNameWithoutExtension(filepath);
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
        }*/

        /*int a = 0x422E020C; // 43.502
        float b = (float)a;
        float c = BitConverter.ToSingle(BitConverter.GetBytes(a), 0);*/

        //string[] roomFiles = Directory.GetFiles(filepath, "room*.crm", SearchOption.TopDirectoryOnly);
        /*string foldername = "resaved";
        //NOTE(adm244): funny how Path.GetDirectoryName just finds last '/' slash symbol
        // and cuts everything that is following after, essentially introducing a bug
        // where you're trying to find a directory name of a filepath to a directory (not a file)
        //string test = Path.GetDirectoryName(filepath);
        string folderpath = Path.Combine(filepath, foldername);

        if (!Directory.Exists(folderpath))
        {
          Directory.CreateDirectory(folderpath);
        }*/

        /*for (int i = 0; i < roomFiles.Length; ++i)
        {
          string filename = Path.GetFileNameWithoutExtension(roomFiles[i]);
          string imagepath = filename + ".bmp";
          AGSRoom room = new AGSRoom();
          room.LoadFromFile(roomFiles[i]);

          room.backgrounds[0] = (Bitmap)Bitmap.FromFile(imagepath);

          /*string newfilepath = Path.Combine(folderpath, filename + ".crm");
          room.SaveToFile(newfilepath, room.version);

          room = new AGSRoom();
          room.LoadFromFile(newfilepath);

          room.backgrounds[0].Save(imagepath, ImageFormat.Png);
        }*/

        //string[] files = AGSClibUtils.UnpackAGSAssetFiles(filepath, "Data");
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

        //AGSSpriteSet.UnpackSprites(args[0], args[1]);
        //AGSSpriteSet.PackSprites(args[0], args[1]);

        //AGSRoom room = new AGSRoom();
        //room.ReadFromFile(filepath);
        //using (FileStream stream = new FileStream(filepath + ".o", FileMode.Create, FileAccess.Write))
        //{
        //    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
        //    {
        //        room.Script.SCOM3.WriteToStream(writer);
        //    }
        //}

        /*using (StreamWriter writer = new StreamWriter(filepath + ".dump", false, Encoding.GetEncoding(1252)))
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

    struct AlphabetEntry
    {
      public char Symbol;
      public int SeenTimes;

      public AlphabetEntry(char symbol)
      {
        Symbol = symbol;
        SeenTimes = 0;
      }

      public bool Encountered { get { return SeenTimes > 0; } }
    }

    private static AlphabetEntry[] GetAlphabetEntries(string text, string alphabet)
    {
      AlphabetEntry[] entries = new AlphabetEntry[alphabet.Length];

      for (int i = 0; i < entries.Length; ++i)
      {
        entries[i] = new AlphabetEntry(alphabet[i]);
      }

      text = text.ToLower();
      text = text.Trim();

      for (int i = 0; i < text.Length; ++i)
      {
        int index = alphabet.IndexOf(text[i]);
        if (index != -1)
          ++entries[index].SeenTimes;
      }

      return entries;
    }

    private static void DumpAlphabetUsage(string filepath)
    {
      string alphabet = "абвгдеёжзийклмнопрстуфхцчшщьыъэюя";
      string text = "Быстй! Нагяп -- зЧлоь, еву и крю. -Дж.М. х";

      AlphabetEntry[] entries = GetAlphabetEntries(text, alphabet);

      using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          writer.WriteLine("Text: {0}", text);
          writer.WriteLine("Alphabet: {0}", alphabet);

          writer.WriteLine();

          writer.WriteLine("Symbols used: [");
          for (int i = 0; i < entries.Length; ++i)
          {
            if (!entries[i].Encountered)
              continue;

            writer.WriteLine("\t{0} - {1}", entries[i].Symbol, entries[i].SeenTimes);
          }
          writer.WriteLine("]");

          writer.WriteLine();

          writer.Write("Symbols unused: [");
          for (int i = 0, j = 0; i < entries.Length; ++i)
          {
            if (entries[i].Encountered)
              continue;

            if (j > 0)
              writer.Write(", ");

            writer.Write(entries[i].Symbol);

            ++j;
          }
          writer.WriteLine("]");
        }
      }
    }
  }
}
