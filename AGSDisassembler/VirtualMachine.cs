
namespace AGSDisassembler
{
  public enum BinaryArgumentType
  {
    Literal,
    Register,
  }

  public class BinaryInstruction
  {
    public Opcodes Opcode;
    public string Mnemonic;
    public BinaryArgumentType[] Arguments;

    public BinaryInstruction(Opcodes opcode, string mnemonic, params BinaryArgumentType[] arguments)
    {
      Opcode = opcode;
      Mnemonic = mnemonic;
      //TODO(adm244): check if arguments is null when not passed
      Arguments = arguments;
    }
  }

  public enum Opcodes
  {
    NULL = 0,
    ADD = 1,
    SUB = 2,
    REGTOREG = 3,
    WRITELIT = 4,
    RET = 5,
    LITTOREG = 6,
    MEMREAD = 7,
    MEMWRITE = 8,
    MULREG = 9,
    DIVREG = 10,
    ADDREG = 11,
    SUBREG = 12,
    BITAND = 13,
    BITOR = 14,
    ISEQUAL = 15,
    NOTEQUAL = 16,
    GREATER = 17,
    LESSTHAN = 18,
    GTE = 19,
    LTE = 20,
    AND = 21,
    OR = 22,
    CALL = 23,
    MEMREADB = 24,
    MEMREADW = 25,
    MEMWRITEB = 26,
    MEMWRITEW = 27,
    JZ = 28,
    PUSHREG = 29,
    POPREG = 30,
    JMP = 31,
    MUL = 32,
    CALLEXT = 33,
    PUSHREAL = 34,
    SUBREALSTACK = 35,
    LINENUM = 36,
    CALLAS = 37,
    THISBASE = 38,
    NUMFUNCARGS = 39,
    MODREG = 40,
    XORREG = 41,
    NOTREG = 42,
    SHIFTLEFT = 43,
    SHIFTRIGHT = 44,
    CALLOBJ = 45,
    CHECKBOUNDS = 46,
    MEMWRITEPTR = 47,
    MEMREADPTR = 48,
    MEMZEROPTR = 49,
    MEMINITPTR = 50,
    LOADSPOFFS = 51,
    CHECKNULL = 52,
    FADD = 53,
    FSUB = 54,
    FMULREG = 55,
    FDIVREG = 56,
    FADDREG = 57,
    FSUBREG = 58,
    FGREATER = 59,
    FLESSTHAN = 60,
    FGTE = 61,
    FLTE = 62,
    ZEROMEMORY = 63,
    CREATESTRING = 64,
    STRINGSEQUAL = 65,
    STRINGSNOTEQ = 66,
    CHECKNULLREG = 67,
    LOOPCHECKOFF = 68,
    MEMZEROPTRND = 69,
    JNZ = 70,
    DYNAMICBOUNDS = 71,
    NEWARRAY = 72,
    NEWUSEROBJECT = 73,
  }

  public static class VM
  {
    public static readonly string[] RegisterNames = new string[] {
      "null", "sp", "mar", "ax", "bx", "cx", "op", "dx",
    };

    //IMPORTANT(adm244): make sure that opcodes are equal to index
    public static BinaryInstruction[] Instructions = new BinaryInstruction[] {
      new BinaryInstruction(Opcodes.NULL,           "null"),
      new BinaryInstruction(Opcodes.ADD,            "add",            BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.SUB,            "sub",            BinaryArgumentType.Register, BinaryArgumentType.Literal),
      
      //IMPORTANT(adm244): !!! REMEMBER TO SWAP DEST AND SRC ON THIS ONE !!!
      new BinaryInstruction(Opcodes.REGTOREG,       "mov",            BinaryArgumentType.Register, BinaryArgumentType.Register),

      new BinaryInstruction(Opcodes.WRITELIT,       "memwritelit",    BinaryArgumentType.Literal, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.RET,            "ret"),
      new BinaryInstruction(Opcodes.LITTOREG,       "mov",            BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.MEMREAD,        "memread",        BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMWRITE,       "memwrite",       BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MULREG,         "mul",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.DIVREG,         "div",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.ADDREG,         "add",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.SUBREG,         "sub",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.BITAND,         "bit_and",        BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.BITOR,          "bit_or",         BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.ISEQUAL,        "cmp",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.NOTEQUAL,       "ncmp",           BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.GREATER,        "gt",             BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.LESSTHAN,       "lt",             BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.GTE,            "gte",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.LTE,            "lte",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.AND,            "and",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.OR,             "or",             BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.CALL,           "call",           BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMREADB,       "memread.b",      BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMREADW,       "memread.w",      BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMWRITEB,      "memwrite.b",     BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMWRITEW,      "memwrite.w",     BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.JZ,             "jz",             BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.PUSHREG,        "push",           BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.POPREG,         "pop",            BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.JMP,            "jmp",            BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.MUL,            "mul",            BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.CALLEXT,        "farcall",        BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.PUSHREAL,       "farpush",        BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.SUBREALSTACK,   "farsubsp",       BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.LINENUM,        "sourceline",     BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.CALLAS,         "callsrc",        BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.THISBASE,       "thisaddr",       BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.NUMFUNCARGS,    "setfuncargs",    BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.MODREG,         "mod",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.XORREG,         "xor",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.NOTREG,         "not",            BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.SHIFTLEFT,      "shl",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.SHIFTRIGHT,     "shr",            BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.CALLOBJ,        "callobj",        BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.CHECKBOUNDS,    "checkbounds",    BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.MEMWRITEPTR,    "memwrite.ptr",   BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMREADPTR,     "memread.ptr",    BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.MEMZEROPTR,     "memwrite.ptr.0"),
      new BinaryInstruction(Opcodes.MEMINITPTR,     "meminit.ptr",    BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.LOADSPOFFS,     "load.sp.offs",   BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.CHECKNULL,      "checknull.ptr"),
      new BinaryInstruction(Opcodes.FADD,           "f.add",          BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.FSUB,           "f.sub",          BinaryArgumentType.Register, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.FMULREG,        "f.mul",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FDIVREG,        "f.div",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FADDREG,        "f.add",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FSUBREG,        "f.sub",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FGREATER,       "f.gt",           BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FLESSTHAN,      "f.lt",           BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FGTE,           "f.gte",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.FLTE,           "f.lte",          BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.ZEROMEMORY,     "zeromem",        BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.CREATESTRING,   "newstring",      BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.STRINGSEQUAL,   "strcmp",         BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.STRINGSNOTEQ,   "strncmp",        BinaryArgumentType.Register, BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.CHECKNULLREG,   "checknull",      BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.LOOPCHECKOFF,   "loopcheckoff"),
      new BinaryInstruction(Opcodes.MEMZEROPTRND,   "memwrite.prt.0.nd"),
      new BinaryInstruction(Opcodes.JNZ,            "jnz",            BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.DYNAMICBOUNDS,  "dynamicbounds",  BinaryArgumentType.Register),
      new BinaryInstruction(Opcodes.NEWARRAY,       "newarray",       BinaryArgumentType.Register, BinaryArgumentType.Literal, BinaryArgumentType.Literal),
      new BinaryInstruction(Opcodes.NEWUSEROBJECT,  "newuserobject",  BinaryArgumentType.Register, BinaryArgumentType.Literal),
    };
  }
}
