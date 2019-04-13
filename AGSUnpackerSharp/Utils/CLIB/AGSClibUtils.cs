using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  public static class AGSClibUtils
  {
    private static readonly string CLIB_HEAD_SIGNATURE = "CLIB\x1a";
    private static readonly string CLIB_TAIL_SIGNATURE = "CLIB\x1\x2\x3\x4SIGE";

    private static readonly Int32 EncryptionRandSeed = 9338638;

    public static string[] UnpackAGSAssetFiles(string agsfile)
    {
      FileStream fs = new FileStream(agsfile, FileMode.Open, FileAccess.Read, FileShare.Read);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      Console.Write("Parsing {0}...", agsfile);
      AGSAssetInfo[] assetInfos = ParseAGSAssetInfos(r);
      Console.WriteLine(" Done!");

      Console.WriteLine("Extracting data files...");
      string[] filenames = ExtractAGSAssetFiles(r, assetInfos, "Data");
      Console.WriteLine("Done!");

      r.Close();

      return filenames;
    }

    private static AGSAssetInfo[] ParseAGSAssetInfos(BinaryReader r)
    {
      // verify tail signature
      r.BaseStream.Seek(-CLIB_TAIL_SIGNATURE.Length, SeekOrigin.End);
      char[] tail_sig = r.ReadChars(CLIB_TAIL_SIGNATURE.Length);
      string tail_sig_string = new string(tail_sig);
      Debug.Assert(CLIB_TAIL_SIGNATURE == tail_sig_string);

      // get clib offset
      r.BaseStream.Seek(-(CLIB_TAIL_SIGNATURE.Length + 4), SeekOrigin.End);
      UInt32 clib_offset = r.ReadUInt32();
      r.BaseStream.Seek(clib_offset, SeekOrigin.Begin);
      Debug.Assert(r.BaseStream.Position == clib_offset);

      // verify clib signature
      char[] head_sig = r.ReadChars(CLIB_HEAD_SIGNATURE.Length);
      string head_sig_string = new string(head_sig);
      Debug.Assert(CLIB_HEAD_SIGNATURE == head_sig_string);

      // parse clib
      byte clib_version = r.ReadByte();
      Debug.Assert(clib_version == 0x15);

      byte asset_index = r.ReadByte();
      Debug.Assert(asset_index == 0);

      Int32 rand_val = r.ReadInt32() + EncryptionRandSeed;

      AGSEncoder encoder = new AGSEncoder(rand_val);
      Int32 files_count = encoder.ReadInt32(r);

      string[] lib_filenames = new string[files_count];
      for (int i = 0; i < files_count; ++i)
      {
        lib_filenames[i] = encoder.ReadString(r);
      }

      Int32 asset_count = encoder.ReadInt32(r);
      AGSAssetInfo[] assetInfos = new AGSAssetInfo[asset_count];
      for (int i = 0; i < asset_count; ++i)
      {
        assetInfos[i].Filename = encoder.ReadString(r);
      }
      for (int i = 0; i < asset_count; ++i)
      {
        assetInfos[i].Offset = encoder.ReadInt32(r) + (Int32)clib_offset;
      }
      for (int i = 0; i < asset_count; ++i)
      {
        assetInfos[i].Size = encoder.ReadInt32(r);
      }
      for (int i = 0; i < asset_count; ++i)
      {
        assetInfos[i].UId = encoder.ReadInt8(r);
      }

      return assetInfos;
    }

    //TODO(adm244): support for multifile packaging
    private static string[] ExtractAGSAssetFiles(BinaryReader r, AGSAssetInfo[] assetInfos, string targetpath)
    {
      string[] filenames = new string[assetInfos.Length];

      string dirpath = Path.Combine(Environment.CurrentDirectory, targetpath);
      if (!Directory.Exists(dirpath))
      {
        Directory.CreateDirectory(dirpath);
      }

      for (int i = 0; i < assetInfos.Length; ++i)
      {
        string filepath = Path.Combine(dirpath, assetInfos[i].Filename);
        filenames[i] = filepath;

        FileStream fs = new FileStream(filepath, FileMode.Create);
        BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

        r.BaseStream.Seek(assetInfos[i].Offset, SeekOrigin.Begin);

        Console.Write("\tExtracting {0}...", assetInfos[i].Filename);

        // 1048576 bytes = 1 mb
        byte[] buffer = new byte[1048576];
        int bytesRead = 0;
        while (bytesRead < assetInfos[i].Size)
        {
          int bytesLeftToRead = assetInfos[i].Size - bytesRead;
          int bytesToRead = Math.Min(buffer.Length, bytesLeftToRead);
          buffer = r.ReadBytes(bytesToRead);

          //NOTE(adm244): check for end-of-stream
          Debug.Assert(buffer.Length > 0);

          w.Write(buffer);

          bytesRead += bytesToRead;
        }

        w.Close();

        Console.WriteLine(" Done!");
      }

      return filenames;
    }
  }
}
