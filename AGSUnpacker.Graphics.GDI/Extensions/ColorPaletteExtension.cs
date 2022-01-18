namespace AGSUnpacker.Graphics.GDI.Extensions
{
  internal static class ColorPaletteExtension
  {
    internal static Palette ToAGSPalette(this System.Drawing.Imaging.ColorPalette palette)
    {
      Color[] entries = new Color[palette.Entries.Length];

      for (int i = 0; i < entries.Length; ++i)
      {
        System.Drawing.Color color = palette.Entries[i];
        entries[i] = new Color(color.R, color.G, color.B, color.A);
      }

      return new Palette(entries);
    }
  }
}
