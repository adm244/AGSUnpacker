using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Extensions;
using AGSUnpackerSharp.Utils;

namespace AGSUnpackerSharp.Graphics
{
  public class AGSSpriteSet
  {
    private static readonly string SpriteSetSignature = "\x20Sprite File\x20";
    private static readonly string SpriteSetFilename = "acsprset.spr";
    private static readonly string SpriteSetIndexSignature = "SPRINDEX";
    private static readonly string SpriteSetIndexFileName = "sprindex.dat";

    public static Color[] DefaultPalette = new Color[] {
      Color.FromArgb(255,   0,   0,   0),  Color.FromArgb(255,   0,   0, 168),  Color.FromArgb(255,   0, 168,   0),
      Color.FromArgb(255,   0, 168, 168),  Color.FromArgb(255, 168,   0,   0),  Color.FromArgb(255, 168,   0, 168),
      Color.FromArgb(255, 168,  84,   0),  Color.FromArgb(255, 168, 168, 168),  Color.FromArgb(255,  84,  84,  84),
      Color.FromArgb(255,  84,  84, 252),  Color.FromArgb(255,  84, 252,  84),  Color.FromArgb(255,  84, 252, 252),
      Color.FromArgb(255, 252,  84,  84),  Color.FromArgb(255, 252,  84, 252),  Color.FromArgb(255, 252, 252,  84),
      Color.FromArgb(255, 252, 252, 252),  Color.FromArgb(255,   0,   0,   0),  Color.FromArgb(255,  20,  20,  20),
      Color.FromArgb(255,  32,  32,  32),  Color.FromArgb(255,  44,  44,  44),  Color.FromArgb(255,  56,  56,  56),
      Color.FromArgb(255,  68,  68,  68),  Color.FromArgb(255,  80,  80,  80),  Color.FromArgb(255,  96,  96,  96),
      Color.FromArgb(255, 112, 112, 112),  Color.FromArgb(255, 128, 128, 128),  Color.FromArgb(255, 144, 144, 144),
      Color.FromArgb(255, 160, 160, 160),  Color.FromArgb(255, 180, 180, 180),  Color.FromArgb(255, 200, 200, 200),
      Color.FromArgb(255, 224, 224, 224),  Color.FromArgb(255, 252, 252, 252),  Color.FromArgb(255,   0,   0, 252),
      Color.FromArgb(255,  68,   0, 252),  Color.FromArgb(255, 124,   0, 252),  Color.FromArgb(255, 184, 148,   0),
      Color.FromArgb(255, 184, 128,   0),  Color.FromArgb(255, 252,   0, 188),  Color.FromArgb(255, 252,   0, 128),
      Color.FromArgb(255, 252,   0,  84),  Color.FromArgb(255, 252,   0,   0),  Color.FromArgb(255, 252,  68,   0),
      Color.FromArgb(255, 128, 120, 120),  Color.FromArgb(255, 136, 116, 112),  Color.FromArgb(255, 132,   8,   4),
      Color.FromArgb(255,  20,   4,   4),  Color.FromArgb(255, 196,  20,   4),  Color.FromArgb(255, 148, 136, 132),
      Color.FromArgb(255,  68,  12,   4),  Color.FromArgb(255, 148, 124, 120),  Color.FromArgb(255, 176, 168, 168),
      Color.FromArgb(255, 164, 128, 124),  Color.FromArgb(255,  88,  84,  84),  Color.FromArgb(255, 128, 116, 112),
      Color.FromArgb(255, 148, 120, 112),  Color.FromArgb(255, 192, 160, 148),  Color.FromArgb(255, 152, 128, 116),
      Color.FromArgb(255, 172, 152, 140),  Color.FromArgb(255, 104,  44,  12),  Color.FromArgb(255, 160, 144, 132),
      Color.FromArgb(255, 156,  76,  24),  Color.FromArgb(255, 176, 128,  92),  Color.FromArgb(255, 200, 108,  32),
      Color.FromArgb(255, 204, 192, 176),  Color.FromArgb(255, 176, 168, 148),  Color.FromArgb(255, 188, 180, 164),
      Color.FromArgb(255, 136, 132, 120),  Color.FromArgb(255, 232, 228, 196),  Color.FromArgb(255, 212, 208, 200),
      Color.FromArgb(255, 116, 112,  92),  Color.FromArgb(255, 232, 232, 232),  Color.FromArgb(255, 188, 188,  44),
      Color.FromArgb(255, 216, 212,  16),  Color.FromArgb(255, 144, 144,  92),  Color.FromArgb(255, 168, 168,  68),
      Color.FromArgb(255, 124, 128, 104),  Color.FromArgb(255, 196, 200, 192),  Color.FromArgb(255,   0, 228, 204),
      Color.FromArgb(255,  40, 200, 188),  Color.FromArgb(255,  80, 172, 168),  Color.FromArgb(255,   4, 184, 176),
      Color.FromArgb(255,  16, 144, 156),  Color.FromArgb(255, 176, 188, 192),  Color.FromArgb(255, 188, 212, 216),
      Color.FromArgb(255, 152, 180, 192),  Color.FromArgb(255,  56, 152, 212),  Color.FromArgb(255, 112, 176, 216),
      Color.FromArgb(255,   0, 120, 200),  Color.FromArgb(255,  96, 104, 112),  Color.FromArgb(255, 112, 120, 128),
      Color.FromArgb(255, 180, 200, 216),  Color.FromArgb(255, 128, 164, 192),  Color.FromArgb(255, 140, 164, 180),
      Color.FromArgb(255, 108, 148, 192),  Color.FromArgb(255,  88, 140, 192),  Color.FromArgb(255, 116, 144, 172),
      Color.FromArgb(255,  68, 128, 192),  Color.FromArgb(255, 120, 132, 144),  Color.FromArgb(255,  32,  76, 140),
      Color.FromArgb(255,  64,  76,  96),  Color.FromArgb(255,  68,  76,  84),  Color.FromArgb(255,  68,  96, 140),
      Color.FromArgb(255, 112, 120, 140),  Color.FromArgb(255,  76,  84, 100),  Color.FromArgb(255,  52,  60,  76),
      Color.FromArgb(255,  80, 108, 152),  Color.FromArgb(255,  96, 104, 120),  Color.FromArgb(255, 100, 120, 160),
      Color.FromArgb(255,  80,  92, 112),  Color.FromArgb(255,  96, 108, 140),  Color.FromArgb(255,   8,  32,  88),
      Color.FromArgb(255,  96, 108, 128),  Color.FromArgb(255,  88, 100, 120),  Color.FromArgb(255,   8,  32, 100),
      Color.FromArgb(255,  88, 100, 132),  Color.FromArgb(255,   8,  24,  80),  Color.FromArgb(255,  80,  88, 120),
      Color.FromArgb(255,   8,  24,  88),  Color.FromArgb(255,   0,  16,  80),  Color.FromArgb(255,   0,  16,  88),
      Color.FromArgb(255, 112, 112, 128),  Color.FromArgb(255,  56,  64, 104),  Color.FromArgb(255,  72,  80, 128),
      Color.FromArgb(255,  40,  48,  96),  Color.FromArgb(255,  36,  48, 116),  Color.FromArgb(255,  24,  36, 100),
      Color.FromArgb(255,  24,  36, 120),  Color.FromArgb(255,   4,  16,  72),  Color.FromArgb(255,  48,  56, 104),
      Color.FromArgb(255,  48,  56, 116),  Color.FromArgb(255,  44,  56, 136),  Color.FromArgb(255,  24,  32,  88),
      Color.FromArgb(255,   8,  24, 100),  Color.FromArgb(255,  64,  72, 136),  Color.FromArgb(255,  56,  64, 124),
      Color.FromArgb(255,  16,  24,  80),  Color.FromArgb(255,  16,  24,  88),  Color.FromArgb(255,   8,  16,  80),
      Color.FromArgb(255, 128, 132, 148),  Color.FromArgb(255,  68,  72, 120),  Color.FromArgb(255,  16,  24,  96),
      Color.FromArgb(255,   8,  16,  88),  Color.FromArgb(255,   0,   8,  88),  Color.FromArgb(255,  96,  96, 112),
      Color.FromArgb(255, 104, 108, 140),  Color.FromArgb(255,  84,  88, 132),  Color.FromArgb(255,  36,  40,  96),
      Color.FromArgb(255,  24,  28,  80),  Color.FromArgb(255,  56,  56,  96),  Color.FromArgb(255,  44,  48, 108),
      Color.FromArgb(255,  36,  40,  88),  Color.FromArgb(255,  24,  32, 164),  Color.FromArgb(255,  32,  40, 216),
      Color.FromArgb(255,  24,  32, 216),  Color.FromArgb(255,  20,  28, 200),  Color.FromArgb(255,  24,  36, 228),
      Color.FromArgb(255,  16,  24, 216),  Color.FromArgb(255,  12,  20, 192),  Color.FromArgb(255,   8,  20, 232),
      Color.FromArgb(255,  96,  96, 140),  Color.FromArgb(255,  72,  76, 112),  Color.FromArgb(255,   8,   8,  72),
      Color.FromArgb(255,  44,  48, 232),  Color.FromArgb(255,  32,  40, 228),  Color.FromArgb(255,  16,  24, 228),
      Color.FromArgb(255, 104, 104, 112),  Color.FromArgb(255, 120, 120, 128),  Color.FromArgb(255, 104, 104, 128),
      Color.FromArgb(255, 112, 112, 140),  Color.FromArgb(255,  96,  96, 120),  Color.FromArgb(255,  88,  88, 112),
      Color.FromArgb(255,  96,  96, 128),  Color.FromArgb(255,  88,  88, 120),  Color.FromArgb(255,  24,  24,  36),
      Color.FromArgb(255,  68,  68, 104),  Color.FromArgb(255,  80,  80, 124),  Color.FromArgb(255,  56,  56, 108),
      Color.FromArgb(255,  48,  48,  96),  Color.FromArgb(255,  96,  96, 228),  Color.FromArgb(255,  24,  24,  88),
      Color.FromArgb(255,  16,  16,  80),  Color.FromArgb(255,  16,  16,  88),  Color.FromArgb(255, 124, 120, 140),
      Color.FromArgb(255,  44,  44,  60),  Color.FromArgb(255,  68,  64,  96),  Color.FromArgb(255,  84,  80, 112),
      Color.FromArgb(255,  36,  28,  80),  Color.FromArgb(255,  32,  24,  96),  Color.FromArgb(255,  24,  16,  88),
      Color.FromArgb(255,  16,  12,  72),  Color.FromArgb(255,  56,  48,  88),  Color.FromArgb(255,  56,  48,  96),
      Color.FromArgb(255,  56,  48, 108),  Color.FromArgb(255,  88,  80, 124),  Color.FromArgb(255,  64,  56, 100),
      Color.FromArgb(255, 104,  96, 136),  Color.FromArgb(255,  68,  56, 120),  Color.FromArgb(255,  76,  64, 104),
      Color.FromArgb(255,  80,  72,  96),  Color.FromArgb(255, 104,  96, 128),  Color.FromArgb(255,  96,  88, 120),
      Color.FromArgb(255, 100,  88, 132),  Color.FromArgb(255,  52,  40,  88),  Color.FromArgb(255,  84,  72, 112),
      Color.FromArgb(255, 104,  96, 120),  Color.FromArgb(255, 120, 112, 140),  Color.FromArgb(255,  96,  88, 112),
      Color.FromArgb(255, 144, 140, 148),  Color.FromArgb(255,  68,  52,  88),  Color.FromArgb(255,  88,  72, 104),
      Color.FromArgb(255, 120, 112, 128),  Color.FromArgb(255, 112, 104, 120),  Color.FromArgb(255, 116, 104, 128),
      Color.FromArgb(255, 104,  88, 120),  Color.FromArgb(255,  96,  80, 112),  Color.FromArgb(255, 104,  96, 112),
      Color.FromArgb(255, 136, 128, 140),  Color.FromArgb(255, 100,  68, 120),  Color.FromArgb(255,  92,  80, 100),
      Color.FromArgb(255, 112,  96, 120),  Color.FromArgb(255,  84,  64,  96),  Color.FromArgb(255, 140, 108, 156),
      Color.FromArgb(255, 104,  88, 112),  Color.FromArgb(255, 120,  84, 132),  Color.FromArgb(255, 160, 120, 168),
      Color.FromArgb(255, 116,  88, 120),  Color.FromArgb(255, 132,  88, 136),  Color.FromArgb(255, 128, 112, 128),
      Color.FromArgb(255, 120, 104, 120),  Color.FromArgb(255, 124,  72, 120),  Color.FromArgb(255, 112, 108, 112),
      Color.FromArgb(255, 120,  96, 116),  Color.FromArgb(255, 108,  84, 100),  Color.FromArgb(255, 148, 104, 136),
      Color.FromArgb(255, 140,  80, 120),  Color.FromArgb(255, 156, 152, 156),  Color.FromArgb(255, 112,  96, 108),
      Color.FromArgb(255, 180, 120, 156),  Color.FromArgb(255, 176,  88, 140),  Color.FromArgb(255, 152,  56, 112),
      Color.FromArgb(255, 116, 116, 116),  Color.FromArgb(255, 128, 112, 120),  Color.FromArgb(255, 212,  84, 136),
      Color.FromArgb(255, 144, 120, 132),  Color.FromArgb(255, 188,  28,  88),  Color.FromArgb(255, 136, 124, 128),
      Color.FromArgb(255, 136, 112, 120),  Color.FromArgb(255, 124,  96, 104),  Color.FromArgb(255, 124,  36,  52),
      Color.FromArgb(255, 132, 104, 108),  Color.FromArgb(255, 120, 108, 108),  Color.FromArgb(255, 228, 224, 224),
      Color.FromArgb(255, 180, 180, 180),  Color.FromArgb(255, 200, 200, 200),  Color.FromArgb(255, 160, 160, 160),
      Color.FromArgb(255, 120, 120, 120),
    };

