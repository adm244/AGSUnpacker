using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Utils.Encryption;

namespace AGSUnpackerSharp.Utils
{
  public static class AGSClibUtils
  {
    private static readonly string SignatureHead = "CLIB\x1a";
    private static readonly string SignatureTail = "CLIB\x1\x2\x3\x4SIGE";

    private static readonly Int32 EncryptionRandSeed = 9338638;

    public static string[] UnpackAGSAssetFiles(string filePath, string targetFolder)
    {
      using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.GetEncoding(1252)))
        {
          Console.Write("Parsing {0}...", filePath);
          AGSAssetInfo[] assetInfos = ParseAGSAssetInfos(reader);
          Console.WriteLine(" Done!");
      
          Console.WriteLine("Extracting data files...");
          string[] filenames = ExtractAGSAssetFiles(reader, assetInfos, targetFolder);
          Console.WriteLine("Done!");
      
          return filenames;
        }
      }
    }

    public static AGSAssetInfo[] ParseAGSAssetInfos(BinaryReader r)
    {
      // verify tail signature
      r.BaseStream.Seek(-SignatureTail.Length, SeekOrigin.End);
      char[] tail_sig = r.ReadChars(SignatureTail.Length);
      string tail_sig_string = new string(tail_sig);
      Debug.Assert(SignatureTail == tail_sig_string);

      // get clib offset
      r.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt32)), SeekOrigin.End);
      UInt32 clib_offset = r.ReadUInt32();

      r.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt64)), SeekOrigin.End);
      UInt64 clib_offset_64 = r.ReadUInt64();

      r.BaseStream.Seek((long)clib_offset_64, SeekOrigin.Begin);
      //Debug.Assert(r.BaseStream.Position == clib_offset);

      // verify clib signature
      char[] head_sig = r.ReadChars(SignatureHead.Length);
      string head_sig_string = new string(head_sig);
      //Debug.Assert(CLIB_HEAD_SIGNATURE == head_sig_string);

      // using old 32-bit offset
      if (SignatureHead != head_sig_string)
      {
        r.BaseStream.Seek(clib_offset, SeekOrigin.Begin);
        head_sig = r.ReadChars(SignatureHead.Length);
        head_sig_string = new string(head_sig);
        Debug.Assert(SignatureHead == head_sig_string);
      }

      // parse clib
      //TODO(adm244): list all released CLIB versions
      byte clib_version = r.ReadByte();
      //Debug.Assert(clib_version == 0x15);

      byte asset_index = r.ReadByte();
      Debug.Assert(asset_index == 0);

      AGSAssetInfo[] assetInfos = new AGSAssetInfo[0];
      if (clib_version < 0x1E) // pre 30
      {
        if (clib_version < 0x15) // pre 21, 3.1.2
        {
          if (clib_version == 0x14) // 20, 3.1
          {
            Int32 files_count = r.ReadInt32();
            string[] lib_filenames = new string[files_count];
            for (int i = 0; i < lib_filenames.Length; ++i)
              lib_filenames[i] = r.ReadCString(50);

            Int32 asset_count = r.ReadInt32();
            assetInfos = new AGSAssetInfo[asset_count];
            for (int i = 0; i < assetInfos.Length; ++i)
            {
              Int16 length = r.ReadInt16();
              length /= 5;
              byte[] jibzler = r.ReadBytes(length);
              assetInfos[i].Filename = AGSEncryption.DecryptJibzle(jibzler);
            }
          }
          else // 2.72 and older
          {
            Int32 files_count = r.ReadInt32();
            string[] lib_filenames = new string[files_count];
            for (int i = 0; i < lib_filenames.Length; ++i)
              lib_filenames[i] = r.ReadFixedCString(20);

            Int32 asset_count = r.ReadInt32();
            assetInfos = new AGSAssetInfo[asset_count];
            for (int i = 0; i < assetInfos.Length; ++i)
            {
              if (clib_version < 11) // unk version
                assetInfos[i].Filename = r.ReadFixedCString(25);
              else
              {
                byte[] jibzler = r.ReadBytes(25);
                assetInfos[i].Filename = AGSEncryption.DecryptJibzle(jibzler);
              }
            }
          }

          //NOTE(adm244): this file format is quite a jibzler, eh?

          for (int i = 0; i < assetInfos.Length; ++i)
            assetInfos[i].Offset = r.ReadInt32() + (Int32)clib_offset;
          for (int i = 0; i < assetInfos.Length; ++i)
            assetInfos[i].Size = r.ReadInt32();
          for (int i = 0; i < assetInfos.Length; ++i)
            assetInfos[i].UId = r.ReadByte();
        }
        else
        {
          Int32 rand_val = r.ReadInt32() + EncryptionRandSeed;

          AGSEncoder encoder = new AGSEncoder(rand_val);
          Int32 files_count = encoder.ReadInt32(r);

          string[] lib_filenames = new string[files_count];
          for (int i = 0; i < files_count; ++i)
          {
            lib_filenames[i] = encoder.ReadString(r);
          }

          Int32 asset_count = encoder.ReadInt32(r);
          assetInfos = new AGSAssetInfo[asset_count];
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
        }
      }
      else // 30+
      {
        Int32 rand_val = r.ReadInt32();
        
        Int32 files_count = r.ReadInt32();
        string[] lib_filenames = new string[files_count];
        for (int i = 0; i < files_count; ++i)
        {
          lib_filenames[i] = r.ReadCString();
        }

        Int32 asset_count = r.ReadInt32();
        assetInfos = new AGSAssetInfo[asset_count];
        for (int i = 0; i < asset_count; ++i)
        {
          assetInfos[i].Filename = r.ReadCString();
          assetInfos[i].UId = r.ReadByte();
          assetInfos[i].Offset = r.ReadInt64() + (Int64)clib_offset_64;
          assetInfos[i].Size = r.ReadInt64();
        }
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
        long bytesRead = 0;
        while (bytesRead < assetInfos[i].Size)
        {
          long bytesLeftToRead = assetInfos[i].Size - bytesRead;
          long bytesToRead = Math.Min(buffer.Length, bytesLeftToRead);

          // make sure buffer length is within 32-bit boundary
          buffer = r.ReadBytes((int)bytesToRead);

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
