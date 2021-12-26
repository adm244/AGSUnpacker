using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AGSUnpackerSharp.Room;

namespace AGSUnpackerGUI
{
  public partial class RoomViewer : Form
  {
    private static readonly string STATUS_READY = "Ready.";
    private static readonly string STATUS_LOADING = "Loading...";
    private static readonly string STATUS_LOADED = "Loaded.";
    private static readonly string STATUS_FAILED = "Error occured.";
    private static readonly string STATUS_CHANGED = "Background changed.";
    private static readonly string STATUS_SAVED = "Saved.";

    private OpenFileDialog _ofd;
    private SaveFileDialog _sfd;
    private AGSRoom _loadedRoom;
    private bool _hasChanges;

    public RoomViewer()
    {
      InitializeComponent();

      _ofd = new OpenFileDialog();
      _sfd = new SaveFileDialog();

      _loadedRoom = null;
      _hasChanges = false;

      SetFileStatus(false);
      ctrlStatusLabel.Text = STATUS_READY;
    }

    private void SetFileStatus(bool loaded)
    {
      saveToolStripMenuItem.Enabled = loaded;
      closeToolStripMenuItem.Enabled = loaded;

      ctrlBackgroundFrame.Enabled = loaded;
      btnBackgroundSave.Enabled = loaded;
      btnBackgroundReplace.Enabled = loaded;
    }

    private bool VerifyAction()
    {
      if (_hasChanges)
      {
        DialogResult result = MessageBox.Show(this,
          "You have unsaved changes. Continue?",
          "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
          return false;
      }

      return true;
    }

    private void quitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!VerifyAction())
        return;

      this.Close();
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!VerifyAction())
        return;

      _ofd.CheckPathExists = true;
      _ofd.CheckFileExists = true;
      _ofd.Title = "Select room file";
      _ofd.Filter = "AGS Room file|*.crm";

      if (_ofd.ShowDialog(this) == DialogResult.OK)
      {
        ctrlStatusLabel.Text = STATUS_LOADING;

        string fileFullPath = _ofd.FileName;
        string sourceFolder = Path.GetDirectoryName(fileFullPath);

        _loadedRoom = new AGSRoom();

      retry:
        try
        {
          _loadedRoom.ReadFromFile(fileFullPath);

          ctrlBackgroundFrame.Items.Clear();

          int framesCount = (_loadedRoom.Background.Frames.Length == 0) ? 1 : _loadedRoom.Background.Frames.Length;
          for (int i = 0; i < framesCount; ++i)
          {
            if (_loadedRoom.Background.Frames[i] == null)
              continue;

            Bitmap image = _loadedRoom.Background.Frames[i];
            string name = (i == 0) ? "Main background" : ("Frame " + i);
            RoomFrame frame = new RoomFrame(image, name);

            ctrlBackgroundFrame.Items.Add(frame);
          }
          ctrlBackgroundFrame.SelectedIndex = 0;

          SetFileStatus(true);
          ctrlStatusLabel.Text = STATUS_LOADED;
        }
        catch (Exception ex)
        {
          string message = string.Format("Could not load file: {0}\n\n{1}\n\nDo you want to try again?", fileFullPath, ex.Message);
          DialogResult result = MessageBox.Show(this, message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
          if (result == DialogResult.Retry)
            goto retry;
          else
            ctrlStatusLabel.Text = STATUS_FAILED;
        }
      }
    }

