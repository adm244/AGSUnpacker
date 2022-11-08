using System;
using System.IO;
using System.Security.AccessControl;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib
{
  // !!!
  // FIXME(adm244): I don't trust this entire aligment thing here;
  // DO make sure it's working correctly, because it doesn't seem so
  // !!!

  // FIXME(adm244): implement a proper IDisposable I guess...
  public class AGSAlignedStream
  {
    private readonly byte BASE_ALIGNMENT = 2;

    private BinaryReader r;
    private BinaryWriter w;

    private bool _isWriting;
    private int _curPos;

    public long Position
    {
      get { return r.BaseStream.Position; }
    }

    public AGSAlignedStream(BinaryReader r)
    {
      this.r = r;
      w = null;

      _isWriting = false;
      _curPos = 0;
    }

    public AGSAlignedStream(BinaryWriter w)
    {
      r = null;
      this.w = w;

      _isWriting = true;
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

          if (_isWriting)
            w.Write(new byte[bytesToSkip]);
          else
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
      if (_isWriting)
        throw new InvalidOperationException();

      // FIXME(adm244): I doubt this padding is correct, double check!
      SkipPadding(sizeof(byte));
      byte[] rawData = r.ReadBytes(count);
      _curPos += rawData.Length;

      return rawData;
    }

    public void WriteBytes(byte[] value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      // FIXME(adm244): I doubt this padding is correct, double check!
      SkipPadding(sizeof(byte));
      w.Write((byte[])value);
      _curPos += value.Length;
    }

    public string ReadFixedString(int length)
    {
      if (_isWriting)
        throw new InvalidOperationException();

      // FIXME(adm244): I doubt this padding is correct, double check!
      SkipPadding(sizeof(byte));
      // FIXME(adm244): is this supposed to be null-terminated or not?
      string rawData = r.ReadFixedCString(length);
      _curPos += length;

      return rawData;
    }

    public void WriteFixedString(string value, int length)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      // FIXME(adm244): I doubt this padding is correct, double check!
      SkipPadding(sizeof(byte));
      w.WriteFixedString(value, length);
      _curPos += length;
    }

    public Int16[] ReadArrayInt16(int count)
    {
      if (_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int16));
      Int16[] rawData = new Int16[count];
      for (int i = 0; i < count; ++i)
      {
        rawData[i] = r.ReadInt16();
        _curPos += sizeof(Int16);
      }

      return rawData;
    }

    public void WriteArrayInt16(Int16[] value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int16));

      for (int i = 0; i < value.Length; ++i)
        w.Write((Int16)value[i]);

      _curPos += value.Length * sizeof(Int16);
    }

    public Int32[] ReadArrayInt32(int count)
    {
      if (_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int32));
      Int32[] rawData = new Int32[count];
      for (int i = 0; i < count; ++i)
      {
        rawData[i] = r.ReadInt32();
        _curPos += sizeof(Int32);
      }

      return rawData;
    }

    public void WriteArrayInt32(Int32[] value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int32));

      for (int i = 0; i < value.Length; ++i)
        w.Write((Int32)value[i]);

      _curPos += value.Length * sizeof(Int32);
    }

    public byte ReadByte()
    {
      if (_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(byte));
      byte value = r.ReadByte();
      _curPos += sizeof(byte);

      return value;
    }

    public void WriteByte(byte value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(byte));
      w.Write((byte)value);
      _curPos += sizeof(byte);
    }

    public Int16 ReadInt16()
    {
      if (_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int16));
      Int16 value = r.ReadInt16();
      _curPos += sizeof(Int16);

      return value;
    }

    public void WriteInt16(Int16 value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int16));
      w.Write((Int16)value);
      _curPos += sizeof(Int16);
    }

    public Int32 ReadInt32()
    {
      if (_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int32));
      Int32 value = r.ReadInt32();
      _curPos += sizeof(Int32);

      return value;
    }

    public void WriteInt32(Int32 value)
    {
      if (!_isWriting)
        throw new InvalidOperationException();

      SkipPadding(sizeof(Int32));
      w.Write((Int32)value);
      _curPos += sizeof(Int32);
    }
  }
}
