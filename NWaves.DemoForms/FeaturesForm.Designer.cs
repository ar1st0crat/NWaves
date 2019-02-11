using NWaves.DemoForms.UserControls;

namespace NWaves.DemoForms
{
    partial class FeaturesForm
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
            this.featuresListView = new System.Windows.Forms.ListView();
            this.spectrumPictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.peaksListBox = new System.Windows.Forms.ListBox();
            this.featureLabel = new System.Windows.Forms.Label();
            this.spectrogramPlot = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spectrumPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1183, 28);
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
            // featuresListView
            // 
            this.featuresListView.FullRowSelect = true;
            this.featuresListView.GridLines = true;
            this.featuresListView.Location = new System.Drawing.Point(12, 37);
            this.featuresListView.Name = "featuresListView";
            this.featuresListView.Size = new System.Drawing.Size(680, 319);
            this.featuresListView.TabIndex = 5;
            this.featuresListView.UseCompatibleStateImageBehavior = false;
            this.featuresListView.View = System.Windows.Forms.View.Details;
            this.featuresListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.featuresListView_ColumnClick);
            this.featuresListView.SelectedIndexChanged += new System.EventHandler(this.featuresListView_SelectedIndexChanged);
            // 
            // spectrumPictureBox
            // 
            this.spectrumPictureBox.BackColor = System.Drawing.SystemColors.Window;
            this.spectrumPictureBox.Location = new System.Drawing.Point(698, 37);
            this.spectrumPictureBox.Name = "spectrumPictureBox";
            this.spectrumPictureBox.Size = new System.Drawing.Size(473, 233);
            this.spectrumPictureBox.TabIndex = 6;
            this.spectrumPictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(557, 365);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Click column header";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(886, 280);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Harmonic peaks";
            // 
            // peaksListBox
            // 
            this.peaksListBox.Font = new System.Drawing.Font("Courier New", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.peaksListBox.FormattingEnabled = true;
            this.peaksListBox.ItemHeight = 20;
            this.peaksListBox.Location = new System.Drawing.Point(698, 320);
            this.peaksListBox.Name = "peaksListBox";
            this.peaksListBox.Size = new System.Drawing.Size(473, 324);
            this.peaksListBox.TabIndex = 9;
            // 
            // featureLabel
            // 
            this.featureLabel.AutoSize = true;
            this.featureLabel.Location = new System.Drawing.Point(15, 365);
            this.featureLabel.Name = "featureLabel";
            this.featureLabel.Size = new System.Drawing.Size(0, 17);
            this.featureLabel.TabIndex = 10;
            // 
            // spectrogramPlot
            // 
            this.spectrogramPlot.AutoScroll = true;
            this.spectrogramPlot.BackColor = System.Drawing.Color.Black;
            this.spectrogramPlot.ColorMapName = "magma";
            this.spectrogramPlot.Location = new System.Drawing.Point(12, 386);
            this.spectrogramPlot.Markline = null;
            this.spectrogramPlot.Name = "spectrogramPlot";
            this.spectrogramPlot.Size = new System.Drawing.Size(680, 258);
            this.spectrogramPlot.Spectrogram = null;
            this.spectrogramPlot.TabIndex = 11;
            // 
            // FeaturesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1183, 652);
            this.Controls.Add(this.spectrogramPlot);
            this.Controls.Add(this.featureLabel);
            this.Controls.Add(this.peaksListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.spectrumPictureBox);
            this.Controls.Add(this.featuresListView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FeaturesForm";
            this.Text = "FeaturesForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spectrumPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ListView featuresListView;
        private System.Windows.Forms.PictureBox spectrumPictureBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox peaksListBox;
        private System.Windows.Forms.Label featureLabel;
        private SpectrogramPlot spectrogramPlot;
    }
}