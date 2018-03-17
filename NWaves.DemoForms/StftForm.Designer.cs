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
            this.signalPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.processedSignalPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.spectrogramPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playToolStripMenuItem});
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
            this.playToolStripMenuItem.Size = new System.Drawing.Size(48, 24);
            this.playToolStripMenuItem.Text = "&Play";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // signalPanel
            // 
            this.signalPanel.AutoScroll = true;
            this.signalPanel.BackColor = System.Drawing.Color.White;
            this.signalPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalPanel.Gain = 1;
            this.signalPanel.Location = new System.Drawing.Point(12, 42);
            this.signalPanel.Name = "signalPanel";
            this.signalPanel.Signal = null;
            this.signalPanel.Size = new System.Drawing.Size(1081, 165);
            this.signalPanel.Stride = 64;
            this.signalPanel.TabIndex = 9;
            // 
            // processedSignalPanel
            // 
            this.processedSignalPanel.AutoScroll = true;
            this.processedSignalPanel.BackColor = System.Drawing.Color.White;
            this.processedSignalPanel.ForeColor = System.Drawing.Color.Blue;
            this.processedSignalPanel.Gain = 1;
            this.processedSignalPanel.Location = new System.Drawing.Point(13, 214);
            this.processedSignalPanel.Name = "processedSignalPanel";
            this.processedSignalPanel.Signal = null;
            this.processedSignalPanel.Size = new System.Drawing.Size(1080, 173);
            this.processedSignalPanel.Stride = 64;
            this.processedSignalPanel.TabIndex = 10;
            // 
            // spectrogramPanel
            // 
            this.spectrogramPanel.AutoScroll = true;
            this.spectrogramPanel.BackColor = System.Drawing.Color.Black;
            this.spectrogramPanel.ColorMapName = "magma";
            this.spectrogramPanel.Location = new System.Drawing.Point(13, 394);
            this.spectrogramPanel.Markline = null;
            this.spectrogramPanel.Name = "spectrogramPanel";
            this.spectrogramPanel.Size = new System.Drawing.Size(1080, 230);
            this.spectrogramPanel.Spectrogram = null;
            this.spectrogramPanel.TabIndex = 11;
            // 
            // StftForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1105, 636);
            this.Controls.Add(this.spectrogramPanel);
            this.Controls.Add(this.processedSignalPanel);
            this.Controls.Add(this.signalPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StftForm";
            this.Text = "StftForm";
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
    }
}