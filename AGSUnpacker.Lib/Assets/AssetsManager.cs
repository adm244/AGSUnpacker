using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Utils.Encryption;

namespace AGSUnpacker.Lib.Assets
{
  public class AssetsManager
  {
    private static readonly string SignatureHead = "CLIB\x1a";
    private static readonly string SignatureTail = "CLIB\x1\x2\x3\x4SIGE";

    private static readonly Int32 EncryptionSeedSalt = 9338638;

    private Encoding FileEncoding = Encoding.Latin1;

    private string RootFilename;
    private string RootFolder;
    public CLibFile[] Files { get; private set; }

    private string RootFile
    {
      get { return Path.Combine(RootFolder, RootFilename); }
    }

    private AssetsManager()
    {
      RootFilename = string.Empty;
      RootFolder = string.Empty;
      Files = new CLibFile[0];
    }

    public static AssetsManager Create(string filePath)
    {
      AssetsManager manager = new AssetsManager();

      manager.RootFolder = Path.GetDirectoryName(filePath);

      if (manager.TryReadMainCLibFile(filePath))
        return manager;

      if (manager.TryReadMainCLibFile(Path.Combine(manager.RootFolder, "ac2game.dat")))
        return manager;

      if (manager.TryReadMainCLibFile(Path.Combine(manager.RootFolder, "ac2game.ags")))
        return manager;

      return null;
    }

    //TODO(adm244): implement packing

