using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Shared.Script;

namespace AGSDisassembler
{
  public static class Disassembler
  {
    public static void Disassemble(AGSScript script)
    {
      IList<Instruction> instructions = Decode(script);

      DumpInstructions("code.asm", script, instructions);
    }

    private static void DumpInstructions(string filepath, AGSScript script, IList<Instruction> instructions)
    {
      using (FileStream fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter outputWriter = new StreamWriter(fileStream, Encoding.GetEncoding(1252)))
        {
          for (int i = 0; i < instructions.Count; ++i)
          {
            outputWriter.Write("{0:X8}: {1}", instructions[i].Address, instructions[i].Mnemonic);

            for (int j = 0; j < instructions[i].Arguments.Length; ++j)
            {
              if (j != 0)
                outputWriter.Write(",");

              if (instructions[i].Arguments[j].Value == null)
                throw new InvalidDataException("Argument value is null!");

              switch (instructions[i].Arguments[j].Type)
              {
                case ArgumentType.Register:
                  {
                    outputWriter.Write(" {0}", VM.RegisterNames[instructions[i].Arguments[j].IntValue]);
                  } break;

                case ArgumentType.GlobalData:
                  outputWriter.Write("[unknown]");
                  break;

                case ArgumentType.Function:
                  outputWriter.Write(" localfunc:{0}", instructions[i].Arguments[j].FuncValue.Name);
                  break;

                case ArgumentType.String:
                  outputWriter.Write(" str:\"{0}\"", instructions[i].Arguments[j].StrValue);
                  break;

                case ArgumentType.Import:
                  {
                    int importIndex = instructions[i].Arguments[j].IntValue;
                    outputWriter.Write(" func:{0}", script.Imports[importIndex]);
                  } break;

                case ArgumentType.GlobalDataPtr:
                  outputWriter.Write("[unknown]");
                  break;

                case ArgumentType.Stack:
                  outputWriter.Write("[unknown]");
                  break;

                default:
                  {
                    outputWriter.Write(" {0:X}h", instructions[i].Arguments[j].IntValue);
                  } break;
              }
            }

            outputWriter.WriteLine();
          }
        }
      }
    }

    private static IList<Instruction> Decode(AGSScript script)
    {
      List<Instruction> instructions = new List<Instruction>();

      InstructionReader reader = new InstructionReader(script);
      while (!reader.EOF)
      {
        Instruction instruction = reader.ReadInstruction();
        if (instruction == null)
          throw new InvalidDataException();

        instructions.Add(instruction);
      }

      return instructions;
    }
  }
}
