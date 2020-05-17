using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace AGSUnpackerSharp.Shared.Script
{
  public class AGSScriptDecoder
  {
    private AGSScript Script;

    public AGSScriptDecoder(AGSScript script)
    {
      Debug.Assert(script != null);

      Script = script;
    }

    /*public void Decode()
    {
      int pos = 0;
      while (pos < Script.Code.Length)
      {
        int value = Script.Code[pos];
        int instance = (value >> 24);
        AGSOpcodes opcode = (AGSOpcodes)(value & 0x00FFFFFF);

        AGSBinaryInstruction inst = GetInstruction(opcode);
        if (inst == null)
          throw new NotImplementedException("Instruction was not found!");

        
      }
    }

    private AGSBinaryInstruction GetInstruction(AGSOpcodes opcode)
    {
      for (int i = 0; i < AGSVirtualMachine.Instructions.Length; ++i)
      {
        if (AGSVirtualMachine.Instructions[i].Opcode == opcode)
        {
          return AGSVirtualMachine.Instructions[i];
        }
      }

      return null;
    }*/
  }
}
