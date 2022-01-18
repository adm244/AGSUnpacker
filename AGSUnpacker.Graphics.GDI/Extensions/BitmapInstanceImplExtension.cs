using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace AGSUnpacker.Graphics.GDI.Extensions
{
  internal static class BitmapInstanceImplExtension
  {
    internal static System.Drawing.Imaging.PixelFormat ReadBitmapPixelFormat(string filepath)
    {
      System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Undefined;

      using (FileStream file = new FileStream(filepath, FileMode.Open))
      {
        //NOTE(adm244): is there any reason for us to use Latin1 encoding here specifically?
        using (BinaryReader reader = new BinaryReader(file, Encoding.Latin1))
        {
          reader.BaseStream.Seek(28, SeekOrigin.Begin);
          UInt16 bitCount = reader.ReadUInt16();

          format = BitmapGDIExtension.GetPixelFormat(bitCount / 8);
        }
      }

      return format;
    }

    internal static ImageCodecInfo GetEncoderInfo(System.Drawing.Imaging.ImageFormat format)
    {
      ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

      for (int i = 0; i < encoders.Length; ++i)
      {
        if (encoders[i].FormatID == format.Guid)
          return encoders[i];
      }

      throw new NotSupportedException();
    }

    internal static void SaveAsPng(this BitmapInstanceImpl bitmap, string filepath)
    {
      // NOTE(adm244): looks like we won't be able to modify result png file structure...
      // we need that to support tRNS chunk
      //
      //ImageCodecInfo encoder = GetEncoderInfo(System.Drawing.Imaging.ImageFormat.Png);
      //EncoderParameters encoderParams = bitmap.Instance.GetEncoderParameterList(encoder.Clsid);
      //
      //bitmap.Instance.Save(filepath, encoder, encoderParams);

      bitmap.Instance.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
    }
  }
}
