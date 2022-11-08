using System;

using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Lib.Shared.Interaction;

namespace AGSUnpacker.Lib
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
      picture = ar.ReadInt32();
      cursor_picture = ar.ReadInt32();
      hotspot_x = ar.ReadInt32();
      hotspot_y = ar.ReadInt32();
      reserved = ar.ReadArrayInt32(5);

      // FIXME(adm244): this is probably bugged... double check
      //NOTE(adm244): structure is aligned at 4-byte boundary,
      // read with a padding and discard it
      flag = (byte)ar.ReadInt32();
    }

    public void WriteToStream(AGSAlignedStream aw)
    {
      aw.WriteFixedString(name, 25);
      aw.WriteInt32(picture);
      aw.WriteInt32(cursor_picture);
      aw.WriteInt32(hotspot_x);
      aw.WriteInt32(hotspot_y);
      aw.WriteArrayInt32(new Int32[5]);

      // FIXME(adm244): this is probably bugged... double check
      aw.WriteInt32(flag);
    }
  }
}