    private void ctrlBackgroundFrame_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_loadedRoom == null)
        return;
      if (!(ctrlBackgroundFrame.SelectedItem is RoomFrame))
        return;

      RoomFrame roomFrame = (RoomFrame)ctrlBackgroundFrame.SelectedItem;
      roomFrame.Image = _loadedRoom.Background.Frames[ctrlBackgroundFrame.SelectedIndex];
      ctrlBackgroundImage.Image = roomFrame.Image;
    }

    private void btnBackgroundReplace_Click(object sender, EventArgs e)
    {
    retry:
      _ofd.CheckPathExists = true;
      _ofd.CheckFileExists = true;
      _ofd.Title = "Select bitmap file";
      _ofd.Filter = "Bitmap|*.bmp";

      if (_ofd.ShowDialog(this) == DialogResult.OK)
      {
        string fileFullPath = _ofd.FileName;

        //Image image = Bitmap.FromFile(fileFullPath);
        //NOTE(adm244): why this constructor converts image into Format32bppArgb?
        // WHAT IS THIS CRAP?! HELLO???
        //Bitmap bitmap = new Bitmap(image);

        Bitmap bitmap = new Bitmap(fileFullPath);

        int index = ctrlBackgroundFrame.SelectedIndex;

        if ((bitmap.Width != _loadedRoom.Background.Frames[index].Width)
          || (bitmap.Height != _loadedRoom.Background.Frames[index].Height))
        {
          DialogResult result = MessageBox.Show(this,
            "Selected image has different size than background image.\n\nSelect another image?",
            "Invalid image", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
          if (result == DialogResult.Yes)
            goto retry;

          return;
        }

        if (bitmap.PixelFormat != _loadedRoom.Background.Frames[index].PixelFormat)
        {
          //TODO(adm244): convert images
          string message = string.Format("Selected image has different pixel format than background image.\nExpected: {0}, Got: {1}\n\nSelect another image?",
            _loadedRoom.Background.Frames[index].PixelFormat, bitmap.PixelFormat);
          DialogResult result = MessageBox.Show(this,
            message,
            "Invalid image", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
          if (result == DialogResult.Yes)
            goto retry;

          return;
        }

        _loadedRoom.Background.Frames[index] = bitmap;
        //ctrlBackgroundFrame.SelectedIndex = index;
        ctrlBackgroundFrame_SelectedIndexChanged(this, new EventArgs());

        _hasChanges = true;
        ctrlStatusLabel.Text = STATUS_CHANGED;
      }
    }

    private void btnBackgroundSave_Click(object sender, EventArgs e)
    {
      _sfd.CheckPathExists = true;
      _sfd.CheckFileExists = false;
      _sfd.Title = "Save to";
      _sfd.Filter = "Bitmap|*.bmp";

      if (_sfd.ShowDialog(this) == DialogResult.OK)
      {
        string fileFullPath = _sfd.FileName;

        int index = ctrlBackgroundFrame.SelectedIndex;
        _loadedRoom.Background.Frames[index].Save(fileFullPath, ImageFormat.Bmp);

        MessageBox.Show(this, "Background image was successefully exported.",
          "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        ctrlStatusLabel.Text = STATUS_SAVED;
      }
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!VerifyAction())
        return;

      _loadedRoom = null;
      _hasChanges = false;
      ctrlBackgroundFrame.Items.Clear();
      ctrlBackgroundImage.Image = ctrlBackgroundImage.InitialImage;
      SetFileStatus(false);
      ctrlStatusLabel.Text = STATUS_READY;
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      _sfd.CheckPathExists = true;
      _sfd.CheckFileExists = false;
      _sfd.Title = "Save to";
      _sfd.Filter = "AGS Room file|*.crm";

      if (_sfd.ShowDialog(this) == DialogResult.OK)
      {
        string fileFullPath = _sfd.FileName;

      retry:
        try
        {
          _loadedRoom.WriteToFile(fileFullPath, _loadedRoom.Version);

          MessageBox.Show(this, "Room was successefully saved.",
            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

          _hasChanges = false;
          ctrlStatusLabel.Text = STATUS_SAVED;
        }
        catch (Exception ex)
        {
          string message = string.Format("Could not save file: {0}\n\n{1}\n\nDo you want to try again?", fileFullPath, ex.Message);
          DialogResult result = MessageBox.Show(this, message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
          if (result == DialogResult.Retry)
            goto retry;
          else
            ctrlStatusLabel.Text = STATUS_FAILED;
        }
      }
    }
  }
}
