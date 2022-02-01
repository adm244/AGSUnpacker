using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using AGSUnpacker.Graphics;
using AGSUnpacker.Lib.Graphics;

namespace AGSUnpacker.Lib.Room
{
  public class AGSRoomBackground
  {
    public int BytesPerPixel;
    public int AnimationSpeed;
    public IList<Bitmap> Frames;
    public byte[] PaletteShareFlags;

    public Bitmap WalkableAreasMask;
    public Bitmap WalkbehindAreasMask;
    public Bitmap HotspotsMask;
    public Bitmap RegionsMask;

    public Bitmap MainBackground => Frames[0];

    public AGSRoomBackground()
    {
      BytesPerPixel = 1;
      AnimationSpeed = 4;
      Frames = new List<Bitmap>(5);
      PaletteShareFlags = new byte[0];

      WalkableAreasMask = null;
      WalkbehindAreasMask = null;
      HotspotsMask = null;
      RegionsMask = null;
    }

    //NOTE(adm244): make sure that this block is read AFTER the main block,
    // since main block stores BytesPerPixel value and it is required here
    // to read the image data correctly
    public void ReadBlock(BinaryReader reader, int roomVersion)
    {
      byte framesCount = reader.ReadByte();
      AnimationSpeed = reader.ReadByte();

      if (roomVersion >= 20)
        PaletteShareFlags = reader.ReadBytes(framesCount);

      for (int i = 1; i < framesCount; ++i)
        Frames.Add(AGSGraphics.ReadLZ77Image(reader, BytesPerPixel));

      Debug.Assert(Frames.Count >= framesCount);
    }

    public void WriteBlock(BinaryWriter writer, int roomVersion)
    {
      Debug.Assert(PaletteShareFlags.Length == Frames.Count);

      writer.Write((byte)Frames.Count);
      writer.Write((byte)AnimationSpeed);

      if (roomVersion >= 20) // ???
        writer.Write((byte[])PaletteShareFlags);

      for (int i = 1; i < Frames.Count; ++i)
        AGSGraphics.WriteLZ77Image(writer, Frames[i], BytesPerPixel);
    }
  }
}
