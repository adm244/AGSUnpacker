using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace AGSUnpacker.Graphics.GDI.Extensions
{
  internal static class BitmapInstanceImplExtension
  {
    //private static readonly string PngSignature = "\x89PNG\x0D\x0A\x1F\x0A";

    internal static PixelFormat ReadBitmapPixelFormat(string filepath)
    {
      PixelFormat format = PixelFormat.Undefined;

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

    //internal static PixelFormat ReadPngPixelFormat(string filepath)
    //{
    //  using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
    //  {
    //    using (BinaryReader reader = new BinaryReader(stream))
    //    {
    //      string signature = reader.ReadFixedString(PngSignature.Length);
    //      if (signature != PngSignature)
    //        throw new InvalidDataException($"Invalid png signature: {signature}");
    //
    //      byte[] buffer = PngFindBlock(reader, "IHDR");
    //      if (buffer == null)
    //        return PixelFormat.Undefined;
    //
    //      byte bpp = buffer[8];
    //      byte colorType = buffer[9];
    //
    //      Debug.Assert(bpp == 8);
    //
    //      switch (colorType)
    //      {
    //        // FIXME(adm244): png doesn't support 16-bit bitmaps...
    //        // well... pull the lever, stop the train
    //        // BAD times to be a programmer; a plumber has to deal with less shit
    //        //
    //        // Three options:
    //        //  1) Respect users experience and support 'png' and 'bmp mess' file formats
    //        //  2) Respect yourself and use 'tga'
    //        //  3) Blame everyone and cry
    //        // as you can see below there's no option 1, only 2 and 3
    //
    //        case 2:
    //          return PixelFormat.Format24bppRgb;
    //        case 3:
    //          return PixelFormat.Format8bppIndexed;
    //        case 6:
    //          return PixelFormat.Format32bppArgb;
    //
    //        default:
    //          throw new InvalidDataException();
    //      }
    //    }
    //  }
    //}
    //
    //private static byte[] PngFindBlock(BinaryReader reader, string blockType)
    //{
    //  while (!reader.EOF())
    //  {
    //    int length = reader.ReadInt32BE();
    //    string type = reader.ReadFixedString(sizeof(UInt32));
    //
    //    if (type == "IEND")
    //      break;
    //
    //    if (type == blockType)
    //      return reader.ReadBytes(length);
    //
    //    reader.BaseStream.Seek(length + sizeof(UInt32), SeekOrigin.Current);
    //  }
    //
    //  return null;
    //}

    //internal static ImageCodecInfo GetEncoderInfo(System.Drawing.Imaging.ImageFormat format)
    //{
    //  ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
    //
    //  for (int i = 0; i < encoders.Length; ++i)
    //  {
    //    if (encoders[i].FormatID == format.Guid)
    //      return encoders[i];
    //  }
    //
    //  throw new NotSupportedException();
    //}

    internal static void SaveAsBmp(this BitmapInstanceImpl bitmap, string filepath)
    {
      bitmap.Instance.Save(filepath, ImageFormat.Bmp);
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

      //PropertyItem indexTransparent = bitmap.Instance.GetPropertyItem(0x5104); // PropertyTagIndexTransparent

      // NOTE(adm244): no luck, the GDI+ png support is busted, just read\write file yourself
      // trick below is for gif image only...
      //
      //System.Reflection.ConstructorInfo propertyItemConstructor = typeof(PropertyItem).GetConstructor
      //  (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);
      //PropertyItem property = propertyItemConstructor.Invoke(null) as PropertyItem;
      //
      //property.Id = 0x5104; // PropertyTagIndexTransparent
      //property.Type = 1; // array of bytes
      //property.Len = 1;
      //property.Value = new byte[] { 0 };
      //
      //bitmap.Instance.SetPropertyItem(property);

      // NOTE(adm244): we could just modify a png file after it's been written
      // and add tRNS chunk right before IEND (since we're not networking, order doesn't matter)
      // but there'll be problems with loading it all back (source pixel format goes through a window)
      // so it's EVEN MORE workarounds...
      //
      // a MUCH better solution is to just give up on GDI and implement our custom png reader\writer
      // don't care if it'll be slow, full control is a top priority

      bitmap.Instance.Save(filepath, ImageFormat.Png);
    }
  }
}
