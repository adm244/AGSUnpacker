using System;
using System.Diagnostics;
using AGSUnpackerSharp.Extensions;
using AGSUnpackerSharp.Utils;

namespace AGSUnpackerSharp.Shared.Script
{
  public struct AGSInstruction
  {
    public byte InstanceId;
    public AGSOpcodes Opcode;
    public string Mnemonic;
    public AGSArgument[] Arguments;

    public int ArgumentsCount
    {
      get { return Arguments.Length; }
    }
  }

  public enum AGSArgumentType
  {
    IntConstant,
    FloatConstant,
    RegisterIndex,
    ImportsOffset,
    StringsOffset,
    FunctionOffset,
    StackOffset,
  }

  public enum AGSFixupType
  {
    Literal = 0,
    GlobalData = 1,
    Function = 2,
    String = 3,
    Import = 4,
    DataData = 5,
    Stack = 6,
  }

  public struct AGSArgument
  {
    public AGSArgumentType Type;
    public Int32 IntValue;
    public float FloatValue;
    public string Name;

    public override string ToString()
    {
      string result = string.Empty;

      switch (Type)
      {
        case AGSArgumentType.IntConstant:
        case AGSArgumentType.StackOffset:
          result = IntValue.ToString();
          break;

        case AGSArgumentType.FloatConstant:
          result = FloatValue.ToString();
          break;

        case AGSArgumentType.FunctionOffset:
        case AGSArgumentType.RegisterIndex:
        case AGSArgumentType.ImportsOffset:
        case AGSArgumentType.StringsOffset:
          result = Name;
          break;

        default:
          Debug.Assert(false);
          break;
      }

      return result;
    }
  }

  public enum AGSBinaryArgumentType
  {
    IntLiteral,
    FloatLiteral,
    Register,
  }

  public class AGSBinaryInstruction
  {
    public AGSOpcodes Opcode;
    public string Mnemonic;
    public AGSBinaryArgumentType[] Arguments;

    public int ArgumentsCount
    {
      get { return Arguments.Length; }
    }

    public AGSBinaryInstruction(AGSOpcodes opcode, string mnemonic, params AGSBinaryArgumentType[] arguments)
    {
      Opcode = opcode;
      Mnemonic = mnemonic;
      Arguments = arguments;
    }
  }

  public enum AGSOpcodes
  {
    ADD = 1, // reg1 += arg2
    SUB = 2, // reg1 -= arg2
    MUL = 32, // reg1 *= arg2

    FADD = 53, // reg1 += arg2 (float)
    FSUB = 54, // reg1 -= arg2 (float)

    MULREG = 9, // reg1 *= reg2
    DIVREG = 10, // reg1 /= reg2
    ADDREG = 11, // reg1 += reg2
    SUBREG = 12, // reg1 -= reg2

    FMULREG = 55, // reg1 *= reg2 (float)
    FDIVREG = 56, // reg1 /= reg2 (float)
    FADDREG = 57, // reg1 += reg2 (float)
    FSUBREG = 58, // reg1 -= reg2 (float)

    //NOTE(adm244): yeah, it's in a reversed order for some reason...
    REGTOREG = 3, // reg2 = reg1
    LITTOREG = 6, // reg1 = arg2

    WRITELIT = 4, // m[MAR] = arg2 (copy arg1 bytes)
    MEMWRITE = 8, // m[MAR] = reg1
    MEMWRITEB = 26, // m[MAR] = reg1 (1 byte)
    MEMWRITEW = 27, // m[MAR] = reg1 (2 bytes)
    MEMREAD = 7, // reg1 = m[MAR]
    MEMREADB = 24, // reg1 = m[MAR] (1 byte)
    MEMREADW = 25, // reg1 = m[MAR] (2 bytes)
    ZEROMEMORY = 63, // m[MAR]..m[MAR+(arg1-1)] = 0
    MEMWRITEPTR = 47, // m[MAR] = reg1 (adjust ptr addr)
    MEMREADPTR = 48, // reg1 = m[MAR] (adjust ptr addr)
    MEMZEROPTR = 49, // m[MAR] = 0    (blank ptr)
    MEMINITPTR = 50, // m[MAR] = reg1 (but don't free old one)
    MEMZEROPTRND = 69, // m[MAR] = 0    (blank ptr, no dispose if = ax)

    CALL = 23, // jump to subroutine at reg1
    CALLEXT = 33, // call external (imported) function reg1
    CALLAS = 37, // call external script function
    RET =  5, // return from subroutine

