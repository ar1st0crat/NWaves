namespace NWaves.DemoForms
{
    partial class NoiseForm
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
            this.signalPlot = new NWaves.DemoForms.UserControls.SignalPlot();
            this.noisePlot = new NWaves.DemoForms.UserControls.SignalPlot();
            this.processedPlot = new NWaves.DemoForms.UserControls.SignalPlot();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadnoiseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadsignalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.processToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.playSignalButton = new System.Windows.Forms.Button();
            this.playNoiseButton = new System.Windows.Forms.Button();
            this.playProcessedButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // signalPlot
            // 
            this.signalPlot.AutoScroll = true;
            this.signalPlot.BackColor = System.Drawing.Color.White;
            this.signalPlot.ForeColor = System.Drawing.Color.Blue;
            this.signalPlot.Gain = 1F;
            this.signalPlot.Location = new System.Drawing.Point(13, 53);
            this.signalPlot.Name = "signalPlot";
            this.signalPlot.PaddingX = 24;
            this.signalPlot.PaddingY = 5;
            this.signalPlot.Signal = null;
            this.signalPlot.Size = new System.Drawing.Size(993, 156);
            this.signalPlot.Stride = 64;
            this.signalPlot.TabIndex = 0;
            // 
            // noisePlot
            // 
            this.noisePlot.AutoScroll = true;
            this.noisePlot.BackColor = System.Drawing.Color.White;
            this.noisePlot.ForeColor = System.Drawing.Color.Blue;
            this.noisePlot.Gain = 1F;
            this.noisePlot.Location = new System.Drawing.Point(13, 242);
            this.noisePlot.Name = "noisePlot";
            this.noisePlot.PaddingX = 24;
            this.noisePlot.PaddingY = 5;
            this.noisePlot.Signal = null;
            this.noisePlot.Size = new System.Drawing.Size(993, 156);
            this.noisePlot.Stride = 64;
            this.noisePlot.TabIndex = 1;
            // 
            // processedPlot
            // 
            this.processedPlot.AutoScroll = true;
            this.processedPlot.BackColor = System.Drawing.Color.White;
            this.processedPlot.ForeColor = System.Drawing.Color.Blue;
            this.processedPlot.Gain = 1F;
            this.processedPlot.Location = new System.Drawing.Point(12, 434);
            this.processedPlot.Name = "processedPlot";
            this.processedPlot.PaddingX = 24;
            this.processedPlot.PaddingY = 5;
            this.processedPlot.Signal = null;
            this.processedPlot.Size = new System.Drawing.Size(994, 156);
            this.processedPlot.Stride = 64;
            this.processedPlot.TabIndex = 2;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.processToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1018, 28);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadsignalToolStripMenuItem,
            this.loadnoiseToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadnoiseToolStripMenuItem
            // 
            this.loadnoiseToolStripMenuItem.Name = "loadnoiseToolStripMenuItem";
            this.loadnoiseToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.loadnoiseToolStripMenuItem.Text = "Load &noise";
            this.loadnoiseToolStripMenuItem.Click += new System.EventHandler(this.loadnoiseToolStripMenuItem_Click);
            // 
            // loadsignalToolStripMenuItem
            // 
            this.loadsignalToolStripMenuItem.Name = "loadsignalToolStripMenuItem";
            this.loadsignalToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.loadsignalToolStripMenuItem.Text = "Load &signal";
            this.loadsignalToolStripMenuItem.Click += new System.EventHandler(this.loadsignalToolStripMenuItem_Click);
            // 
            // processToolStripMenuItem
            // 
            this.processToolStripMenuItem.Name = "processToolStripMenuItem";
            this.processToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.processToolStripMenuItem.Text = "&Process";
            this.processToolStripMenuItem.Click += new System.EventHandler(this.processToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(494, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Signal";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(494, 222);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Noise";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(479, 414);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Processed";
            // 
            // playSignalButton
            // 
            this.playSignalButton.Location = new System.Drawing.Point(931, 53);
            this.playSignalButton.Name = "playSignalButton";
            this.playSignalButton.Size = new System.Drawing.Size(75, 31);
            this.playSignalButton.TabIndex = 7;
            this.playSignalButton.Text = "Play";
            this.playSignalButton.UseVisualStyleBackColor = true;
            this.playSignalButton.Click += new System.EventHandler(this.playSignalButton_Click);
            // 
            // playNoiseButton
            // 
            this.playNoiseButton.Location = new System.Drawing.Point(931, 242);
            this.playNoiseButton.Name = "playNoiseButton";
            this.playNoiseButton.Size = new System.Drawing.Size(75, 31);
            this.playNoiseButton.TabIndex = 8;
            this.playNoiseButton.Text = "Play";
            this.playNoiseButton.UseVisualStyleBackColor = true;
            this.playNoiseButton.Click += new System.EventHandler(this.playNoiseButton_Click);
            // 
            // playProcessedButton
            // 
            this.playProcessedButton.Location = new System.Drawing.Point(931, 434);
            this.playProcessedButton.Name = "playProcessedButton";
            this.playProcessedButton.Size = new System.Drawing.Size(75, 31);
            this.playProcessedButton.TabIndex = 9;
            this.playProcessedButton.Text = "Play";
            this.playProcessedButton.UseVisualStyleBackColor = true;
            this.playProcessedButton.Click += new System.EventHandler(this.playProcessedButton_Click);
            // 
            // NoiseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1018, 602);
            this.Controls.Add(this.playProcessedButton);
            this.Controls.Add(this.playNoiseButton);
            this.Controls.Add(this.playSignalButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.processedPlot);
            this.Controls.Add(this.noisePlot);
            this.Controls.Add(this.signalPlot);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "NoiseForm";
            this.Text = "NoiseForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UserControls.SignalPlot signalPlot;
        private UserControls.SignalPlot noisePlot;
        private UserControls.SignalPlot processedPlot;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadsignalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadnoiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem processToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button playSignalButton;
        private System.Windows.Forms.Button playNoiseButton;
        private System.Windows.Forms.Button playProcessedButton;
    }
}