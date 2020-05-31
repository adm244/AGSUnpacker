
namespace AGSDisassembler
{
  public enum ArgumentType
  {
    Literal = 0,
    Register,
    GlobalData,
    Function,
    String,
    Import,
    GlobalDataPtr,
    Stack,
  }

  public class ExportedFunction
  {
    public int Offset { get; private set; }
    public string Name { get; private set; }

    public ExportedFunction(int offset, string name)
    {
      Offset = offset;
      Name = name;
    }
  }

  public class Argument
  {
    public ArgumentType Type;
    public object Value;

    public int IntValue { get { return (int)Value; } }
    public string StrValue { get { return (string)Value; } }
    public ExportedFunction FuncValue { get { return (ExportedFunction)Value; } }
  }

  public class Instruction
  {
    public long Address;
    public Opcodes Opcode;
    public Argument[] Arguments;

    public string Mnemonic
    {
      get { return VM.Instructions[(int)Opcode].Mnemonic; }
    }
  }
}
