using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AGSUnpackerSharp.Utils
{
  public enum LiteralType
  {
    Symbol,
    Lookback,
  }

  public struct Element
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
    public static int ToABGR(Color color)
    {
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
    }

    public static Color FromABGR(int abgr)
    {
      int red = (abgr & 0xFF);
      int green = ((abgr >> 8) & 0xFF);
      int blue = ((abgr >> 16) & 0xFF);
      int alpha = ((abgr >> 24) & 0xFF);

      return Color.FromArgb(alpha, red, green, blue);
    }

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

    public static Bitmap ParseAllegroCompressedImage(BinaryReader r)
    {
      Int16 bpp = 1;
      Int16 width = r.ReadInt16();
      Int16 height = r.ReadInt16();

      MemoryStream stream = new MemoryStream();
      for (int y = 0; y < height; ++y)
      {
        int pixelsRead = 0;
        while (pixelsRead < width)
        {
          sbyte index = (sbyte)r.ReadByte();
          if (index == -128) index = 0;

          if (index < 0)
          {
            int count = (1 - index);
            byte value = r.ReadByte();

            while ((count--) > 0)
              stream.WriteByte(value);

            pixelsRead += (1 - index);
          }
          else
          {
            byte[] buffer = r.ReadBytes(index + 1);
            stream.Write(buffer, 0, buffer.Length);
            pixelsRead += (index + 1);
          }
        }
      }

      byte[] data = stream.ToArray();
      Debug.Assert(data.Length == (width * height * bpp));

      byte[] palette = r.ReadBytes(256 * 3);

      return ConvertToBitmap(data, width, height, bpp, palette, true);
    }

    public static void WriteAllegroCompressedImage(BinaryWriter w, Bitmap image)
    {
      w.Write((Int16)image.Width);
      w.Write((Int16)image.Height);

      byte[] pixels = ConvertToRaw(image);
      for (int y = 0; y < image.Height; ++y)
      {
        byte[] pixelsRow = new byte[image.Width];
        Buffer.BlockCopy(pixels, y * image.Width, pixelsRow, 0, image.Width);

        byte[] data = WriteRLEData8(pixelsRow);
        w.Write(data, 0, data.Length);
      }

      byte[] palette = new byte[256 * 3];
      if (image.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        Debug.Assert(image.Palette.Entries.Length == 256);
        for (int i = 0; i < image.Palette.Entries.Length; ++i)
        {
          palette[3 * i + 0] = image.Palette.Entries[i].R;
          palette[3 * i + 1] = image.Palette.Entries[i].G;
          palette[3 * i + 2] = image.Palette.Entries[i].B;
        }
      }

      w.Write(palette);
    }

    public static Bitmap ParseLZ77Image(BinaryReader r, int bpp)
    {
      // skip palette
      //r.BaseStream.Seek(256 * sizeof(Int32), SeekOrigin.Current);
      byte[] palette = r.ReadBytes(256 * sizeof(Int32));

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
      int width = ((output[3] << 24) | (output[2] << 16) | (output[1] << 8) | output[0]) / bpp;
      int height = ((output[7] << 24) | (output[6] << 16) | (output[5] << 8) | output[4]);

      //bitmap.Save("room_background.bmp", ImageFormat.Bmp);

      byte[] pixels = new byte[output.Length - 8];
      Array.Copy(output, 8, pixels, 0, pixels.Length);

      Bitmap bitmap = ConvertToBitmap(pixels, width, height, bpp, palette, false);
      
      //NOTE(adm244): since AGS doesn't use alpha-channel for room background it is nullyfied
      // and we have to get rid of it by converting image into 24-bit pixel format
      return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format24bppRgb);
    }

    private static Bitmap ConvertToBitmap(byte[] data, int width, int height, int bpp, byte[] palette, bool convertTo8bit)
    {
      //TODO(adm244): pick according to bpp
      //PixelFormat format = PixelFormat.Format32bppArgb;
      PixelFormat format = GetPixelFormat(bpp);

      Bitmap bitmap = new Bitmap(width, height, format);
      BitmapData lockData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);

      /*byte[] rawData = new byte[data.Length - 8];
      Array.Copy(data, 8, rawData, 0, rawData.Length);*/

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < height; ++row)
      {
        Marshal.Copy(data, row * width * bpp, p, width * bpp);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }

      bitmap.UnlockBits(lockData);

      if ((bpp == 1) && (palette != null))
      {
        Color[] paletteColors = new Color[256];
        for (int i = 0; i < paletteColors.Length; ++i)
        {
          int red = palette[3 * i];
          int green = palette[3 * i + 1];
          int blue = palette[3 * i + 2];

          if (convertTo8bit)
          {
            //NOTE(adm244): AGS is using only 6-bits per channel, so we have to convert it to full 8-bit range
            blue = (byte)((blue / 64f) * 256f);
            green = (byte)((green / 64f) * 256f);
            red = (byte)((red / 64f) * 256f);
          }

          paletteColors[i] = Color.FromArgb(red, green, blue);
        }

        ColorPalette bitmapPalette = bitmap.Palette;
        for (int j = 0; j < bitmapPalette.Entries.Length; ++j)
        {
          bitmapPalette.Entries[j] = paletteColors[j];
        }
        bitmap.Palette = bitmapPalette;
      }

      return bitmap;
    }

    private static byte[] ConvertToRaw(Bitmap image)
    {
      int width = image.Width;
      int height = image.Height;

      //TODO(adm244): extract from Bitmap
      //int bpp = 4;
      int bpp = GetBytesPerPixel(image);

      byte[] pixels = new byte[width * height * bpp];

      BitmapData lockData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, image.PixelFormat);
      IntPtr p = lockData.Scan0;
      for (int row = 0; row < height; ++row)
      {
        Marshal.Copy(p, pixels, row * width * bpp, width * bpp);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      image.UnlockBits(lockData);

      return pixels;
    }

    private static byte[] AppendImageSize(int width, int height, int bpp, byte[] pixels)
    {
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

    private static PixelFormat GetPixelFormat(int bpp)
    {
      PixelFormat format = PixelFormat.Undefined;
      switch (bpp)
      {
        case 1:
          format = PixelFormat.Format8bppIndexed;
          break;
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
          Debug.Assert(false);
          break;
      }

      return format;
    }

    private static int GetBytesPerPixel(Bitmap image)
    {
      int bpp = -1;
      switch (image.PixelFormat)
      {
        case PixelFormat.Format8bppIndexed:
          bpp = 1;
          break;
        case PixelFormat.Format16bppRgb565:
          bpp = 2;
          break;
        case PixelFormat.Format24bppRgb:
          bpp = 3;
          break;
        case PixelFormat.Format32bppRgb:
        case PixelFormat.Format32bppArgb:
          bpp = 4;
          break;

        default:
          Debug.Assert(false);
          break;
      }

      return bpp;
    }

    public static void WriteLZ77Image(BinaryWriter w, Bitmap image, int bpp)
    {
      /*w.Write((Int32)image.picture_maxsize);
      w.Write((Int32)image.picture_data_size);
      w.Write(image.rawBackground);*/

      //FIX(adm244): 24-bit bitmaps are all messed up !!!

      int image_bpp = GetBytesPerPixel(image);
      if (image_bpp != bpp)
      {
        //NOTE(adm244): convert background image to room background format
        PixelFormat newFormat = GetPixelFormat(bpp);
        image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), newFormat);
        /*Bitmap newImage = new Bitmap(image.Width, image.Height, newFormat);

        BitmapData lockData1 = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height), ImageLockMode.WriteOnly, newFormat);
        BitmapData lockData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, newFormat);

        IntPtr p1 = lockData1.Scan0;
        IntPtr p = lockData.Scan0;
        for (int row = 0; row < image.Height; ++row)
        {
          Marshal.Copy(p, row * image.Width * bpp, p1, image.Width * bpp);
          p = new IntPtr(p.ToInt64() + lockData.Stride);
          p1 = new IntPtr(p1.ToInt64() + lockData1.Stride);
        }
        image.UnlockBits(lockData);
        newImage.UnlockBits(lockData1);*/
      }

      byte[] pixels = ConvertToRaw(image);
      byte[] rawData = AppendImageSize(image.Width, image.Height, bpp, pixels);
      byte[] rawDataCompressed = LZ77Compress(rawData);

      Debug.Assert(pixels.Length == (image.Width * image.Height * bpp));

      byte[] palette = new byte[256 * sizeof(Int32)];
      if (image.Palette.Entries.Length > 0)
      {
        Debug.Assert(image.Palette.Entries.Length == 256);
        for (int i = 0; i < image.Palette.Entries.Length; ++i)
        {
          palette[4 * i + 0] = image.Palette.Entries[i].R;
          palette[4 * i + 1] = image.Palette.Entries[i].G;
          palette[4 * i + 2] = image.Palette.Entries[i].B;
          palette[4 * i + 3] = image.Palette.Entries[i].A;
        }
      }

      w.Write(palette);
      w.Write((UInt32)rawData.Length);
      w.Write((UInt32)rawDataCompressed.Length);
      w.Write(rawDataCompressed);
    }

    private static byte[] LZ77Compress(byte[] data)
    {
      //NOTE(adm244): it performs worse than the original, i.e. low speed, bigger file size
      MemoryStream stream = new MemoryStream(data.Length);

      const long LookbackSize = 4095;
      const byte LookbackSequenceLength = 18;
      const byte ElementsSize = 8;

      Element[] elements = new Element[ElementsSize];
      byte elementsCount = 0;

      long bufferPosition = 0;
      while (bufferPosition < data.Length)
      {
        //NOTE(adm244): PART 1: Find largest matching sequence in a lookback buffer

        //NOTE(adm244): BRUTE-FORCE approach

        //NOTE(adm244): lookback buffer includes current buffer position
        long lookbackLength = Math.Min(bufferPosition, LookbackSize);
        long lookbackStart = bufferPosition - lookbackLength;

        long bestRun = 0;
        long bestRunOffset = 0;
        long lookbackEnd = lookbackStart + lookbackLength;
        for (long lookbackPosition = lookbackStart; lookbackPosition < lookbackEnd; )
        {
          //NOTE(adm244): allow lookback buffer to go beyond current buffer position
          long sequenceLength = 0;
          long lookbackSubEnd = (lookbackPosition + LookbackSequenceLength);
          for (long lookbackSubPosition = lookbackPosition; lookbackSubPosition < lookbackSubEnd; ++lookbackSubPosition)
          {
            long bufferSubPosition = bufferPosition + (lookbackSubPosition - lookbackPosition);
            if (bufferSubPosition == data.Length)
              break;

            if (data[lookbackSubPosition] == data[bufferSubPosition])
              ++sequenceLength;
            else
              break;
          }

          if (bestRun < sequenceLength)
          {
            bestRun = sequenceLength;
            bestRunOffset = bufferPosition - lookbackPosition - 1;
          }

          if (sequenceLength > 0)
            lookbackPosition += sequenceLength;
          else
            lookbackPosition += 1;
        }

        //NOTE(adm244): PART 2: Encode either LOOKBACK (RUN) or LITERAL

        if (bestRun >= 3)
        {
          //NOTE(adm244): encode looback (run)
          elements[elementsCount].SetLookback(bestRunOffset, bestRun);
          elementsCount += 1;

          bufferPosition += bestRun;
        }
        else
        {
          //NOTE(adm244): encode literal
          elements[elementsCount].SetSymbol(data[bufferPosition]);
          elementsCount += 1;

          bufferPosition += 1;
        }

        //NOTE(adm244): PART 3: Flush elements buffer if it's full or we're at the end of a stream

        if ((elementsCount == ElementsSize) || (bufferPosition == data.Length))
        {
          int controlMask = 0;
          int elementsLength = (elementsCount < ElementsSize) ? elementsCount : ElementsSize;

          //NOTE(adm244): make mask for control byte
          for (int i = 0; i < elementsLength; ++i)
          {
            if (elements[i].Type == LiteralType.Lookback)
              controlMask |= (1 << i);
          }

          Debug.Assert(controlMask == (int)((byte)(controlMask)));
          stream.WriteByte((byte)controlMask);

          //NOTE(adm244): write elements
          for (int i = 0; i < elementsLength; ++i)
          {
            if (elements[i].Type == LiteralType.Lookback)
            {
              UInt16 offset = (UInt16)(elements[i].Offset & 0x0FFF);
              Debug.Assert(elements[i].Offset == (long)offset);

              byte length = (byte)(elements[i].Length - 3);
              Debug.Assert(elements[i].Length == (long)((length + 3)));
              Debug.Assert((length >= 0) && (length <= 15));

              UInt16 runlength = (UInt16)(offset | (length << 12));
              stream.WriteByte((byte)(runlength & 0xFF));
              stream.WriteByte((byte)((runlength >> 8) & 0xFF));
            }
            else
            {
              stream.WriteByte((byte)elements[i].Symbol);
            }
          }

          elementsCount = 0;
        }
      }

      return stream.ToArray();
    }
  }
}
