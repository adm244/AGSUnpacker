using System;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Utils;

using SixLabors.ImageSharp.PixelFormats;

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

    public static readonly Int16 DefaultVersion = 6;
    public static readonly CompressionType DefaultCompression = CompressionType.Uncompressed;
    public static readonly UInt32 DefaultFileID = 0xDEADBEEF;
    public static readonly UInt16 DefaultSpritesCount = 0;
    public static readonly Bgra32[] DefaultPalette = AGSSpriteSet.DefaultPalette;

    public Int16 Version { get; private set; }
    public CompressionType Compression { get; private set; }
    public UInt32 FileID { get; private set; }
    public UInt16 SpritesCount { get; private set; }
    public Bgra32[] Palette { get; private set; }

    private SpriteSetHeader()
      : this(DefaultVersion, DefaultCompression, DefaultFileID, DefaultSpritesCount, DefaultPalette)
    {
    }

    public SpriteSetHeader(Int16 version, CompressionType compression, UInt32 fileID, UInt16 spritesCount, Bgra32[] palette)
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
            header.Palette = ReadPalette(reader);
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
            WritePalette(writer);
        }
      }
    }

    private static Bgra32[] ReadPalette(BinaryReader reader)
    {
      Bgra32[] palette = new Bgra32[256];

      for (int i = 0; i < palette.Length; ++i)
      {
        Int32 color = reader.ReadInt32();
        palette[i] = AGSGraphicUtils.FromABGR(color);
      }

      return palette;
    }

    private void WritePalette(BinaryWriter writer)
    {
      for (int i = 0; i < Palette.Length; ++i)
      {
        Int32 color = AGSGraphicUtils.ToABGR(Palette[i]);
        writer.Write((UInt32)color);
      }
    }
  }
}
