using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AGSUnpackerSharp.Graphics
{
  public class AGSSpritesCache
  {
    private static string SPRITESET_SIGNATURE = "\x20Sprite File\x20";

    public static void ExtractSprites(string spritefile)
    {
      FileStream fs = new FileStream(spritefile, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      Int16 version = r.ReadInt16();
      string signature = r.ReadFixedString(SPRITESET_SIGNATURE.Length);
      Debug.Assert(signature == SPRITESET_SIGNATURE);

      bool compressed = false;
      if (version == 4)
      {
        compressed = false;
      }
      else if (version == 5)
      {
        compressed = true;
      }
      else if (version >= 6)
      {
        compressed = (r.ReadByte() == 1) ? true : false;
        UInt32 fileID = r.ReadUInt32();
      }

      if (version < 5)
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
        if (version == 5)
        {
          size = r.ReadUInt32();
        }
        else if (version >= 6)
        {
          size = compressed ? r.ReadUInt32() : (UInt32)width * height * bytesPerPixel;
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

        //TODO(adm244): read compressed images
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
        string filename = string.Format("spr{0:D5}.bmp", i);
        string filepath = Path.Combine(foldername, filename);

        if (!Directory.Exists(foldername))
        {
          Directory.CreateDirectory(foldername);
        }

        bitmap.Save(filepath, ImageFormat.Bmp);
      }

      r.Close();
    }
  }
}
