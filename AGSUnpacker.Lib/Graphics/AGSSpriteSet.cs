using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Graphics;
using AGSUnpacker.Graphics.Formats;
using AGSUnpacker.Lib.Graphics.Extensions;
using AGSUnpacker.Lib.Utils;
using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Graphics
{
  public class AGSSpriteSet
  {
    private static readonly string SpriteSetSignature = "\x20Sprite File\x20";
    private static readonly string SpriteSetFilename = "acsprset.spr";
    private static readonly string SpriteSetIndexSignature = "SPRINDEX";
    private static readonly string SpriteSetIndexFileName = "sprindex.dat";

    // TODO(adm244): move this somewhere else
    public static readonly Palette DefaultPalette = new Palette(
      new Color[] {
        new Color(  0,   0,   0), new Color(  0,   0, 168), new Color(  0, 168,   0),
        new Color(  0, 168, 168), new Color(168,   0,   0), new Color(168,   0, 168),
        new Color(168,  84,   0), new Color(168, 168, 168), new Color( 84,  84,  84),
        new Color( 84,  84, 252), new Color( 84, 252,  84), new Color( 84, 252, 252),
        new Color(252,  84,  84), new Color(252,  84, 252), new Color(252, 252,  84),
        new Color(252, 252, 252), new Color(  0,   0,   0), new Color( 20,  20,  20),
        new Color( 32,  32,  32), new Color( 44,  44,  44), new Color( 56,  56,  56),
        new Color( 68,  68,  68), new Color( 80,  80,  80), new Color( 96,  96,  96),
        new Color(112, 112, 112), new Color(128, 128, 128), new Color(144, 144, 144),
        new Color(160, 160, 160), new Color(180, 180, 180), new Color(200, 200, 200),
        new Color(224, 224, 224), new Color(252, 252, 252), new Color(  0,   0, 252),
        new Color( 68,   0, 252), new Color(124,   0, 252), new Color(184, 148,   0),
        new Color(184, 128,   0), new Color(252,   0, 188), new Color(252,   0, 128),
        new Color(252,   0,  84), new Color(252,   0,   0), new Color(252,  68,   0),
        new Color(128, 120, 120), new Color(136, 116, 112), new Color(132,   8,   4),
        new Color( 20,   4,   4), new Color(196,  20,   4), new Color(148, 136, 132),
        new Color( 68,  12,   4), new Color(148, 124, 120), new Color(176, 168, 168),
        new Color(164, 128, 124), new Color( 88,  84,  84), new Color(128, 116, 112),
        new Color(148, 120, 112), new Color(192, 160, 148), new Color(152, 128, 116),
        new Color(172, 152, 140), new Color(104,  44,  12), new Color(160, 144, 132),
        new Color(156,  76,  24), new Color(176, 128,  92), new Color(200, 108,  32),
        new Color(204, 192, 176), new Color(176, 168, 148), new Color(188, 180, 164),
        new Color(136, 132, 120), new Color(232, 228, 196), new Color(212, 208, 200),
        new Color(116, 112,  92), new Color(232, 232, 232), new Color(188, 188,  44),
        new Color(216, 212,  16), new Color(144, 144,  92), new Color(168, 168,  68),
        new Color(124, 128, 104), new Color(196, 200, 192), new Color(  0, 228, 204),
        new Color( 40, 200, 188), new Color( 80, 172, 168), new Color(  4, 184, 176),
        new Color( 16, 144, 156), new Color(176, 188, 192), new Color(188, 212, 216),
        new Color(152, 180, 192), new Color( 56, 152, 212), new Color(112, 176, 216),
        new Color(  0, 120, 200), new Color( 96, 104, 112), new Color(112, 120, 128),
        new Color(180, 200, 216), new Color(128, 164, 192), new Color(140, 164, 180),
        new Color(108, 148, 192), new Color( 88, 140, 192), new Color(116, 144, 172),
        new Color( 68, 128, 192), new Color(120, 132, 144), new Color( 32,  76, 140),
        new Color( 64,  76,  96), new Color( 68,  76,  84), new Color( 68,  96, 140),
        new Color(112, 120, 140), new Color( 76,  84, 100), new Color( 52,  60,  76),
        new Color( 80, 108, 152), new Color( 96, 104, 120), new Color(100, 120, 160),
        new Color( 80,  92, 112), new Color( 96, 108, 140), new Color(  8,  32,  88),
        new Color( 96, 108, 128), new Color( 88, 100, 120), new Color(  8,  32, 100),
        new Color( 88, 100, 132), new Color(  8,  24,  80), new Color( 80,  88, 120),
        new Color(  8,  24,  88), new Color(  0,  16,  80), new Color(  0,  16,  88),
        new Color(112, 112, 128), new Color( 56,  64, 104), new Color( 72,  80, 128),
        new Color( 40,  48,  96), new Color( 36,  48, 116), new Color( 24,  36, 100),
        new Color( 24,  36, 120), new Color(  4,  16,  72), new Color( 48,  56, 104),
        new Color( 48,  56, 116), new Color( 44,  56, 136), new Color( 24,  32,  88),
        new Color(  8,  24, 100), new Color( 64,  72, 136), new Color( 56,  64, 124),
        new Color( 16,  24,  80), new Color( 16,  24,  88), new Color(  8,  16,  80),
        new Color(128, 132, 148), new Color( 68,  72, 120), new Color( 16,  24,  96),
        new Color(  8,  16,  88), new Color(  0,   8,  88), new Color( 96,  96, 112),
        new Color(104, 108, 140), new Color( 84,  88, 132), new Color( 36,  40,  96),
        new Color( 24,  28,  80), new Color( 56,  56,  96), new Color( 44,  48, 108),
        new Color( 36,  40,  88), new Color( 24,  32, 164), new Color( 32,  40, 216),
        new Color( 24,  32, 216), new Color( 20,  28, 200), new Color( 24,  36, 228),
        new Color( 16,  24, 216), new Color( 12,  20, 192), new Color(  8,  20, 232),
        new Color( 96,  96, 140), new Color( 72,  76, 112), new Color(  8,   8,  72),
        new Color( 44,  48, 232), new Color( 32,  40, 228), new Color( 16,  24, 228),
        new Color(104, 104, 112), new Color(120, 120, 128), new Color(104, 104, 128),
        new Color(112, 112, 140), new Color( 96,  96, 120), new Color( 88,  88, 112),
        new Color( 96,  96, 128), new Color( 88,  88, 120), new Color( 24,  24,  36),
        new Color( 68,  68, 104), new Color( 80,  80, 124), new Color( 56,  56, 108),
        new Color( 48,  48,  96), new Color( 96,  96, 228), new Color( 24,  24,  88),
        new Color( 16,  16,  80), new Color( 16,  16,  88), new Color(124, 120, 140),
        new Color( 44,  44,  60), new Color( 68,  64,  96), new Color( 84,  80, 112),
        new Color( 36,  28,  80), new Color( 32,  24,  96), new Color( 24,  16,  88),
        new Color( 16,  12,  72), new Color( 56,  48,  88), new Color( 56,  48,  96),
        new Color( 56,  48, 108), new Color( 88,  80, 124), new Color( 64,  56, 100),
        new Color(104,  96, 136), new Color( 68,  56, 120), new Color( 76,  64, 104),
        new Color( 80,  72,  96), new Color(104,  96, 128), new Color( 96,  88, 120),
        new Color(100,  88, 132), new Color( 52,  40,  88), new Color( 84,  72, 112),
        new Color(104,  96, 120), new Color(120, 112, 140), new Color( 96,  88, 112),
        new Color(144, 140, 148), new Color( 68,  52,  88), new Color( 88,  72, 104),
        new Color(120, 112, 128), new Color(112, 104, 120), new Color(116, 104, 128),
        new Color(104,  88, 120), new Color( 96,  80, 112), new Color(104,  96, 112),
        new Color(136, 128, 140), new Color(100,  68, 120), new Color( 92,  80, 100),
        new Color(112,  96, 120), new Color( 84,  64,  96), new Color(140, 108, 156),
        new Color(104,  88, 112), new Color(120,  84, 132), new Color(160, 120, 168),
        new Color(116,  88, 120), new Color(132,  88, 136), new Color(128, 112, 128),
        new Color(120, 104, 120), new Color(124,  72, 120), new Color(112, 108, 112),
        new Color(120,  96, 116), new Color(108,  84, 100), new Color(148, 104, 136),
        new Color(140,  80, 120), new Color(156, 152, 156), new Color(112,  96, 108),
        new Color(180, 120, 156), new Color(176,  88, 140), new Color(152,  56, 112),
        new Color(116, 116, 116), new Color(128, 112, 120), new Color(212,  84, 136),
        new Color(144, 120, 132), new Color(188,  28,  88), new Color(136, 124, 128),
        new Color(136, 112, 120), new Color(124,  96, 104), new Color(124,  36,  52),
        new Color(132, 104, 108), new Color(120, 108, 108), new Color(228, 224, 224),
        new Color(180, 180, 180), new Color(200, 200, 200), new Color(160, 160, 160),
        new Color(120, 120, 120)
      }
    );

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
          return AGSCompression.WriteRLE8Rows(buffer, width, height);
        case 2:
          return AGSCompression.WriteRLE16Rows(buffer, width, height);
        case 4:
          return AGSCompression.WriteRLE32Rows(buffer, width, height);

        default:
          throw new InvalidDataException("Unknown bytesPerPixel value is encountered!");
      }
    }

    private static byte[] CompressLZW(byte[] buffer)
    {
      if (buffer.Length < 16)
        return buffer;

      //TODO(adm244): is it LZ77 or LZW? dig into that...
      return AGSCompression.WriteLZ77(buffer);
    }

    private static byte[] DecompressRLE(BinaryReader reader, long sizeUncompressed, int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return AGSCompression.ReadRLE8(reader, sizeUncompressed);
        case 2:
          return AGSCompression.ReadRLE16(reader, sizeUncompressed);
        case 4:
          return AGSCompression.ReadRLE32(reader, sizeUncompressed);

        default:
          throw new InvalidDataException("Unknown bytesPerPixel value is encountered!");
      }
    }

    private static byte[] DecompressLZW(BinaryReader reader, long sizeUncompressed)
    {
      // NOTE(adm244): you might think they'd set "uncompressed" as compression type when it's useless to compress
      // (since they've added this piece of data into each sprite header anyway)
      // so that you never have to check this, but they suuuure know better...
      if (sizeUncompressed < 16)
        return reader.ReadBytes((int)sizeUncompressed);

      return AGSCompression.ReadLZ77(reader, sizeUncompressed);
    }

    private static int GetSpriteIndexVersion(SpriteSetHeader header)
    {
      switch (header.Version)
      {
        // NOTE(adm244): version 5 is between 3.1.0 and 3.1.2
        // 3.1.0 has version 4; 3.1.2 has version 6
        // guess it was never released? why bump it then?
        case 4:
        case 5:
          return 1;

        case 6:
          return 2;

        case 10:
        case 11:
          return header.Version;

        //NOTE(adm244): yep...
        case 12:
          return 11;

        default:
          throw new NotSupportedException(
            $"Cannot determine sprite index file version.\n\nUnknown sprite set version: {header.Version}");
      }
    }

    private static void PackSpritesInternal(string outputFilepath, string headerFilepath, params string[] filepaths)
    {
      SpriteSetHeader header = SpriteSetHeader.ReadFromFile(headerFilepath);
      List<SpriteEntry> sprites = GetSortedSpritesList(filepaths);

      using (FileStream stream = new FileStream(outputFilepath, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          int spritesCount = GetLargestIndex(sprites);
          WriteSpriteSetHeader(writer, header, spritesCount);
          SpriteIndexInfo[] spritesWritten = WriteSprites(writer, header, sprites);

          // HACK(adm244): temp solution
          string indexFilepath = Path.GetDirectoryName(outputFilepath);

          int version = GetSpriteIndexVersion(header);
          WriteSpriteIndexFile(indexFilepath, header, spritesWritten, version);
        }
      }
    }

    private static bool UnpackSpritesInternal(string spriteFilePath, string targetFolderPath)
    {
      Console.Write("Opening {0}...", spriteFilePath);

      SpriteSetHeader header = null;

      using (FileStream stream = new FileStream(spriteFilePath, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.Latin1))
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

            Bitmap sprite = ReadSprite(reader, header, index);
            if (sprite == null)
            {
              Console.WriteLine(" Skipping (empty).");
              continue;
            }

            SaveSprite(sprite, targetFolderPath, index);

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
        string filename = Path.GetFileNameWithoutExtension(filePaths[i]);

        if (!GetSpriteIndex(filename, out int index))
          continue;

        Bitmap sprite = new Bitmap(filePaths[i]);
        sprites.Add(new SpriteEntry(index, sprite));
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
          SpriteIndexInfo spriteWritten = WriteSprite(writer, header, sprites[listIndex].Sprite);
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

      sprite = PrepareSpriteForWritting(sprite, header, out SpriteFormat format);

      writer.Write((byte)sprite.BytesPerPixel);
      writer.Write((byte)format);

      if (header.Version >= 12)
      {
        writer.Write((byte)(sprite.Palette?.Length - 1 ?? 0));
        writer.Write((byte)header.Compression);
      }

      writer.Write((UInt16)sprite.Width);
      writer.Write((UInt16)sprite.Height);

      if (header.Version >= 12)
      {
        if (format != SpriteFormat.Default)
          WriteSpritePalette(writer, sprite, format);
      }

      byte[] buffer = sprite.GetPixels();
      if (header.Compression != CompressionType.Uncompressed)
        buffer = CompressSprite(buffer, sprite, header.Compression);

      if (header.Version >= 6)
      {
        if (header.Version >= 12 || header.Compression == CompressionType.RLE)
          writer.Write((UInt32)buffer.Length);
      }
      else if (header.Version == 5)
        writer.Write((UInt32)buffer.Length);

      writer.Write((byte[])buffer);

      return spriteIndexData;
    }

    private static byte[] CompressSprite(byte[] buffer, Bitmap sprite, CompressionType compressionType)
    {
      switch (compressionType)
      {
        case CompressionType.RLE:
          return CompressRLE(buffer, sprite.Width, sprite.Height, sprite.BytesPerPixel);

        case CompressionType.LZW:
          return CompressLZW(buffer);

        default:
          throw new NotSupportedException(
            $"Sprite compression type '{compressionType}' is not supported.");
      }
    }

    private static void WriteSpritePalette(BinaryWriter writer, Bitmap sprite, SpriteFormat format)
    {
      PixelFormat? pixelFormat = format.ToPixelFormat() ?? sprite.Palette?.SourceFormat;
      byte[] buffer = sprite.Palette?.ToBuffer(pixelFormat.Value);
      writer.Write((byte[])buffer);
    }

    private static Bitmap PrepareSpriteForWritting(Bitmap sprite, SpriteSetHeader header, out SpriteFormat format)
    {
      format = SpriteFormat.Default;

      //TODO(adm244): add support for creating indexed sprites
      //if ((header.StoreFlags & 1) != 0)
      //{
      //  // create indexed sprite...
      //}

      //NOTE(adm244): AGS doesn't support 24bpp RLE compressed images, so we convert them to 32bpp (null alpha)
      // ALSO, AGS seems to treat 24bpp images as RGB while all others as BGR (!)
      // so let's just NOT use 24bpp and convert them to 32bpp
      if (sprite.Format == PixelFormat.Rgb24)
        return sprite.Convert(PixelFormat.Argb32, discardAlpha: true);

      return sprite;
    }

    private static SpriteSetHeader ReadSpriteSetHeader(BinaryReader reader)
    {
      int version = reader.ReadInt16();
      // FIXME(adm244): should probably use ReadFixedString instead; there are many cases like this
      string signature = reader.ReadFixedCString(SpriteSetSignature.Length);
      //Debug.Assert(signature == SpriteSetSignature);

      if (signature != SpriteSetSignature)
        throw new InvalidDataException("Sprite set file signature mismatch!");

      if (version < 4 || version > 12)
        throw new NotSupportedException(
          $"Sprite set version is not supported.\n\nGot: {version}\nMin supported: {4}\nMax supported: {12}");

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

      // TODO(adm244): for correct results we should get palette from a DTA file instead
      Palette palette = SpriteSetHeader.DefaultPalette;
      if (version < 5)
      {
        palette = AGSGraphics.ReadPalette(reader);
        // NOTE(adm244): first 17 colors are locked and cannot be changed in pre 6
        // but for some reason palette in sprite set file differs from game data (2.72)
        // Here we try to restore what we can, but it's not enought to be fully correct
        // AGS. When everything's a stress.
        for (int i = 0; i < 17; ++i)
          palette.Entries[i] = SpriteSetHeader.DefaultPalette[i];
      }

      int spritesCount;
      if (version < 11)
        spritesCount = reader.ReadUInt16();
      else
        spritesCount = reader.ReadInt32();

      int storeFlags = 0;
      if (version >= 12)
      {
        storeFlags = reader.ReadByte();
        _ = reader.ReadBytes(3); // reserved
      }

      return new SpriteSetHeader(version, compression, fileID, spritesCount, palette, storeFlags);
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
        AGSGraphics.WritePalette(writer, header.Palette);

      if (header.Version < 11)
        writer.Write((UInt16)spritesCount);
      else
        writer.Write((Int32)spritesCount);

      if (header.Version >= 12)
      {
        writer.Write((byte)header.StoreFlags);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
      }
    }

    //TODO(adm244): ReadSpriteIndexFile

    private static void WriteSpriteIndexFile(string outputFolder, SpriteSetHeader header, SpriteIndexInfo[] spriteIndexInfo, int version)
    {
      // FIXME(adm244): check all filepaths so that they ALL are either RELATIVE or ABSOLUTE
      // because for now some files are saved in a working directory (relative paths)
      // and some in other places (absolute paths)
      string targetFilepath = Path.Combine(outputFolder, SpriteSetIndexFileName);

      using (FileStream stream = new FileStream(targetFilepath, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          writer.Write((char[])SpriteSetIndexSignature.ToCharArray());

          writer.Write((UInt32)version);

          if (version >= 2)
            writer.Write((UInt32)(header.FileID));

          writer.Write((UInt32)(spriteIndexInfo.Length - 1));
          writer.Write((UInt32)(spriteIndexInfo.Length));

          for (int i = 0; i < spriteIndexInfo.Length; ++i)
            writer.Write((UInt16)spriteIndexInfo[i].Width);

          for (int i = 0; i < spriteIndexInfo.Length; ++i)
            writer.Write((UInt16)spriteIndexInfo[i].Height);

          if (version <= 2)
          {
            for (int i = 0; i < spriteIndexInfo.Length; ++i)
              writer.Write((UInt32)spriteIndexInfo[i].Offset);
          }
          else
          {
            for (int i = 0; i < spriteIndexInfo.Length; ++i)
              writer.Write((UInt64)spriteIndexInfo[i].Offset);
          }
        }
      }
    }

    private static bool IsAlphaChannelUsed(byte[] buffer)
    {
      Debug.Assert(buffer.Length % 4 == 0);

      int length = buffer.Length / 4;
      for (int i = 0; i < length; ++i)
        if (buffer[i * 4 + 3] > 0)
          return true;

      return false;
    }

    private static Bitmap ReadSprite(BinaryReader reader, SpriteSetHeader header, int index)
    {
      // TODO(adm244): maybe return PixelFormat instead of bytesPerPixel?
      byte[] buffer = ReadSprite(reader, header, index, out int width, out int height, out int bytesPerPixel);
      if (buffer == null)
        return null;

      PixelFormat format = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);
      if (format == PixelFormat.Indexed)
        return new Bitmap(width, height, buffer, format, header.Palette);

      Bitmap bitmap = new Bitmap(width, height, buffer, format);

      // NOTE(adm244): since AGS doesn't support 24bpp RLE images it converts them to 32bpp
      // (even if it won't be compressed, you know, 'just in case' case...)
      // in the process alpha channel gets set to 0 (transparent) instead of 255 (opaque)
      // which leads to a problem of fully transparent images (and "features" in GDI decoders)
      //
      // to resolve this issue, we check if alpha channel is used and if not discard it
      if (format == PixelFormat.Argb32 && !IsAlphaChannelUsed(buffer))
        bitmap = bitmap.Convert(PixelFormat.Rgb24);

      return bitmap;
    }

    private static byte[] ReadSprite(BinaryReader reader, SpriteSetHeader header, int index,
      out int width, out int height, out int bytesPerPixel)
    {
      width = 0;
      height = 0;

      bytesPerPixel = reader.ReadByte();
      SpriteFormat format = (SpriteFormat)reader.ReadByte();
      if (!Enum.IsDefined(format))
        throw new NotSupportedException($"Unknown sprite format encountered: {format}");

      if (bytesPerPixel == 0)
        return null;

      int paletteColours = 0;
      CompressionType compression = header.Compression;

      if (header.Version >= 12)
      {
        paletteColours = reader.ReadByte() + 1;
        compression = (CompressionType)reader.ReadByte();

        if (!Enum.IsDefined(compression))
          throw new NotSupportedException(
            $"Unknown compression type {compression} for sprite {index:D5} encountered");
      }

      width = reader.ReadUInt16();
      height = reader.ReadUInt16();

      Palette? palette = ReadSpritePalette(reader, format, paletteColours, index);

      long size = (long)width * height * bytesPerPixel;
      long sizeUncompressed = size;

      //FIXME(adm244): I assume this is wrong... care to investigate? sure... someday
      if (palette.HasValue)
        sizeUncompressed = (long)width * height;

      if (header.Version >= 6)
      {
        if (header.Version >= 12 || header.Compression == CompressionType.RLE)
          size = reader.ReadUInt32();
      }
      else if (header.Version == 5)
        size = reader.ReadUInt32();

      byte[] pixels;
      if (compression == CompressionType.RLE)
        pixels = DecompressRLE(reader, sizeUncompressed,
          format != SpriteFormat.Default ? 1 : bytesPerPixel);
      else if (compression == CompressionType.LZW)
        pixels = DecompressLZW(reader, sizeUncompressed);
      else if (compression == CompressionType.Uncompressed)
        pixels = reader.ReadBytes((int)size);
      else
        throw new NotSupportedException($"Unknown compression type {compression} encountered");

      if (palette.HasValue)
        return UnpackIndexedSprite(pixels, format, palette.Value);

      return pixels;
    }

    private static byte[] UnpackIndexedSprite(byte[] input, SpriteFormat format, Palette palette)
    {
      if (palette.Empty)
        return input;

      PixelFormat? spritePixelFormat = format.ToPixelFormat();
      if (!spritePixelFormat.HasValue)
        throw new InvalidDataException($"Cannot unpack indexed sprite. Sprite pixel format is unknown.");

      //PixelFormat? palettePixelFormat = palette.SourceFormat;
      //if (!palettePixelFormat.HasValue)
      //  throw new InvalidDataException($"Cannot unpack indexed sprite. Palette pixel format is unknown.");
      //
      //if (spritePixelFormat != palettePixelFormat)
      //  throw new InvalidDataException($"Cannot unpack indexed sprite." +
      //    $"Sprite and palette pixel formats must be the same.");

      Color[] colors = new Color[input.Length];
      for (int i = 0; i < input.Length; ++i)
      {
        int index = input[i];
        Debug.Assert(index < palette.Length,
          $"Palette index '{index}' is out of bounds '0..{palette.Length}'. " +
          $"Defaulting to color 0.");

        if (index >= palette.Length)
          index = 0;

        colors[i] = palette.Entries[index];
      }

      return colors.ToBuffer(spritePixelFormat.Value);
    }

    private static Palette? ReadSpritePalette(BinaryReader reader, SpriteFormat format, int colors, int index)
    {
      if (format == SpriteFormat.Default)
        return null;

      if (colors > 0)
      {
        PixelFormat? paletteFormat = format.ToPixelFormat();
        if (!paletteFormat.HasValue)
          throw new InvalidDataException($"Sprite {index} has palette colors, but palette format {format} is unknown");

        byte[] buffer = reader.ReadBytes(colors * paletteFormat.Value.GetBytesPerPixel());
        return Palette.FromBuffer(buffer, paletteFormat.Value, discardAlpha: false);
      }

      return null;
    }

    private static void SaveSprite(Bitmap image, string folderPath, int index)
    {
      string fileName = string.Format("spr{0:D5}", index);
      string filePath = Path.Combine(folderPath, fileName);

      ImageFormat format = ImageFormat.Png;
      if (image.Format == PixelFormat.Rgb565)
        format = ImageFormat.Bmp;

      image.Save(filePath, format);
    }

    private class SpriteEntry : IComparable<SpriteEntry>
    {
      public int Index { get; }
      public Bitmap Sprite { get; }

      public SpriteEntry(int index, Bitmap sprite)
      {
        Index = index;
        Sprite = sprite;
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
