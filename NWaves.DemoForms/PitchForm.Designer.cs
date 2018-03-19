using NWaves.DemoForms.UserControls;

namespace NWaves.DemoForms
{
    partial class PitchForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.prevButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.specNoComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cepstrumSizeTextBox = new System.Windows.Forms.TextBox();
            this.hopSizeTextBox = new System.Windows.Forms.TextBox();
            this.fftSizeTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.spectrogramPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.autoCorrPanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.cepstrumPanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.spectrumPanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1204, 28);
            this.menuStrip1.TabIndex = 0;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(260, 327);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Spectrum";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(678, 327);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Cepstrum";
            // 
            // prevButton
            // 
            this.prevButton.Location = new System.Drawing.Point(1001, 55);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(63, 24);
            this.prevButton.TabIndex = 5;
            this.prevButton.Text = "Prev";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(1135, 55);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(56, 24);
            this.nextButton.TabIndex = 6;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // specNoComboBox
            // 
            this.specNoComboBox.FormattingEnabled = true;
            this.specNoComboBox.ItemHeight = 16;
            this.specNoComboBox.Location = new System.Drawing.Point(1070, 55);
            this.specNoComboBox.Name = "specNoComboBox";
            this.specNoComboBox.Size = new System.Drawing.Size(59, 24);
            this.specNoComboBox.TabIndex = 7;
            this.specNoComboBox.TextChanged += new System.EventHandler(this.specNoComboBox_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cepstrumSizeTextBox);
            this.groupBox1.Controls.Add(this.hopSizeTextBox);
            this.groupBox1.Controls.Add(this.fftSizeTextBox);
            this.groupBox1.Location = new System.Drawing.Point(1001, 105);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(190, 206);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 135);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 17);
            this.label5.TabIndex = 16;
            this.label5.Text = "Cepstrum size";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Hop size";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "FFT size";
            // 
            // cepstrumSizeTextBox
            // 
            this.cepstrumSizeTextBox.Location = new System.Drawing.Point(121, 130);
            this.cepstrumSizeTextBox.Name = "cepstrumSizeTextBox";
            this.cepstrumSizeTextBox.Size = new System.Drawing.Size(55, 22);
            this.cepstrumSizeTextBox.TabIndex = 13;
            this.cepstrumSizeTextBox.Text = "256";
            // 
            // hopSizeTextBox
            // 
            this.hopSizeTextBox.Location = new System.Drawing.Point(121, 89);
            this.hopSizeTextBox.Name = "hopSizeTextBox";
            this.hopSizeTextBox.Size = new System.Drawing.Size(55, 22);
            this.hopSizeTextBox.TabIndex = 12;
            this.hopSizeTextBox.Text = "100";
            // 
            // fftSizeTextBox
            // 
            this.fftSizeTextBox.Location = new System.Drawing.Point(121, 48);
            this.fftSizeTextBox.Name = "fftSizeTextBox";
            this.fftSizeTextBox.Size = new System.Drawing.Size(55, 22);
            this.fftSizeTextBox.TabIndex = 11;
            this.fftSizeTextBox.Text = "1024";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(992, 327);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 17);
            this.label6.TabIndex = 6;
            this.label6.Text = "Auto-correlation";
            // 
            // spectrogramPanel
            // 
            this.spectrogramPanel.AutoScroll = true;
            this.spectrogramPanel.BackColor = System.Drawing.Color.White;
            this.spectrogramPanel.ColorMapName = "magma";
            this.spectrogramPanel.Location = new System.Drawing.Point(12, 55);
            this.spectrogramPanel.Markline = null;
            this.spectrogramPanel.Name = "spectrogramPanel";
            this.spectrogramPanel.Size = new System.Drawing.Size(973, 256);
            this.spectrogramPanel.Spectrogram = null;
            this.spectrogramPanel.TabIndex = 2;
            // 
            // autoCorrPanel
            // 
            this.autoCorrPanel.AutoScroll = true;
            this.autoCorrPanel.BackColor = System.Drawing.Color.White;
            this.autoCorrPanel.ForeColor = System.Drawing.Color.Blue;
            this.autoCorrPanel.Gain = null;
            this.autoCorrPanel.Legend = null;
            this.autoCorrPanel.Line = null;
            this.autoCorrPanel.Location = new System.Drawing.Point(880, 347);
            this.autoCorrPanel.Mark = null;
            this.autoCorrPanel.Markline = null;
            this.autoCorrPanel.Name = "autoCorrPanel";
            this.autoCorrPanel.PaddingX = 30;
            this.autoCorrPanel.PaddingY = 20;
            this.autoCorrPanel.Size = new System.Drawing.Size(311, 256);
            this.autoCorrPanel.Stride = 20;
            this.autoCorrPanel.TabIndex = 5;
            this.autoCorrPanel.Thickness = 1;
            // 
            // cepstrumPanel
            // 
            this.cepstrumPanel.AutoScroll = true;
            this.cepstrumPanel.BackColor = System.Drawing.Color.White;
            this.cepstrumPanel.ForeColor = System.Drawing.Color.Blue;
            this.cepstrumPanel.Gain = null;
            this.cepstrumPanel.Legend = null;
            this.cepstrumPanel.Line = null;
            this.cepstrumPanel.Location = new System.Drawing.Point(570, 347);
            this.cepstrumPanel.Mark = null;
            this.cepstrumPanel.Markline = null;
            this.cepstrumPanel.Name = "cepstrumPanel";
            this.cepstrumPanel.PaddingX = 30;
            this.cepstrumPanel.PaddingY = 20;
            this.cepstrumPanel.Size = new System.Drawing.Size(304, 256);
            this.cepstrumPanel.Stride = 20;
            this.cepstrumPanel.TabIndex = 2;
            this.cepstrumPanel.Thickness = 1;
            // 
            // spectrumPanel
            // 
            this.spectrumPanel.AutoScroll = true;
            this.spectrumPanel.BackColor = System.Drawing.Color.White;
            this.spectrumPanel.ForeColor = System.Drawing.Color.Blue;
            this.spectrumPanel.Gain = null;
            this.spectrumPanel.Legend = null;
            this.spectrumPanel.Line = null;
            this.spectrumPanel.Location = new System.Drawing.Point(14, 347);
            this.spectrumPanel.Mark = null;
            this.spectrumPanel.Markline = null;
            this.spectrumPanel.Name = "spectrumPanel";
            this.spectrumPanel.PaddingX = 30;
            this.spectrumPanel.PaddingY = 20;
            this.spectrumPanel.Size = new System.Drawing.Size(550, 256);
            this.spectrumPanel.Stride = 1;
            this.spectrumPanel.TabIndex = 1;
            this.spectrumPanel.Thickness = 1;
            // 
            // PitchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1204, 617);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.spectrogramPanel);
            this.Controls.Add(this.autoCorrPanel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.specNoComboBox);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.prevButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cepstrumPanel);
            this.Controls.Add(this.spectrumPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PitchForm";
            this.Text = "PitchForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private LinePlot spectrumPanel;
        private LinePlot cepstrumPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.ComboBox specNoComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox cepstrumSizeTextBox;
        private System.Windows.Forms.TextBox hopSizeTextBox;
        private System.Windows.Forms.TextBox fftSizeTextBox;
        private SpectrogramPlot spectrogramPanel;
        private System.Windows.Forms.Label label6;
        private LinePlot autoCorrPanel;
    }
}