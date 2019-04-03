using System;
using System.IO;
using AGSUnpackerSharp.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AGSUnpackerSharp.Utils
{
  public enum LiteralType
  {
    Symbol,
    Lookback,
  }

  public struct Literal
  {
    public LiteralType Type;
    public byte Symbol;
    public long Offset;
    public long Length;

    public void SetSymbol(byte symbol)
    {
      Type = LiteralType.Symbol;
      Symbol = symbol;
    }

    public void SetLookback(long offset, long length)
    {
      Type = LiteralType.Lookback;
      Offset = offset;
      Length = length;
    }
  }

  public static class AGSGraphicUtils
  {
    public static byte[] WriteRLEData8(byte[] data)
    {
      const int maxRuns = 128;
      byte[] buffer = new byte[maxRuns];
      int bufferPosition = 0;

      MemoryStream stream = new MemoryStream();
      for (long i = 0; i < data.Length; )
      {
        byte value = data[i];

        int blockLength = maxRuns - 1;
        if (blockLength > (data.Length - i))
          blockLength = (int)(data.Length - i);

        int run = 1;
        while ((run < blockLength) && (data[i + run] == value))
          ++run;

        if ((run > 1) || (bufferPosition == maxRuns) || (i == (data.Length - 1)))
        {
          //NOTE(adm244): encode a run
          if (bufferPosition > 0)
          {
            stream.WriteByte((byte)(bufferPosition - 1));
            stream.Write(buffer, 0, bufferPosition);
            bufferPosition = 0;
          }
          
          stream.WriteByte((byte)(1 - run));
          stream.WriteByte((byte)value);
          i += run;
        }
        else
        {
          //NOTE(adm244): encode a sequence
          buffer[bufferPosition] = value;
          ++bufferPosition;
          ++i;
        }
      }

      return stream.ToArray();
    }

    public static byte[] WriteRLEData16(UInt16[] data)
    {
      const int maxRuns = 128;
      UInt16[] buffer = new UInt16[maxRuns];
      int bufferPosition = 0;

      MemoryStream stream = new MemoryStream();
      for (long i = 0; i < data.Length; )
      {
        UInt16 value = data[i];

        int blockLength = maxRuns - 1;
        if (blockLength > (data.Length - i))
          blockLength = (int)(data.Length - i);

        int run = 1;
        while ((run < blockLength) && (data[i + run] == value))
          ++run;

        if ((run > 1) || (bufferPosition == maxRuns) || (i == (data.Length - 1)))
        {
          //NOTE(adm244): encode a run
          if (bufferPosition > 0)
          {
            stream.WriteByte((byte)(bufferPosition - 1));
            //stream.Write(buffer, 0, bufferPosition);
            for (int j = 0; j < bufferPosition; ++j)
            {
              stream.WriteByte((byte)buffer[j]);
              stream.WriteByte((byte)(buffer[j] >> 8));
            }

            bufferPosition = 0;
          }
          
          stream.WriteByte((byte)(1 - run));
          stream.WriteByte((byte)value);
          stream.WriteByte((byte)(value >> 8));
          i += run;
        }
        else
        {
          //NOTE(adm244): encode a sequence
          buffer[bufferPosition] = value;
          ++bufferPosition;
          ++i;
        }
      }

      return stream.ToArray();
    }

    public static byte[] WriteRLEData32(UInt32[] data)
    {
      const int maxRuns = 128;
      UInt32[] buffer = new UInt32[maxRuns];
      int bufferPosition = 0;

      MemoryStream stream = new MemoryStream();
      for (long i = 0; i < data.Length; )
      {
        UInt32 value = data[i];

        int blockLength = maxRuns - 1;
        if (blockLength > (data.Length - i))
          blockLength = (int)(data.Length - i);

        int run = 1;
        while ((run < blockLength) && (data[i + run] == value))
          ++run;

        if ((run > 1) || (bufferPosition == maxRuns) || (i == (data.Length - 1)))
        {
          //NOTE(adm244): encode a run
          if (bufferPosition > 0)
          {
            stream.WriteByte((byte)(bufferPosition - 1));
            //stream.Write(buffer, 0, bufferPosition);
            for (int j = 0; j < bufferPosition; ++j)
            {
              stream.WriteByte((byte)buffer[j]);
              stream.WriteByte((byte)(buffer[j] >> 8));
              stream.WriteByte((byte)(buffer[j] >> 16));
              stream.WriteByte((byte)(buffer[j] >> 24));
            }

            bufferPosition = 0;
          }
          
          stream.WriteByte((byte)(1 - run));
          stream.WriteByte((byte)value);
          stream.WriteByte((byte)(value >> 8));
          stream.WriteByte((byte)(value >> 16));
          stream.WriteByte((byte)(value >> 24));
          i += run;
        }
        else
        {
          //NOTE(adm244): encode a sequence
          buffer[bufferPosition] = value;
          ++bufferPosition;
          ++i;
        }
      }

      return stream.ToArray();
    }

    public static byte[] ReadRLEData8(BinaryReader r, long compressedSize, int uncompressedSize)
    {
      byte[] image = new byte[uncompressedSize];
      int imagePosition = 0;

      long dataBase = r.BaseStream.Position;
      for (long n = 0; n < compressedSize; n = (r.BaseStream.Position - dataBase))
      {
        SByte control = (SByte)r.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = 1 - control;
          byte value = r.ReadByte();
          for (int j = 0; j < runCount; ++j)
          {
            image[imagePosition] = value;
            ++imagePosition;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = control + 1;
          for (int j = 0; j < literalsCount; ++j)
          {
            image[imagePosition] = r.ReadByte();
            ++imagePosition;
          }
        }
      }

      return image;
    }

    public static byte[] ReadRLEData16(BinaryReader r, long compressedSize, int uncompressedSize)
    {
      byte[] image = new byte[uncompressedSize];
      int imagePosition = 0;

      long dataBase = r.BaseStream.Position;
      for (long n = 0; n < compressedSize; n = (r.BaseStream.Position - dataBase))
      {
        SByte control = (SByte)r.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = 1 - control;
          UInt16 value = r.ReadUInt16();
          for (int j = 0; j < runCount; ++j)
          {
            image[imagePosition] = (byte)(value >> 0);
            image[imagePosition + 1] = (byte)(value >> 8);
            imagePosition += 2;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = control + 1;
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt16 value = r.ReadUInt16();
            image[imagePosition] = (byte)(value >> 0);
            image[imagePosition + 1] = (byte)(value >> 8);
            imagePosition += 2;
          }
        }
      }

      return image;
    }

    public static byte[] ReadRLEData32(BinaryReader r, long compressedSize, int uncompressedSize)
    {
      byte[] image = new byte[uncompressedSize];
      int imagePosition = 0;

      long dataBase = r.BaseStream.Position;
      for (long n = 0; n < compressedSize; n = (r.BaseStream.Position - dataBase))
      {
        SByte control = (SByte)r.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = 1 - control;
          UInt32 value = r.ReadUInt32();
          for (int j = 0; j < runCount; ++j)
          {
            image[imagePosition] = (byte)(value >> 0);
            image[imagePosition + 1] = (byte)(value >> 8);
            image[imagePosition + 2] = (byte)(value >> 16);
            image[imagePosition + 3] = (byte)(value >> 24);
            imagePosition += 4;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = control + 1;
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt32 value = r.ReadUInt32();
            image[imagePosition] = (byte)(value >> 0);
            image[imagePosition + 1] = (byte)(value >> 8);
            image[imagePosition + 2] = (byte)(value >> 16);
            image[imagePosition + 3] = (byte)(value >> 24);
            imagePosition += 4;
          }
        }
      }

      return image;
    }

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
      //r.BaseStream.Seek(256 * sizeof(Int32), SeekOrigin.Current);
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

      //TODO(adm244): get from DTA info
      int bpp = 4;
      int width = ((output[3] << 24) | (output[2] << 16) | (output[1] << 8) | output[0]) / bpp;
      int height = ((output[7] << 24) | (output[6] << 16) | (output[5] << 8) | output[4]);

      //bitmap.Save("room_background.bmp", ImageFormat.Bmp);

      return ConvertToBitmap(output, width, height, bpp);
    }

    private static Bitmap ConvertToBitmap(byte[] data, int width, int height, int bpp)
    {
      //TODO(adm244): pick according to bpp
      PixelFormat format = PixelFormat.Format32bppArgb;

      Bitmap bitmap = new Bitmap(width, height, format);
      BitmapData lockData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);

      byte[] rawData = new byte[data.Length - 8];
      Array.Copy(data, 8, rawData, 0, rawData.Length);

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < height; ++row)
      {
        Marshal.Copy(rawData, row * width * bpp, p, width * bpp);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }

      bitmap.UnlockBits(lockData);

      return bitmap;
    }

    private static byte[] ConvertToRaw(Bitmap image)
    {
      int width = image.Width;
      int height = image.Height;

      //TODO(adm244): extract from Bitmap
      int bpp = 4;

      byte[] pixels = new byte[width * height * bpp];

      BitmapData lockData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, image.PixelFormat);
      IntPtr p = lockData.Scan0;
      for (int row = 0; row < height; ++row)
      {
        Marshal.Copy(p, pixels, row * width * bpp, width * bpp);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      image.UnlockBits(lockData);

      byte[] rawData = new byte[pixels.Length + 8];

      int newWidth = width * bpp;
      rawData[0] = (byte)(newWidth);
      rawData[1] = (byte)(newWidth >> 8);
      rawData[2] = (byte)(newWidth >> 16);
      rawData[3] = (byte)(newWidth >> 24);

      rawData[4] = (byte)(height);
      rawData[5] = (byte)(height >> 8);
      rawData[6] = (byte)(height >> 16);
      rawData[7] = (byte)(height >> 24);

      //rawData.SetValue(width * bpp, 0);

      Array.Copy(pixels, 0, rawData, 8, pixels.Length);

      return rawData;
    }

    public static void WriteLZWImage(BinaryWriter w, Bitmap image)
    {
      /*w.Write((Int32)image.picture_maxsize);
      w.Write((Int32)image.picture_data_size);
      w.Write(image.rawBackground);*/

      const long LookbackSize = 4096;
      const byte LiteralsSize = 8;

      byte[] rawData = ConvertToRaw(image);
      Literal[] literals = new Literal[LiteralsSize];
      byte literalsCount = 0;

      long i = 0;
      while (i < rawData.Length)
      {
        long lookbackMax = Math.Min(i, LookbackSize);

        long bestRun = 0;
        long bestRunOffset = 0;
        for (long j = 0; j < lookbackMax; ++j)
        {
          long i2 = i;
          long runs = 0;
          for (long k = j; k < lookbackMax; ++k)
          {
            if (rawData[i - k] == rawData[i2++])
              ++runs;
            else
              break;
          }

          if (bestRun < runs)
          {
            bestRun = runs;
            bestRunOffset = j;
          }
        }

        if ((literalsCount == LiteralsSize) || (i == rawData.Length))
        {
          //NOTE(adm244): flush literals buffer

          int mask = 0;
          int literalsMax = (literalsCount < LiteralsSize) ? literalsCount : LiteralsSize;

          //NOTE(adm244): make mask for control byte
          for (int literalIndex = 0; literalIndex < literalsMax; ++literalIndex)
          {
            if (literals[literalIndex].Type == LiteralType.Lookback)
            {
              mask &= (1 << literalIndex);
            }
          }

          w.Write((byte)mask);

          //NOTE(adm244): write literals data
          for (int literalIndex = 0; literalIndex < literalsMax; ++literalIndex)
          {
            if (literals[literalIndex].Type == LiteralType.Lookback)
            {
              UInt16 offset = (UInt16)(literals[literalIndex].Offset & 0x0FFF);
              UInt16 length = (UInt16)((literals[literalIndex].Length - 3) << 12);
              UInt16 runlength = (UInt16)(offset & length);
              w.Write(runlength);
            }
            else
            {
              w.Write((byte)literals[literalIndex].Symbol);
            }
          }

          literalsCount = 0;
        }
        else
        {
          if (bestRun > 2)
          {
            //NOTE(adm244): encode runs
            literals[literalsCount].SetLookback(bestRunOffset, bestRun);
          }
          else
          {
            //NOTE(adm244): encode literals
            literals[literalsCount].SetSymbol(rawData[i]);
          }
          ++literalsCount;
        }

        ++i;
      }
    }
  }
}
