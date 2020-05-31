using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared.Script;

namespace AGSUnpackerSharp.Disassembler
{
  //public class AGSFunction
  //{
  //  public AGSFunction(AGSScript script, string name, int startPosition)
  //  {
  //
  //  }
  //}

  //public class AGSInstruction
  //{
  //
  //}
  //
  //public class AGSInstructionReader
  //{
  //  public AGSScript Script { private set; get; }
  //  public long Position { private set; get; }
  //
  //  public AGSInstructionReader(AGSScript script)
  //  {
  //    Script = script;
  //    Position = 0;
  //  }
  //
  //  public bool EOF()
  //  {
  //    return Position >= Script.Code.Length;
  //  }
  //
  //  public AGSInstruction Read()
  //  {
  //    int instanceID = ((Script.Code[Position] >> 24) & 0xFF);
  //    int opcode = (Script.Code[Position] & 0x00FFFFFF);
  //
  //    // ...
  //  }
  //}
  //
  //public static class AGSDisassembler
  //{
  //  public static void Disassemble(AGSScript script)
  //  {
  //    List<AGSInstruction> instructions = new List<AGSInstruction>();
  //
  //    AGSInstructionReader reader = new AGSInstructionReader(script);
  //
  //    while (!reader.EOF())
  //    {
  //      AGSInstruction instruction = reader.Read();
  //      instructions.Add(instruction);
  //    }
  //
  //    //List<AGSFunction> functions = GetExportedFunctions(script);
  //  }
  //
  //  //private static List<AGSFunction> GetExportedFunctions(AGSScript script)
  //  //{
  //  //  List<AGSFunction> functions = new List<AGSFunction>(script.Exports.Length);
  //  //
  //  //  for (int i = 0; i < script.Exports.Length; ++i)
  //  //  {
  //  //    if (script.Exports[i].Type == AGSScriptExport.ExportType.Function)
  //  //    {
  //  //      string name = script.Exports[i].Name;
  //  //      int position = script.Exports[i].Pointer;
  //  //
  //  //      functions.Add(new AGSFunction(script, name, position));
  //  //    }
  //  //  }
  //  //
  //  //  return functions;
  //  //}
  //}
}
