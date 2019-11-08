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
      this.tbLogOutput = new System.Windows.Forms.TextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.ctrlStatus = new System.Windows.Forms.ToolStripStatusLabel();
      this.gbUnpacking.SuspendLayout();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbUnpacking
      // 
      this.gbUnpacking.Controls.Add(this.btnExtractTrsExe);
      this.gbUnpacking.Controls.Add(this.btnUnpackSprites);
      this.gbUnpacking.Controls.Add(this.btnExtractExe);
      this.gbUnpacking.Location = new System.Drawing.Point(12, 12);
      this.gbUnpacking.Name = "gbUnpacking";
      this.gbUnpacking.Size = new System.Drawing.Size(200, 108);
      this.gbUnpacking.TabIndex = 0;
      this.gbUnpacking.TabStop = false;
      this.gbUnpacking.Text = "Unpacking";
      // 
      // btnExtractTrsExe
      // 
      this.btnExtractTrsExe.Location = new System.Drawing.Point(6, 77);
      this.btnExtractTrsExe.Name = "btnExtractTrsExe";
      this.btnExtractTrsExe.Size = new System.Drawing.Size(188, 23);
      this.btnExtractTrsExe.TabIndex = 2;
      this.btnExtractTrsExe.Text = "Extract TRS from EXE";
      this.btnExtractTrsExe.UseVisualStyleBackColor = true;
      this.btnExtractTrsExe.Click += new System.EventHandler(this.btnExtractTrsExe_Click);
      // 
      // btnUnpackSprites
      // 
      this.btnUnpackSprites.Location = new System.Drawing.Point(6, 48);
      this.btnUnpackSprites.Name = "btnUnpackSprites";
      this.btnUnpackSprites.Size = new System.Drawing.Size(188, 23);
      this.btnUnpackSprites.TabIndex = 1;
      this.btnUnpackSprites.Text = "Extract Sprites";
      this.btnUnpackSprites.UseVisualStyleBackColor = true;
      this.btnUnpackSprites.Click += new System.EventHandler(this.btnUnpackSprites_Click);
      // 
      // btnExtractExe
      // 
      this.btnExtractExe.Location = new System.Drawing.Point(6, 19);
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
      this.splitContainer.Panel1.Controls.Add(this.gbUnpacking);
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.tbLogOutput);
      this.splitContainer.Size = new System.Drawing.Size(574, 250);
      this.splitContainer.SplitterDistance = 222;
      this.splitContainer.TabIndex = 1;
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
      this.tbLogOutput.Text = "AGSUnpacker v0.1 Log Output\r\n\r\n> ";
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
      this.Text = "AGSUnpacker v0.1";
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.gbUnpacking.ResumeLayout(false);
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      this.splitContainer.Panel2.PerformLayout();
      this.splitContainer.ResumeLayout(false);
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
  }
}