    PUSHREG = 29, // m[sp]=reg1; sp++
    POPREG = 30, // sp--; reg1=m[sp]
    PUSHREAL = 34, // push reg1 onto real stack
    SUBREALSTACK = 35, // decrement stack ptr by literal
    LOADSPOFFS = 51, // MAR = SP - arg1 (optimization for local var access)

    MODREG = 40, // reg1 %= reg2
    XORREG = 41, // reg1 ^= reg2
    NOTREG = 42, // reg1 = !reg1
    SHIFTLEFT = 43, // reg1 = reg1 << reg2
    SHIFTRIGHT = 44, // reg1 = reg1 >> reg2

    BITAND = 13, // bitwise  reg1 & reg2
    BITOR = 14, // bitwise  reg1 | reg2
    AND = 21, // (reg1!=0) && (reg2!=0) -> reg1
    OR = 22, // (reg1!=0) || (reg2!=0) -> reg1

    ISEQUAL = 15, // reg1 == reg2   reg1=1 if true, =0 if not
    NOTEQUAL = 16, // reg1 != reg2
    GREATER = 17, // reg1 > reg2
    LESSTHAN = 18, // reg1 < reg2
    GTE = 19, // reg1 >= reg2
    LTE = 20, // reg1 <= reg2
    FGREATER = 59, // reg1 > reg2 (float)
    FLESSTHAN = 60, // reg1 < reg2 (float)
    FGTE = 61, // reg1 >= reg2 (float)
    FLTE = 62, // reg1 <= reg2 (float)

    JZ = 28, // jump if ax == 0 to arg1
    JNZ = 70, // jump to arg1 if ax != 0
    JMP = 31, // jump to arg1

    LINENUM = 36, // debug info - source code line number
    
    NUMFUNCARGS = 39, // number of arguments for ext func call
    LOOPCHECKOFF = 68, // no loop checking for this function
    NEWARRAY = 72, // reg1 = new array of reg1 elements, each of size arg2 (arg3=managed type?)

    CHECKNULL = 52, // error if MAR==0
    CHECKNULLREG = 67, // error if reg1 == NULL
    CHECKBOUNDS = 46, // check reg1 is between 0 and arg2
    DYNAMICBOUNDS = 71, // check reg1 is between 0 and m[MAR-4]

    //NOTE(adm244): use "baseptr" mnemonic, or something like that...
    THISBASE = 38, // current relative address
    //NOTE(adm244): funny how this "call" instruction have nothing to do with calling
    // it only sets op register, so it's more like "mov op, reg1".
    // use "thisptr" mnemonic
    CALLOBJ = 45, // next call is member function of reg1

    CREATESTRING = 64, // reg1 = new String(reg1)
    STRINGSEQUAL = 65, // (char*)reg1 == (char*)reg2   reg1 = 1 if true, reg1 = 0 if not
    STRINGSNOTEQ = 66, // (char*)reg1 != (char*)reg2
  }

  public static class AGSVirtualMachine
  {
    public static readonly string[] RegisterNames = new string[] {
      "null", "sp", "mar", "ax", "bx", "cx", "op", "dx",
    };

