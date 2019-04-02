using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
        //FIX(adm244): get the original pixel format!
        Bitmap sprite = new Bitmap(filepaths[i]);
        //UInt16 bytesPerPixel = (UInt16)(Bitmap.GetPixelFormatSize(sprite.PixelFormat) / 8);
        PixelFormat format = PixelFormat.Format32bppArgb;
        UInt16 bytesPerPixel = (UInt16)(Bitmap.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8);

        UInt16 width = (UInt16)sprite.Width;
        UInt16 height = (UInt16)sprite.Height;
        UInt32 size = (UInt32)width * height * bytesPerPixel;

        spritesData[i].Width = width;
        spritesData[i].Height = height;
        spritesData[i].Offset = (UInt32)w.BaseStream.Position;

        w.Write(bytesPerPixel);
        w.Write(width);
        w.Write(height);

        if (meta.Version == 5)
        {
          w.Write(size);
        }
        else if (meta.Version >= 6)
        {
          if (meta.Compression == 1)
          {
            //TODO(adm244): compressed size
            w.Write(size);
          }
        }

        //TODO(adm244): write raw image data
        /*PixelFormat format;
        switch (bytesPerPixel)
        {
          case 2:
            format = PixelFormat.Format16bppRgb565;
            break;

          case 3:
            format = PixelFormat.Format24bppRgb;
            break;

          case 4:
            format = PixelFormat.Format32bppArgb;
            break;

          default:
            format = PixelFormat.Format8bppIndexed;
            break;
        }*/

        byte[] rawData = new byte[size];

        BitmapData lockData = sprite.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
        IntPtr p = lockData.Scan0;
        for (int row = 0; row < height; ++row)
        {
          Marshal.Copy(p, rawData, row * width * bytesPerPixel, width * bytesPerPixel);
          p = new IntPtr(p.ToInt64() + lockData.Stride);
        }
        sprite.UnlockBits(lockData);

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
      {
        w.Write((UInt16)spritesData[i].Width);
      }
      for (int i = 0; i < spritesData.Length; ++i)
      {
        w.Write((UInt16)spritesData[i].Height);
      }
      for (int i = 0; i < spritesData.Length; ++i)
      {
        w.Write((UInt32)spritesData[i].Offset);
      }

      w.Close();
    }

    public static void ExtractSprites(string spritefile)
    {
      FileStream fs = new FileStream(spritefile, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      SpritesMeta meta = new SpritesMeta();

      meta.Version = r.ReadInt16();
      string signature = r.ReadFixedString(SPRITESET_SIGNATURE.Length);
      Debug.Assert(signature == SPRITESET_SIGNATURE);

      if (meta.Version == 4)
      {
        meta.Compression = 0;
      }
      else if (meta.Version == 5)
      {
        meta.Compression = 1;
      }
      else if (meta.Version >= 6)
      {
        meta.Compression = r.ReadByte();
        meta.FileID = r.ReadUInt32();
      }

      //TODO(adm244): read compressed images
      Debug.Assert(meta.Compression == 0);

      if (meta.Version < 5)
      {
        // skip palette
        r.BaseStream.Seek(256 * 3, SeekOrigin.Current);
      }

      UInt16 spritesCount = r.ReadUInt16();

      //TODO(adm244): read sprindex.dat

      for (int i = 0; i <= spritesCount; ++i)
      {
        UInt16 bytesPerPixel = r.ReadUInt16();
        if (bytesPerPixel == 0) continue;

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

        byte[] rawData = r.ReadBytes((int)size);

        PixelFormat format;
        switch (bytesPerPixel)
        {
          case 2:
            format = PixelFormat.Format16bppRgb565;
            break;

          case 3:
            format = PixelFormat.Format24bppRgb;
            break;

          case 4:
            format = PixelFormat.Format32bppArgb;
            break;

          default:
            format = PixelFormat.Format8bppIndexed;
            break;
        }

        //TODO(adm244): read palette for 8-bit image data
        //TODO(adm244): check all pixel format cases on real data

        Bitmap bitmap = new Bitmap(width, height, format);
        BitmapData lockData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);

        IntPtr p = lockData.Scan0;
        for (int row = 0; row < height; ++row)
        {
          Marshal.Copy(rawData, row * width * bytesPerPixel, p, width * bytesPerPixel);
          p = new IntPtr(p.ToInt64() + lockData.Stride);
        }

        bitmap.UnlockBits(lockData);

        string foldername = "acsprset";
        if (!Directory.Exists(foldername))
        {
          Directory.CreateDirectory(foldername);
        }

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
      }

      r.Close();

      meta.WriteMetaFile();
    }
  }
}
