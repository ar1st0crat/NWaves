namespace NWaves.DemoForms
{
    partial class ModulationSpectrumForm
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
            this.envelopesPanel = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.band4ComboBox = new System.Windows.Forms.ComboBox();
            this.band3ComboBox = new System.Windows.Forms.ComboBox();
            this.band2ComboBox = new System.Windows.Forms.ComboBox();
            this.band1ComboBox = new System.Windows.Forms.ComboBox();
            this.filterbankComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.filterCountTextBox = new System.Windows.Forms.TextBox();
            this.filterbankButton = new System.Windows.Forms.Button();
            this.modulationSpectrumPanel = new System.Windows.Forms.Panel();
            this.lowFreqTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.highFreqTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.samplingRateTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.fftSizeTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.computeButton = new System.Windows.Forms.Button();
            this.longTermHopSizeTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.longTermFftSizeTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.hopSizeTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.analysisFftTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.prevButton = new System.Windows.Forms.Button();
            this.temporalCheckBox = new System.Windows.Forms.CheckBox();
            this.herzTextBox = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.shapeComboBox = new System.Windows.Forms.ComboBox();
            this.overlapCheckBox = new System.Windows.Forms.CheckBox();
            this.filterbankPanel = new NWaves.DemoForms.UserControls.GroupPlot();
            this.menuStrip1.SuspendLayout();
            this.envelopesPanel.SuspendLayout();
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
            this.menuStrip1.Size = new System.Drawing.Size(1187, 28);
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
            // envelopesPanel
            // 
            this.envelopesPanel.BackColor = System.Drawing.Color.White;
            this.envelopesPanel.Controls.Add(this.label15);
            this.envelopesPanel.Controls.Add(this.label14);
            this.envelopesPanel.Controls.Add(this.label13);
            this.envelopesPanel.Controls.Add(this.label12);
            this.envelopesPanel.Controls.Add(this.band4ComboBox);
            this.envelopesPanel.Controls.Add(this.band3ComboBox);
            this.envelopesPanel.Controls.Add(this.band2ComboBox);
            this.envelopesPanel.Controls.Add(this.band1ComboBox);
            this.envelopesPanel.Location = new System.Drawing.Point(12, 319);
            this.envelopesPanel.Name = "envelopesPanel";
            this.envelopesPanel.Size = new System.Drawing.Size(1168, 370);
            this.envelopesPanel.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(1055, 255);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(49, 17);
            this.label15.TabIndex = 7;
            this.label15.Text = "Band#";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(1055, 170);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(49, 17);
            this.label14.TabIndex = 6;
            this.label14.Text = "Band#";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(1055, 89);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 17);
            this.label13.TabIndex = 5;
            this.label13.Text = "Band#";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(1055, 8);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(49, 17);
            this.label12.TabIndex = 4;
            this.label12.Text = "Band#";
            // 
            // band4ComboBox
            // 
            this.band4ComboBox.FormattingEnabled = true;
            this.band4ComboBox.Location = new System.Drawing.Point(1110, 252);
            this.band4ComboBox.Name = "band4ComboBox";
            this.band4ComboBox.Size = new System.Drawing.Size(52, 24);
            this.band4ComboBox.TabIndex = 3;
            this.band4ComboBox.TextChanged += new System.EventHandler(this.bandComboBox_TextChanged);
            // 
            // band3ComboBox
            // 
            this.band3ComboBox.FormattingEnabled = true;
            this.band3ComboBox.Location = new System.Drawing.Point(1110, 167);
            this.band3ComboBox.Name = "band3ComboBox";
            this.band3ComboBox.Size = new System.Drawing.Size(52, 24);
            this.band3ComboBox.TabIndex = 2;
            this.band3ComboBox.TextChanged += new System.EventHandler(this.bandComboBox_TextChanged);
            // 
            // band2ComboBox
            // 
            this.band2ComboBox.FormattingEnabled = true;
            this.band2ComboBox.Location = new System.Drawing.Point(1110, 86);
            this.band2ComboBox.Name = "band2ComboBox";
            this.band2ComboBox.Size = new System.Drawing.Size(52, 24);
            this.band2ComboBox.TabIndex = 1;
            this.band2ComboBox.TextChanged += new System.EventHandler(this.bandComboBox_TextChanged);
            // 
            // band1ComboBox
            // 
            this.band1ComboBox.FormattingEnabled = true;
            this.band1ComboBox.Location = new System.Drawing.Point(1110, 5);
            this.band1ComboBox.Name = "band1ComboBox";
            this.band1ComboBox.Size = new System.Drawing.Size(52, 24);
            this.band1ComboBox.TabIndex = 0;
            this.band1ComboBox.TextChanged += new System.EventHandler(this.bandComboBox_TextChanged);
            // 
            // filterbankComboBox
            // 
            this.filterbankComboBox.FormattingEnabled = true;
            this.filterbankComboBox.Items.AddRange(new object[] {
            "Herz",
            "Mel",
            "Bark",
            "Critical bands",
            "ERB"});
            this.filterbankComboBox.Location = new System.Drawing.Point(184, 42);
            this.filterbankComboBox.Name = "filterbankComboBox";
            this.filterbankComboBox.Size = new System.Drawing.Size(92, 24);
            this.filterbankComboBox.TabIndex = 2;
            this.filterbankComboBox.Text = "Mel";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(104, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter bank";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Size";
            // 
            // filterCountTextBox
            // 
            this.filterCountTextBox.Location = new System.Drawing.Point(54, 43);
            this.filterCountTextBox.Name = "filterCountTextBox";
            this.filterCountTextBox.Size = new System.Drawing.Size(38, 22);
            this.filterCountTextBox.TabIndex = 6;
            this.filterCountTextBox.Text = "13";
            // 
            // filterbankButton
            // 
            this.filterbankButton.Location = new System.Drawing.Point(372, 78);
            this.filterbankButton.Name = "filterbankButton";
            this.filterbankButton.Size = new System.Drawing.Size(240, 51);
            this.filterbankButton.TabIndex = 7;
            this.filterbankButton.Text = ">>";
            this.filterbankButton.UseVisualStyleBackColor = true;
            this.filterbankButton.Click += new System.EventHandler(this.filterbankButton_Click);
            // 
            // modulationSpectrumPanel
            // 
            this.modulationSpectrumPanel.BackColor = System.Drawing.Color.White;
            this.modulationSpectrumPanel.Location = new System.Drawing.Point(865, 65);
            this.modulationSpectrumPanel.Name = "modulationSpectrumPanel";
            this.modulationSpectrumPanel.Size = new System.Drawing.Size(315, 248);
            this.modulationSpectrumPanel.TabIndex = 8;
            // 
            // lowFreqTextBox
            // 
            this.lowFreqTextBox.Location = new System.Drawing.Point(105, 78);
            this.lowFreqTextBox.Name = "lowFreqTextBox";
            this.lowFreqTextBox.Size = new System.Drawing.Size(70, 22);
            this.lowFreqTextBox.TabIndex = 10;
            this.lowFreqTextBox.Text = "200";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "LowFreq";
            // 
            // highFreqTextBox
            // 
            this.highFreqTextBox.Location = new System.Drawing.Point(105, 107);
            this.highFreqTextBox.Name = "highFreqTextBox";
            this.highFreqTextBox.Size = new System.Drawing.Size(70, 22);
            this.highFreqTextBox.TabIndex = 12;
            this.highFreqTextBox.Text = "3800";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "HighFreq";
            // 
            // samplingRateTextBox
            // 
            this.samplingRateTextBox.Location = new System.Drawing.Point(282, 106);
            this.samplingRateTextBox.Name = "samplingRateTextBox";
            this.samplingRateTextBox.Size = new System.Drawing.Size(70, 22);
            this.samplingRateTextBox.TabIndex = 16;
            this.samplingRateTextBox.Text = "16000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(181, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "Sampling rate";
            // 
            // fftSizeTextBox
            // 
            this.fftSizeTextBox.Location = new System.Drawing.Point(282, 78);
            this.fftSizeTextBox.Name = "fftSizeTextBox";
            this.fftSizeTextBox.Size = new System.Drawing.Size(70, 22);
            this.fftSizeTextBox.TabIndex = 14;
            this.fftSizeTextBox.Text = "512";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(181, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 17);
            this.label6.TabIndex = 13;
            this.label6.Text = "FFT size";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.computeButton);
            this.groupBox1.Controls.Add(this.longTermHopSizeTextBox);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.longTermFftSizeTextBox);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.hopSizeTextBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.analysisFftTextBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.groupBox1.Location = new System.Drawing.Point(621, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(238, 253);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Extractor parameters";
            // 
            // computeButton
            // 
            this.computeButton.Location = new System.Drawing.Point(22, 185);
            this.computeButton.Name = "computeButton";
            this.computeButton.Size = new System.Drawing.Size(193, 58);
            this.computeButton.TabIndex = 36;
            this.computeButton.Text = ">>";
            this.computeButton.UseVisualStyleBackColor = true;
            this.computeButton.Click += new System.EventHandler(this.computeButton_Click);
            // 
            // longTermHopSizeTextBox
            // 
            this.longTermHopSizeTextBox.Location = new System.Drawing.Point(156, 146);
            this.longTermHopSizeTextBox.Name = "longTermHopSizeTextBox";
            this.longTermHopSizeTextBox.Size = new System.Drawing.Size(59, 22);
            this.longTermHopSizeTextBox.TabIndex = 35;
            this.longTermHopSizeTextBox.Text = "32";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 146);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(130, 17);
            this.label9.TabIndex = 34;
            this.label9.Text = "Long-term hop size";
            // 
            // longTermFftSizeTextBox
            // 
            this.longTermFftSizeTextBox.Location = new System.Drawing.Point(156, 111);
            this.longTermFftSizeTextBox.Name = "longTermFftSizeTextBox";
            this.longTermFftSizeTextBox.Size = new System.Drawing.Size(59, 22);
            this.longTermFftSizeTextBox.TabIndex = 33;
            this.longTermFftSizeTextBox.Text = "64";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(19, 111);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(131, 17);
            this.label10.TabIndex = 32;
            this.label10.Text = "Long-term FFT size";
            // 
            // hopSizeTextBox
            // 
            this.hopSizeTextBox.Location = new System.Drawing.Point(156, 76);
            this.hopSizeTextBox.Name = "hopSizeTextBox";
            this.hopSizeTextBox.Size = new System.Drawing.Size(59, 22);
            this.hopSizeTextBox.TabIndex = 31;
            this.hopSizeTextBox.Text = "0,03125";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 17);
            this.label7.TabIndex = 30;
            this.label7.Text = "Overlap size";
            // 
            // analysisFftTextBox
            // 
            this.analysisFftTextBox.Location = new System.Drawing.Point(156, 42);
            this.analysisFftTextBox.Name = "analysisFftTextBox";
            this.analysisFftTextBox.Size = new System.Drawing.Size(59, 22);
            this.analysisFftTextBox.TabIndex = 29;
            this.analysisFftTextBox.Text = "0,0625";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 17);
            this.label8.TabIndex = 28;
            this.label8.Text = "Window size";
            // 
            // nextButton
            // 
            this.nextButton.BackColor = System.Drawing.Color.Transparent;
            this.nextButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nextButton.Location = new System.Drawing.Point(1142, 31);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(38, 34);
            this.nextButton.TabIndex = 29;
            this.nextButton.Text = ">";
            this.nextButton.UseVisualStyleBackColor = false;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(1040, 40);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(62, 17);
            this.infoLabel.TabIndex = 30;
            this.infoLabel.Text = "10x1024";
            // 
            // prevButton
            // 
            this.prevButton.BackColor = System.Drawing.Color.Transparent;
            this.prevButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.prevButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.prevButton.Location = new System.Drawing.Point(1108, 31);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(38, 34);
            this.prevButton.TabIndex = 31;
            this.prevButton.Text = "<";
            this.prevButton.UseVisualStyleBackColor = false;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // temporalCheckBox
            // 
            this.temporalCheckBox.AutoSize = true;
            this.temporalCheckBox.Location = new System.Drawing.Point(865, 38);
            this.temporalCheckBox.Name = "temporalCheckBox";
            this.temporalCheckBox.Size = new System.Drawing.Size(104, 21);
            this.temporalCheckBox.TabIndex = 32;
            this.temporalCheckBox.Text = "time plot for";
            this.temporalCheckBox.UseVisualStyleBackColor = true;
            this.temporalCheckBox.CheckedChanged += new System.EventHandler(this.temporalCheckBox_CheckedChanged);
            // 
            // herzTextBox
            // 
            this.herzTextBox.Location = new System.Drawing.Point(965, 38);
            this.herzTextBox.Name = "herzTextBox";
            this.herzTextBox.Size = new System.Drawing.Size(27, 22);
            this.herzTextBox.TabIndex = 33;
            this.herzTextBox.Text = "4";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(994, 40);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(25, 17);
            this.label16.TabIndex = 34;
            this.label16.Text = "Hz";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(369, 45);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(49, 17);
            this.label11.TabIndex = 36;
            this.label11.Text = "Shape";
            // 
            // shapeComboBox
            // 
            this.shapeComboBox.FormattingEnabled = true;
            this.shapeComboBox.Items.AddRange(new object[] {
            "Triangular",
            "Rectangular",
            "Trapezoidal",
            "BiQuad"});
            this.shapeComboBox.Location = new System.Drawing.Point(443, 42);
            this.shapeComboBox.Name = "shapeComboBox";
            this.shapeComboBox.Size = new System.Drawing.Size(169, 24);
            this.shapeComboBox.TabIndex = 35;
            this.shapeComboBox.Text = "Triangular";
            // 
            // overlapCheckBox
            // 
            this.overlapCheckBox.AutoSize = true;
            this.overlapCheckBox.Location = new System.Drawing.Point(283, 43);
            this.overlapCheckBox.Name = "overlapCheckBox";
            this.overlapCheckBox.Size = new System.Drawing.Size(77, 21);
            this.overlapCheckBox.TabIndex = 37;
            this.overlapCheckBox.Text = "overlap";
            this.overlapCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterbankPanel
            // 
            this.filterbankPanel.AutoScroll = true;
            this.filterbankPanel.BackColor = System.Drawing.Color.White;
            this.filterbankPanel.Gain = 100;
            this.filterbankPanel.Groups = null;
            this.filterbankPanel.Location = new System.Drawing.Point(12, 138);
            this.filterbankPanel.Name = "filterbankPanel";
            this.filterbankPanel.Size = new System.Drawing.Size(600, 175);
            this.filterbankPanel.Stride = 2;
            this.filterbankPanel.TabIndex = 38;
            // 
            // ModulationSpectrumForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1187, 701);
            this.Controls.Add(this.filterbankPanel);
            this.Controls.Add(this.overlapCheckBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.shapeComboBox);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.herzTextBox);
            this.Controls.Add(this.temporalCheckBox);
            this.Controls.Add(this.prevButton);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.samplingRateTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.fftSizeTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.highFreqTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lowFreqTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modulationSpectrumPanel);
            this.Controls.Add(this.filterbankButton);
            this.Controls.Add(this.filterCountTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.filterbankComboBox);
            this.Controls.Add(this.envelopesPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ModulationSpectrumForm";
            this.Text = "ModulationSpectrumForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.envelopesPanel.ResumeLayout(false);
            this.envelopesPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Panel envelopesPanel;
        private System.Windows.Forms.ComboBox filterbankComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox filterCountTextBox;
        private System.Windows.Forms.Button filterbankButton;
        private System.Windows.Forms.Panel modulationSpectrumPanel;
        private System.Windows.Forms.TextBox lowFreqTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox highFreqTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox samplingRateTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox fftSizeTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button computeButton;
        private System.Windows.Forms.TextBox longTermHopSizeTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox longTermFftSizeTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox hopSizeTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox analysisFftTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox band4ComboBox;
        private System.Windows.Forms.ComboBox band3ComboBox;
        private System.Windows.Forms.ComboBox band2ComboBox;
        private System.Windows.Forms.ComboBox band1ComboBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.CheckBox temporalCheckBox;
        private System.Windows.Forms.TextBox herzTextBox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox shapeComboBox;
        private System.Windows.Forms.CheckBox overlapCheckBox;
        private UserControls.GroupPlot filterbankPanel;
    }
}