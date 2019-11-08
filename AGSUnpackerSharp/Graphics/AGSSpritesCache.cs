using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AGSUnpackerSharp.Utils;

namespace AGSUnpackerSharp.Graphics
{
  public struct SpriteData
  {
    public int Width;
    public int Height;
    public UInt32 Offset;
  }

  public class AGSSpritesCache
  {
    private static readonly string SPRITESET_SIGNATURE = "\x20Sprite File\x20";
    private static readonly string SPRITESET_INDEX_SIGNATURE = "SPRINDEX";
    private static readonly string SPRITESET_FILENAME = "acsprset.spr";
    private static readonly string SPRITESET_INDEX_FILENAME = "sprindex.dat";

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

    public static void PackSprites(params string[] filepaths)
    {
      FileStream fs = new FileStream(SPRITESET_FILENAME, FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

      SpritesMeta meta = new SpritesMeta();
      meta.ReadMetaFile();

      w.Write(meta.Version);
      w.Write(SPRITESET_SIGNATURE.ToCharArray());
      w.Write(meta.Compression);
      if (meta.Version >= 6)
      {
        w.Write(meta.FileID);
      }

      if (meta.Version < 5)
      {
        //TODO(adm244): write palette
      }

      w.Write((UInt16)(filepaths.Length - 1));
      SpriteData[] spritesData = new SpriteData[filepaths.Length];
      for (int i = 0; i < filepaths.Length; ++i)
      {
        //NOTE(adm244): .NET decides for some reason that 16-bits is 32-bits when loading bmp (bug?),
        // so we have to parse bits count field manualy...
        PixelFormat format = PixelFormat.Undefined;
        if (Path.GetExtension(filepaths[i]) == ".bmp")
        {
          FileStream file = new FileStream(filepaths[i], FileMode.Open);
          BinaryReader reader = new BinaryReader(file, Encoding.ASCII);
          reader.BaseStream.Seek(28, SeekOrigin.Begin);
          UInt16 bitCount = reader.ReadUInt16();
          reader.Close();

          switch (bitCount)
          {
            case 8:
              format = PixelFormat.Format8bppIndexed;
              break;
            case 16:
              format = PixelFormat.Format16bppRgb565;
              break;
            case 32:
              format = PixelFormat.Format32bppArgb;
              break;

            default:
              Debug.Assert(false);
              break;
          }
        }

        Bitmap sprite = new Bitmap(filepaths[i]);

        if (format == PixelFormat.Undefined)
          format = sprite.PixelFormat;

        UInt16 bytesPerPixel = (UInt16)(Bitmap.GetPixelFormatSize(format) / 8);

        UInt16 width = (UInt16)sprite.Width;
        UInt16 height = (UInt16)sprite.Height;
        UInt32 size = (UInt32)width * height * bytesPerPixel;

        spritesData[i].Width = width;
        spritesData[i].Height = height;
        spritesData[i].Offset = (UInt32)w.BaseStream.Position;

        w.Write(bytesPerPixel);
        w.Write(width);
        w.Write(height);

        byte[] rawData = new byte[size];

        BitmapData lockData = sprite.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
        IntPtr p = lockData.Scan0;
        for (int row = 0; row < height; ++row)
        {
          Marshal.Copy(p, rawData, row * width * bytesPerPixel, width * bytesPerPixel);
          p = new IntPtr(p.ToInt64() + lockData.Stride);
        }
        sprite.UnlockBits(lockData);

        if (meta.Compression == 1)
        {
          switch (bytesPerPixel)
          {
            case 1:
              //rawData = AGSGraphicUtils.WriteRLEData8(rawData);
              {
                MemoryStream stream = new MemoryStream(rawData.Length);
                for (int y = 0; y < height; ++y)
                {
                  byte[] pixels = new byte[width];
                  Buffer.BlockCopy(rawData, y * width, pixels, 0, width);

                  byte[] buf = AGSGraphicUtils.WriteRLEData8(pixels);
                  stream.Write(buf, 0, buf.Length);
                }
                rawData = stream.ToArray();
              }
              break;
            case 2:
              {
                /*UInt16[] pixels = new UInt16[rawData.Length / 2];
                Buffer.BlockCopy(rawData, 0, pixels, 0, rawData.Length);
                rawData = AGSGraphicUtils.WriteRLEData16(pixels);*/

                MemoryStream stream = new MemoryStream(rawData.Length);
                for (int y = 0; y < height; ++y)
                {
                  UInt16[] pixels = new UInt16[width];
                  Buffer.BlockCopy(rawData, y * width * 2, pixels, 0, width * 2);

                  byte[] buf = AGSGraphicUtils.WriteRLEData16(pixels);
                  stream.Write(buf, 0, buf.Length);
                }
                rawData = stream.ToArray();
              }
              break;
            case 4:
              {
                MemoryStream stream = new MemoryStream(rawData.Length);
                for (int y = 0; y < height; ++y)
                {
                  UInt32[] pixels = new UInt32[width];
                  Buffer.BlockCopy(rawData, y * width * 4, pixels, 0, width * 4);

                  byte[] buf = AGSGraphicUtils.WriteRLEData32(pixels);
                  stream.Write(buf, 0, buf.Length);
                }
                rawData = stream.ToArray();
              }
              break;

            default:
              Debug.Assert(false);
              break;
          }
        }

        if (meta.Version >= 5)
          w.Write(rawData.Length);

        w.Write(rawData);
      }

      w.Close();

      WriteSpriteIndexFile(meta, spritesData);
    }

    private static void WriteSpriteIndexFile(SpritesMeta meta, SpriteData[] spritesData)
    {
      FileStream fs = new FileStream(SPRITESET_INDEX_FILENAME, FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

      w.Write(SPRITESET_INDEX_SIGNATURE.ToCharArray());
      w.Write((UInt32)2);
      w.Write(meta.FileID);
      w.Write((UInt32)(spritesData.Length - 1));
      w.Write((UInt32)spritesData.Length);

      for (int i = 0; i < spritesData.Length; ++i)
        w.Write((UInt16)spritesData[i].Width);

      for (int i = 0; i < spritesData.Length; ++i)
        w.Write((UInt16)spritesData[i].Height);

      for (int i = 0; i < spritesData.Length; ++i)
        w.Write((UInt32)spritesData[i].Offset);

      w.Close();
    }

    public static bool ExtractSprites(string spritefile, string targetFolder)
    {
      Console.Write("Opening {0}...", spritefile);

      FileStream fs = new FileStream(spritefile, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      Console.WriteLine(" Done!");
      Console.Write("Parsing {0}...", spritefile);

      SpritesMeta meta = new SpritesMeta();

      meta.Version = r.ReadInt16();
      string signature = r.ReadFixedString(SPRITESET_SIGNATURE.Length);
      Debug.Assert(signature == SPRITESET_SIGNATURE);

      if (meta.Version == 4)
        meta.Compression = 0;
      else if (meta.Version == 5)
        meta.Compression = 1;
      else if (meta.Version >= 6)
      {
        meta.Compression = r.ReadByte();
        meta.FileID = r.ReadUInt32();
      }

      if (meta.Version < 5)
      {
        //TODO(adm244): it seems like we should take paluses into consideration here
        byte[] rawPalette = r.ReadBytes(256 * 3);

        meta.Palette = new Color[256];
        for (int i = 0; i < meta.Palette.Length; ++i)
        {
          int red = rawPalette[3 * i + 0];
          int green = rawPalette[3 * i + 1];
          int blue = rawPalette[3 * i + 2];

          //NOTE(adm244): AGS is using only 6-bits per channel, so we have to convert it to full 8-bit range
          blue = (byte)((blue / 64f) * 256f);
          green = (byte)((green / 64f) * 256f);
          red = (byte)((red / 64f) * 256f);

          meta.Palette[i] = Color.FromArgb(red, green, blue);
        }
      }

      UInt16 spritesCount = r.ReadUInt16();

      Console.WriteLine(" Done!");
      Console.WriteLine("Extracting...");

      string foldername = targetFolder;
      if (string.IsNullOrEmpty(targetFolder))
      {
        foldername = "acsprset";
        if (!Directory.Exists(foldername))
          Directory.CreateDirectory(foldername);
      }

      //TODO(adm244): read sprindex.dat

      for (int i = 0; i <= spritesCount; ++i)
      {
        Console.Write(string.Format("\tExtracting spr{0:D5}...", i));

        UInt16 bytesPerPixel = r.ReadUInt16();
        if (bytesPerPixel == 0)
        {
          Console.WriteLine(" Skipped (empty).");
          continue;
        }

        UInt16 width = r.ReadUInt16();
        UInt16 height = r.ReadUInt16();

        UInt32 size = 0;
        if (meta.Version == 5)
        {
          size = r.ReadUInt32();
        }
        else if (meta.Version >= 6)
        {
          size = (meta.Compression == 1) ? r.ReadUInt32() : (UInt32)width * height * bytesPerPixel;
        }
        else
        {
          size = (UInt32)width * height * bytesPerPixel;
        }

        byte[] rawData = null;
        if (meta.Compression == 0)
        {
          rawData = r.ReadBytes((int)size);
        }
        else
        {
          int imageSize = width * height * bytesPerPixel;
          switch (bytesPerPixel)
          {
            case 1:
              rawData = AGSGraphicUtils.ReadRLEData8(r, size, imageSize);
              break;
            case 2:
              rawData = AGSGraphicUtils.ReadRLEData16(r, size, imageSize);
              break;
            case 3:
              Debug.Assert(false);
              break;
            case 4:
              rawData = AGSGraphicUtils.ReadRLEData32(r, size, imageSize);
              break;

            default:
              Debug.Assert(false);
              break;
          }
        }

        PixelFormat format = PixelFormat.Undefined;
        switch (bytesPerPixel)
        {
          case 1:
            format = PixelFormat.Format8bppIndexed;
            break;

          case 2:
            format = PixelFormat.Format16bppRgb565;
            break;

          case 3:
            //format = PixelFormat.Format24bppRgb;
            Debug.Assert(false);
            break;

          case 4:
            format = PixelFormat.Format32bppArgb;
            break;

          default:
            Debug.Assert(false);
            break;
        }

        //TODO(adm244): check all pixel format cases on real data

        Bitmap bitmap = new Bitmap(width, height, format);

        if (format == PixelFormat.Format8bppIndexed)
        {
          if (meta.Palette == null)
          {
            //TODO(adm244): use default palette (read from dta file)
            meta.Palette = DefaultPalette;
          }

          ColorPalette bitmapPalette = bitmap.Palette;
          for (int j = 0; j < bitmapPalette.Entries.Length; ++j)
          {
            bitmapPalette.Entries[j] = meta.Palette[j];
          }
          bitmap.Palette = bitmapPalette;
        }

        BitmapData lockData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);

        IntPtr p = lockData.Scan0;
        for (int row = 0; row < height; ++row)
        {
          Marshal.Copy(rawData, row * width * bytesPerPixel, p, width * bytesPerPixel);
          p = new IntPtr(p.ToInt64() + lockData.Stride);
        }

        bitmap.UnlockBits(lockData);

        ImageFormat imageFormat;
        switch (format)
        {
          case PixelFormat.Format32bppArgb:
            imageFormat = ImageFormat.Png;
            break;

          default:
            imageFormat = ImageFormat.Bmp;
            break;
        }

        string filename = string.Format("spr{0:D5}.{1}", i, (imageFormat == ImageFormat.Png) ? "png" : "bmp");
        string filepath = Path.Combine(foldername, filename);
        bitmap.Save(filepath, imageFormat);

        Console.WriteLine(" Done!");
      }

      r.Close();

      Console.WriteLine("Done!");
      Console.Write("Writting meta file...");

      meta.WriteMetaFile(foldername);

      Console.WriteLine(" Done!");

      return spritesCount > 0;
    }
  }
}
