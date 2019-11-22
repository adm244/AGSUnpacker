using System.Drawing;

namespace AGSUnpackerGUI
{
  internal class RoomFrame
  {
    public Bitmap Image;
    public string Name;

    public RoomFrame(Bitmap image, string name)
    {
      Image = image;
      Name = name;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
