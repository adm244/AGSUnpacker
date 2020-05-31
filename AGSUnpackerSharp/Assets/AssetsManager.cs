using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Utils.Encryption;

namespace AGSUnpackerSharp.Assets
{
  public class AssetsManager
  {
    private static readonly string SignatureHead = "CLIB\x1a";
    private static readonly string SignatureTail = "CLIB\x1\x2\x3\x4SIGE";

    private static readonly Int32 EncryptionSeedSalt = 9338638;

    private Encoding FileEncoding = Encoding.GetEncoding(1252);

    private string RootFilename;
    private string RootFolder;
    private CLibFile[] Files;

    private AssetsManager()
    {
      RootFilename = string.Empty;
      RootFolder = string.Empty;
      Files = null;
    }

    public static AssetsManager Create(string filePath)
    {
      AssetsManager manager = new AssetsManager();

      manager.RootFolder = Path.GetDirectoryName(filePath);

      if (manager.ReadMainCLibFile(filePath))
        return manager;

      if (manager.ReadMainCLibFile(Path.Combine(manager.RootFolder, "ac2game.dat")))
        return manager;

      if (manager.ReadMainCLibFile(Path.Combine(manager.RootFolder, "ac2game.ags")))
        return manager;

      return null;
    }

    //TODO(adm244): implement unpacking
    //TODO(adm244): implement packing

    //FIX(adm244): rename to BuildAssetsList and make it return a List<Asset>
    private bool ReadMainCLibFile(string filePath)
    {
      RootFilename = Path.GetFileName(filePath);
      Files = null;

      using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
      {
        using (BinaryReader reader = new BinaryReader(stream, FileEncoding))
        {
          string signatureHead = reader.ReadFixedString(SignatureHead.Length);
          if (signatureHead != SignatureHead)
          {
            if (!FindAppendedCLIBFile(reader))
              return false;
          }

          byte version = reader.ReadByte();

          //TODO(adm244): legacy\community engine supports only these, add support for other versions
          if ((version != 6) || (version != 10)
           || (version != 11) || (version != 15)
           || (version != 20) || (version != 21)
           || (version != 30))
            return false;

          if (version >= 10) // multi-file
          {
            byte index = reader.ReadByte();
            if (index != 0)
              return false;

            if (version >= 30)
              Files = ReadCLib30(reader);
            else if (version >= 21)
              Files = ReadCLib21(reader);
            else
              Files = ReadCLibPre21(reader, version);
          }
          else
            Files = ReadCLibPre10(reader, version);
        }
      }

      return (Files != null);
    }

    private CLibFile[] ReadCLib30(BinaryReader reader)
    {
      Int32 seed = reader.ReadInt32();

      //NOTE(adm244): assuming asset files are stored sequentially
      Int32 filesCount = reader.ReadInt32();
      CLibFile[] files = new CLibFile[filesCount];
      for (int i = 0; i < files.Length; ++i)
        files[i].Filename = reader.ReadCString();

      Int32 assetsCount = reader.ReadInt32();
      for (int i = 0; i < assetsCount; ++i)
      {
        CLibAsset asset = new CLibAsset();

        asset.Filename = reader.ReadCString();
        byte index = reader.ReadByte();
        asset.Offset = (ulong)reader.ReadInt64();
        asset.Size = (ulong)reader.ReadInt64();

        if (index < 0)
          return null;
        if (index >= files.Length)
          return null;

        files[index].Assets.Add(asset);
      }

      return files;
    }

    private CLibFile[] ReadCLib21(BinaryReader reader)
    {
      Int32 seed = reader.ReadInt32() + EncryptionSeedSalt;
      AGSEncoder encoder = new AGSEncoder(seed);

      //NOTE(adm244): assuming asset files are stored sequentially
      Int32 filesCount = encoder.ReadInt32(reader);
      CLibFile[] files = new CLibFile[filesCount];
      for (int i = 0; i < files.Length; ++i)
        files[i].Filename = encoder.ReadString(reader);

      Int32 assetsCount = encoder.ReadInt32(reader);
      AGSCLibAsset[] assets = new AGSCLibAsset[assetsCount];

      for (int i = 0; i < assetsCount; ++i)
        assets[i].Filename = encoder.ReadString(reader);

      for (int i = 0; i < assetsCount; ++i)
        assets[i].Offset = encoder.ReadInt32(reader);

      for (int i = 0; i < assetsCount; ++i)
        assets[i].Size = encoder.ReadInt32(reader);

      for (int i = 0; i < assetsCount; ++i)
        assets[i].AssetFileIndex = encoder.ReadInt8(reader);

      return BuildAssetsLists(assets, ref files);
    }

    //private CLibFile[] ReadCLib20(BinaryReader reader)
    //{
    //  //NOTE(adm244): assuming asset files are stored sequentially
    //  Int32 filesCount = reader.ReadInt32();
    //  CLibFile[] files = new CLibFile[filesCount];
    //  for (int i = 0; i < files.Length; ++i)
    //    files[i].Filename = reader.ReadCString(50);
    //
    //  Int32 assetsCount = reader.ReadInt32();
    //  AGSCLibAsset[] assets = new AGSCLibAsset[assetsCount];
    //
    //  for (int i = 0; i < assets.Length; ++i)
    //  {
    //    Int16 length = reader.ReadInt16();
    //    length /= 5;
    //
    //    byte[] jibzler = reader.ReadBytes(length);
    //    assets[i].Filename = AGSEncryption.DecryptJibzle(jibzler);
    //  }
    //
    //  for (int i = 0; i < assets.Length; ++i)
    //    assets[i].Offset = reader.ReadInt32();
    //
    //  for (int i = 0; i < assets.Length; ++i)
    //    assets[i].Size = reader.ReadInt32();
    //
    //  for (int i = 0; i < assets.Length; ++i)
    //    assets[i].AssetFileIndex = reader.ReadByte();
    //
    //  return BuildAssetsLists(assets, ref files);
    //}

