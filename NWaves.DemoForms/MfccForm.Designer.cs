using NWaves.DemoForms.UserControls;

namespace NWaves.DemoForms
{
    partial class MfccForm
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
            this.melFilterBankPanel = new NWaves.DemoForms.UserControls.GroupPlot();
            this.mfccPanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.mfccListView = new System.Windows.Forms.ListView();
            this.checkBoxOverlap = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.comboBoxShape = new System.Windows.Forms.ComboBox();
            this.textBoxFftSize = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.textBoxHighFreq = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBoxLowFreq = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.buttonCompute = new System.Windows.Forms.Button();
            this.textBoxSize = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.comboBoxFilterbank = new System.Windows.Forms.ComboBox();
            this.textBoxVtlnAlpha = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxSpectrum = new System.Windows.Forms.ComboBox();
            this.comboBoxNonLinearity = new System.Windows.Forms.ComboBox();
            this.textBoxLogFloor = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxNormalize = new System.Windows.Forms.CheckBox();
            this.textBoxVtlnLow = new System.Windows.Forms.TextBox();
            this.textBoxVtlnHigh = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxVtln = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxDct = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(937, 28);
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
            // melFilterBankPanel
            // 
            this.melFilterBankPanel.AutoScroll = true;
            this.melFilterBankPanel.BackColor = System.Drawing.Color.White;
            this.melFilterBankPanel.Gain = 100;
            this.melFilterBankPanel.Groups = null;
            this.melFilterBankPanel.Location = new System.Drawing.Point(12, 465);
            this.melFilterBankPanel.Name = "melFilterBankPanel";
            this.melFilterBankPanel.Size = new System.Drawing.Size(588, 160);
            this.melFilterBankPanel.Stride = 2;
            this.melFilterBankPanel.TabIndex = 1;
            // 
            // mfccPanel
            // 
            this.mfccPanel.AutoScroll = true;
            this.mfccPanel.BackColor = System.Drawing.Color.White;
            this.mfccPanel.ForeColor = System.Drawing.Color.Blue;
            this.mfccPanel.Location = new System.Drawing.Point(606, 465);
            this.mfccPanel.Name = "mfccPanel";
            this.mfccPanel.PaddingX = 30;
            this.mfccPanel.PaddingY = 20;
            this.mfccPanel.Size = new System.Drawing.Size(322, 160);
            this.mfccPanel.Stride = 1;
            this.mfccPanel.TabIndex = 2;
            this.mfccPanel.Thickness = 1;
            // 
            // mfccListView
            // 
            this.mfccListView.FullRowSelect = true;
            this.mfccListView.GridLines = true;
            this.mfccListView.Location = new System.Drawing.Point(13, 32);
            this.mfccListView.Name = "mfccListView";
            this.mfccListView.Size = new System.Drawing.Size(915, 322);
            this.mfccListView.TabIndex = 3;
            this.mfccListView.UseCompatibleStateImageBehavior = false;
            this.mfccListView.View = System.Windows.Forms.View.Details;
            this.mfccListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.mfccListView_ItemSelectionChanged);
            // 
            // checkBoxOverlap
            // 
            this.checkBoxOverlap.AutoSize = true;
            this.checkBoxOverlap.Location = new System.Drawing.Point(107, 366);
            this.checkBoxOverlap.Name = "checkBoxOverlap";
            this.checkBoxOverlap.Size = new System.Drawing.Size(77, 21);
            this.checkBoxOverlap.TabIndex = 53;
            this.checkBoxOverlap.Text = "overlap";
            this.checkBoxOverlap.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(12, 432);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(49, 17);
            this.label27.TabIndex = 52;
            this.label27.Text = "Shape";
            // 
            // comboBoxShape
            // 
            this.comboBoxShape.FormattingEnabled = true;
            this.comboBoxShape.Items.AddRange(new object[] {
            "Triangular",
            "Rectangular",
            "Trapezoidal",
            "BiQuad"});
            this.comboBoxShape.Location = new System.Drawing.Point(75, 430);
            this.comboBoxShape.Name = "comboBoxShape";
            this.comboBoxShape.Size = new System.Drawing.Size(109, 24);
            this.comboBoxShape.TabIndex = 51;
            this.comboBoxShape.Text = "Triangular";
            // 
            // textBoxFftSize
            // 
            this.textBoxFftSize.Location = new System.Drawing.Point(340, 430);
            this.textBoxFftSize.Name = "textBoxFftSize";
            this.textBoxFftSize.Size = new System.Drawing.Size(60, 22);
            this.textBoxFftSize.TabIndex = 48;
            this.textBoxFftSize.Text = "512";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(338, 402);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(62, 17);
            this.label21.TabIndex = 47;
            this.label21.Text = "FFT size";
            // 
            // textBoxHighFreq
            // 
            this.textBoxHighFreq.Location = new System.Drawing.Point(275, 430);
            this.textBoxHighFreq.Name = "textBoxHighFreq";
            this.textBoxHighFreq.Size = new System.Drawing.Size(54, 22);
            this.textBoxHighFreq.TabIndex = 46;
            this.textBoxHighFreq.Text = "8000";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(203, 430);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(66, 17);
            this.label20.TabIndex = 45;
            this.label20.Text = "HighFreq";
            // 
            // textBoxLowFreq
            // 
            this.textBoxLowFreq.Location = new System.Drawing.Point(275, 400);
            this.textBoxLowFreq.Name = "textBoxLowFreq";
            this.textBoxLowFreq.Size = new System.Drawing.Size(54, 22);
            this.textBoxLowFreq.TabIndex = 44;
            this.textBoxLowFreq.Text = "0";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(203, 402);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(62, 17);
            this.label19.TabIndex = 43;
            this.label19.Text = "LowFreq";
            // 
            // buttonCompute
            // 
            this.buttonCompute.Location = new System.Drawing.Point(859, 369);
            this.buttonCompute.Name = "buttonCompute";
            this.buttonCompute.Size = new System.Drawing.Size(69, 87);
            this.buttonCompute.TabIndex = 42;
            this.buttonCompute.Text = ">>";
            this.buttonCompute.UseVisualStyleBackColor = true;
            this.buttonCompute.Click += new System.EventHandler(this.buttonCompute_Click);
            // 
            // textBoxSize
            // 
            this.textBoxSize.Location = new System.Drawing.Point(53, 364);
            this.textBoxSize.Name = "textBoxSize";
            this.textBoxSize.Size = new System.Drawing.Size(38, 22);
            this.textBoxSize.TabIndex = 41;
            this.textBoxSize.Text = "13";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(12, 366);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(35, 17);
            this.label18.TabIndex = 40;
            this.label18.Text = "Size";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(12, 401);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(74, 17);
            this.label17.TabIndex = 39;
            this.label17.Text = "Filter bank";
            // 
            // comboBoxFilterbank
            // 
            this.comboBoxFilterbank.FormattingEnabled = true;
            this.comboBoxFilterbank.Items.AddRange(new object[] {
            "Herz",
            "Mel",
            "Mel Slaney",
            "Bark",
            "Bark Slaney",
            "Critical bands",
            "ERB",
            "Octave bands"});
            this.comboBoxFilterbank.Location = new System.Drawing.Point(92, 398);
            this.comboBoxFilterbank.Name = "comboBoxFilterbank";
            this.comboBoxFilterbank.Size = new System.Drawing.Size(92, 24);
            this.comboBoxFilterbank.TabIndex = 38;
            this.comboBoxFilterbank.Text = "Mel";
            // 
            // textBoxVtlnAlpha
            // 
            this.textBoxVtlnAlpha.Location = new System.Drawing.Point(546, 372);
            this.textBoxVtlnAlpha.Name = "textBoxVtlnAlpha";
            this.textBoxVtlnAlpha.Size = new System.Drawing.Size(54, 22);
            this.textBoxVtlnAlpha.TabIndex = 55;
            this.textBoxVtlnAlpha.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(485, 373);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 17);
            this.label1.TabIndex = 54;
            this.label1.Text = "alpha";
            // 
            // comboBoxSpectrum
            // 
            this.comboBoxSpectrum.FormattingEnabled = true;
            this.comboBoxSpectrum.Location = new System.Drawing.Point(737, 359);
            this.comboBoxSpectrum.Name = "comboBoxSpectrum";
            this.comboBoxSpectrum.Size = new System.Drawing.Size(116, 24);
            this.comboBoxSpectrum.TabIndex = 56;
            this.comboBoxSpectrum.Text = "Power";
            // 
            // comboBoxNonLinearity
            // 
            this.comboBoxNonLinearity.FormattingEnabled = true;
            this.comboBoxNonLinearity.Location = new System.Drawing.Point(737, 386);
            this.comboBoxNonLinearity.Name = "comboBoxNonLinearity";
            this.comboBoxNonLinearity.Size = new System.Drawing.Size(116, 24);
            this.comboBoxNonLinearity.TabIndex = 57;
            this.comboBoxNonLinearity.Text = "LogE";
            // 
            // textBoxLogFloor
            // 
            this.textBoxLogFloor.Location = new System.Drawing.Point(737, 413);
            this.textBoxLogFloor.Name = "textBoxLogFloor";
            this.textBoxLogFloor.Size = new System.Drawing.Size(116, 22);
            this.textBoxLogFloor.TabIndex = 59;
            this.textBoxLogFloor.Text = "1e-45";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(620, 413);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 58;
            this.label2.Text = "Log floor";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(619, 361);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 17);
            this.label3.TabIndex = 60;
            this.label3.Text = "Spectrum type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(619, 388);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 61;
            this.label4.Text = "Non-linearity";
            // 
            // checkBoxNormalize
            // 
            this.checkBoxNormalize.AutoSize = true;
            this.checkBoxNormalize.Location = new System.Drawing.Point(192, 366);
            this.checkBoxNormalize.Name = "checkBoxNormalize";
            this.checkBoxNormalize.Size = new System.Drawing.Size(91, 21);
            this.checkBoxNormalize.TabIndex = 62;
            this.checkBoxNormalize.Text = "normalize";
            this.checkBoxNormalize.UseVisualStyleBackColor = true;
            // 
            // textBoxVtlnLow
            // 
            this.textBoxVtlnLow.Location = new System.Drawing.Point(546, 400);
            this.textBoxVtlnLow.Name = "textBoxVtlnLow";
            this.textBoxVtlnLow.Size = new System.Drawing.Size(54, 22);
            this.textBoxVtlnLow.TabIndex = 63;
            this.textBoxVtlnLow.Text = "0";
            // 
            // textBoxVtlnHigh
            // 
            this.textBoxVtlnHigh.Location = new System.Drawing.Point(546, 428);
            this.textBoxVtlnHigh.Name = "textBoxVtlnHigh";
            this.textBoxVtlnHigh.Size = new System.Drawing.Size(54, 22);
            this.textBoxVtlnHigh.TabIndex = 64;
            this.textBoxVtlnHigh.Text = "8000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(485, 402);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 17);
            this.label5.TabIndex = 65;
            this.label5.Text = "low";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(485, 431);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 17);
            this.label6.TabIndex = 66;
            this.label6.Text = "high";
            // 
            // checkBoxVtln
            // 
            this.checkBoxVtln.AutoSize = true;
            this.checkBoxVtln.Location = new System.Drawing.Point(413, 366);
            this.checkBoxVtln.Name = "checkBoxVtln";
            this.checkBoxVtln.Size = new System.Drawing.Size(66, 21);
            this.checkBoxVtln.TabIndex = 67;
            this.checkBoxVtln.Text = "VTLN";
            this.checkBoxVtln.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(619, 440);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 17);
            this.label7.TabIndex = 69;
            this.label7.Text = "DCT type";
            // 
            // comboBoxDct
            // 
            this.comboBoxDct.FormattingEnabled = true;
            this.comboBoxDct.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "1N",
            "2N",
            "3N",
            "4N"});
            this.comboBoxDct.Location = new System.Drawing.Point(737, 438);
            this.comboBoxDct.Name = "comboBoxDct";
            this.comboBoxDct.Size = new System.Drawing.Size(116, 24);
            this.comboBoxDct.TabIndex = 68;
            this.comboBoxDct.Text = "2N";
            // 
            // MfccForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 637);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.comboBoxDct);
            this.Controls.Add(this.checkBoxVtln);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxVtlnHigh);
            this.Controls.Add(this.textBoxVtlnLow);
            this.Controls.Add(this.checkBoxNormalize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxLogFloor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxNonLinearity);
            this.Controls.Add(this.comboBoxSpectrum);
            this.Controls.Add(this.textBoxVtlnAlpha);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxOverlap);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.comboBoxShape);
            this.Controls.Add(this.textBoxFftSize);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.textBoxHighFreq);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.textBoxLowFreq);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.buttonCompute);
            this.Controls.Add(this.textBoxSize);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.comboBoxFilterbank);
            this.Controls.Add(this.mfccListView);
            this.Controls.Add(this.mfccPanel);
            this.Controls.Add(this.melFilterBankPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MfccForm";
            this.Text = "MfccForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private GroupPlot melFilterBankPanel;
        private LinePlot mfccPanel;
        private System.Windows.Forms.ListView mfccListView;
        private System.Windows.Forms.CheckBox checkBoxOverlap;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.ComboBox comboBoxShape;
        private System.Windows.Forms.TextBox textBoxFftSize;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox textBoxHighFreq;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBoxLowFreq;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button buttonCompute;
        private System.Windows.Forms.TextBox textBoxSize;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox comboBoxFilterbank;
        private System.Windows.Forms.TextBox textBoxVtlnAlpha;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxSpectrum;
        private System.Windows.Forms.ComboBox comboBoxNonLinearity;
        private System.Windows.Forms.TextBox textBoxLogFloor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxNormalize;
        private System.Windows.Forms.TextBox textBoxVtlnLow;
        private System.Windows.Forms.TextBox textBoxVtlnHigh;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxVtln;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxDct;
    }
}