using System;
using System.IO;
using System.Text;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Extractors.SourceExtractors
{
  public class SourceExtractor262 : SourceExtractor
  {
    private static readonly string EDITORINFO_FILENAME = "editor.dat";
    private static readonly string EDITORINFO_SIGNATURE = "AGSEditorInfo";

    public struct AGSSpritesFolder
    {
      // 2.62 limits
      public static readonly int MAX_FOLDERS = 500;
      public static readonly int MAX_SPRITES = 240;
      public static readonly int MAX_NAME_LENGTH = 30;

      public int SpritesCount;
      public Int16[] Sprites;
      public int ParentIndex;
      public string Name;
    }

    public SourceExtractor262()
      : base()
    {
    }

    public override bool Extract(string sourceFilename)
    {
      //TODO(adm244): implement project source extraction from compiled assets (2.62 -ish)

      //NOTE(adm244): this probably won't be useful, but used for reversing file formats
      string filename = Path.GetFileName(sourceFilename);
      if (filename != EDITORINFO_FILENAME)
        throw new NotSupportedException();

      return ReadEditorInfoFile(sourceFilename);
    }

    private bool ReadEditorInfoFile(string sourceFilename)
    {
      // Read editor.dat file
      using (FileStream inputStream = new FileStream(sourceFilename, FileMode.Open))
      {
        using (BinaryReader inputReader = new BinaryReader(inputStream, Encoding.GetEncoding(1251)))
        {
          // read signature
          string signature = inputReader.ReadCString();
          if (signature != EDITORINFO_SIGNATURE)
            throw new InvalidDataException();

          // read file version
          int version = inputReader.ReadInt32();
          if ((version > 6) || (version < 1))
            throw new InvalidDataException();

          // read global script header
          if (version >= 3)
          {
            int globalScriptHeaderSize = inputReader.ReadInt32();
            string globalScriptHeader = inputReader.ReadCString();
          }

          // read global script
          if (version >= 2)
          {
            int globalScriptSize = inputReader.ReadInt32();
            string globalScript = inputReader.ReadCString();
          }

          // read sprites and folders
          if (version >= 4)
          {
            int foldersCount = inputReader.ReadInt32();
            if (foldersCount > AGSSpritesFolder.MAX_FOLDERS)
              throw new InvalidDataException();

            AGSSpritesFolder[] folders = new AGSSpritesFolder[foldersCount];
            for (int i = 0; i < foldersCount; ++i)
            {
              folders[i] = new AGSSpritesFolder();

              folders[i].SpritesCount = inputReader.ReadInt32();
              folders[i].Sprites = inputReader.ReadArrayInt16(AGSSpritesFolder.MAX_SPRITES);

              // sprite values:
              //  0xFC19 (-999) ignored (2.62)
              //  0xFE0C (-500) has some special meaning (2.62)
              // also other negative numbers...

              folders[i].ParentIndex = inputReader.ReadInt16();
              folders[i].Name = inputReader.ReadFixedCString(AGSSpritesFolder.MAX_NAME_LENGTH);
            }
          }

          // read room descriptions strings
          int roomDescriptionsCount = inputReader.ReadInt32();
          if (roomDescriptionsCount > 300) // 2.62, technically it's 299 but room[0] is reserved
            throw new InvalidDataException();

          string[] roomDescriptions = new string[roomDescriptionsCount];
          for (int i = 0; i < roomDescriptionsCount; ++i)
          {
            roomDescriptions[i] = inputReader.ReadCString();
          }

          // read unknown blob
          if (version >= 5)
          {
            //NOTE(adm244): couldn't find any game that were using this :-(
            int unkCount = inputReader.ReadInt32();
            if (unkCount > 0)
            {
              byte[] unk = inputReader.ReadBytes(unkCount);
            }
          }

          // read plugins data
          if (version >= 6)
          {
            int pluginsDataVersion = inputReader.ReadInt32();
            if (pluginsDataVersion != 1)
              throw new InvalidDataException();

            int pluginsCount = inputReader.ReadInt32();
            for (int i = 0; i < pluginsCount; ++i)
            {
              //NOTE(adm244): 2.62 reads 50.000.000(!) but pluginName's buffer size is 200 (ouch!)
              string pluginName = inputReader.ReadCString();

              //NOTE(adm244): might be just a size of a blob
              int pluginDataOffset = inputReader.ReadInt32();
              if (pluginDataOffset != 0)
                throw new NotImplementedException();

              int pluginDataMagic = inputReader.ReadInt32();
              if (pluginDataMagic != 0x08C216BF)
                throw new InvalidDataException();
            }
          }
        }
      }
      // #Read editor.dat file

      //TODO(adm244): catch error
      return true;
    }
  }
}
