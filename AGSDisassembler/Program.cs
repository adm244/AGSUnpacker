using AGSUnpackerSharp.Room;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Shared.Script;

namespace AGSDisassembler
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      string filepath = args[0];

      //AGSGameData dta = new AGSGameData();
      //dta.LoadFromFile(filepath);
      AGSRoom room = new AGSRoom();
      room.LoadFromFile(filepath);

      DumpScript(filepath + ".o", room.script);

      Disassembler.Disassemble(room.script);
    }

    private static void DumpScript(string filepath, AGSScript script)
    {
      using (FileStream fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (BinaryWriter outputWriter = new BinaryWriter(fileStream, Encoding.GetEncoding(1252)))
        {
          script.WriteToStream(outputWriter, script.Version);
        }
      }
    }
  }
}
