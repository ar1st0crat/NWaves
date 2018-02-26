namespace NWaves.DemoForms
{
    partial class EffectsForm
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
            this.playFilteredSignalButton = new System.Windows.Forms.Button();
            this.playSignalButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.spectrogramAfterFilteringPanel = new System.Windows.Forms.Panel();
            this.signalAfterFilteringPanel = new System.Windows.Forms.Panel();
            this.spectrogramBeforeFilteringPanel = new System.Windows.Forms.Panel();
            this.signalBeforeFilteringPanel = new System.Windows.Forms.Panel();
            this.stopButton = new System.Windows.Forms.Button();
            this.stopFilteredButton = new System.Windows.Forms.Button();
            this.applyEffectButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tremoloIndexTextBox = new System.Windows.Forms.TextBox();
            this.tremoloFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.echoDecayTextBox = new System.Windows.Forms.TextBox();
            this.echoDelayTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.distTextBox = new System.Windows.Forms.TextBox();
            this.qTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.distortionMixTextBox = new System.Windows.Forms.TextBox();
            this.distortionGainTextBox = new System.Windows.Forms.TextBox();
            this.wahwahRadioButton = new System.Windows.Forms.RadioButton();
            this.echoRadioButton = new System.Windows.Forms.RadioButton();
            this.tubeDistortionRadioButton = new System.Windows.Forms.RadioButton();
            this.distortionRadioButton = new System.Windows.Forms.RadioButton();
            this.overdriveRadioButton = new System.Windows.Forms.RadioButton();
            this.tremoloRadioButton = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // playFilteredSignalButton
            // 
            this.playFilteredSignalButton.Location = new System.Drawing.Point(571, 291);
            this.playFilteredSignalButton.Name = "playFilteredSignalButton";
            this.playFilteredSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playFilteredSignalButton.TabIndex = 27;
            this.playFilteredSignalButton.Text = "Play";
            this.playFilteredSignalButton.UseVisualStyleBackColor = true;
            this.playFilteredSignalButton.Click += new System.EventHandler(this.playFilteredSignalButton_Click);
            // 
            // playSignalButton
            // 
            this.playSignalButton.Location = new System.Drawing.Point(118, 291);
            this.playSignalButton.Name = "playSignalButton";
            this.playSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playSignalButton.TabIndex = 26;
            this.playSignalButton.Text = "Play";
            this.playSignalButton.UseVisualStyleBackColor = true;
            this.playSignalButton.Click += new System.EventHandler(this.playSignalButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(477, 296);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 17);
            this.label6.TabIndex = 25;
            this.label6.Text = "After effect";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 296);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 17);
            this.label5.TabIndex = 24;
            this.label5.Text = "Before effect";
            // 
            // spectrogramAfterFilteringPanel
            // 
            this.spectrogramAfterFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.spectrogramAfterFilteringPanel.Location = new System.Drawing.Point(480, 476);
            this.spectrogramAfterFilteringPanel.Name = "spectrogramAfterFilteringPanel";
            this.spectrogramAfterFilteringPanel.Size = new System.Drawing.Size(475, 137);
            this.spectrogramAfterFilteringPanel.TabIndex = 23;
            // 
            // signalAfterFilteringPanel
            // 
            this.signalAfterFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalAfterFilteringPanel.Location = new System.Drawing.Point(480, 316);
            this.signalAfterFilteringPanel.Name = "signalAfterFilteringPanel";
            this.signalAfterFilteringPanel.Size = new System.Drawing.Size(475, 153);
            this.signalAfterFilteringPanel.TabIndex = 21;
            // 
            // spectrogramBeforeFilteringPanel
            // 
            this.spectrogramBeforeFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.spectrogramBeforeFilteringPanel.Location = new System.Drawing.Point(13, 475);
            this.spectrogramBeforeFilteringPanel.Name = "spectrogramBeforeFilteringPanel";
            this.spectrogramBeforeFilteringPanel.Size = new System.Drawing.Size(461, 138);
            this.spectrogramBeforeFilteringPanel.TabIndex = 22;
            // 
            // signalBeforeFilteringPanel
            // 
            this.signalBeforeFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalBeforeFilteringPanel.Location = new System.Drawing.Point(13, 316);
            this.signalBeforeFilteringPanel.Name = "signalBeforeFilteringPanel";
            this.signalBeforeFilteringPanel.Size = new System.Drawing.Size(461, 153);
            this.signalBeforeFilteringPanel.TabIndex = 20;
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(183, 291);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(59, 26);
            this.stopButton.TabIndex = 28;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // stopFilteredButton
            // 
            this.stopFilteredButton.Location = new System.Drawing.Point(636, 291);
            this.stopFilteredButton.Name = "stopFilteredButton";
            this.stopFilteredButton.Size = new System.Drawing.Size(59, 26);
            this.stopFilteredButton.TabIndex = 29;
            this.stopFilteredButton.Text = "Stop";
            this.stopFilteredButton.UseVisualStyleBackColor = true;
            this.stopFilteredButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // applyEffectButton
            // 
            this.applyEffectButton.Location = new System.Drawing.Point(15, 247);
            this.applyEffectButton.Name = "applyEffectButton";
            this.applyEffectButton.Size = new System.Drawing.Size(940, 38);
            this.applyEffectButton.TabIndex = 30;
            this.applyEffectButton.Text = "Apply!";
            this.applyEffectButton.UseVisualStyleBackColor = true;
            this.applyEffectButton.Click += new System.EventHandler(this.applyEffectButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.tremoloIndexTextBox);
            this.groupBox1.Controls.Add(this.tremoloFrequencyTextBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.echoDecayTextBox);
            this.groupBox1.Controls.Add(this.echoDelayTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.distTextBox);
            this.groupBox1.Controls.Add(this.qTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.distortionMixTextBox);
            this.groupBox1.Controls.Add(this.distortionGainTextBox);
            this.groupBox1.Controls.Add(this.wahwahRadioButton);
            this.groupBox1.Controls.Add(this.echoRadioButton);
            this.groupBox1.Controls.Add(this.tubeDistortionRadioButton);
            this.groupBox1.Controls.Add(this.distortionRadioButton);
            this.groupBox1.Controls.Add(this.overdriveRadioButton);
            this.groupBox1.Controls.Add(this.tremoloRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(13, 31);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(942, 210);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sound Effects";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(257, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 17);
            this.label9.TabIndex = 21;
            this.label9.Text = "Index";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(145, 40);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 17);
            this.label10.TabIndex = 20;
            this.label10.Text = "Freq";
            // 
            // tremoloIndexTextBox
            // 
            this.tremoloIndexTextBox.Location = new System.Drawing.Point(306, 37);
            this.tremoloIndexTextBox.Name = "tremoloIndexTextBox";
            this.tremoloIndexTextBox.Size = new System.Drawing.Size(41, 22);
            this.tremoloIndexTextBox.TabIndex = 19;
            this.tremoloIndexTextBox.Text = "0,5";
            // 
            // tremoloFrequencyTextBox
            // 
            this.tremoloFrequencyTextBox.Location = new System.Drawing.Point(189, 37);
            this.tremoloFrequencyTextBox.Name = "tremoloFrequencyTextBox";
            this.tremoloFrequencyTextBox.Size = new System.Drawing.Size(54, 22);
            this.tremoloFrequencyTextBox.TabIndex = 18;
            this.tremoloFrequencyTextBox.Text = "10";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(256, 149);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 17);
            this.label7.TabIndex = 17;
            this.label7.Text = "Decay";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(145, 149);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 16;
            this.label8.Text = "Delay";
            // 
            // echoDecayTextBox
            // 
            this.echoDecayTextBox.Location = new System.Drawing.Point(306, 146);
            this.echoDecayTextBox.Name = "echoDecayTextBox";
            this.echoDecayTextBox.Size = new System.Drawing.Size(41, 22);
            this.echoDecayTextBox.TabIndex = 15;
            this.echoDecayTextBox.Text = "0,5";
            // 
            // echoDelayTextBox
            // 
            this.echoDelayTextBox.Location = new System.Drawing.Point(189, 146);
            this.echoDelayTextBox.Name = "echoDelayTextBox";
            this.echoDelayTextBox.Size = new System.Drawing.Size(54, 22);
            this.echoDelayTextBox.TabIndex = 14;
            this.echoDelayTextBox.Text = "0,18";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Dist";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(145, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Q";
            // 
            // distTextBox
            // 
            this.distTextBox.Location = new System.Drawing.Point(306, 118);
            this.distTextBox.Name = "distTextBox";
            this.distTextBox.Size = new System.Drawing.Size(41, 22);
            this.distTextBox.TabIndex = 11;
            this.distTextBox.Text = "5";
            // 
            // qTextBox
            // 
            this.qTextBox.Location = new System.Drawing.Point(189, 118);
            this.qTextBox.Name = "qTextBox";
            this.qTextBox.Size = new System.Drawing.Size(54, 22);
            this.qTextBox.TabIndex = 10;
            this.qTextBox.Text = "-0,2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(257, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 17);
            this.label2.TabIndex = 9;
            this.label2.Text = "Mix";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Gain";
            // 
            // distortionMixTextBox
            // 
            this.distortionMixTextBox.Location = new System.Drawing.Point(306, 91);
            this.distortionMixTextBox.Name = "distortionMixTextBox";
            this.distortionMixTextBox.Size = new System.Drawing.Size(41, 22);
            this.distortionMixTextBox.TabIndex = 7;
            this.distortionMixTextBox.Text = "0,9";
            // 
            // distortionGainTextBox
            // 
            this.distortionGainTextBox.Location = new System.Drawing.Point(189, 91);
            this.distortionGainTextBox.Name = "distortionGainTextBox";
            this.distortionGainTextBox.Size = new System.Drawing.Size(54, 22);
            this.distortionGainTextBox.TabIndex = 6;
            this.distortionGainTextBox.Text = "20";
            // 
            // wahwahRadioButton
            // 
            this.wahwahRadioButton.AutoSize = true;
            this.wahwahRadioButton.Location = new System.Drawing.Point(7, 172);
            this.wahwahRadioButton.Name = "wahwahRadioButton";
            this.wahwahRadioButton.Size = new System.Drawing.Size(87, 21);
            this.wahwahRadioButton.TabIndex = 5;
            this.wahwahRadioButton.Text = "WahWah";
            this.wahwahRadioButton.UseVisualStyleBackColor = true;
            // 
            // echoRadioButton
            // 
            this.echoRadioButton.AutoSize = true;
            this.echoRadioButton.Location = new System.Drawing.Point(7, 145);
            this.echoRadioButton.Name = "echoRadioButton";
            this.echoRadioButton.Size = new System.Drawing.Size(61, 21);
            this.echoRadioButton.TabIndex = 4;
            this.echoRadioButton.Text = "Echo";
            this.echoRadioButton.UseVisualStyleBackColor = true;
            // 
            // tubeDistortionRadioButton
            // 
            this.tubeDistortionRadioButton.AutoSize = true;
            this.tubeDistortionRadioButton.Location = new System.Drawing.Point(7, 118);
            this.tubeDistortionRadioButton.Name = "tubeDistortionRadioButton";
            this.tubeDistortionRadioButton.Size = new System.Drawing.Size(126, 21);
            this.tubeDistortionRadioButton.TabIndex = 3;
            this.tubeDistortionRadioButton.Text = "Tube Distortion";
            this.tubeDistortionRadioButton.UseVisualStyleBackColor = true;
            // 
            // distortionRadioButton
            // 
            this.distortionRadioButton.AutoSize = true;
            this.distortionRadioButton.Location = new System.Drawing.Point(7, 91);
            this.distortionRadioButton.Name = "distortionRadioButton";
            this.distortionRadioButton.Size = new System.Drawing.Size(89, 21);
            this.distortionRadioButton.TabIndex = 2;
            this.distortionRadioButton.Text = "Distortion";
            this.distortionRadioButton.UseVisualStyleBackColor = true;
            // 
            // overdriveRadioButton
            // 
            this.overdriveRadioButton.AutoSize = true;
            this.overdriveRadioButton.Location = new System.Drawing.Point(7, 64);
            this.overdriveRadioButton.Name = "overdriveRadioButton";
            this.overdriveRadioButton.Size = new System.Drawing.Size(91, 21);
            this.overdriveRadioButton.TabIndex = 1;
            this.overdriveRadioButton.Text = "Overdrive";
            this.overdriveRadioButton.UseVisualStyleBackColor = true;
            // 
            // tremoloRadioButton
            // 
            this.tremoloRadioButton.AutoSize = true;
            this.tremoloRadioButton.Checked = true;
            this.tremoloRadioButton.Location = new System.Drawing.Point(7, 37);
            this.tremoloRadioButton.Name = "tremoloRadioButton";
            this.tremoloRadioButton.Size = new System.Drawing.Size(81, 21);
            this.tremoloRadioButton.TabIndex = 0;
            this.tremoloRadioButton.TabStop = true;
            this.tremoloRadioButton.Text = "Tremolo";
            this.tremoloRadioButton.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(970, 28);
            this.menuStrip1.TabIndex = 32;
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
            // EffectsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 625);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.applyEffectButton);
            this.Controls.Add(this.stopFilteredButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.playFilteredSignalButton);
            this.Controls.Add(this.playSignalButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.spectrogramAfterFilteringPanel);
            this.Controls.Add(this.signalAfterFilteringPanel);
            this.Controls.Add(this.spectrogramBeforeFilteringPanel);
            this.Controls.Add(this.signalBeforeFilteringPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EffectsForm";
            this.Text = "EffectsForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playFilteredSignalButton;
        private System.Windows.Forms.Button playSignalButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel spectrogramAfterFilteringPanel;
        private System.Windows.Forms.Panel signalAfterFilteringPanel;
        private System.Windows.Forms.Panel spectrogramBeforeFilteringPanel;
        private System.Windows.Forms.Panel signalBeforeFilteringPanel;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button stopFilteredButton;
        private System.Windows.Forms.Button applyEffectButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.RadioButton wahwahRadioButton;
        private System.Windows.Forms.RadioButton echoRadioButton;
        private System.Windows.Forms.RadioButton tubeDistortionRadioButton;
        private System.Windows.Forms.RadioButton distortionRadioButton;
        private System.Windows.Forms.RadioButton overdriveRadioButton;
        private System.Windows.Forms.RadioButton tremoloRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox distortionMixTextBox;
        private System.Windows.Forms.TextBox distortionGainTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox distTextBox;
        private System.Windows.Forms.TextBox qTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tremoloIndexTextBox;
        private System.Windows.Forms.TextBox tremoloFrequencyTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox echoDecayTextBox;
        private System.Windows.Forms.TextBox echoDelayTextBox;
    }
}