using System;
using System.IO;

namespace AGSUnpacker.Lib
{
  public class AGSEncoder
  {
    private readonly UInt32 MaxDataFileLen = 50;

    private Int32 prevValue;

    public AGSEncoder(Int32 startValue)
    {
      prevValue = startValue;
    }

    private Int32 GetNextPseudoRand()
    {
      prevValue = (Int32)((long)prevValue * 214013L + 2531011L);
      return (prevValue >> 16) & 0x7fff;
    }

    public byte[] ReadArray(BinaryReader r, int count)
    {
      byte[] buffer = r.ReadBytes(count);
      for (int i = 0; i < buffer.Length; ++i)
      {
        buffer[i] = (byte)(buffer[i] - GetNextPseudoRand());
      }

      return buffer;
    }

    public byte ReadInt8(BinaryReader r)
    {
      return (byte)(r.ReadByte() - GetNextPseudoRand());
    }

    public Int32 ReadInt32(BinaryReader r)
    {
      byte[] bytes = ReadArray(r, sizeof(Int32));
      return (Int32)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
    }

    public Int64 ReadInt64(BinaryReader r)
    {
      byte[] bytes = ReadArray(r, sizeof(Int64));
      Int32 bottom = (Int32)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
      Int32 top = (Int32)((bytes[7] << 24) | (bytes[6] << 16) | (bytes[5] << 8) | bytes[4]);
      return (Int64)((top << 32) | bottom);
    }

    public string ReadString(BinaryReader r)
    {
      char[] buffer = new char[MaxDataFileLen];

      int i = 0;
      for (; i < buffer.Length; ++i)
      {
        buffer[i] = (char)((byte)(r.ReadByte() - GetNextPseudoRand()));
        if (buffer[i] == 0) break;
      }

      return new string(buffer, 0, i);
    }
  }
}
