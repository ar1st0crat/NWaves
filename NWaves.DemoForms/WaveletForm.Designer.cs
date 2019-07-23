namespace NWaves.DemoForms
{
    partial class WaveletForm
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
            this.comboBoxFamily = new System.Windows.Forms.ComboBox();
            this.comboBoxTaps = new System.Windows.Forms.ComboBox();
            this.textBoxSize = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.linePlotWavelet = new NWaves.DemoForms.UserControls.LinePlot();
            this.textBoxCoeffs = new System.Windows.Forms.TextBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.buttonCompute = new System.Windows.Forms.Button();
            this.labelWaveletName = new System.Windows.Forms.Label();
            this.textBoxResultInv = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxFamily
            // 
            this.comboBoxFamily.FormattingEnabled = true;
            this.comboBoxFamily.Location = new System.Drawing.Point(66, 34);
            this.comboBoxFamily.Name = "comboBoxFamily";
            this.comboBoxFamily.Size = new System.Drawing.Size(162, 24);
            this.comboBoxFamily.TabIndex = 0;
            // 
            // comboBoxTaps
            // 
            this.comboBoxTaps.FormattingEnabled = true;
            this.comboBoxTaps.Location = new System.Drawing.Point(66, 64);
            this.comboBoxTaps.Name = "comboBoxTaps";
            this.comboBoxTaps.Size = new System.Drawing.Size(162, 24);
            this.comboBoxTaps.TabIndex = 1;
            // 
            // textBoxSize
            // 
            this.textBoxSize.Location = new System.Drawing.Point(66, 6);
            this.textBoxSize.Name = "textBoxSize";
            this.textBoxSize.Size = new System.Drawing.Size(162, 22);
            this.textBoxSize.TabIndex = 3;
            this.textBoxSize.Text = "64";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Size";
            // 
            // linePlotWavelet
            // 
            this.linePlotWavelet.AutoScroll = true;
            this.linePlotWavelet.BackColor = System.Drawing.Color.White;
            this.linePlotWavelet.ForeColor = System.Drawing.Color.Blue;
            this.linePlotWavelet.Location = new System.Drawing.Point(15, 170);
            this.linePlotWavelet.Name = "linePlotWavelet";
            this.linePlotWavelet.PaddingX = 30;
            this.linePlotWavelet.PaddingY = 20;
            this.linePlotWavelet.Size = new System.Drawing.Size(357, 317);
            this.linePlotWavelet.Stride = 1;
            this.linePlotWavelet.TabIndex = 5;
            this.linePlotWavelet.Thickness = 1;
            // 
            // textBoxCoeffs
            // 
            this.textBoxCoeffs.Location = new System.Drawing.Point(378, 6);
            this.textBoxCoeffs.Multiline = true;
            this.textBoxCoeffs.Name = "textBoxCoeffs";
            this.textBoxCoeffs.Size = new System.Drawing.Size(208, 481);
            this.textBoxCoeffs.TabIndex = 6;
            // 
            // textBoxResult
            // 
            this.textBoxResult.Location = new System.Drawing.Point(592, 6);
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.Size = new System.Drawing.Size(203, 481);
            this.textBoxResult.TabIndex = 7;
            // 
            // buttonCompute
            // 
            this.buttonCompute.Location = new System.Drawing.Point(66, 95);
            this.buttonCompute.Name = "buttonCompute";
            this.buttonCompute.Size = new System.Drawing.Size(162, 56);
            this.buttonCompute.TabIndex = 8;
            this.buttonCompute.Text = "Go!";
            this.buttonCompute.UseVisualStyleBackColor = true;
            this.buttonCompute.Click += new System.EventHandler(this.buttonCompute_Click);
            // 
            // labelWaveletName
            // 
            this.labelWaveletName.Location = new System.Drawing.Point(252, 64);
            this.labelWaveletName.Name = "labelWaveletName";
            this.labelWaveletName.Size = new System.Drawing.Size(101, 23);
            this.labelWaveletName.TabIndex = 9;
            this.labelWaveletName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxResultInv
            // 
            this.textBoxResultInv.Location = new System.Drawing.Point(801, 6);
            this.textBoxResultInv.Multiline = true;
            this.textBoxResultInv.Name = "textBoxResultInv";
            this.textBoxResultInv.Size = new System.Drawing.Size(203, 481);
            this.textBoxResultInv.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Family";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "Taps";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(110, 505);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "Lo_Dec and Hi_Dec plot";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(432, 505);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 17);
            this.label5.TabIndex = 14;
            this.label5.Text = "Lo_Dec coeffs";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(608, 505);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(158, 17);
            this.label6.TabIndex = 15;
            this.label6.Text = "FWT of [0,1,2,...,Size-1]";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(818, 505);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 17);
            this.label7.TabIndex = 16;
            this.label7.Text = "Reconstructed from FWT";
            // 
            // WaveletForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1015, 547);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxResultInv);
            this.Controls.Add(this.labelWaveletName);
            this.Controls.Add(this.buttonCompute);
            this.Controls.Add(this.textBoxResult);
            this.Controls.Add(this.textBoxCoeffs);
            this.Controls.Add(this.linePlotWavelet);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSize);
            this.Controls.Add(this.comboBoxTaps);
            this.Controls.Add(this.comboBoxFamily);
            this.Name = "WaveletForm";
            this.Text = "WaveletForm";
            this.Load += new System.EventHandler(this.WaveletForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxFamily;
        private System.Windows.Forms.ComboBox comboBoxTaps;
        private System.Windows.Forms.TextBox textBoxSize;
        private System.Windows.Forms.Label label1;
        private UserControls.LinePlot linePlotWavelet;
        private System.Windows.Forms.TextBox textBoxCoeffs;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.Button buttonCompute;
        private System.Windows.Forms.Label labelWaveletName;
        private System.Windows.Forms.TextBox textBoxResultInv;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}