    public static readonly AGSBinaryInstruction[] Instructions = new AGSBinaryInstruction[] {
      // arithmetic operations
      new AGSBinaryInstruction(AGSOpcodes.ADD, "add", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.SUB, "sub", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.MUL, "mul", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral),
      //NOTE(adm244): yep, there's no instruction for reg1 /= arg2 (int)

      new AGSBinaryInstruction(AGSOpcodes.FADD, "fadd", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.FloatLiteral),
      new AGSBinaryInstruction(AGSOpcodes.FSUB, "fsub", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.FloatLiteral),
      //NOTE(adm244): yep, there's no instruction for reg1 *= arg2 (float)
      //NOTE(adm244): yep, there's no instruction for reg1 /= arg2 (float)

      new AGSBinaryInstruction(AGSOpcodes.ADDREG, "addreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.SUBREG, "subreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MULREG, "mulreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.DIVREG, "divreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.FADDREG, "faddreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FSUBREG, "fsubreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FMULREG, "fmulreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FDIVREG, "fdivreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),

      // move operations
      //NOTE(adm244): make sure to NOT forget to swap src and dest in asm dumps!
      new AGSBinaryInstruction(AGSOpcodes.REGTOREG, "movreg", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.LITTOREG, "movlit", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral),

      new AGSBinaryInstruction(AGSOpcodes.WRITELIT, "memwritelit", AGSBinaryArgumentType.IntLiteral, AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.MEMWRITE, "memwrite", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMWRITEB, "memwriteb", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMWRITEW, "memwritew", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMREAD, "memread", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMREADB, "memreadb", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMREADW, "memreadw", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.ZEROMEMORY, "zeromem", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.MEMWRITEPTR, "memwriteptr", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMREADPTR, "memreadptr", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMZEROPTR, "memzeroptr"),
      new AGSBinaryInstruction(AGSOpcodes.MEMINITPTR, "meminitptr", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.MEMZEROPTRND, "memzeroptrnd"),

      new AGSBinaryInstruction(AGSOpcodes.CALL, "call", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.CALLEXT, "callext", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.CALLAS, "callas", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.RET, "ret"),

      new AGSBinaryInstruction(AGSOpcodes.PUSHREG, "push", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.POPREG, "pop", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.PUSHREAL, "pushreal", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.SUBREALSTACK, "subreal", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.LOADSPOFFS, "loadspoffs", AGSBinaryArgumentType.IntLiteral),

      new AGSBinaryInstruction(AGSOpcodes.MODREG, "mod", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.XORREG, "xor", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.NOTREG, "not", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.SHIFTLEFT, "sal", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.SHIFTRIGHT, "sar", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.BITAND, "and", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.BITOR, "or", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.AND, "land", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.OR, "lor", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.ISEQUAL, "eq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.NOTEQUAL, "neq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.GREATER, "gr", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.LESSTHAN, "le", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.GTE, "geq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.LTE, "leq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FGREATER, "fgr", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FLESSTHAN, "fle", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FGTE, "fgeq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.FLTE, "fleq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.JZ, "jz", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.JNZ, "jnz", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.JMP, "jmp", AGSBinaryArgumentType.IntLiteral),

      new AGSBinaryInstruction(AGSOpcodes.LINENUM, "sourceline", AGSBinaryArgumentType.IntLiteral),

      new AGSBinaryInstruction(AGSOpcodes.NUMFUNCARGS, "setfuncargs", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.LOOPCHECKOFF, "noloopcheck"),
      new AGSBinaryInstruction(AGSOpcodes.NEWARRAY, "newarray", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral, AGSBinaryArgumentType.IntLiteral),

      new AGSBinaryInstruction(AGSOpcodes.CHECKNULL, "checknull"),
      new AGSBinaryInstruction(AGSOpcodes.CHECKNULLREG, "checknullreg", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.CHECKBOUNDS, "checkbounds", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.DYNAMICBOUNDS, "checkboundsdyn", AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.THISBASE, "baseptr", AGSBinaryArgumentType.IntLiteral),
      new AGSBinaryInstruction(AGSOpcodes.CALLOBJ, "thisptr", AGSBinaryArgumentType.Register),

      new AGSBinaryInstruction(AGSOpcodes.CREATESTRING, "newstr", AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.STRINGSEQUAL, "streq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
      new AGSBinaryInstruction(AGSOpcodes.STRINGSNOTEQ, "strneq", AGSBinaryArgumentType.Register, AGSBinaryArgumentType.Register),
    };

    private static AGSArgumentType GetArgumentType(AGSFixupType fixuptype, AGSBinaryArgumentType argtype)
    {
      AGSArgumentType type = AGSArgumentType.IntConstant;
      switch (fixuptype)
      {
        case AGSFixupType.Stack:
        case AGSFixupType.Literal:
          {
            switch (argtype)
            {
              case AGSBinaryArgumentType.IntLiteral:
                type = AGSArgumentType.IntConstant;
                break;
              case AGSBinaryArgumentType.FloatLiteral:
                type = AGSArgumentType.FloatConstant;
                break;
              case AGSBinaryArgumentType.Register:
                type = AGSArgumentType.RegisterIndex;
                break;

              default:
                Debug.Assert(false);
                break;
            }
          }
          break;

        case AGSFixupType.Import:
          type = AGSArgumentType.ImportsOffset;
          break;

        case AGSFixupType.Function:
          type = AGSArgumentType.FunctionOffset;
          break;

        case AGSFixupType.String:
          type = AGSArgumentType.StringsOffset;
          break;

        default:
          Debug.Assert(false);
          break;
      }

      return type;
    }

    private static int GetExportIndex(AGSScript script, int offset)
    {
      int index = -1;
      for (int i = 0; i < script.Exports.Length; ++i)
      {
        int functionOffset = script.Exports[i].Pointer & 0x00FFFFFF;
        if (functionOffset == offset)
        {
          index = i;
          break;
        }
      }

      Debug.Assert(index != -1);
      return index;
    }

    private static int GetStringIndex(AGSScript script, int offset)
    {
      int acc = 0;
      int i = 0;
      for (; i < script.Strings.Length; ++i)
      {
        if (acc == offset)
          break;

        acc += script.Strings[i].Length + 1;
      }

      Debug.Assert(acc == offset);
      return i;
    }

    public static AGSInstruction DisassembleInstruction(AGSScript script, byte[] fixups, int[] code, int ip)
    {
      AGSInstruction instruction = new AGSInstruction();

      byte instanceId = (byte)((code[ip] >> 24) & 0xFF);
      Int32 opcodeRaw = (Int32)(code[ip] & 0x00FFFFFF);

      Debug.Assert(Enum.IsDefined(typeof(AGSOpcodes), opcodeRaw));

      AGSOpcodes opcode = (AGSOpcodes)(opcodeRaw);

      for (int i = 0; i < Instructions.Length; ++i)
      {
        if (Instructions[i].Opcode == opcode)
        {
          instruction.InstanceId = instanceId;
          instruction.Opcode = opcode;
          instruction.Mnemonic = Instructions[i].Mnemonic;

          Debug.Assert((ip + Instructions[i].ArgumentsCount) < code.Length);

          instruction.Arguments = new AGSArgument[Instructions[i].ArgumentsCount];
          for (int arg = 0; arg < instruction.ArgumentsCount; ++arg)
          {
            int index = ip + arg + 1;
            int value = code[index];

            switch (Instructions[i].Arguments[arg])
            {
              case AGSBinaryArgumentType.IntLiteral:
                {
                  //FIX(adm244): type IS AGSFixupType !!!
                  AGSFixupType fixuptype = (AGSFixupType)fixups[index];
                  //Debug.Assert(Enum.IsDefined(typeof(AGSArgumentType), type));

                  AGSArgumentType type = GetArgumentType(fixuptype, Instructions[i].Arguments[arg]);

                  instruction.Arguments[arg].Type = type;
                  instruction.Arguments[arg].IntValue = 0;
                  instruction.Arguments[arg].FloatValue = float.NaN;
                  instruction.Arguments[arg].Name = string.Empty;

                  switch (type)
                  {
                    case AGSArgumentType.IntConstant:
                    case AGSArgumentType.StackOffset:
                      instruction.Arguments[arg].IntValue = value;
                      break;

                    case AGSArgumentType.FloatConstant:
                      instruction.Arguments[arg].FloatValue = IEEE754Utils.Int32BitsToFloat(value);
                      break;

                    case AGSArgumentType.ImportsOffset:
                      instruction.Arguments[arg].Name = script.Imports[value];
                      break;

                    case AGSArgumentType.FunctionOffset:
                      int exportIndex = GetExportIndex(script, value);
                      instruction.Arguments[arg].Name = script.Exports[exportIndex].Name;
                      break;

                    case AGSArgumentType.StringsOffset:
                      int stringIndex = GetStringIndex(script, value);
                      instruction.Arguments[arg].Name = '"' + script.Strings[stringIndex] + '"';
                      break;

                    default:
                      Debug.Assert(false, "Unknown argument type!");
                      break;
                  }
                }
                break;

              case AGSBinaryArgumentType.Register:
                instruction.Arguments[arg].Type = AGSArgumentType.RegisterIndex;
                instruction.Arguments[arg].IntValue = value;
                instruction.Arguments[arg].FloatValue = float.NaN;
                instruction.Arguments[arg].Name = RegisterNames[value];
                break;

              default:
                Debug.Assert(false, "Unknown binary argument type!");
                break;
            }
          }

          break;
        }
      }

      //NOTE(adm244): swap arguments to match "mov <dest>, <src>" pattern
      if (instruction.Opcode == AGSOpcodes.REGTOREG)
      {
        Debug.Assert(instruction.ArgumentsCount == 2);

        AGSArgument temp = instruction.Arguments[0];
        instruction.Arguments[0] = instruction.Arguments[1];
        instruction.Arguments[1] = temp;
      }

      return instruction;
    }
  }
}
