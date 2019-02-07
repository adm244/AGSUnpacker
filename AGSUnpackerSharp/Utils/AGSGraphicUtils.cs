using System;
using System.IO;

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

    public static void ParseLZWImage(BinaryReader r)
    {
      // skip palette
      r.BaseStream.Seek(256 * sizeof(Int32), SeekOrigin.Current);
      Int32 picture_maxsize = r.ReadInt32();
      Int32 picture_data_size = r.ReadInt32();

      //TODO(adm244): parse background data

      // skip lzw compressed image
      //r.BaseStream.Seek(picture_data_size, SeekOrigin.Current);
      byte[] rawBackground = r.ReadBytes(picture_data_size);

      /*FileStream fs = new FileStream("room_background", FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, System.Text.Encoding.GetEncoding(1252));
      w.Write((UInt32)picture_maxsize);
      w.Write((UInt32)picture_data_size);
      w.Write(rawBackground);
      w.Close();*/
    }
  }
}