    public static void PackSprites(string folderPath, string outputFolderPath)
    {
      if (!Directory.Exists(outputFolderPath))
      {
        Trace.Assert(false, string.Format("PackSprites: Output folder \"{0}\" does not exist!", outputFolderPath));
        return;
      }

      string spritesetHeaderFilepath = Path.Combine(folderPath, SpriteSetHeader.FileName);
      if (!File.Exists(spritesetHeaderFilepath))
      {
        Trace.Assert(false, string.Format("PackSprites: SpriteSetHeader file \"{0}\" does not exist!", spritesetHeaderFilepath));
        return;
      }

      string[] filepaths = Directory.GetFiles(folderPath, "spr*");
      if (filepaths.Length < 1)
      {
        Trace.Assert(false, string.Format("PackSprites: Folder \"{0}\" does not have any sprite files!", folderPath));
        return;
      }

      string spritesetFilepath = Path.Combine(outputFolderPath, SpriteSetFilename);

      PackSpritesInternal(spritesetFilepath, spritesetHeaderFilepath, filepaths);
    }

    public static void PackSprites(string spritesetFilepath, string spritesetHeaderFilepath, params string[] filepaths)
    {
      if (!File.Exists(spritesetHeaderFilepath))
      {
        Trace.Assert(false, string.Format("PackSprites: SpriteSetHeader file \"{0}\" does not exist!", spritesetHeaderFilepath));
        return;
      }

      for (int i = 0; i < filepaths.Length; ++i)
      {
        if (!File.Exists(filepaths[i]))
        {
          Trace.Assert(false, string.Format("PackSprites: File \"{0}\" does not exist!", filepaths[i]));
          return;
        }
      }

      PackSpritesInternal(spritesetFilepath, spritesetHeaderFilepath, filepaths);
    }