    private CLibFile[] ReadCLibPre21(BinaryReader reader, int version)
    {
      Int32 filesCount = reader.ReadInt32();
      CLibFile[] files = new CLibFile[filesCount];
      for (int i = 0; i < files.Length; ++i)
      {
        if (version == 20)
          files[i].Filename = reader.ReadCString(50);
        else
          files[i].Filename = reader.ReadFixedString(20);
      }

      Int32 asset_count = reader.ReadInt32();
      AGSCLibAsset[] assets = new AGSCLibAsset[asset_count];

      for (int i = 0; i < assets.Length; ++i)
      {
        if (version == 20)
        {
          Int16 length = reader.ReadInt16();
          length /= 5;

          byte[] jibzler = reader.ReadBytes(length);
          assets[i].Filename = AGSEncryption.DecryptJibzle(jibzler);
        }
        else if (version >= 11)
        {
          byte[] jibzler = reader.ReadBytes(25);
          assets[i].Filename = AGSEncryption.DecryptJibzle(jibzler);
        }
        else
          assets[i].Filename = reader.ReadFixedString(25);
      }

      for (int i = 0; i < assets.Length; ++i)
        assets[i].Offset = reader.ReadInt32();

      for (int i = 0; i < assets.Length; ++i)
        assets[i].Size = reader.ReadInt32();

      for (int i = 0; i < assets.Length; ++i)
        assets[i].AssetFileIndex = reader.ReadByte();

      return BuildAssetsLists(assets, ref files);
    }

    private CLibFile[] ReadCLibPre10(BinaryReader reader, int version)
    {
      CLibFile[] files = new CLibFile[1];
      files[0].Filename = RootFilename;

      byte salt = reader.ReadByte();
      byte reserved = reader.ReadByte();

      Int16 assetsCount = reader.ReadInt16();
      AGSCLibAsset[] assets = new AGSCLibAsset[assetsCount];

      //TODO(adm244): figure out how it's used in pre 6, ignored for now
      byte[] password = reader.ReadBytes(13);

      for (int i = 0; i < assets.Length; ++i)
      {
        string filename = reader.ReadFixedString(13);
        assets[i].Filename = AGSEncryption.DecryptSalt(filename, salt);
      }

      for (int i = 0; i < assetsCount; ++i)
        assets[i].Size = reader.ReadInt32();

      //TODO(adm244): read "flags and ratio", skipping for now
      reader.BaseStream.Seek(assetsCount * sizeof(Int16), SeekOrigin.Current);

      assets[0].Offset = reader.BaseStream.Position;
      for (int i = 1; i < assets.Length; ++i)
        assets[i].Offset = assets[i - 1].Offset + assets[i - 1].Size;

      return BuildAssetsLists(assets, ref files);
    }

    private CLibFile[] BuildAssetsLists(AGSCLibAsset[] assets, ref CLibFile[] files)
    {
      for (int i = 0; i < assets.Length; ++i)
      {
        if (assets[i].AssetFileIndex < 0)
          return null;
        if (assets[i].AssetFileIndex >= files.Length)
          return null;

        CLibAsset asset = new CLibAsset();
        asset.Filename = assets[i].Filename;
        asset.Offset = (ulong)assets[i].Offset;
        asset.Size = (ulong)assets[i].Size;

        int index = assets[i].AssetFileIndex;
        files[index].Assets.Add(asset);
      }

      return files;
    }

    private bool FindAppendedCLIBFile(BinaryReader reader)
    {
      reader.BaseStream.Seek(-(SignatureTail.Length), SeekOrigin.End);

      string signatureTail = reader.ReadFixedString(SignatureTail.Length);
      if (signatureTail != SignatureTail)
        return false;

      reader.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt32)), SeekOrigin.End);
      UInt32 offset32 = reader.ReadUInt32();

      reader.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt64)), SeekOrigin.End);
      UInt64 offset64 = reader.ReadUInt64();

      //NOTE(adm244): .NET is a setup for a failure
      reader.BaseStream.Seek((Int64)offset64, SeekOrigin.Begin);

      string signatureHead = reader.ReadFixedString(SignatureHead.Length);
      if (signatureHead == SignatureHead)
        return true;

      reader.BaseStream.Seek(offset32, SeekOrigin.Begin);

      signatureHead = reader.ReadFixedString(SignatureHead.Length);
      if (signatureHead == SignatureHead)
        return true;

      return false;
    }

    private struct AGSCLibAsset
    {
      public string Filename;
      public Int64 Offset;
      public Int64 Size;
      public byte AssetFileIndex;

      public AGSCLibAsset()
      {
        Filename = string.Empty;
        Offset = 0;
        Size = 0;
        AssetFileIndex = 0;
      }
    }

    private struct CLibAsset
    {
      public string Filename;
      public ulong Offset;
      public ulong Size;

      public CLibAsset()
      {
        Filename = string.Empty;
        Offset = 0;
        Size = 0;
      }
    }

    private class CLibFile
    {
      public string Filename;
      public List<CLibAsset> Assets;

      public CLibFile()
      {
        Filename = string.Empty;
        Assets = null;
      }
    }
  }
}
