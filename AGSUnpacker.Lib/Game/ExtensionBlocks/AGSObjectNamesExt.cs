using System;
using System.Collections.Generic;

namespace AGSUnpacker.Lib.Game.ExtensionBlocks
{
  public class AGSObjectNamesExt
  {
    public string GameName { get; set; }
    public string SaveGameFolderName { get; set; }

    public List<ObjectNameExt> Characters { get; }
    public List<ObjectNameExt> InventoryItems { get; }
    public List<ObjectNameExt> Cursors { get; }
    public List<ObjectNameExt> AudioClips { get; }

    public AGSObjectNamesExt()
    {
        GameName = string.Empty;
        SaveGameFolderName = string.Empty;

        Characters = new List<ObjectNameExt>();
        InventoryItems = new List<ObjectNameExt>();
        Cursors = new List<ObjectNameExt>();
        AudioClips = new List<ObjectNameExt>();
    }
  }

  public struct ObjectNameExt
  {
    public string ScriptName { get; set; }
    public string RealName { get; set; }
  }
}
