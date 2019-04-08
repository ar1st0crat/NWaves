namespace NWaves.DemoForms
{
    partial class AdaptiveFiltersForm
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
            this.adaptButton = new System.Windows.Forms.Button();
            this.weightsListBox = new System.Windows.Forms.ListBox();
            this.linePlot1 = new NWaves.DemoForms.UserControls.LinePlot();
            this.label1 = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.noiseRadioButton = new System.Windows.Forms.RadioButton();
            this.sinRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lmsRadioButton = new System.Windows.Forms.RadioButton();
            this.nlmsRadioButton = new System.Windows.Forms.RadioButton();
            this.lmfRadioButton = new System.Windows.Forms.RadioButton();
            this.rlsRadioButton = new System.Windows.Forms.RadioButton();
            this.muTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // adaptButton
            // 
            this.adaptButton.Location = new System.Drawing.Point(112, 72);
            this.adaptButton.Name = "adaptButton";
            this.adaptButton.Size = new System.Drawing.Size(273, 62);
            this.adaptButton.TabIndex = 1;
            this.adaptButton.Text = "Adapt!";
            this.adaptButton.UseVisualStyleBackColor = true;
            this.adaptButton.Click += new System.EventHandler(this.adaptButton_Click);
            // 
            // weightsListBox
            // 
            this.weightsListBox.FormattingEnabled = true;
            this.weightsListBox.ItemHeight = 16;
            this.weightsListBox.Location = new System.Drawing.Point(653, 304);
            this.weightsListBox.Name = "weightsListBox";
            this.weightsListBox.Size = new System.Drawing.Size(135, 132);
            this.weightsListBox.TabIndex = 2;
            // 
            // linePlot1
            // 
            this.linePlot1.AutoScroll = true;
            this.linePlot1.BackColor = System.Drawing.Color.White;
            this.linePlot1.ForeColor = System.Drawing.Color.Blue;
            this.linePlot1.Gain = null;
            this.linePlot1.Legend = null;
            this.linePlot1.Line = null;
            this.linePlot1.Location = new System.Drawing.Point(13, 47);
            this.linePlot1.Mark = null;
            this.linePlot1.Markline = null;
            this.linePlot1.Name = "linePlot1";
            this.linePlot1.PaddingX = 30;
            this.linePlot1.PaddingY = 20;
            this.linePlot1.Size = new System.Drawing.Size(775, 204);
            this.linePlot1.Stride = 1;
            this.linePlot1.TabIndex = 0;
            this.linePlot1.Thickness = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filtered:";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(34, 341);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(151, 62);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate signal";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // noiseRadioButton
            // 
            this.noiseRadioButton.AutoSize = true;
            this.noiseRadioButton.Checked = true;
            this.noiseRadioButton.Location = new System.Drawing.Point(102, 13);
            this.noiseRadioButton.Name = "noiseRadioButton";
            this.noiseRadioButton.Size = new System.Drawing.Size(65, 21);
            this.noiseRadioButton.TabIndex = 5;
            this.noiseRadioButton.TabStop = true;
            this.noiseRadioButton.Text = "Noise";
            this.noiseRadioButton.UseVisualStyleBackColor = true;
            // 
            // sinRadioButton
            // 
            this.sinRadioButton.AutoSize = true;
            this.sinRadioButton.Location = new System.Drawing.Point(188, 13);
            this.sinRadioButton.Name = "sinRadioButton";
            this.sinRadioButton.Size = new System.Drawing.Size(83, 21);
            this.sinRadioButton.TabIndex = 6;
            this.sinRadioButton.Text = "Sinusoid";
            this.sinRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(686, 274);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Weights";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.muTextBox);
            this.groupBox1.Controls.Add(this.rlsRadioButton);
            this.groupBox1.Controls.Add(this.lmfRadioButton);
            this.groupBox1.Controls.Add(this.nlmsRadioButton);
            this.groupBox1.Controls.Add(this.lmsRadioButton);
            this.groupBox1.Controls.Add(this.adaptButton);
            this.groupBox1.Location = new System.Drawing.Point(231, 285);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(406, 153);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Adaptive filters";
            // 
            // lmsRadioButton
            // 
            this.lmsRadioButton.AutoSize = true;
            this.lmsRadioButton.Checked = true;
            this.lmsRadioButton.Location = new System.Drawing.Point(18, 32);
            this.lmsRadioButton.Name = "lmsRadioButton";
            this.lmsRadioButton.Size = new System.Drawing.Size(57, 21);
            this.lmsRadioButton.TabIndex = 2;
            this.lmsRadioButton.TabStop = true;
            this.lmsRadioButton.Text = "LMS";
            this.lmsRadioButton.UseVisualStyleBackColor = true;
            // 
            // nlmsRadioButton
            // 
            this.nlmsRadioButton.AutoSize = true;
            this.nlmsRadioButton.Location = new System.Drawing.Point(18, 59);
            this.nlmsRadioButton.Name = "nlmsRadioButton";
            this.nlmsRadioButton.Size = new System.Drawing.Size(67, 21);
            this.nlmsRadioButton.TabIndex = 3;
            this.nlmsRadioButton.Text = "NLMS";
            this.nlmsRadioButton.UseVisualStyleBackColor = true;
            // 
            // lmfRadioButton
            // 
            this.lmfRadioButton.AutoSize = true;
            this.lmfRadioButton.Location = new System.Drawing.Point(18, 86);
            this.lmfRadioButton.Name = "lmfRadioButton";
            this.lmfRadioButton.Size = new System.Drawing.Size(56, 21);
            this.lmfRadioButton.TabIndex = 4;
            this.lmfRadioButton.Text = "LMF";
            this.lmfRadioButton.UseVisualStyleBackColor = true;
            // 
            // rlsRadioButton
            // 
            this.rlsRadioButton.AutoSize = true;
            this.rlsRadioButton.Location = new System.Drawing.Point(18, 113);
            this.rlsRadioButton.Name = "rlsRadioButton";
            this.rlsRadioButton.Size = new System.Drawing.Size(56, 21);
            this.rlsRadioButton.TabIndex = 5;
            this.rlsRadioButton.Text = "RLS";
            this.rlsRadioButton.UseVisualStyleBackColor = true;
            // 
            // muTextBox
            // 
            this.muTextBox.Location = new System.Drawing.Point(142, 36);
            this.muTextBox.Name = "muTextBox";
            this.muTextBox.Size = new System.Drawing.Size(53, 22);
            this.muTextBox.TabIndex = 6;
            this.muTextBox.Text = "0,5";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(109, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "mu";
            // 
            // AdaptiveFiltersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sinRadioButton);
            this.Controls.Add(this.noiseRadioButton);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.weightsListBox);
            this.Controls.Add(this.linePlot1);
            this.Name = "AdaptiveFiltersForm";
            this.Text = "AdaptiveFiltersForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UserControls.LinePlot linePlot1;
        private System.Windows.Forms.Button adaptButton;
        private System.Windows.Forms.ListBox weightsListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.RadioButton noiseRadioButton;
        private System.Windows.Forms.RadioButton sinRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rlsRadioButton;
        private System.Windows.Forms.RadioButton lmfRadioButton;
        private System.Windows.Forms.RadioButton nlmsRadioButton;
        private System.Windows.Forms.RadioButton lmsRadioButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox muTextBox;
    }
}