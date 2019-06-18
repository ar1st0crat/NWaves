namespace NWaves.DemoForms
{
    partial class StftForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsComboBox = new System.Windows.Forms.ComboBox();
            this.windowPlot = new NWaves.DemoForms.UserControls.LinePlot();
            this.spectrogramPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.processedSignalPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.signalPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.play2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playToolStripMenuItem,
            this.play2ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1105, 28);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.playToolStripMenuItem.Text = "Play&1";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // windowsComboBox
            // 
            this.windowsComboBox.FormattingEnabled = true;
            this.windowsComboBox.Location = new System.Drawing.Point(773, 394);
            this.windowsComboBox.Name = "windowsComboBox";
            this.windowsComboBox.Size = new System.Drawing.Size(319, 24);
            this.windowsComboBox.TabIndex = 13;
            this.windowsComboBox.SelectedIndexChanged += new System.EventHandler(this.windowsComboBox_SelectedIndexChanged);
            // 
            // windowPlot
            // 
            this.windowPlot.AutoScroll = true;
            this.windowPlot.BackColor = System.Drawing.Color.White;
            this.windowPlot.ForeColor = System.Drawing.Color.Blue;
            this.windowPlot.Gain = null;
            this.windowPlot.Legend = null;
            this.windowPlot.Line = null;
            this.windowPlot.Location = new System.Drawing.Point(773, 426);
            this.windowPlot.Mark = null;
            this.windowPlot.Markline = null;
            this.windowPlot.Name = "windowPlot";
            this.windowPlot.PaddingX = 30;
            this.windowPlot.PaddingY = 20;
            this.windowPlot.Size = new System.Drawing.Size(320, 221);
            this.windowPlot.Stride = 1;
            this.windowPlot.TabIndex = 12;
            this.windowPlot.Thickness = 1;
            // 
            // spectrogramPanel
            // 
            this.spectrogramPanel.AutoScroll = true;
            this.spectrogramPanel.BackColor = System.Drawing.Color.Black;
            this.spectrogramPanel.ColorMapName = "magma";
            this.spectrogramPanel.Location = new System.Drawing.Point(13, 394);
            this.spectrogramPanel.Markline = null;
            this.spectrogramPanel.MarklineThickness = 0;
            this.spectrogramPanel.Name = "spectrogramPanel";
            this.spectrogramPanel.Size = new System.Drawing.Size(754, 256);
            this.spectrogramPanel.Spectrogram = null;
            this.spectrogramPanel.TabIndex = 11;
            // 
            // processedSignalPanel
            // 
            this.processedSignalPanel.AutoScroll = true;
            this.processedSignalPanel.BackColor = System.Drawing.Color.White;
            this.processedSignalPanel.ForeColor = System.Drawing.Color.Blue;
            this.processedSignalPanel.Gain = 1F;
            this.processedSignalPanel.Location = new System.Drawing.Point(12, 213);
            this.processedSignalPanel.Name = "processedSignalPanel";
            this.processedSignalPanel.PaddingX = 24;
            this.processedSignalPanel.PaddingY = 5;
            this.processedSignalPanel.Signal = null;
            this.processedSignalPanel.Size = new System.Drawing.Size(1080, 165);
            this.processedSignalPanel.Stride = 64;
            this.processedSignalPanel.TabIndex = 10;
            // 
            // signalPanel
            // 
            this.signalPanel.AutoScroll = true;
            this.signalPanel.BackColor = System.Drawing.Color.White;
            this.signalPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalPanel.Gain = 1F;
            this.signalPanel.Location = new System.Drawing.Point(12, 42);
            this.signalPanel.Name = "signalPanel";
            this.signalPanel.PaddingX = 24;
            this.signalPanel.PaddingY = 5;
            this.signalPanel.Signal = null;
            this.signalPanel.Size = new System.Drawing.Size(1081, 165);
            this.signalPanel.Stride = 64;
            this.signalPanel.TabIndex = 9;
            // 
            // play2ToolStripMenuItem
            // 
            this.play2ToolStripMenuItem.Name = "play2ToolStripMenuItem";
            this.play2ToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.play2ToolStripMenuItem.Text = "Play&2";
            this.play2ToolStripMenuItem.Click += new System.EventHandler(this.play2ToolStripMenuItem_Click);
            // 
            // StftForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1105, 659);
            this.Controls.Add(this.windowsComboBox);
            this.Controls.Add(this.windowPlot);
            this.Controls.Add(this.spectrogramPanel);
            this.Controls.Add(this.processedSignalPanel);
            this.Controls.Add(this.signalPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StftForm";
            this.Text = "StftForm";
            this.Load += new System.EventHandler(this.StftForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private UserControls.SignalPlot signalPanel;
        private UserControls.SignalPlot processedSignalPanel;
        private UserControls.SpectrogramPlot spectrogramPanel;
        private UserControls.LinePlot windowPlot;
        private System.Windows.Forms.ComboBox windowsComboBox;
        private System.Windows.Forms.ToolStripMenuItem play2ToolStripMenuItem;
    }
}