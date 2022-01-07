namespace AGSUnpacker.Lib.Disassembler
{
//  public static class AGSDecompiler
//  {
//    public enum AGSOpcodes
//    {
//      ADD = 1, // reg1 += arg2
//      SUB = 2, // reg1 -= arg2
//      MUL = 32, // reg1 *= arg2
//   
//      FADD = 53, // reg1 += arg2 (float)
//      FSUB = 54, // reg1 -= arg2 (float)
//   
//      MULREG = 9, // reg1 *= reg2
//      DIVREG = 10, // reg1 /= reg2
//      ADDREG = 11, // reg1 += reg2
//      SUBREG = 12, // reg1 -= reg2
//   
//      FMULREG = 55, // reg1 *= reg2 (float)
//      FDIVREG = 56, // reg1 /= reg2 (float)
//      FADDREG = 57, // reg1 += reg2 (float)
//      FSUBREG = 58, // reg1 -= reg2 (float)
//   
//      //NOTE(adm244): yeah, it's in a reversed order for some reason...
//      REGTOREG = 3, // reg2 = reg1
//      LITTOREG = 6, // reg1 = arg2
//   
//      WRITELIT = 4, // m[MAR] = arg2 (copy arg1 bytes)
//      MEMWRITE = 8, // m[MAR] = reg1
//      MEMWRITEB = 26, // m[MAR] = reg1 (1 byte)
//      MEMWRITEW = 27, // m[MAR] = reg1 (2 bytes)
//      MEMREAD = 7, // reg1 = m[MAR]
//      MEMREADB = 24, // reg1 = m[MAR] (1 byte)
//      MEMREADW = 25, // reg1 = m[MAR] (2 bytes)
//      ZEROMEMORY = 63, // m[MAR]..m[MAR+(arg1-1)] = 0
//      MEMWRITEPTR = 47, // m[MAR] = reg1 (adjust ptr addr)
//      MEMREADPTR = 48, // reg1 = m[MAR] (adjust ptr addr)
//      MEMZEROPTR = 49, // m[MAR] = 0    (blank ptr)
//      MEMINITPTR = 50, // m[MAR] = reg1 (but don't free old one)
//      MEMZEROPTRND = 69, // m[MAR] = 0    (blank ptr, no dispose if = ax)
//   
//      CALL = 23, // jump to subroutine at reg1
//      CALLEXT = 33, // call external (imported) function reg1
//      CALLAS = 37, // call external script function
//      RET = 5, // return from subroutine
//   
//      PUSHREG = 29, // m[sp]=reg1; sp++
//      POPREG = 30, // sp--; reg1=m[sp]
//      PUSHREAL = 34, // push reg1 onto real stack
//      SUBREALSTACK = 35, // decrement stack ptr by literal
//      LOADSPOFFS = 51, // MAR = SP - arg1 (optimization for local var access)
//   
//      MODREG = 40, // reg1 %= reg2
//      XORREG = 41, // reg1 ^= reg2
//      NOTREG = 42, // reg1 = !reg1
//      SHIFTLEFT = 43, // reg1 = reg1 << reg2
//      SHIFTRIGHT = 44, // reg1 = reg1 >> reg2
//   
//      BITAND = 13, // bitwise  reg1 & reg2
//      BITOR = 14, // bitwise  reg1 | reg2
//      AND = 21, // (reg1!=0) && (reg2!=0) -> reg1
//      OR = 22, // (reg1!=0) || (reg2!=0) -> reg1
//   
//      ISEQUAL = 15, // reg1 == reg2   reg1=1 if true, =0 if not
//      NOTEQUAL = 16, // reg1 != reg2
//      GREATER = 17, // reg1 > reg2
//      LESSTHAN = 18, // reg1 < reg2
//      GTE = 19, // reg1 >= reg2
//      LTE = 20, // reg1 <= reg2
//      FGREATER = 59, // reg1 > reg2 (float)
//      FLESSTHAN = 60, // reg1 < reg2 (float)
//      FGTE = 61, // reg1 >= reg2 (float)
//      FLTE = 62, // reg1 <= reg2 (float)
//   
//      JZ = 28, // jump if ax == 0 to arg1
//      JNZ = 70, // jump to arg1 if ax != 0
//      JMP = 31, // jump to arg1
//   
//      LINENUM = 36, // debug info - source code line number
//   
//      NUMFUNCARGS = 39, // number of arguments for ext func call
//      LOOPCHECKOFF = 68, // no loop checking for this function
//      NEWARRAY = 72, // reg1 = new array of reg1 elements, each of size arg2 (arg3=managed type?)
//   
//      CHECKNULL = 52, // error if MAR==0
//      CHECKNULLREG = 67, // error if reg1 == NULL
//      CHECKBOUNDS = 46, // check reg1 is between 0 and arg2
//      DYNAMICBOUNDS = 71, // check reg1 is between 0 and m[MAR-4]
//   
//      //NOTE(adm244): use "baseptr" mnemonic, or something like that...
//      THISBASE = 38, // current relative address
//      //NOTE(adm244): funny how this "call" instruction have nothing to do with calling
//      // it only sets op register, so it's more like "mov op, reg1".
//      // use "thisptr" mnemonic
//      CALLOBJ = 45, // next call is member function of reg1
//   
//      CREATESTRING = 64, // reg1 = new String(reg1)
//      STRINGSEQUAL = 65, // (char*)reg1 == (char*)reg2   reg1 = 1 if true, reg1 = 0 if not
//      STRINGSNOTEQ = 66, // (char*)reg1 != (char*)reg2
//    }
//
//    public struct AGSInstruction
//    {
//      public byte InstanceId;
//      public AGSOpcodes Opcode;
//      public string Mnemonic;
//      public int[] Arguments;
//    }
//
//    public class BasicBlock
//    {
//      public BlockType Type { private set; get; }
//      public int StartPosition { private set; get; }
//      public int EndPosition { private set; get; }
//      public List<AGSInstruction> Instructions { private set; get; }
//
//      public BasicBlock(int startPosition)
//      {
//        Type = BlockType.Normal;
//        StartPosition = startPosition;
//        EndPosition = 0;
//        Instructions = new List<AGSInstruction>();
//      }
//
//      public enum BlockType
//      {
//        Normal,
//      }
//    }
//
//    public class AGSInstructionReader
//    {
//      public AGSScript Script { private set; get; }
//      public int Position { private set; get; }
//  
//      public AGSInstructionReader(AGSScript script)
//        : this(script, 0)
//      {
//      }
//  
//      public AGSInstructionReader(AGSScript script, int startPosition)
//      {
//        Script = script;
//        Position = startPosition;
//      }
//  
//      public bool EOF()
//      {
//        return (Position >= Script.Code.Length);
//      }
//  
//      public AGSInstruction Read()
//      {
//        throw new NotImplementedException();
//      }
//    }
//
//    public class AGSFunction
//    {
//      public AGSScript Script { private set; get; }
//      public string Name { private set; get; }
//      public int Position { private set; get; }
//
//      private BasicBlock Root;
//      private List<BasicBlock> Visited;
//
//      public AGSFunction(AGSScript script, string name, int position)
//      {
//        Script = script;
//        Name = name;
//        Position = position;
//        Visited = new List<BasicBlock>();
//      }
//
//      public bool Disassemble()
//      {
//        Root = null;
//        Visited.Clear();
//
//        return Disassemble(Position, out Root);
//      }
//
//      private bool IsVisited(int position)
//      {
//        // EndPosition is NOT included in a basic block code range
//
//        for (int i = 0; i < Visited.Count; ++i)
//        {
//          if ((Visited[i].EndPosition < position)
//            && (Visited[i].StartPosition >= position))
//          {
//            return true;
//          }
//        }
//
//        return false;
//      }
//
//      private bool Disassemble(int startPosition, out BasicBlock block)
//      {
//        block = null;
//
//        if (IsVisited(startPosition))
//          return true;
//
//        AGSInstructionReader input = new AGSInstructionReader(Script, startPosition);
//        block = new BasicBlock(startPosition);
//        Visited.Add(block);
//
//        while (!input.EOF())
//        {
//          AGSInstruction instruction = input.Read();
//          block.Instructions.Add(instruction);
//
//          switch (instruction.Opcode)
//          {
//            case AGSOpcodes.RET:
//              return true;
//
//            case AGSOpcodes.JZ:
//            case AGSOpcodes.JNZ:
//              {
//                BasicBlock outDestination;
//                Disassemble(instruction.Arguments[0], out outDestination);
//                Visited.Add(outDestination);
//
//                BasicBlock outFallThrough;
//                Disassemble(input.Position, out outFallThrough);
//                Visited.Add(outFallThrough);
//
//                block.AddLeft(outDestination);
//                block.AddRight(outFallThrough);
//
//                //if (!Disassemble(instruction.Arguments[0], node.AddLeft()))
//                //{
//                //  return false;
//                //}
//                //
//                //if (!Disassemble(input.Position, node.AddRight()))
//                //{
//                //  return false;
//                //}
//              }
//              break;
//
//            default:
//              break;
//          }
//        }
//
//        //NOTE(adm244): if we get to this point then function is corrupted
//        throw new InvalidDataException("Unexpected EndOfFile encountered!\nFunction is corrupted.");
//      }
//    }
//
//    public static void Disassemble(AGSScript script)
//    {
//      List<AGSFunction> functions = GetExportedFunctions(script);
//
//      // create a tree for functions basic blocks
//      // analyze a function to get a tree of basic blocks
//      // traverse a basic blocks tree in a reverse order to dump instructures sequentially
//      for (int i = 0; i < functions.Count; ++i)
//      {
//        functions[i].Disassemble();
//      }
//    }
//
//    private static List<AGSFunction> GetExportedFunctions(AGSScript script)
//    {
//      List<AGSFunction> functions = new List<AGSFunction>(script.Exports.Length);
//
//      for (int i = 0; i < script.Exports.Length; ++i)
//      {
//        if (script.Exports[i].Type == AGSScriptExport.ExportType.Function)
//        {
//          string name = script.Exports[i].Name;
//          int position = script.Exports[i].Pointer;
//
//          functions.Add(new AGSFunction(script, name, position));
//        }
//      }
//
//      return functions;
//    }
//  }
}
