using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib
{
  public class AGSAlignedStream
  {
    private readonly byte BASE_ALIGNMENT = 2;

    private BinaryReader r;
    private int _curPos;

    public long Position
    {
      get { return r.BaseStream.Position; }
    }

    public AGSAlignedStream(BinaryReader r)
    {
      this.r = r;
      _curPos = 0;
    }

    public void Reset()
    {
      _curPos = 0;
    }

    private void SkipPadding(byte aligment)
    {
      if (aligment == 0) return;

      if (aligment % BASE_ALIGNMENT == 0)
      {
        int padding = _curPos % aligment;
        if (padding > 0)
        {
          int bytesToSkip = aligment - padding;
          r.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
          _curPos += bytesToSkip;
        }

        if (_curPos % 8 == 0)
        {
          _curPos = 0;
        }
      }
    }

    public byte[] ReadBytes(int count)
    {
      SkipPadding(sizeof(byte));
      byte[] rawData = r.ReadBytes(count);
      _curPos += rawData.Length;

      return rawData;
    }

    public string ReadFixedString(int length)
    {
      SkipPadding(sizeof(byte));
      string rawData = r.ReadFixedCString(length);
      _curPos += length;

      return rawData;
    }

    public Int16[] ReadArrayInt16(int count)
    {
      SkipPadding(sizeof(Int16));
      Int16[] rawData = new Int16[count];
      for (int i = 0; i < count; ++i)
      {
        rawData[i] = r.ReadInt16();
        _curPos += sizeof(Int16);
      }

      return rawData;
    }

    public Int32[] ReadArrayInt32(int count)
    {
      SkipPadding(sizeof(Int32));
      Int32[] rawData = new Int32[count];
      for (int i = 0; i < count; ++i)
      {
        rawData[i] = r.ReadInt32();
        _curPos += sizeof(Int32);
      }

      return rawData;
    }

    public byte ReadByte()
    {
      SkipPadding(sizeof(byte));
      byte value = r.ReadByte();
      _curPos += sizeof(byte);

      return value;
    }

    public Int16 ReadInt16()
    {
      SkipPadding(sizeof(Int16));
      Int16 value = r.ReadInt16();
      _curPos += sizeof(Int16);

      return value;
    }

    public Int32 ReadInt32()
    {
      SkipPadding(sizeof(Int32));
      Int32 value = r.ReadInt32();
      _curPos += sizeof(Int32);

      return value;
    }
  }
}
