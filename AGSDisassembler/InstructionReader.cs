using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using AGSUnpackerSharp.Shared.Script;

namespace AGSDisassembler
{
  public class InstructionReader
  {
    private AGSScript Script;
    private Hashtable FixupsTable;
    private Hashtable ExportedFunctions;

    public InstructionReader(AGSScript script)
    {
      Script = script;
      Position = 0;

      FixupsTable = new Hashtable(script.Fixups.Length);
      for (int i = 0; i < script.Fixups.Length; ++i)
      {
        FixupsTable.Add(script.Fixups[i].Offset, script.Fixups[i].Type);
      }

      ExportedFunctions = new Hashtable(script.Exports.Length);
      for (int i = 0; i < script.Exports.Length; ++i)
      {
        if (script.Exports[i].Type == AGSScriptExport.ExportType.Function)
        {
          ExportedFunction func = new ExportedFunction(script.Exports[i].Pointer, script.Exports[i].Name);
          ExportedFunctions.Add(func.Offset, func);
        }
      }
    }

    public Instruction ReadInstruction()
    {
      UInt32 value = (UInt32)Script.Code[Position];

      //NOTE(adm244): do we care about instanceIDs?
      // can't seem to find out what they are used for...
      int instanceID = (int)((value >> 24) & 0xFF);
      int opcode = (int)(value & 0x00FFFFFF);

      if (((opcode >= VM.Instructions.Length) || (opcode < 0))
        || !Enum.IsDefined(typeof(Opcodes), opcode))
        throw new InvalidDataException("Invalid opcode!");

      BinaryInstruction binaryInstruction = VM.Instructions[opcode];

      int instructionLength = (Position + 1 + binaryInstruction.Arguments.Length);
      if (instructionLength > Script.Code.Length)
        throw new InvalidDataException("Attempting to read arguments out of bounds!");

      Instruction instruction = new Instruction();
      instruction.Address = Position;
      instruction.Opcode = (Opcodes)opcode;

      instruction.Arguments = new Argument[binaryInstruction.Arguments.Length];
      for (int i = 0; i < instruction.Arguments.Length; ++i)
      {
        int argPosition = (Position + i + 1);
        Int32 argValue = Script.Code[argPosition];

        instruction.Arguments[i] = new Argument();

        if (binaryInstruction.Arguments[i] == BinaryArgumentType.Register)
        {
          instruction.Arguments[i].Type = ArgumentType.Register;
          instruction.Arguments[i].Value = argValue;
        }
        else
        {
          object fixupType = FixupsTable[argPosition];
          if (fixupType == null)
          {
            instruction.Arguments[i].Type = ArgumentType.Literal;
            instruction.Arguments[i].Value = argValue;
          }
          else
          {
            instruction.Arguments[i].Value = argValue;

            switch ((AGSFixupType)fixupType)
            {
              case AGSFixupType.GlobalData:
                {
                  instruction.Arguments[i].Type = ArgumentType.GlobalData;
                  //instruction.Arguments[i].IValue = ;
                } break;

              case AGSFixupType.Function:
                {
                  instruction.Arguments[i].Type = ArgumentType.Function;
                  instruction.Arguments[i].Value = ExportedFunctions[argValue];
                } break;

              case AGSFixupType.String:
                {
                  instruction.Arguments[i].Type = ArgumentType.String;
                  instruction.Arguments[i].Value = (string)Script.StringsTable[argValue];
                } break;

              case AGSFixupType.Import:
                {
                  instruction.Arguments[i].Type = ArgumentType.Import;
                } break;

              case AGSFixupType.DataData:
                {
                  instruction.Arguments[i].Type = ArgumentType.GlobalDataPtr;
                  //instruction.Arguments[i].IValue = ;
                } break;

              case AGSFixupType.Stack:
                {
                  instruction.Arguments[i].Type = ArgumentType.Stack;
                } break;

              default:
                throw new InvalidDataException("Unknown fixup type!");
            }
          }
        }
      }

      //NOTE(adm244): fix dest\src inconsistency
      if (instruction.Opcode == Opcodes.REGTOREG)
      {
        Debug.Assert(instruction.Arguments.Length == 2);

        Argument srcArg = instruction.Arguments[0];
        instruction.Arguments[0] = instruction.Arguments[1];
        instruction.Arguments[1] = srcArg;
      }

      Position += (instruction.Arguments.Length + 1);

      return instruction;
    }

    public int Position { get; private set; }

    public bool EOF
    {
      get { return Position >= Script.Code.Length; }
    }
  }
}
