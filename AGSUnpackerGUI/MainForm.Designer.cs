namespace AGSUnpackerGUI
{
  partial class MainForm
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
      this.gbUnpacking = new System.Windows.Forms.GroupBox();
      this.btnExtractTrsExe = new System.Windows.Forms.Button();
      this.btnUnpackSprites = new System.Windows.Forms.Button();
      this.btnExtractExe = new System.Windows.Forms.Button();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      this.gbExtra = new System.Windows.Forms.GroupBox();
      this.btnChangeRoomBackground = new System.Windows.Forms.Button();
      this.btnGetUIDName = new System.Windows.Forms.Button();
      this.tbLogOutput = new System.Windows.Forms.TextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.ctrlStatus = new System.Windows.Forms.ToolStripStatusLabel();
      this.gbUnpacking.SuspendLayout();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.gbExtra.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbUnpacking
      // 
      this.gbUnpacking.Controls.Add(this.btnExtractTrsExe);
      this.gbUnpacking.Controls.Add(this.btnUnpackSprites);
      this.gbUnpacking.Controls.Add(this.btnExtractExe);
      this.gbUnpacking.Dock = System.Windows.Forms.DockStyle.Top;
      this.gbUnpacking.Location = new System.Drawing.Point(5, 5);
      this.gbUnpacking.Name = "gbUnpacking";
      this.gbUnpacking.Padding = new System.Windows.Forms.Padding(12);
      this.gbUnpacking.Size = new System.Drawing.Size(212, 108);
      this.gbUnpacking.TabIndex = 0;
      this.gbUnpacking.TabStop = false;
      this.gbUnpacking.Text = "Unpacking";
      // 
      // btnExtractTrsExe
      // 
      this.btnExtractTrsExe.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnExtractTrsExe.Location = new System.Drawing.Point(12, 71);
      this.btnExtractTrsExe.Name = "btnExtractTrsExe";
      this.btnExtractTrsExe.Size = new System.Drawing.Size(188, 23);
      this.btnExtractTrsExe.TabIndex = 2;
      this.btnExtractTrsExe.Text = "Extract TRS from Assets";
      this.btnExtractTrsExe.UseVisualStyleBackColor = true;
      this.btnExtractTrsExe.Click += new System.EventHandler(this.btnExtractTrsExe_Click);
      // 
      // btnUnpackSprites
      // 
      this.btnUnpackSprites.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnUnpackSprites.Location = new System.Drawing.Point(12, 48);
      this.btnUnpackSprites.Name = "btnUnpackSprites";
      this.btnUnpackSprites.Size = new System.Drawing.Size(188, 23);
      this.btnUnpackSprites.TabIndex = 1;
      this.btnUnpackSprites.Text = "Extract Sprites";
      this.btnUnpackSprites.UseVisualStyleBackColor = true;
      this.btnUnpackSprites.Click += new System.EventHandler(this.btnUnpackSprites_Click);
      // 
      // btnExtractExe
      // 
      this.btnExtractExe.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnExtractExe.Location = new System.Drawing.Point(12, 25);
      this.btnExtractExe.Name = "btnExtractExe";
      this.btnExtractExe.Size = new System.Drawing.Size(188, 23);
      this.btnExtractExe.TabIndex = 0;
      this.btnExtractExe.Text = "Extract from EXE";
      this.btnExtractExe.UseVisualStyleBackColor = true;
      this.btnExtractExe.Click += new System.EventHandler(this.btnExtractExe_Click);
      // 
      // splitContainer
      // 
      this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer.IsSplitterFixed = true;
      this.splitContainer.Location = new System.Drawing.Point(0, 0);
      this.splitContainer.Name = "splitContainer";
      // 
      // splitContainer.Panel1
      // 
      this.splitContainer.Panel1.Controls.Add(this.gbExtra);
      this.splitContainer.Panel1.Controls.Add(this.gbUnpacking);
      this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(5);
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.tbLogOutput);
      this.splitContainer.Size = new System.Drawing.Size(574, 250);
      this.splitContainer.SplitterDistance = 222;
      this.splitContainer.TabIndex = 1;
      // 
      // gbExtra
      // 
      this.gbExtra.Controls.Add(this.btnChangeRoomBackground);
      this.gbExtra.Controls.Add(this.btnGetUIDName);
      this.gbExtra.Dock = System.Windows.Forms.DockStyle.Top;
      this.gbExtra.Location = new System.Drawing.Point(5, 113);
      this.gbExtra.Name = "gbExtra";
      this.gbExtra.Padding = new System.Windows.Forms.Padding(12);
      this.gbExtra.Size = new System.Drawing.Size(212, 100);
      this.gbExtra.TabIndex = 1;
      this.gbExtra.TabStop = false;
      this.gbExtra.Text = "Extra";
      // 
      // btnChangeRoomBackground
      // 
      this.btnChangeRoomBackground.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnChangeRoomBackground.Location = new System.Drawing.Point(12, 48);
      this.btnChangeRoomBackground.Name = "btnChangeRoomBackground";
      this.btnChangeRoomBackground.Size = new System.Drawing.Size(188, 23);
      this.btnChangeRoomBackground.TabIndex = 1;
      this.btnChangeRoomBackground.Text = "Change Room Background";
      this.btnChangeRoomBackground.UseVisualStyleBackColor = true;
      this.btnChangeRoomBackground.Click += new System.EventHandler(this.btnChangeRoomBackground_Click);
      // 
      // btnGetUIDName
      // 
      this.btnGetUIDName.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnGetUIDName.Location = new System.Drawing.Point(12, 25);
      this.btnGetUIDName.Name = "btnGetUIDName";
      this.btnGetUIDName.Size = new System.Drawing.Size(188, 23);
      this.btnGetUIDName.TabIndex = 0;
      this.btnGetUIDName.Text = "Get UniqueID and Game Name";
      this.btnGetUIDName.UseVisualStyleBackColor = true;
      this.btnGetUIDName.Click += new System.EventHandler(this.btnGetUIDName_Click);
      // 
      // tbLogOutput
      // 
      this.tbLogOutput.AcceptsReturn = true;
      this.tbLogOutput.AcceptsTab = true;
      this.tbLogOutput.CausesValidation = false;
      this.tbLogOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbLogOutput.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbLogOutput.Location = new System.Drawing.Point(0, 0);
      this.tbLogOutput.Multiline = true;
      this.tbLogOutput.Name = "tbLogOutput";
      this.tbLogOutput.ReadOnly = true;
      this.tbLogOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbLogOutput.Size = new System.Drawing.Size(348, 250);
      this.tbLogOutput.TabIndex = 0;
      this.tbLogOutput.Text = "AGSUnpacker v0.3 Log Output\r\n\r\n> ";
      this.tbLogOutput.WordWrap = false;
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctrlStatus});
      this.statusStrip1.Location = new System.Drawing.Point(0, 250);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(574, 22);
      this.statusStrip1.TabIndex = 2;
      // 
      // ctrlStatus
      // 
      this.ctrlStatus.Name = "ctrlStatus";
      this.ctrlStatus.Size = new System.Drawing.Size(42, 17);
      this.ctrlStatus.Text = "Ready.";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(574, 272);
      this.Controls.Add(this.splitContainer);
      this.Controls.Add(this.statusStrip1);
      this.MinimumSize = new System.Drawing.Size(590, 310);
      this.Name = "MainForm";
      this.Text = "AGSUnpacker v0.3";
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.gbUnpacking.ResumeLayout(false);
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      this.splitContainer.Panel2.PerformLayout();
      this.splitContainer.ResumeLayout(false);
      this.gbExtra.ResumeLayout(false);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox gbUnpacking;
    private System.Windows.Forms.Button btnExtractExe;
    private System.Windows.Forms.Button btnUnpackSprites;
    private System.Windows.Forms.Button btnExtractTrsExe;
    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.TextBox tbLogOutput;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel ctrlStatus;
    private System.Windows.Forms.GroupBox gbExtra;
    private System.Windows.Forms.Button btnGetUIDName;
    private System.Windows.Forms.Button btnChangeRoomBackground;
  }
}

