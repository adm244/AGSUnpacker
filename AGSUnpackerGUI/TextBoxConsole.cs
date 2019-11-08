using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace AGSUnpackerGUI
{
  public class TextBoxConsole<T> : TextWriter
    where T : Form, ITextBoxConsole
  {
    private T _form = null;
    private MemoryStream _stream = new MemoryStream(1024);
    private Encoding _encoding = null;

    public TextBoxConsole(T form, Encoding encoding)
      : base()
    {
      _form = form;
      _encoding = encoding;
    }

    public override void Write(char value)
    {
      byte[] data = Encoding.GetBytes(value.ToString());
      _stream.Write(data, 0, data.Length);
    }

    public override void Write(string value)
    {
      byte[] data = Encoding.GetBytes(value);
      _stream.Write(data, 0, data.Length);
    }

    public override void Flush()
    {
      if (_stream.Length < 1)
        return;

      string value = Encoding.GetString(_stream.GetBuffer(), 0, (int)_stream.Length);
      _stream.Position = 0;
      _stream.SetLength(0);

      if (_form.InvokeRequired)
      {
        _form.BeginInvoke(new Action<string>(_form.AppendTextBox), new object[] { value });
        return;
      }

      _form.AppendTextBox(value);
    }

    public override Encoding Encoding
    {
      get { return _encoding; }
    }
  }
}
