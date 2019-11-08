using System;
using AGSUnpackerSharp.Shared;
using AGSUnpackerSharp.Shared.Interaction;

namespace AGSUnpackerSharp
{
  public class AGSInventoryItem
  {
    public string name;
    public Int32 picture;
    public Int32 cursor_picture;
    public Int32 hotspot_x;
    public Int32 hotspot_y;
    public Int32[] reserved;
    public byte flag;
    public string scriptName;

    public AGSInteraction interactions_old;
    public AGSInteractionScript interactions;
    public AGSPropertyStorage properties;

    public AGSInventoryItem()
    {
      name = string.Empty;
      picture = 0;
      cursor_picture = 0;
      hotspot_x = 0;
      hotspot_y = 0;
      reserved = new Int32[0];
      flag = 0;
      scriptName = string.Empty;

      interactions_old = new AGSInteraction();
      interactions = new AGSInteractionScript();
      properties = new AGSPropertyStorage();
    }

    public void LoadFromStream(AGSAlignedStream ar)
    {
      name = ar.ReadFixedString(25);
      //Debug.Assert(r.BaseStream.Position == 0x851E);

      picture = ar.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8525);

      cursor_picture = ar.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8529);

      hotspot_x = ar.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x852D);

      hotspot_y = ar.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8531);

      reserved = ar.ReadArrayInt32(5);
      //Debug.Assert(r.BaseStream.Position == 0x8545);

      //NOTE(adm244): structure is aligned at 4-byte boundary,
      // read with a padding and discard it
      flag = (byte)ar.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8549);
    }
  }
}
