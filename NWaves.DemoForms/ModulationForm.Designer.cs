namespace NWaves.DemoForms
{
    partial class ModulationForm
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
            this.demodulateButton = new System.Windows.Forms.Button();
            this.modulateButton = new System.Windows.Forms.Button();
            this.demodulatedPlot = new NWaves.DemoForms.UserControls.LinePlot();
            this.modulatedPlot = new NWaves.DemoForms.UserControls.LinePlot();
            this.carrierFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.modulationFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.modulationIndexTextBox = new System.Windows.Forms.TextBox();
            this.amplitudeRadioButton = new System.Windows.Forms.RadioButton();
            this.frequencyRadioButton = new System.Windows.Forms.RadioButton();
            this.phaseRadioButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // demodulateButton
            // 
            this.demodulateButton.Location = new System.Drawing.Point(633, 305);
            this.demodulateButton.Name = "demodulateButton";
            this.demodulateButton.Size = new System.Drawing.Size(204, 39);
            this.demodulateButton.TabIndex = 2;
            this.demodulateButton.Text = "Demodulate";
            this.demodulateButton.UseVisualStyleBackColor = true;
            this.demodulateButton.Click += new System.EventHandler(this.demodulateButton_Click);
            // 
            // modulateButton
            // 
            this.modulateButton.Location = new System.Drawing.Point(633, 39);
            this.modulateButton.Name = "modulateButton";
            this.modulateButton.Size = new System.Drawing.Size(204, 39);
            this.modulateButton.TabIndex = 3;
            this.modulateButton.Text = "Modulate";
            this.modulateButton.UseVisualStyleBackColor = true;
            this.modulateButton.Click += new System.EventHandler(this.modulateButton_Click);
            // 
            // demodulatedPlot
            // 
            this.demodulatedPlot.AutoScroll = true;
            this.demodulatedPlot.BackColor = System.Drawing.Color.White;
            this.demodulatedPlot.ForeColor = System.Drawing.Color.Blue;
            this.demodulatedPlot.Gain = null;
            this.demodulatedPlot.Legend = null;
            this.demodulatedPlot.Line = null;
            this.demodulatedPlot.Location = new System.Drawing.Point(12, 356);
            this.demodulatedPlot.Mark = null;
            this.demodulatedPlot.Markline = null;
            this.demodulatedPlot.Name = "demodulatedPlot";
            this.demodulatedPlot.PaddingX = 30;
            this.demodulatedPlot.PaddingY = 20;
            this.demodulatedPlot.Size = new System.Drawing.Size(825, 165);
            this.demodulatedPlot.Stride = 1;
            this.demodulatedPlot.TabIndex = 1;
            this.demodulatedPlot.Thickness = 1;
            // 
            // modulatedPlot
            // 
            this.modulatedPlot.AutoScroll = true;
            this.modulatedPlot.BackColor = System.Drawing.Color.White;
            this.modulatedPlot.ForeColor = System.Drawing.Color.Blue;
            this.modulatedPlot.Gain = null;
            this.modulatedPlot.Legend = null;
            this.modulatedPlot.Line = null;
            this.modulatedPlot.Location = new System.Drawing.Point(13, 123);
            this.modulatedPlot.Mark = null;
            this.modulatedPlot.Markline = null;
            this.modulatedPlot.Name = "modulatedPlot";
            this.modulatedPlot.PaddingX = 30;
            this.modulatedPlot.PaddingY = 20;
            this.modulatedPlot.Size = new System.Drawing.Size(826, 165);
            this.modulatedPlot.Stride = 1;
            this.modulatedPlot.TabIndex = 0;
            this.modulatedPlot.Thickness = 1;
            // 
            // carrierFrequencyTextBox
            // 
            this.carrierFrequencyTextBox.Location = new System.Drawing.Point(163, 24);
            this.carrierFrequencyTextBox.Name = "carrierFrequencyTextBox";
            this.carrierFrequencyTextBox.Size = new System.Drawing.Size(65, 22);
            this.carrierFrequencyTextBox.TabIndex = 4;
            this.carrierFrequencyTextBox.Text = "3000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Carrier frequency";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Modulation frequency";
            // 
            // modulationFrequencyTextBox
            // 
            this.modulationFrequencyTextBox.Location = new System.Drawing.Point(163, 52);
            this.modulationFrequencyTextBox.Name = "modulationFrequencyTextBox";
            this.modulationFrequencyTextBox.Size = new System.Drawing.Size(65, 22);
            this.modulationFrequencyTextBox.TabIndex = 6;
            this.modulationFrequencyTextBox.Text = "100";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Modulation index";
            // 
            // modulationIndexTextBox
            // 
            this.modulationIndexTextBox.Location = new System.Drawing.Point(163, 80);
            this.modulationIndexTextBox.Name = "modulationIndexTextBox";
            this.modulationIndexTextBox.Size = new System.Drawing.Size(65, 22);
            this.modulationIndexTextBox.TabIndex = 8;
            this.modulationIndexTextBox.Text = "0,5";
            // 
            // amplitudeRadioButton
            // 
            this.amplitudeRadioButton.AutoSize = true;
            this.amplitudeRadioButton.Checked = true;
            this.amplitudeRadioButton.Location = new System.Drawing.Point(317, 24);
            this.amplitudeRadioButton.Name = "amplitudeRadioButton";
            this.amplitudeRadioButton.Size = new System.Drawing.Size(91, 21);
            this.amplitudeRadioButton.TabIndex = 10;
            this.amplitudeRadioButton.TabStop = true;
            this.amplitudeRadioButton.Text = "Amplitude";
            this.amplitudeRadioButton.UseVisualStyleBackColor = true;
            this.amplitudeRadioButton.CheckedChanged += new System.EventHandler(this.amplitudeRadioButton_CheckedChanged);
            // 
            // frequencyRadioButton
            // 
            this.frequencyRadioButton.AutoSize = true;
            this.frequencyRadioButton.Location = new System.Drawing.Point(317, 53);
            this.frequencyRadioButton.Name = "frequencyRadioButton";
            this.frequencyRadioButton.Size = new System.Drawing.Size(96, 21);
            this.frequencyRadioButton.TabIndex = 11;
            this.frequencyRadioButton.Text = "Frequency";
            this.frequencyRadioButton.UseVisualStyleBackColor = true;
            this.frequencyRadioButton.CheckedChanged += new System.EventHandler(this.frequencyRadioButton_CheckedChanged);
            // 
            // phaseRadioButton
            // 
            this.phaseRadioButton.AutoSize = true;
            this.phaseRadioButton.Location = new System.Drawing.Point(317, 81);
            this.phaseRadioButton.Name = "phaseRadioButton";
            this.phaseRadioButton.Size = new System.Drawing.Size(69, 21);
            this.phaseRadioButton.TabIndex = 12;
            this.phaseRadioButton.Text = "Phase";
            this.phaseRadioButton.UseVisualStyleBackColor = true;
            this.phaseRadioButton.CheckedChanged += new System.EventHandler(this.phaseRadioButton_CheckedChanged);
            // 
            // ModulationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 533);
            this.Controls.Add(this.phaseRadioButton);
            this.Controls.Add(this.frequencyRadioButton);
            this.Controls.Add(this.amplitudeRadioButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modulationIndexTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.modulationFrequencyTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.carrierFrequencyTextBox);
            this.Controls.Add(this.modulateButton);
            this.Controls.Add(this.demodulateButton);
            this.Controls.Add(this.demodulatedPlot);
            this.Controls.Add(this.modulatedPlot);
            this.Name = "ModulationForm";
            this.Text = "ModulationForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UserControls.LinePlot modulatedPlot;
        private UserControls.LinePlot demodulatedPlot;
        private System.Windows.Forms.Button demodulateButton;
        private System.Windows.Forms.Button modulateButton;
        private System.Windows.Forms.TextBox carrierFrequencyTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox modulationFrequencyTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox modulationIndexTextBox;
        private System.Windows.Forms.RadioButton amplitudeRadioButton;
        private System.Windows.Forms.RadioButton frequencyRadioButton;
        private System.Windows.Forms.RadioButton phaseRadioButton;
    }
}