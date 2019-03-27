using System;
using System.IO;
using AGSUnpackerSharp.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AGSUnpackerSharp.Utils
{
  public static class AGSGraphicUtils
  {
    public static void ParseAllegroCompressedImage(BinaryReader r)
    {
      Int16 width = r.ReadInt16();
      Int16 height = r.ReadInt16();

      //TODO(adm244): do real parsing
      for (int y = 0; y < height; ++y)
      {
        int pixelsRead = 0;
        while (pixelsRead < width)
        {
          sbyte index = (sbyte)r.ReadByte();
          if (index == -128) index = 0;

          if (index < 0)
          {
            r.BaseStream.Seek(1, SeekOrigin.Current);
            pixelsRead += (1 - index);
          }
          else
          {
            r.BaseStream.Seek(index + 1, SeekOrigin.Current);
            pixelsRead += (index + 1);
          }
        }
      }

      // skip palette
      // 768 = 256 * 3
      r.BaseStream.Seek(768, SeekOrigin.Current);
    }

    public static Bitmap ParseLZWImage(BinaryReader r)
    {
      // skip palette
      r.BaseStream.Seek(256 * sizeof(Int32), SeekOrigin.Current);
      Int32 uncompressed_size = r.ReadInt32();
      Int32 compressed_size = r.ReadInt32();

      //TODO(adm244): parse background data

      // skip lzw compressed image
      //r.BaseStream.Seek(picture_data_size, SeekOrigin.Current);
      //byte[] rawBackground = r.ReadBytes(picture_data_size);

      /*FileStream fs = new FileStream("room_background", FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, System.Text.Encoding.GetEncoding(1252));
      w.Write((UInt32)picture_maxsize);
      w.Write((UInt32)picture_data_size);
      w.Write(rawBackground);
      w.Close();*/

      /*
       * AGS background image decompression algorithm:
       * 
       * byte[] output = new byte[uncompressed_size];
       * int output_pos = 0;
       * 
       * do {
       *   byte control = stream.read8();
       *   for (int mask = 1; mask & 0xFF; mask <<= 1) {
       *     if (control & mask) {
       *       uint16 runlength = stream.read16();
       *       
       *       //NOTE(adm244): first 12 bits are used to encode offset from current position in stream
       *       // which is equals to length of the buffer that was used to store a dictionary - 1 (0x0FFF = 4096 - 1)
       *       uint16 sequence_start_offset = runlength & 0x0FFF;
       *       
       *       //NOTE(adm244): +3 part is here to shift [min,max] range by 3 (resulting in [3, 18]),
       *       // because there's no point in compressing sequences of bytes with length 1 or 2.
       *       // If sequence length is 1 then you just output it, no compression can be applied.
       *       // If sequence length is 2 then it doesn't matter if you compress it or not,
       *       // because 2 bytes are already used to store offset and length of that sequence,
       *       // so the result will have the same length.
       *       byte sequence_length = (runlength >> 12) + 3;
       *       
       *       //NOTE(adm244): -1 because current position in the stream points to the next byte
       *       uin16 sequence_pos = output_pos - sequence_start_offset - 1;
       *       for (int i = 0; i < sequence_length; ++i) {
       *         output[output_pos] = output[sequence_pos];
       *         output_pos++;
       *         sequence_pos++;
       *       }
       *     } else {
       *       output[output_pos] = stream.read8();
       *       output_pos++;
       *     }
       *     
       *     if (output_pos >= uncompressed_size)
       *       break;
       *   }
       * } while (!stream.EOF());
       * 
       */

      byte[] output = new byte[uncompressed_size];
      int output_pos = 0;

      while (output_pos < uncompressed_size)
      {
        byte control = r.ReadByte();
        for (int mask = 1; (mask & 0xFF) != 0; mask <<= 1)
        {
          if ((control & mask) == 0)
          {
            output[output_pos] = r.ReadByte();
            output_pos++;
          }
          else
          {
            UInt16 runlength = r.ReadUInt16();
            UInt16 sequence_start_offset = (UInt16)(runlength & 0x0FFF);
            byte sequence_length = (byte)((runlength >> 12) + 3);

            int sequence_pos = output_pos - sequence_start_offset - 1;
            for (int i = 0; i < sequence_length; ++i)
            {
              output[output_pos] = output[sequence_pos];
              output_pos++;
              sequence_pos++;
            }
          }

          if (output_pos >= uncompressed_size)
            break;
        }
      }

      /*FileStream fs = new FileStream("room_background_decompressed", FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, System.Text.Encoding.GetEncoding(1252));
      w.Write(output);
      w.Close();*/

      //return new LZWImage(picture_maxsize, picture_data_size, rawBackground);

      int bpp = 4;
      int width = ((output[3] << 24) | (output[2] << 16) | (output[1] << 8) | output[0]) / bpp;
      int height = ((output[7] << 24) | (output[6] << 16) | (output[5] << 8) | output[4]);

      PixelFormat format = PixelFormat.Format32bppArgb;
      Bitmap bitmap = new Bitmap(width, height, format);
      BitmapData lockData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);

      byte[] rawData = new byte[output.Length - 8];
      Array.Copy(output, 8, rawData, 0, rawData.Length);

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < height; ++row)
      {
        Marshal.Copy(rawData, row * width * bpp, p, width * bpp);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }

      bitmap.UnlockBits(lockData);

      //bitmap.Save("room_background.bmp", ImageFormat.Bmp);

      return bitmap;
    }

    public static void WriteLZWImage(BinaryWriter w, LZWImage image)
    {
      w.Write((Int32)image.picture_maxsize);
      w.Write((Int32)image.picture_data_size);
      w.Write(image.rawBackground);
    }
  }
}
