using System;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Graphics
{
  public class SpritesMeta
  {
    private static readonly string SPRITESET_META_FILENAME = "meta.bin";

    private static readonly Int16 DEFAULT_VERSION = 6;
    private static readonly byte DEFUALT_COMPRESSION = 0;
    private static readonly UInt32 DEFAULT_FILEID = 0xDEADBEEF;

    public Int16 Version { get; set; }
    public byte Compression { get; set; }
    public UInt32 FileID { get; set; }

    public SpritesMeta()
    {
      Version = DEFAULT_VERSION;
      Compression = DEFUALT_COMPRESSION;
      FileID = DEFAULT_FILEID;
    }

    public SpritesMeta(Int16 version, byte compression, UInt32 fileID)
    {
      Version = version;
      Compression = compression;
      FileID = fileID;
    }

    public void WriteMetaFile()
    {
      FileStream fs = new FileStream(SPRITESET_META_FILENAME, FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

      w.Write(Version);
      w.Write(Compression);
      w.Write(FileID);

      w.Close();
    }

    public void ReadMetaFile()
    {
      FileStream fs = new FileStream(SPRITESET_META_FILENAME, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      Version = r.ReadInt16();
      Compression = r.ReadByte();
      FileID = r.ReadUInt32();

      r.Close();
    }
  }
}
