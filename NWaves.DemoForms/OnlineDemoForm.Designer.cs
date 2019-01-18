namespace NWaves.DemoForms
{
    partial class OnlineDemoForm
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
            this.components = new System.ComponentModel.Container();
            this.signalPlot = new NWaves.DemoForms.UserControls.SignalPlot();
            this.nextButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.intervalTextBox = new System.Windows.Forms.TextBox();
            this.kernelSizeTextBox = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSizeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chunkTimer = new System.Windows.Forms.Timer(this.components);
            this.applyButton = new System.Windows.Forms.Button();
            this.filteredSignalPlot = new NWaves.DemoForms.UserControls.SignalPlot();
            this.labelInfo = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // signalPlot
            // 
            this.signalPlot.AutoScroll = true;
            this.signalPlot.BackColor = System.Drawing.Color.White;
            this.signalPlot.ForeColor = System.Drawing.Color.Blue;
            this.signalPlot.Gain = 75F;
            this.signalPlot.Location = new System.Drawing.Point(10, 203);
            this.signalPlot.Name = "signalPlot";
            this.signalPlot.PaddingX = 24;
            this.signalPlot.PaddingY = 5;
            this.signalPlot.Signal = null;
            this.signalPlot.Size = new System.Drawing.Size(680, 140);
            this.signalPlot.Stride = 64;
            this.signalPlot.TabIndex = 0;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(281, 52);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(133, 87);
            this.nextButton.TabIndex = 2;
            this.nextButton.Text = "Next chunk";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(420, 52);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(133, 87);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(559, 52);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(133, 87);
            this.stopButton.TabIndex = 4;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // intervalTextBox
            // 
            this.intervalTextBox.Location = new System.Drawing.Point(159, 105);
            this.intervalTextBox.Name = "intervalTextBox";
            this.intervalTextBox.Size = new System.Drawing.Size(54, 22);
            this.intervalTextBox.TabIndex = 5;
            this.intervalTextBox.Text = "100";
            // 
            // kernelSizeTextBox
            // 
            this.kernelSizeTextBox.Location = new System.Drawing.Point(159, 78);
            this.kernelSizeTextBox.Name = "kernelSizeTextBox";
            this.kernelSizeTextBox.Size = new System.Drawing.Size(54, 22);
            this.kernelSizeTextBox.TabIndex = 6;
            this.kernelSizeTextBox.Text = "1025";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(702, 28);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(63, 24);
            this.loadToolStripMenuItem.Text = "&Load...";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // fftSizeTextBox
            // 
            this.fftSizeTextBox.Location = new System.Drawing.Point(159, 52);
            this.fftSizeTextBox.Name = "fftSizeTextBox";
            this.fftSizeTextBox.Size = new System.Drawing.Size(54, 22);
            this.fftSizeTextBox.TabIndex = 8;
            this.fftSizeTextBox.Text = "16384";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 17);
            this.label1.TabIndex = 9;
            this.label1.Text = "FFT size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 17);
            this.label2.TabIndex = 10;
            this.label2.Text = "Kernel size";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(64, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Time interval";
            // 
            // chunkTimer
            // 
            this.chunkTimer.Tick += new System.EventHandler(this.ProcessNewChunk);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(67, 144);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(146, 34);
            this.applyButton.TabIndex = 12;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // filteredSignalPlot
            // 
            this.filteredSignalPlot.AutoScroll = true;
            this.filteredSignalPlot.BackColor = System.Drawing.Color.White;
            this.filteredSignalPlot.ForeColor = System.Drawing.Color.Blue;
            this.filteredSignalPlot.Gain = 75F;
            this.filteredSignalPlot.Location = new System.Drawing.Point(10, 377);
            this.filteredSignalPlot.Name = "filteredSignalPlot";
            this.filteredSignalPlot.PaddingX = 24;
            this.filteredSignalPlot.PaddingY = 5;
            this.filteredSignalPlot.Signal = null;
            this.filteredSignalPlot.Size = new System.Drawing.Size(680, 140);
            this.filteredSignalPlot.Stride = 64;
            this.filteredSignalPlot.TabIndex = 13;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(278, 161);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(208, 17);
            this.labelInfo.TabIndex = 14;
            this.labelInfo.Text = "Chunk #1 Processed 0 seconds";
            // 
            // OnlineDemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 539);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.filteredSignalPlot);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.fftSizeTextBox);
            this.Controls.Add(this.kernelSizeTextBox);
            this.Controls.Add(this.intervalTextBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.signalPlot);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "OnlineDemoForm";
            this.Text = "OnlineDemoForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UserControls.SignalPlot signalPlot;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.TextBox intervalTextBox;
        private System.Windows.Forms.TextBox kernelSizeTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.TextBox fftSizeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer chunkTimer;
        private System.Windows.Forms.Button applyButton;
        private UserControls.SignalPlot filteredSignalPlot;
        private System.Windows.Forms.Label labelInfo;
    }
}