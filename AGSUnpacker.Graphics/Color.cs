namespace AGSUnpacker.Graphics
{
  public struct Color
  {
    public Color(byte r, byte g, byte b)
      : this(r, g, b, 255)
    {
    }

    public Color(byte r, byte g, byte b, byte a)
    {
      R = r;
      G = g;
      B = b;
      A = a;
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public int ToRgba32()
    {
      return (A << 24) | (B << 16) | (G << 8) | R;
    }

    public static Color FromRgba32(int rgba32)
    {
      byte red   = (byte)((rgba32 >>  0) & 0xFF);
      byte green = (byte)((rgba32 >>  8) & 0xFF);
      byte blue  = (byte)((rgba32 >> 16) & 0xFF);
      byte alpha = (byte)((rgba32 >> 24) & 0xFF);

      return new Color(red, green, blue, alpha);
    }
  }
}
