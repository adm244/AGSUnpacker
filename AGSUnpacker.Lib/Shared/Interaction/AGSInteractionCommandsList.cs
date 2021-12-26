using System;
using System.IO;

namespace AGSUnpacker.Shared.Interaction
{
  public class AGSInteractionCommandsList
  {
    public AGSInteractionCommand[] commands;
    public Int32 type;
    public Int32 times_run;

    public AGSInteractionCommandsList()
    {
      commands = new AGSInteractionCommand[0];
      type = 0;
      times_run = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 commands_count = r.ReadInt32();
      times_run = r.ReadInt32();

      commands = new AGSInteractionCommand[commands_count];
      for (int i = 0; i < commands.Length; ++i)
      {
        commands[i] = new AGSInteractionCommand();
        commands[i].LoadFromStream(r);
      }

      for (int i = 0; i < commands.Length; ++i)
      {
        if (commands[i].children != null)
        {
          commands[i].children = new AGSInteractionCommandsList();
          commands[i].children.LoadFromStream(r);
        }
        commands[i].parent = this;
      }
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.Write((Int32)commands.Length);
      w.Write((Int32)times_run);

      for (int i = 0; i < commands.Length; ++i)
        commands[i].WriteToStream(w);

      for (int i = 0; i < commands.Length; ++i)
      {
        if (commands[i].children != null)
          commands[i].children.WriteToStream(w);
      }
    }
  }
}
