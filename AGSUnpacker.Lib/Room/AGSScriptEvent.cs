using System;
using System.IO;

namespace AGSUnpacker.Lib.Room
{
  public class AGSScriptEvent
  {
    public EventType Type { get; private set; }
    public int Param1 { get; private set; }
    public int Param2 { get; private set; }
    public int Param3 { get; private set; }
    public int NextBlock { get; private set; }

    // NOTE(adm244): these are used only by room editor
    public byte ExecType { get; private set; } // sort
    public int OffsetY { get; private set; } // screeny

    private AGSScriptEvent()
    {
      Type = EventType.NOP;
      Param1 = 0;
      Param2 = 0;
      Param3 = 0;
      NextBlock = 0;

      ExecType = 0;
      OffsetY = 0;
    }

    static public AGSScriptEvent ReadFromStream(BinaryReader reader)
    {
      AGSScriptEvent scriptEvent = new AGSScriptEvent();

      int type = reader.ReadInt32();
      if (!Enum.IsDefined(typeof(EventType), type))
        throw new InvalidDataException($"Unknown graphical script block type: {type}");

      scriptEvent.Type = (EventType)type;
      scriptEvent.ExecType = reader.ReadByte();
      scriptEvent.Param1 = reader.ReadInt32();
      scriptEvent.Param2 = reader.ReadInt32();
      scriptEvent.Param3 = reader.ReadInt32();
      scriptEvent.NextBlock = reader.ReadInt32();
      scriptEvent.OffsetY = reader.ReadInt32();

      return scriptEvent;
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)Type);
      writer.Write((byte)ExecType);
      writer.Write((Int32)Param1);
      writer.Write((Int32)Param2);
      writer.Write((Int32)Param3);
      writer.Write((Int32)NextBlock);
      writer.Write((Int32)OffsetY);
    }

    /* NOTE(adm244): events exec types:
     *
     *  [0]   0, 2, 2, 2
     *  [4]   2, 2, 2, 2
     *  [8]   2, 2, 2, 2
     *  [12]  2, 2, 1, 1
     *  [16]  2, 2, 2, 1
     *  [20]  2, 1, 1, 2
     *  [24]  1, 2, 1, 0
     *
     * 0: null
     * 1: recursive event (runs another script event)
     * 2: normal event
     */

    public enum EventType
    {
      NOP                     = 0,
      NewRoom                 = 1,  // "Go to screen %d"
      GiveScore               = 2,  // "Give score %d"
      StopPlayer              = 3,  // "Stop man walking"
      LoseGame                = 4,  // "Lose game"
      RunAnimation            = 5,  // "Run animation %d"
      DisplayMessage          = 6,  // "Display message %d"
      ObjectOff               = 7,  // "Remove object %d"
      RunDialog               = 8,  // "Run dialog %d"
      AddInventory            = 9,  // "Add object %d to inventory"
      RunTextScript           = 10, // "Run text script %d"
      SetFlag                 = 11, // "Set flag %s"
      ClearFlag               = 12, // "Clear flag %s"
      StopScript              = 13, // "Stop script"
      RunScriptIfFlagClear    = 14, // "If flag %s is clear"
      RunScriptIfFlagSet      = 15, // "If flag %s is set"
      PlaySound               = 16, // "Play sound effect %d"
      PlayFLICAnimation       = 17, // "Play FLI/FLC animation %d"
      ObjectOn                = 18, // "Turn object %d on"
      RunScriptIfHasItem      = 19, // "If player has inv %d"
      LoseInventory           = 20, // "Lose inventory %d"
      RunScriptEveryNLoop     = 21, // "Every %d loops"
      RunScriptRandom1toN     = 22, // "Random chance 1 in %d"
      SetTimer                = 23, // "Set timer to %d loops"
      RunScriptIfTimerExpired = 24, // "If timer expired"
      MoveToObject            = 25, // "Move man to object %d"
      RunScriptIfItemWasUsed  = 26, // "If inventory %d was used"
    }
  }
}
