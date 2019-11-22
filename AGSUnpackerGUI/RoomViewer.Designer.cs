namespace AGSUnpackerGUI
{
  partial class RoomViewer
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.ctrlStatus = new System.Windows.Forms.StatusStrip();
      this.ctrlStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.ctrlMainMenu = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.splitContainerMain = new System.Windows.Forms.SplitContainer();
      this.ctrlBackgroundFrame = new System.Windows.Forms.ComboBox();
      this.labelFrame = new System.Windows.Forms.Label();
      this.btnBackgroundReplace = new System.Windows.Forms.Button();
      this.ctrlBackgroundImage = new System.Windows.Forms.PictureBox();
      this.btnBackgroundSave = new System.Windows.Forms.Button();
      this.ctrlStatus.SuspendLayout();
      this.ctrlMainMenu.SuspendLayout();
      this.splitContainerMain.Panel1.SuspendLayout();
      this.splitContainerMain.Panel2.SuspendLayout();
      this.splitContainerMain.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ctrlBackgroundImage)).BeginInit();
      this.SuspendLayout();
      // 
      // ctrlStatus
      // 
      this.ctrlStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctrlStatusLabel});
      this.ctrlStatus.Location = new System.Drawing.Point(0, 258);
      this.ctrlStatus.Name = "ctrlStatus";
      this.ctrlStatus.Size = new System.Drawing.Size(464, 22);
      this.ctrlStatus.TabIndex = 0;
      // 
      // ctrlStatusLabel
      // 
      this.ctrlStatusLabel.Name = "ctrlStatusLabel";
      this.ctrlStatusLabel.Size = new System.Drawing.Size(42, 17);
      this.ctrlStatusLabel.Text = "Ready.";
      // 
      // ctrlMainMenu
      // 
      this.ctrlMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
      this.ctrlMainMenu.Location = new System.Drawing.Point(0, 0);
      this.ctrlMainMenu.Name = "ctrlMainMenu";
      this.ctrlMainMenu.Size = new System.Drawing.Size(464, 24);
      this.ctrlMainMenu.TabIndex = 1;
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.toolStripMenuItem1,
            this.quitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "&File";
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.openToolStripMenuItem.Text = "&Open";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
      // 
      // saveToolStripMenuItem
      // 
      this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
      this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.saveToolStripMenuItem.Text = "&Save";
      this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
      // 
      // closeToolStripMenuItem
      // 
      this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
      this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
      this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.closeToolStripMenuItem.Text = "&Close";
      this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
      // 
      // quitToolStripMenuItem
      // 
      this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
      this.quitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.quitToolStripMenuItem.Text = "&Quit";
      this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
      // 
      // splitContainerMain
      // 
      this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainerMain.IsSplitterFixed = true;
      this.splitContainerMain.Location = new System.Drawing.Point(0, 24);
      this.splitContainerMain.Name = "splitContainerMain";
      this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainerMain.Panel1
      // 
      this.splitContainerMain.Panel1.Controls.Add(this.btnBackgroundReplace);
      this.splitContainerMain.Panel1.Controls.Add(this.btnBackgroundSave);
      this.splitContainerMain.Panel1.Controls.Add(this.ctrlBackgroundFrame);
      this.splitContainerMain.Panel1.Controls.Add(this.labelFrame);
      this.splitContainerMain.Panel1MinSize = 23;
      // 
      // splitContainerMain.Panel2
      // 
      this.splitContainerMain.Panel2.AutoScroll = true;
      this.splitContainerMain.Panel2.Controls.Add(this.ctrlBackgroundImage);
      this.splitContainerMain.Size = new System.Drawing.Size(464, 234);
      this.splitContainerMain.SplitterDistance = 23;
      this.splitContainerMain.TabIndex = 2;
      // 
      // ctrlBackgroundFrame
      // 
      this.ctrlBackgroundFrame.Dock = System.Windows.Forms.DockStyle.Left;
      this.ctrlBackgroundFrame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ctrlBackgroundFrame.FormattingEnabled = true;
      this.ctrlBackgroundFrame.Location = new System.Drawing.Point(47, 0);
      this.ctrlBackgroundFrame.Name = "ctrlBackgroundFrame";
      this.ctrlBackgroundFrame.Size = new System.Drawing.Size(121, 21);
      this.ctrlBackgroundFrame.TabIndex = 0;
      this.ctrlBackgroundFrame.SelectedIndexChanged += new System.EventHandler(this.ctrlBackgroundFrame_SelectedIndexChanged);
      // 
      // labelFrame
      // 
      this.labelFrame.AutoSize = true;
      this.labelFrame.Dock = System.Windows.Forms.DockStyle.Left;
      this.labelFrame.Location = new System.Drawing.Point(0, 0);
      this.labelFrame.Name = "labelFrame";
      this.labelFrame.Padding = new System.Windows.Forms.Padding(8, 4, 0, 0);
      this.labelFrame.Size = new System.Drawing.Size(47, 17);
      this.labelFrame.TabIndex = 1;
      this.labelFrame.Text = "Frame:";
      // 
      // btnBackgroundReplace
      // 
      this.btnBackgroundReplace.Dock = System.Windows.Forms.DockStyle.Left;
      this.btnBackgroundReplace.Location = new System.Drawing.Point(243, 0);
      this.btnBackgroundReplace.Name = "btnBackgroundReplace";
      this.btnBackgroundReplace.Size = new System.Drawing.Size(75, 23);
      this.btnBackgroundReplace.TabIndex = 2;
      this.btnBackgroundReplace.Text = "Replace";
      this.btnBackgroundReplace.UseVisualStyleBackColor = true;
      this.btnBackgroundReplace.Click += new System.EventHandler(this.btnBackgroundReplace_Click);
      // 
      // ctrlBackgroundImage
      // 
      this.ctrlBackgroundImage.Location = new System.Drawing.Point(0, 0);
      this.ctrlBackgroundImage.Name = "ctrlBackgroundImage";
      this.ctrlBackgroundImage.Size = new System.Drawing.Size(100, 50);
      this.ctrlBackgroundImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.ctrlBackgroundImage.TabIndex = 0;
      this.ctrlBackgroundImage.TabStop = false;
      // 
      // btnBackgroundSave
      // 
      this.btnBackgroundSave.Dock = System.Windows.Forms.DockStyle.Left;
      this.btnBackgroundSave.Location = new System.Drawing.Point(168, 0);
      this.btnBackgroundSave.Name = "btnBackgroundSave";
      this.btnBackgroundSave.Size = new System.Drawing.Size(75, 23);
      this.btnBackgroundSave.TabIndex = 3;
      this.btnBackgroundSave.Text = "Save";
      this.btnBackgroundSave.UseVisualStyleBackColor = true;
      this.btnBackgroundSave.Click += new System.EventHandler(this.btnBackgroundSave_Click);
      // 
      // RoomViewer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(464, 280);
      this.Controls.Add(this.splitContainerMain);
      this.Controls.Add(this.ctrlStatus);
      this.Controls.Add(this.ctrlMainMenu);
      this.MainMenuStrip = this.ctrlMainMenu;
      this.Name = "RoomViewer";
      this.Text = "RoomViewer";
      this.ctrlStatus.ResumeLayout(false);
      this.ctrlStatus.PerformLayout();
      this.ctrlMainMenu.ResumeLayout(false);
      this.ctrlMainMenu.PerformLayout();
      this.splitContainerMain.Panel1.ResumeLayout(false);
      this.splitContainerMain.Panel1.PerformLayout();
      this.splitContainerMain.Panel2.ResumeLayout(false);
      this.splitContainerMain.Panel2.PerformLayout();
      this.splitContainerMain.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.ctrlBackgroundImage)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.StatusStrip ctrlStatus;
    private System.Windows.Forms.ToolStripStatusLabel ctrlStatusLabel;
    private System.Windows.Forms.MenuStrip ctrlMainMenu;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
    private System.Windows.Forms.SplitContainer splitContainerMain;
    private System.Windows.Forms.ComboBox ctrlBackgroundFrame;
    private System.Windows.Forms.Label labelFrame;
    private System.Windows.Forms.Button btnBackgroundReplace;
    private System.Windows.Forms.PictureBox ctrlBackgroundImage;
    private System.Windows.Forms.Button btnBackgroundSave;
  }
}