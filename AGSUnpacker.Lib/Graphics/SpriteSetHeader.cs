using System;
using System.IO;
using System.Text;

using AGSUnpacker.Graphics;

namespace AGSUnpacker.Lib.Graphics
{
  public enum CompressionType
  {
    Unknown = -1,
    Uncompressed = 0,
    RLE = 1,
  }

  public class SpriteSetHeader
  {
    public static readonly string FileName = "header.bin";

    public static readonly int DefaultVersion = 6;
    public static readonly CompressionType DefaultCompression = CompressionType.Uncompressed;
    public static readonly uint DefaultFileID = 0xDEADBEEF;
    public static readonly int DefaultSpritesCount = 0;
    public static readonly Palette DefaultPalette = AGSSpriteSet.DefaultPalette;

    public int Version { get; private set; }
    public CompressionType Compression { get; private set; }
    public uint FileID { get; private set; }
    public int SpritesCount { get; private set; }
    public Palette Palette { get; private set; }

    private SpriteSetHeader()
      : this(DefaultVersion, DefaultCompression, DefaultFileID, DefaultSpritesCount, DefaultPalette)
    {
    }

    public SpriteSetHeader(int version, CompressionType compression, uint fileID, int spritesCount, Palette palette)
    {
      Version = version;
      Compression = compression;
      FileID = fileID;
      SpritesCount = spritesCount;
      Palette = palette;
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
          header.SpritesCount = reader.ReadUInt16();

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
          writer.Write((UInt16)SpritesCount);

          if (Version < 5)
            AGSGraphics.WritePalette(writer, Palette);
        }
      }
    }
  }
}