    public void Extract(string folderPath)
    {
      for (int i = 0; i < Files.Length; ++i)
      {
        string fileFolderPath = Path.Combine(folderPath, Files[i].Filename);
        Directory.CreateDirectory(fileFolderPath);

        string libFilePath = Path.Combine(RootFolder, Files[i].Filename);

        //NOTE(adm244): if doesn't exist try the root file
        if (!File.Exists(libFilePath))
          libFilePath = RootFile;

        //NOTE(adm244): if STILL doesn't exist -- throw up
        if (!File.Exists(libFilePath))
          throw new InvalidDataException("CLib file does NOT exist!");

        using (FileStream stream = new FileStream(libFilePath, FileMode.Open, FileAccess.Read))
        {
          using (BinaryReader reader = new BinaryReader(stream, FileEncoding))
          {
            //TODO(adm244): verify signature?

            for (int j = 0; j < Files[i].Assets.Count; ++j)
            {
              reader.BaseStream.Seek(Files[i].Assets[j].Offset + Files[i].Offset, SeekOrigin.Begin);

              string filePath = Path.Combine(fileFolderPath, Files[i].Assets[j].Filename);
              using (FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
              {
                using (BinaryWriter outputWriter = new BinaryWriter(outputStream, FileEncoding))
                {
                  long bytesToRead = Files[i].Assets[j].Size;
                  while (bytesToRead > 0)
                  {
                    //NOTE(adm244): maybe try a bigger I/O buffer size
                    long bytesRead = Math.Min(bytesToRead, 65536);
                    byte[] buffer = reader.ReadBytes((int)bytesRead);
                    bytesToRead -= bytesRead;

                    outputWriter.Write(buffer);
                  }
                }
              }
            }
          }
        }
      }
    }

    private bool TryReadMainCLibFile(string filePath)
    {
      RootFilename = Path.GetFileName(filePath);
      Files = null;

      // FIXME(adm244): check if file exists (!)

      using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
      {
        using (BinaryReader reader = new BinaryReader(stream, FileEncoding))
        {
          long offset = 0;

          string signatureHead = reader.ReadFixedCString(SignatureHead.Length);
          if (signatureHead != SignatureHead)
          {
            if (!FindAppendedCLIBFile(reader, out offset))
              return false;
          }

          Files = ReadCLib(reader);

          //HACK(adm244): quick 'n dirty appended clib support
          // assumes that the first entry is the appended clib
          Files[0].Offset = offset;
        }
      }

      return (Files != null);
    }

    private CLibFile[] ReadCLib(BinaryReader reader)
    {
      CLibFile[] files = new CLibFile[0];

      byte version = reader.ReadByte();

      //TODO(adm244): legacy\community engine supports only these, add support for other versions
      if ((version != 6) && (version != 10)
       && (version != 11) && (version != 15)
       && (version != 20) && (version != 21)
       && (version != 30))
        throw new NotImplementedException("CLib version is not supported!");

      if (version >= 10) // multi-file
      {
        byte index = reader.ReadByte();
        if (index != 0)
          throw new InvalidDataException("CLib file index is not 0!\nAre you trying to read a wrong CLib file?");

        if (version >= 30)
          files = ReadCLib30(reader);
        else if (version >= 21)
          files = ReadCLib21(reader);
        else
          files = ReadCLibPre21(reader, version);
      }
      else
        files = ReadCLibPre10(reader, version);

      return files;
    }

    private CLibFile[] ReadCLib30(BinaryReader reader)
    {
      Int32 seed = reader.ReadInt32();

      //NOTE(adm244): assuming asset files are stored sequentially
      Int32 filesCount = reader.ReadInt32();
      CLibFile[] files = new CLibFile[filesCount];
      for (int i = 0; i < files.Length; ++i)
      {
        files[i] = new CLibFile();
        files[i].Filename = reader.ReadCString();
      }

      Int32 assetsCount = reader.ReadInt32();
      for (int i = 0; i < assetsCount; ++i)
      {
        CLibAsset asset = new CLibAsset();

        asset.Filename = reader.ReadCString();
        byte index = reader.ReadByte();
        asset.Offset = reader.ReadInt64();
        asset.Size = reader.ReadInt64();

        if (index >= files.Length)
          throw new InvalidDataException("CLib file index is incorrect!");

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
        files[i] = new CLibFile();

      for (int i = 0; i < files.Length; ++i)
        files[i].Filename = encoder.ReadString(reader);

      Int32 assetsCount = encoder.ReadInt32(reader);
      AGSCLibAsset[] assets = new AGSCLibAsset[assetsCount];
      for (int i = 0; i < assets.Length; ++i)
        assets[i] = new AGSCLibAsset();

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
        files[i] = new CLibFile();
        if (version == 20)
          files[i].Filename = reader.ReadCString(50);
        else
          files[i].Filename = reader.ReadFixedCString(20);
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
          assets[i].Filename = reader.ReadFixedCString(25);
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
      files[0] = new CLibFile();
      files[0].Filename = RootFilename;

      byte salt = reader.ReadByte();
      byte reserved = reader.ReadByte();

      Int16 assetsCount = reader.ReadInt16();
      AGSCLibAsset[] assets = new AGSCLibAsset[assetsCount];

      //TODO(adm244): figure out how it's used in pre 6, ignored for now
      byte[] password = reader.ReadBytes(13);

      for (int i = 0; i < assets.Length; ++i)
      {
        string filename = reader.ReadFixedCString(13);
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
        asset.Offset = assets[i].Offset;
        asset.Size = assets[i].Size;

        int index = assets[i].AssetFileIndex;
        files[index].Assets.Add(asset);
      }

      return files;
    }

    private bool FindAppendedCLIBFile(BinaryReader reader, out long offset)
    {
      reader.BaseStream.Seek(-(SignatureTail.Length), SeekOrigin.End);
      offset = 0;

      string signatureTail = reader.ReadFixedCString(SignatureTail.Length);
      if (signatureTail != SignatureTail)
        return false;

      reader.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt32)), SeekOrigin.End);
      UInt32 offset32 = reader.ReadUInt32();

      reader.BaseStream.Seek(-(SignatureTail.Length + sizeof(UInt64)), SeekOrigin.End);
      UInt64 offset64 = reader.ReadUInt64();

      //NOTE(adm244): .NET is a setup for a failure
      reader.BaseStream.Seek((Int64)offset64, SeekOrigin.Begin);
      offset = (long)offset64;

      string signatureHead = reader.ReadFixedCString(SignatureHead.Length);
      if (signatureHead == SignatureHead)
        return true;

      reader.BaseStream.Seek(offset32, SeekOrigin.Begin);
      offset = offset32;

      signatureHead = reader.ReadFixedCString(SignatureHead.Length);
      if (signatureHead == SignatureHead)
        return true;

      offset = 0;
      return false;
    }

    private class AGSCLibAsset
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

    public class CLibAsset
    {
      public string Filename;
      public long Offset;
      public long Size;

      public CLibAsset()
      {
        Filename = string.Empty;
        Offset = 0;
        Size = 0;
      }
    }

    public class CLibFile
    {
      public string Filename;
      public long Offset;
      public List<CLibAsset> Assets;

      public CLibFile()
      {
        Filename = string.Empty;
        Offset = 0;
        Assets = new List<CLibAsset>();
      }
    }
  }
}
