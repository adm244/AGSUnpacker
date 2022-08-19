using System;
using System.Diagnostics;
using System.IO;

namespace AGSUnpacker.Lib.Utils
{
  internal static class AGSCompression
  {
    internal static byte[] ReadRLE8(BinaryReader reader, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      while (positionImage < sizeUncompressed)
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          byte value = reader.ReadByte();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage] = value;
            ++positionImage;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            buffer[positionImage] = reader.ReadByte();
            ++positionImage;
          }
        }
      }

      return buffer;
    }

    internal static byte[] ReadRLE16(BinaryReader reader, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      while (positionImage < sizeUncompressed)
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          UInt16 value = reader.ReadUInt16();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            positionImage += 2;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt16 value = reader.ReadUInt16();
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            positionImage += 2;
          }
        }
      }

      return buffer;
    }

    internal static byte[] ReadRLE32(BinaryReader reader, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      while (positionImage < sizeUncompressed)
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          UInt32 value = reader.ReadUInt32();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            buffer[positionImage + 2] = (byte)(value >> 16);
            buffer[positionImage + 3] = (byte)(value >> 24);
            positionImage += 4;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt32 value = reader.ReadUInt32();
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            buffer[positionImage + 2] = (byte)(value >> 16);
            buffer[positionImage + 3] = (byte)(value >> 24);
            positionImage += 4;
          }
        }
      }

      return buffer;
    }

    internal static byte[] WriteRLE8(byte[] buffer)
    {
      const int maxRuns = 128;
      byte[] bufferCompressed = new byte[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length;)
        {
          byte value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              stream.Write(bufferCompressed, 0, bufferPosition);
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    internal static byte[] WriteRLE16(UInt16[] buffer)
    {
      const int maxRuns = 128;
      UInt16[] bufferCompressed = new UInt16[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length;)
        {
          UInt16 value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              for (int j = 0; j < bufferPosition; ++j)
              {
                stream.WriteByte((byte)(bufferCompressed[j]));
                stream.WriteByte((byte)(bufferCompressed[j] >> 8));
              }
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            stream.WriteByte((byte)(value >> 8));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    internal static byte[] WriteRLE32(UInt32[] buffer)
    {
      const int maxRuns = 128;
      UInt32[] bufferCompressed = new UInt32[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length;)
        {
          UInt32 value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              for (int j = 0; j < bufferPosition; ++j)
              {
                stream.WriteByte((byte)(bufferCompressed[j]));
                stream.WriteByte((byte)(bufferCompressed[j] >> 8));
                stream.WriteByte((byte)(bufferCompressed[j] >> 16));
                stream.WriteByte((byte)(bufferCompressed[j] >> 24));
              }
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    internal static byte[] WriteRLE8Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          byte[] row = new byte[width];
          Buffer.BlockCopy(buffer, y * width, row, 0, width);

          byte[] bufferCompressed = AGSCompression.WriteRLE8(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    internal static byte[] WriteRLE16Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          UInt16[] row = new UInt16[width];
          Buffer.BlockCopy(buffer, y * width * 2, row, 0, width * 2);

          byte[] bufferCompressed = AGSCompression.WriteRLE16(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    internal static byte[] WriteRLE32Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          UInt32[] row = new UInt32[width];
          Buffer.BlockCopy(buffer, y * width * 4, row, 0, width * 4);

          byte[] bufferCompressed = AGSCompression.WriteRLE32(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    internal static byte[] ReadLZ77(BinaryReader reader, long sizeUncompressed)
    {
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

      byte[] output = new byte[sizeUncompressed];
      long output_pos = 0;

      while (output_pos < sizeUncompressed)
      {
        byte control = reader.ReadByte();
        for (int mask = 1; (mask & 0xFF) != 0; mask <<= 1)
        {
          if ((control & mask) == 0)
          {
            output[output_pos] = reader.ReadByte();
            output_pos++;
          }
          else
          {
            UInt16 runlength = reader.ReadUInt16();
            UInt16 sequence_start_offset = (UInt16)(runlength & 0x0FFF);
            byte sequence_length = (byte)((runlength >> 12) + 3);

            long sequence_pos = output_pos - sequence_start_offset - 1;
            for (int i = 0; i < sequence_length; ++i)
            {
              output[output_pos] = output[sequence_pos];
              output_pos++;
              sequence_pos++;
            }
          }

          if (output_pos >= sizeUncompressed)
            break;
        }
      }

      return output;
    }

    // NOTE(adm244): deprecated
    //internal static byte[] ReadAllegro(BinaryReader reader, int width, int height)
    //{
    //  // TODO(adm244): see if we can use ReadRLE8() here
    //  using (MemoryStream stream = new MemoryStream())
    //  {
    //    for (int y = 0; y < height; ++y)
    //    {
    //      int pixelsRead = 0;
    //      while (pixelsRead < width)
    //      {
    //        sbyte index = (sbyte)reader.ReadByte();
    //        if (index == -128) index = 0;
    //
    //        if (index < 0)
    //        {
    //          int count = (1 - index);
    //          byte value = reader.ReadByte();
    //
    //          while ((count--) > 0)
    //            stream.WriteByte(value);
    //
    //          pixelsRead += (1 - index);
    //        }
    //        else
    //        {
    //          byte[] buffer = reader.ReadBytes(index + 1);
    //          stream.Write(buffer, 0, buffer.Length);
    //          pixelsRead += (index + 1);
    //        }
    //      }
    //    }
    //
    //    return stream.ToArray();
    //  }
    //}

    // TODO(adm244): rename to something like "WriteRLE8Chuncked"?
    internal static void WriteAllegro(BinaryWriter writer, byte[] buffer, int width, int height)
    {
      for (int y = 0; y < height; ++y)
      {
        byte[] row = new byte[width];
        Buffer.BlockCopy(buffer, y * width, row, 0, width);

        byte[] bufferCompressed = WriteRLE8(row);
        writer.Write(bufferCompressed, 0, bufferCompressed.Length);
      }
    }

    //NOTE(adm244): it performs worse than the original (i.e. low speed) but
    // the file size is actually smaller since we don't interrupt the sequence
    //TODO(adm244): write a faster implementation
    internal static byte[] WriteLZ77(byte[] buffer)
    {
      using (MemoryStream stream = new MemoryStream(buffer.Length))
      {
        const long LookbackSize = 4095;
        const byte LookbackSequenceLength = 18;
        const byte ElementsSize = 8;

        Element[] elements = new Element[ElementsSize];
        byte elementsCount = 0;

        long bufferPosition = 0;
        while (bufferPosition < buffer.Length)
        {
          //NOTE(adm244): PART 1: Find largest matching sequence in a lookback buffer

          //NOTE(adm244): BRUTE-FORCE approach

          //NOTE(adm244): lookback buffer includes current buffer position
          long lookbackLength = Math.Min(bufferPosition, LookbackSize);
          long lookbackStart = bufferPosition - lookbackLength;

          long bestRun = 0;
          long bestRunOffset = 0;
          long lookbackEnd = lookbackStart + lookbackLength;
          for (long lookbackPosition = lookbackStart; lookbackPosition < lookbackEnd;)
          {
            //NOTE(adm244): allow lookback buffer to go beyond current buffer position
            long sequenceLength = 0;
            long lookbackSubEnd = (lookbackPosition + LookbackSequenceLength);
            for (long lookbackSubPosition = lookbackPosition; lookbackSubPosition < lookbackSubEnd; ++lookbackSubPosition)
            {
              long bufferSubPosition = bufferPosition + (lookbackSubPosition - lookbackPosition);
              if (bufferSubPosition == buffer.Length)
                break;

              if (buffer[lookbackSubPosition] == buffer[bufferSubPosition])
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
            elements[elementsCount].SetSymbol(buffer[bufferPosition]);
            elementsCount += 1;

            bufferPosition += 1;
          }

          //NOTE(adm244): PART 3: Flush elements buffer if it's full or we're at the end of a stream

          if ((elementsCount == ElementsSize) || (bufferPosition == buffer.Length))
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
                byte length = (byte)(elements[i].Length - 3);

                Debug.Assert(elements[i].Offset == (long)(offset));
                Debug.Assert(elements[i].Length == (long)((length + 3)));

                Debug.Assert((length >= 0) && (length <= 15));

                UInt16 runlength = (UInt16)(offset | (length << 12));
                stream.WriteByte((byte)((runlength >> 0) & 0xFF));
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

    private enum LiteralType
    {
      Symbol,
      Lookback,
    }

    private struct Element
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
  }
}