    public static bool UnpackSprites(string spriteFilePath, string targetFolderPath)
    {
      if (!File.Exists(spriteFilePath))
      {
        Trace.Assert(false, string.Format("UnpackSprites: Sprite file \"{0}\" does not exist!", spriteFilePath));
        return false;
      }

      if (!Directory.Exists(targetFolderPath))
      {
        Trace.Assert(false, string.Format("UnpackSprites: Target folder \"{0}\" does not exist!", targetFolderPath));
        return false;
      }

      return UnpackSpritesInternal(spriteFilePath, targetFolderPath);
    }

    private static byte[] CompressRLE(byte[] buffer, int width, int height, int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return AGSGraphicUtils.WriteRLE8Rows(buffer, width, height);
        case 2:
          return AGSGraphicUtils.WriteRLE16Rows(buffer, width, height);
        case 4:
          return AGSGraphicUtils.WriteRLE32Rows(buffer, width, height);

        default:
          throw new InvalidDataException("Unknown bytesPerPixel value is encountered!");
      }
    }

    private static byte[] DecompressRLE(BinaryReader reader, long sizeCompressed, long sizeUncompressed, int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return AGSGraphicUtils.ReadRLE8(reader, sizeCompressed, sizeUncompressed);
        case 2:
          return AGSGraphicUtils.ReadRLE16(reader, sizeCompressed, sizeUncompressed);
        case 4:
          return AGSGraphicUtils.ReadRLE32(reader, sizeCompressed, sizeUncompressed);

        default:
          throw new InvalidDataException("Unknown bytesPerPixel value is encountered!");
      }
    }

    private static void PackSpritesInternal(string outputFilepath, string headerFilepath, params string[] filepaths)
    {
      SpriteSetHeader header = SpriteSetHeader.ReadFromFile(headerFilepath);
      List<SpriteEntry> sprites = GetSortedSpritesList(filepaths);

      using (FileStream stream = new FileStream(outputFilepath, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
        {
          int spritesCount = GetLargestIndex(sprites);
          WriteSpriteSetHeader(writer, header, spritesCount);
          SpriteIndexInfo[] spritesWritten = WriteSprites(writer, header, sprites);
          WriteSpriteIndexFile(header, spritesWritten);
        }
      }
    }

    private static bool UnpackSpritesInternal(string spriteFilePath, string targetFolderPath)
    {
      Console.Write("Opening {0}...", spriteFilePath);

      SpriteSetHeader header = null;

      using (FileStream stream = new FileStream(spriteFilePath, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.GetEncoding(1252)))
        {
          Console.WriteLine(" Done!");
          Console.Write("Parsing {0}...", spriteFilePath);

          header = ReadSpriteSetHeader(reader);

          Console.WriteLine(" Done!");
          Console.WriteLine("Extracting...");

          //TODO(adm244): read sprindex.dat

          for (int index = 0; index <= header.SpritesCount; ++index)
          {
            Console.Write(string.Format("\tExtracting spr{0:D5}...", index));

            Bitmap bitmap = ReadSprite(reader, header);
            if (bitmap == null)
            {
              Console.WriteLine(" Skipping (empty).");
              continue;
            }

            SaveBitmap(targetFolderPath, bitmap, index);

            Console.WriteLine(" Done!");
          }
        }
      }

      //TODO(adm244): should probably check if file were _actually_ read
      if (header == null)
      {
        Console.WriteLine("Error! Could not read a file.");
        return false;
      }

      Console.WriteLine("Done!");
      Console.Write("Writting meta file...");

      header.WriteMetaFile(targetFolderPath);

      Console.WriteLine(" Done!");

      return (header.SpritesCount > 0);
    }

    private static bool GetSpriteIndex(string filename, out int index)
    {
      index = -1;

      const string prefix = "spr";

      if (!filename.StartsWith(prefix))
        return false;

      return int.TryParse(filename.Substring(prefix.Length), out index);
    }

    private static List<SpriteEntry> GetSortedSpritesList(string[] filePaths)
    {
      List<SpriteEntry> sprites = new List<SpriteEntry>();

      for (int i = 0; i < filePaths.Length; ++i)
      {
        Bitmap bitmap = null;

        //RANT(adm244): thanks to Bitmap's busted constructor that does not close a file in time
        // (and apparently doesn't allow reading), we are forced to handle it ourselves.
        // Have I told you already that .NET is a setup for a failure?
        using (FileStream stream = new FileStream(filePaths[i], FileMode.Open, FileAccess.Read))
          bitmap = new Bitmap(stream);

        if (bitmap == null)
          continue;

        //NOTE(adm244): .NET decides for some reason that 16-bits is 32-bits when loading bmp (a bug?),
        // so we have to parse bits count field manually...
        //TODO(adm244): double check that
        if (Path.GetExtension(filePaths[i]) == ".bmp")
        {
          PixelFormat format = AGSGraphicUtils.ReadBitmapPixelFormat(filePaths[i]);
          bitmap = bitmap.Convert(format);
        }

        string filename = Path.GetFileNameWithoutExtension(filePaths[i]);

        int index;
        if (!GetSpriteIndex(filename, out index))
          continue;

        sprites.Add(new SpriteEntry(index, bitmap));
      }

      sprites.Sort();

      return sprites;
    }

    private static int GetLargestIndex(List<SpriteEntry> sprites)
    {
      //NOTE(adm244): assumes that sprites list is sorted
      return sprites[sprites.Count - 1].Index;
    }

    private static SpriteIndexInfo[] WriteSprites(BinaryWriter writer, SpriteSetHeader header, List<SpriteEntry> sprites)
    {
      List<SpriteIndexInfo> spriteIndexData = new List<SpriteIndexInfo>();

      int spriteIndex = 0;
      int listIndex = 0;

      while (listIndex < sprites.Count)
      {
        if (sprites[listIndex].Index == spriteIndex)
        {
          SpriteIndexInfo spriteWritten = WriteSprite(writer, header, sprites[listIndex].Image);
          spriteIndexData.Add(spriteWritten);
          ++listIndex;
        }
        else
        {
          SpriteIndexInfo spriteEmpty = new SpriteIndexInfo(writer.BaseStream.Position);
          spriteIndexData.Add(spriteEmpty);
          writer.Write((UInt16)0);
        }

        ++spriteIndex;
      }

      return spriteIndexData.ToArray();
    }

    private static SpriteIndexInfo WriteSprite(BinaryWriter writer, SpriteSetHeader header, Bitmap sprite)
    {
      SpriteIndexInfo spriteIndexData = new SpriteIndexInfo();

      spriteIndexData.Width = sprite.Width;
      spriteIndexData.Height = sprite.Height;
      spriteIndexData.Offset = writer.BaseStream.Position;

      int bytesPerPixel = sprite.GetBytesPerPixel();

      writer.Write((UInt16)bytesPerPixel);
      writer.Write((UInt16)sprite.Width);
      writer.Write((UInt16)sprite.Height);

      byte[] buffer = sprite.GetPixels();

      if (header.Compression == CompressionType.RLE)
        buffer = CompressRLE(buffer, sprite.Width, sprite.Height, bytesPerPixel);

      if (header.Version >= 6)
      {
        if (header.Compression == CompressionType.RLE)
          writer.Write((UInt32)buffer.Length);
      }
      else if (header.Version == 5)
        writer.Write((UInt32)buffer.Length);

      writer.Write((byte[])buffer);

      return spriteIndexData;
    }

    private static SpriteSetHeader ReadSpriteSetHeader(BinaryReader reader)
    {
      Int16 version = reader.ReadInt16();
      string signature = reader.ReadFixedCString(SpriteSetSignature.Length);
      Debug.Assert(signature == SpriteSetSignature);

      CompressionType compression = SpriteSetHeader.DefaultCompression;
      UInt32 fileID = SpriteSetHeader.DefaultFileID;

      if (version == 4)
        compression = CompressionType.Uncompressed;
      else if (version == 5)
        compression = CompressionType.RLE;
      else if (version >= 6)
      {
        compression = CompressionType.Unknown;
        byte compressionType = reader.ReadByte();
        if (Enum.IsDefined(typeof(CompressionType), (int)compressionType))
          compression = (CompressionType)compressionType;

        fileID = reader.ReadUInt32();
      }

      //TODO(adm244): we should probably get palette from a DTA file instead
      //TODO(adm244): it seems like we should take paluses into consideration here
      //TODO(adm244): double check that palette color format is RGB6bits
      Color[] palette = SpriteSetHeader.DefaultPalette;
      if (version < 5)
        palette = AGSGraphicUtils.ReadPalette(reader, ColorFormat.RGB6bits);

      UInt16 spritesCount = reader.ReadUInt16();

      return new SpriteSetHeader(version, compression, fileID, spritesCount, palette);
    }

    private static void WriteSpriteSetHeader(BinaryWriter writer, SpriteSetHeader header, int spritesCount)
    {
      writer.Write((UInt16)header.Version);
      writer.Write((char[])SpriteSetSignature.ToCharArray());

      if (header.Version >= 6)
      {
        writer.Write((byte)header.Compression);
        writer.Write((UInt32)header.FileID);
      }

      if (header.Version < 5)
        AGSGraphicUtils.WritePalette(writer, header.Palette, ColorFormat.RGB6bits);

      writer.Write((UInt16)spritesCount);
    }

    //TODO(adm244): ReadSpriteIndexFile

    private static void WriteSpriteIndexFile(SpriteSetHeader header, SpriteIndexInfo[] spriteIndexInfo)
    {
      // FIXME(adm244): check all filepaths so that they ALL are either RELATIVE or ABSOLUTE
      // because for now some files are saved in a working directory (relative paths)
      // and some in other places (absolute paths)
      using (FileStream stream = new FileStream(SpriteSetIndexFileName, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.GetEncoding(1252)))
        {
          writer.Write((char[])SpriteSetIndexSignature.ToCharArray());

          //NOTE(adm244): is this a file version?
          writer.Write((UInt32)2);

          writer.Write((UInt32)(header.FileID));
          writer.Write((UInt32)(spriteIndexInfo.Length - 1));
          writer.Write((UInt32)(spriteIndexInfo.Length));

          for (int i = 0; i < spriteIndexInfo.Length; ++i)
            writer.Write((UInt16)spriteIndexInfo[i].Width);

          for (int i = 0; i < spriteIndexInfo.Length; ++i)
            writer.Write((UInt16)spriteIndexInfo[i].Height);

          for (int i = 0; i < spriteIndexInfo.Length; ++i)
            writer.Write((UInt32)spriteIndexInfo[i].Offset);
        }
      }
    }

    private static Bitmap ReadSprite(BinaryReader reader, SpriteSetHeader header)
    {
      int width;
      int height;
      int bytesPerPixel;

      byte[] buffer = ReadSprite(reader, header, out width, out height, out bytesPerPixel);
      if (buffer == null)
        return null;

      //TODO(adm244): check all pixel format cases on real data
      return AGSGraphicUtils.ConvertToBitmap(buffer, width, height, bytesPerPixel, header.Palette);
    }

    private static byte[] ReadSprite(BinaryReader reader, SpriteSetHeader header, out int width, out int height, out int bytesPerPixel)
    {
      width = 0;
      height = 0;

      bytesPerPixel = (int)reader.ReadUInt16();
      if (bytesPerPixel == 0)
        return null;

      width = (int)reader.ReadUInt16();
      height = (int)reader.ReadUInt16();

      long size = (long)width * height * bytesPerPixel;
      long sizeUncompressed = size;

      if (header.Version >= 6)
      {
        if (header.Compression == CompressionType.RLE)
          size = reader.ReadUInt32();
      }
      else if (header.Version == 5)
        size = reader.ReadUInt32();

      if (header.Compression == CompressionType.RLE)
        return DecompressRLE(reader, size, sizeUncompressed, bytesPerPixel);

      return reader.ReadBytes((int)size);
    }

    private static void SaveBitmap(string folderPath, Bitmap bitmap, int index)
    {
      string fileName = string.Format("spr{0:D5}", index);
      string filePath = Path.Combine(folderPath, fileName);

      if (bitmap.HasAlphaChannel())
        bitmap.Save(filePath + ".png", ImageFormat.Png);
      else
        bitmap.Save(filePath + ".bmp", ImageFormat.Bmp);
    }

    private class SpriteEntry : IComparable<SpriteEntry>
    {
      public int Index { get; private set; }
      public Bitmap Image { get; private set; }

      public SpriteEntry(int index, Bitmap image)
      {
        Index = index;
        Image = image;
      }

      public int CompareTo(SpriteEntry other)
      {
        if (Index == other.Index)
          return 0;

        if (Index > other.Index)
          return 1;
        else
          return -1;
      }
    }

    private struct SpriteIndexInfo
    {
      public int Width;
      public int Height;
      public long Offset;

      public SpriteIndexInfo(long offset)
        : this(0, 0, offset)
      {
      }

      public SpriteIndexInfo(int width, int height, long offset)
      {
        Width = width;
        Height = height;
        Offset = offset;
      }
    }
  }
}
