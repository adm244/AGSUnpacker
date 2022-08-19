using System;
using System.IO;
using System.Text;

using AGSUnpacker.Graphics;

namespace AGSUnpacker.Lib.Graphics
{
  public enum SpriteFormat
  {
    Unknown = -1,
    Default = 0,
    PaletteRGB24 = 32,
    PaletteARGB32 = 33,
    PaletteRGB565 = 34,
  }

  public enum CompressionType
  {
    Unknown = -1,
    Uncompressed = 0,
    RLE = 1,
    LZW = 2,
  }

  public class SpriteSetHeader
  {
    public static readonly string FileName = "header.bin";

    public static readonly int DefaultVersion = 6;
    public static readonly CompressionType DefaultCompression = CompressionType.Uncompressed;
    public static readonly uint DefaultFileID = 0xDEADBEEF;
    public static readonly int DefaultSpritesCount = 0;
    public static readonly Palette DefaultPalette = AGSSpriteSet.DefaultPalette;
    public static readonly int DefaultStoreFlags = 0;

    public int Version { get; private set; }
    public CompressionType Compression { get; private set; }
    public uint FileID { get; private set; }
    public int SpritesCount { get; private set; }
    public Palette Palette { get; private set; }
    public int StoreFlags { get; private set; }

    private SpriteSetHeader()
      : this(DefaultVersion, DefaultCompression, DefaultFileID, DefaultSpritesCount, DefaultPalette, DefaultStoreFlags)
    {
    }

    public SpriteSetHeader(int version, CompressionType compression, uint fileID, int spritesCount, Palette palette, int storeFlags)
    {
      Version = version;
      Compression = compression;
      FileID = fileID;
      SpritesCount = spritesCount;
      Palette = palette;
      StoreFlags = storeFlags;
    }

    public static SpriteSetHeader ReadFromFile(string filepath)
    {
      SpriteSetHeader header = new SpriteSetHeader();

      using (FileStream stream = new FileStream(filepath, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.Latin1))
        {
          header.Version = reader.ReadInt16();

          header.Compression = CompressionType.Unknown;
          byte compressionType = reader.ReadByte();
          if (Enum.IsDefined(typeof(CompressionType), (int)compressionType))
            header.Compression = (CompressionType)compressionType;

          header.FileID = reader.ReadUInt32();

          if (header.Version < 11)
            header.SpritesCount = reader.ReadUInt16();
          else
            header.SpritesCount = reader.ReadInt32();

          header.StoreFlags = 0;
          if (header.Version >= 12)
            header.StoreFlags = reader.ReadByte();

          if (header.Version < 5)
            header.Palette = AGSGraphics.ReadPalette(reader);
        }
      }

      return header;
    }

    public void WriteMetaFile(string folderpath)
    {
      string filepath = Path.Combine(folderpath, FileName);

      using (FileStream stream = new FileStream(filepath, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          writer.Write((UInt16)Version);
          writer.Write((byte)Compression);
          writer.Write((UInt32)FileID);

          if (Version < 11)
            writer.Write((UInt16)SpritesCount);
          else
            writer.Write((Int32)SpritesCount);

          if (Version >= 12)
            writer.Write((byte)StoreFlags);

          if (Version < 5)
            AGSGraphics.WritePalette(writer, Palette);
        }
      }
    }
  }
}
