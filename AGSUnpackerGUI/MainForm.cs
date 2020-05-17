using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AGSUnpackerSharp.Utils;
using System.Threading;
using AGSUnpackerSharp.Graphics;
using System.Diagnostics;
using AGSUnpackerGUI.Properties;

namespace AGSUnpackerGUI
{
  public partial class MainForm : Form, ITextBoxConsole
  {
    private OpenFileDialog _ofd = new OpenFileDialog();
    private FolderBrowserDialog _fbd = new FolderBrowserDialog();

    public MainForm()
    {
      InitializeComponent();
      Icon = Resources.cup_icon;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      TextBoxConsole<MainForm> textBoxConsole = new TextBoxConsole<MainForm>(this, Encoding.UTF8);
      Console.SetOut(textBoxConsole);
    }

    private void ExtractFromFile(string fileTitle, string fileFilter, string folderDescription, ParameterizedThreadStart proc)
    {
      _ofd.CheckPathExists = true;
      _ofd.CheckFileExists = true;
      _ofd.Title = fileTitle;
      _ofd.Filter = fileFilter;

      if (_ofd.ShowDialog(this) == DialogResult.OK)
      {
        string fileFullPath = _ofd.FileName;
        string sourceFolder = Path.GetDirectoryName(fileFullPath);

        _fbd.Description = folderDescription;
        _fbd.SelectedPath = sourceFolder;
        _fbd.ShowNewFolderButton = true;

        if (_fbd.ShowDialog(this) == DialogResult.OK)
        {
          string targetFolder = _fbd.SelectedPath;

          ctrlStatus.Text = "Working...";
          ToggleButtons(false);

          UnpackParams threadParams = new UnpackParams(fileFullPath, targetFolder);
          threadParams.UnpackFinished += UnpackFinished;
          Thread unpackThread = new Thread(proc);
          unpackThread.Start(threadParams);
        }
      }
    }

    private void ExtractFromFolder(string fileTitle, string fileFilter, string folderDescription, ParameterizedThreadStart proc)
    {
      _fbd.Description = folderDescription;
      _fbd.ShowNewFolderButton = false;

      if (_fbd.ShowDialog(this) == DialogResult.OK)
      {
        _ofd.CheckPathExists = false;
        _ofd.CheckFileExists = false;
        _ofd.Title = fileTitle;
        _ofd.Filter = fileFilter;

        if (_ofd.ShowDialog(this) == DialogResult.OK)
        {
          ctrlStatus.Text = "Working...";
          ToggleButtons(false);

          UnpackParams threadParams = new UnpackParams(_ofd.FileName, _fbd.SelectedPath);
          threadParams.UnpackFinished += UnpackFinished;
          Thread unpackThread = new Thread(proc);
          unpackThread.Start(threadParams);
        }
      }
    }

    private void ToggleButtons(bool state)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action<bool>(ToggleButtons), new object[] { state });
        return;
      }

      for (int i = 0; i < gbUnpacking.Controls.Count; ++i)
      {
        Button button = (gbUnpacking.Controls[i] as Button);
        if (button == null)
          continue;

        button.Enabled = state;
      }

      //TODO(adm244): parse splitContainterMain.Panel1.Controls
      for (int i = 0; i < gbExtra.Controls.Count; ++i)
      {
        Button button = (gbExtra.Controls[i] as Button);
        if (button == null)
          continue;

        button.Enabled = state;
      }
    }

    private void btnExtractExe_Click(object sender, EventArgs e)
    {
      ExtractFromFile("Select AGS game executable","AGS game executable|*.exe",
        "Select destination folder", UnpackExeProc);
    }

    private void btnUnpackSprites_Click(object sender, EventArgs e)
    {
      ExtractFromFile("Select acsprset.spr file", "AGS sprite set|*.spr",
        "Select destination folder", UnpackSpritesProc);
    }

    private void btnExtractTrsExe_Click(object sender, EventArgs e)
    {
      ExtractFromFolder("Select a file path where to store a translation", "AGS Translation Source|*.trs",
        "Select AGS game assets folder", ExtractText);
    }

    private static void UnpackExeProc(object procParams)
    {
      UnpackParams p = (procParams as UnpackParams);
      string[] files = AGSClibUtils.UnpackAGSAssetFiles(p.FilePath, p.TargetFolder);

      p.OnUnpackFinished(files.Length > 0);
    }

    private static void UnpackSpritesProc(object procParams)
    {
      UnpackParams p = (procParams as UnpackParams);
      bool result = AGSSpriteSet.UnpackSprites(p.FilePath, p.TargetFolder);

      p.OnUnpackFinished(result);
    }

    private static void ExtractText(object procParams)
    {
      UnpackParams p = (procParams as UnpackParams);
      bool result = TextExtractor.Extract(p.TargetFolder, p.FilePath);

      p.OnUnpackFinished(result);
    }

    private void UnpackFinished(bool success)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action<bool>(UnpackFinished), new object[] { success });
        return;
      }

      Console.Out.Flush();
      WrapLog();

      ctrlStatus.Text = "Done!";
      ToggleButtons(true);

      if (success)
      {
        MessageBox.Show(this, "Successfully extracted files.", "Exctraction successful.",
          MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show(this, "Could not extract files :-(", "Extraction failed.",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void WrapLog()
    {
      AppendTextBox(string.Format("{0}> ", Environment.NewLine));
    }

    public void AppendTextBox(string value)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action<string>(this.AppendTextBox), new object[] { value });
        return;
      }

      tbLogOutput.AppendText(value);
    }

    private void btnGetUIDName_Click(object sender, EventArgs e)
    {
      ExtractFromFile("Select AGS game executable", "AGS game executable|*.exe",
        "Select destination folder", GetUniqueIDNameProc);
    }

    private void GetUniqueIDNameProc(object procParams)
    {
      UnpackParams p = (procParams as UnpackParams);
      bool result = AGSIdentityExtractor.ExtractIdentity(p.FilePath, p.TargetFolder);

      p.OnUnpackFinished(result);
    }

    private void btnChangeRoomBackground_Click(object sender, EventArgs e)
    {
      RoomViewer roomViewer = new RoomViewer();
      roomViewer.Show();
    }
  }
}
