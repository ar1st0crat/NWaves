namespace NWaves.DemoForms
{
    partial class HpssForm
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
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSizeTextBox = new System.Windows.Forms.TextBox();
            this.hopSizeTextBox = new System.Windows.Forms.TextBox();
            this.harmonicWindowTextBox = new System.Windows.Forms.TextBox();
            this.percussiveWindowTextBox = new System.Windows.Forms.TextBox();
            this.maskingComboBox = new System.Windows.Forms.ComboBox();
            this.evaluateButton = new System.Windows.Forms.Button();
            this.playButton1 = new System.Windows.Forms.Button();
            this.playButton2 = new System.Windows.Forms.Button();
            this.playButton3 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.spectrogramPlot3 = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.spectrogramPlot2 = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.spectrogramPlot1 = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1245, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // fftSizeTextBox
            // 
            this.fftSizeTextBox.Location = new System.Drawing.Point(13, 61);
            this.fftSizeTextBox.Name = "fftSizeTextBox";
            this.fftSizeTextBox.Size = new System.Drawing.Size(121, 22);
            this.fftSizeTextBox.TabIndex = 4;
            this.fftSizeTextBox.Text = "2048";
            // 
            // hopSizeTextBox
            // 
            this.hopSizeTextBox.Location = new System.Drawing.Point(12, 109);
            this.hopSizeTextBox.Name = "hopSizeTextBox";
            this.hopSizeTextBox.Size = new System.Drawing.Size(122, 22);
            this.hopSizeTextBox.TabIndex = 5;
            this.hopSizeTextBox.Text = "512";
            // 
            // harmonicWindowTextBox
            // 
            this.harmonicWindowTextBox.Location = new System.Drawing.Point(13, 160);
            this.harmonicWindowTextBox.Name = "harmonicWindowTextBox";
            this.harmonicWindowTextBox.Size = new System.Drawing.Size(121, 22);
            this.harmonicWindowTextBox.TabIndex = 6;
            this.harmonicWindowTextBox.Text = "17";
            // 
            // percussiveWindowTextBox
            // 
            this.percussiveWindowTextBox.Location = new System.Drawing.Point(13, 211);
            this.percussiveWindowTextBox.Name = "percussiveWindowTextBox";
            this.percussiveWindowTextBox.Size = new System.Drawing.Size(121, 22);
            this.percussiveWindowTextBox.TabIndex = 7;
            this.percussiveWindowTextBox.Text = "17";
            // 
            // maskingComboBox
            // 
            this.maskingComboBox.FormattingEnabled = true;
            this.maskingComboBox.Items.AddRange(new object[] {
            "Binary mask",
            "Wiener order 1",
            "Wiener order 2"});
            this.maskingComboBox.Location = new System.Drawing.Point(13, 261);
            this.maskingComboBox.Name = "maskingComboBox";
            this.maskingComboBox.Size = new System.Drawing.Size(121, 24);
            this.maskingComboBox.TabIndex = 8;
            this.maskingComboBox.Text = "Binary mask";
            // 
            // evaluateButton
            // 
            this.evaluateButton.Location = new System.Drawing.Point(13, 321);
            this.evaluateButton.Name = "evaluateButton";
            this.evaluateButton.Size = new System.Drawing.Size(121, 42);
            this.evaluateButton.TabIndex = 9;
            this.evaluateButton.Text = "Evaluate";
            this.evaluateButton.UseVisualStyleBackColor = true;
            this.evaluateButton.Click += new System.EventHandler(this.evaluateButton_Click);
            // 
            // playButton1
            // 
            this.playButton1.Location = new System.Drawing.Point(164, 31);
            this.playButton1.Name = "playButton1";
            this.playButton1.Size = new System.Drawing.Size(64, 32);
            this.playButton1.TabIndex = 10;
            this.playButton1.Text = "Play";
            this.playButton1.UseVisualStyleBackColor = true;
            this.playButton1.Click += new System.EventHandler(this.playButton1_Click);
            // 
            // playButton2
            // 
            this.playButton2.Location = new System.Drawing.Point(164, 293);
            this.playButton2.Name = "playButton2";
            this.playButton2.Size = new System.Drawing.Size(64, 32);
            this.playButton2.TabIndex = 11;
            this.playButton2.Text = "Play";
            this.playButton2.UseVisualStyleBackColor = true;
            this.playButton2.Click += new System.EventHandler(this.playButton2_Click);
            // 
            // playButton3
            // 
            this.playButton3.Location = new System.Drawing.Point(164, 555);
            this.playButton3.Name = "playButton3";
            this.playButton3.Size = new System.Drawing.Size(64, 32);
            this.playButton3.TabIndex = 12;
            this.playButton3.Text = "Play";
            this.playButton3.UseVisualStyleBackColor = true;
            this.playButton3.Click += new System.EventHandler(this.playButton3_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "FFT size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Hop size";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 17);
            this.label3.TabIndex = 15;
            this.label3.Text = "Harmonic window";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 17);
            this.label4.TabIndex = 16;
            this.label4.Text = "Percussive window";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 241);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "Masking mode";
            // 
            // spectrogramPlot3
            // 
            this.spectrogramPlot3.AutoScroll = true;
            this.spectrogramPlot3.BackColor = System.Drawing.Color.Black;
            this.spectrogramPlot3.ColorMapName = "magma";
            this.spectrogramPlot3.Location = new System.Drawing.Point(164, 555);
            this.spectrogramPlot3.Markline = null;
            this.spectrogramPlot3.MarklineThickness = 0;
            this.spectrogramPlot3.Name = "spectrogramPlot3";
            this.spectrogramPlot3.Size = new System.Drawing.Size(1069, 256);
            this.spectrogramPlot3.Spectrogram = null;
            this.spectrogramPlot3.TabIndex = 3;
            // 
            // spectrogramPlot2
            // 
            this.spectrogramPlot2.AutoScroll = true;
            this.spectrogramPlot2.BackColor = System.Drawing.Color.Black;
            this.spectrogramPlot2.ColorMapName = "magma";
            this.spectrogramPlot2.Location = new System.Drawing.Point(164, 293);
            this.spectrogramPlot2.Markline = null;
            this.spectrogramPlot2.MarklineThickness = 0;
            this.spectrogramPlot2.Name = "spectrogramPlot2";
            this.spectrogramPlot2.Size = new System.Drawing.Size(1069, 256);
            this.spectrogramPlot2.Spectrogram = null;
            this.spectrogramPlot2.TabIndex = 2;
            // 
            // spectrogramPlot1
            // 
            this.spectrogramPlot1.AutoScroll = true;
            this.spectrogramPlot1.BackColor = System.Drawing.Color.Black;
            this.spectrogramPlot1.ColorMapName = "magma";
            this.spectrogramPlot1.Location = new System.Drawing.Point(164, 31);
            this.spectrogramPlot1.Markline = null;
            this.spectrogramPlot1.MarklineThickness = 0;
            this.spectrogramPlot1.Name = "spectrogramPlot1";
            this.spectrogramPlot1.Size = new System.Drawing.Size(1069, 256);
            this.spectrogramPlot1.Spectrogram = null;
            this.spectrogramPlot1.TabIndex = 1;
            // 
            // HpssForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 818);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.playButton3);
            this.Controls.Add(this.playButton2);
            this.Controls.Add(this.playButton1);
            this.Controls.Add(this.evaluateButton);
            this.Controls.Add(this.maskingComboBox);
            this.Controls.Add(this.percussiveWindowTextBox);
            this.Controls.Add(this.harmonicWindowTextBox);
            this.Controls.Add(this.hopSizeTextBox);
            this.Controls.Add(this.fftSizeTextBox);
            this.Controls.Add(this.spectrogramPlot3);
            this.Controls.Add(this.spectrogramPlot2);
            this.Controls.Add(this.spectrogramPlot1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "HpssForm";
            this.Text = "HpssForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private UserControls.SpectrogramPlot spectrogramPlot1;
        private UserControls.SpectrogramPlot spectrogramPlot2;
        private UserControls.SpectrogramPlot spectrogramPlot3;
        private System.Windows.Forms.TextBox fftSizeTextBox;
        private System.Windows.Forms.TextBox hopSizeTextBox;
        private System.Windows.Forms.TextBox harmonicWindowTextBox;
        private System.Windows.Forms.TextBox percussiveWindowTextBox;
        private System.Windows.Forms.ComboBox maskingComboBox;
        private System.Windows.Forms.Button evaluateButton;
        private System.Windows.Forms.Button playButton1;
        private System.Windows.Forms.Button playButton2;
        private System.Windows.Forms.Button playButton3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}