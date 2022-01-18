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
    public Bitmap[] Frames;
    public byte[] PaletteShareFlags;

    public Bitmap WalkableAreasMask;
    public Bitmap WalkbehindAreasMask;
    public Bitmap HotspotsMask;
    public Bitmap RegionsMask;

    public Bitmap MainBackground
    {
      get { return Frames[0]; }
      set { Frames[0] = value; }
    }

    public AGSRoomBackground()
    {
      BytesPerPixel = 1;
      AnimationSpeed = 4;
      Frames = new Bitmap[5];
      PaletteShareFlags = new byte[5];

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

      //Frames = new Bitmap[framesCount];
      Debug.Assert(Frames.Length >= framesCount);

      for (int i = 1; i < framesCount; ++i)
        Frames[i] = AGSGraphics.ReadLZ77Image(reader, BytesPerPixel);
    }

    public void WriteBlock(BinaryWriter writer, int roomVersion)
    {
      writer.Write((byte)Frames.Length);
      writer.Write((byte)AnimationSpeed);

      if (roomVersion >= 20) // ???
        writer.Write((byte[])PaletteShareFlags);

      for (int i = 1; i < Frames.Length; ++i)
        AGSGraphics.WriteLZ77Image(writer, Frames[i], BytesPerPixel);
    }
  }
}
