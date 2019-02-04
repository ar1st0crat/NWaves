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
            this.featurePlotPanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.spectrumPictureBox = new System.Windows.Forms.PictureBox();
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
            this.featuresListView.Size = new System.Drawing.Size(782, 391);
            this.featuresListView.TabIndex = 5;
            this.featuresListView.UseCompatibleStateImageBehavior = false;
            this.featuresListView.View = System.Windows.Forms.View.Details;
            this.featuresListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.featuresListView_ColumnClick);
            this.featuresListView.SelectedIndexChanged += new System.EventHandler(this.featuresListView_SelectedIndexChanged);
            // 
            // featurePlotPanel
            // 
            this.featurePlotPanel.AutoScroll = true;
            this.featurePlotPanel.BackColor = System.Drawing.Color.White;
            this.featurePlotPanel.ForeColor = System.Drawing.Color.Blue;
            this.featurePlotPanel.Gain = null;
            this.featurePlotPanel.Legend = null;
            this.featurePlotPanel.Line = null;
            this.featurePlotPanel.Location = new System.Drawing.Point(12, 434);
            this.featurePlotPanel.Mark = null;
            this.featurePlotPanel.Markline = null;
            this.featurePlotPanel.Name = "featurePlotPanel";
            this.featurePlotPanel.PaddingX = 30;
            this.featurePlotPanel.PaddingY = 20;
            this.featurePlotPanel.Size = new System.Drawing.Size(782, 160);
            this.featurePlotPanel.Stride = 1;
            this.featurePlotPanel.TabIndex = 4;
            this.featurePlotPanel.Thickness = 1;
            // 
            // spectrumPictureBox
            // 
            this.spectrumPictureBox.BackColor = System.Drawing.SystemColors.Window;
            this.spectrumPictureBox.Location = new System.Drawing.Point(801, 37);
            this.spectrumPictureBox.Name = "spectrumPictureBox";
            this.spectrumPictureBox.Size = new System.Drawing.Size(370, 233);
            this.spectrumPictureBox.TabIndex = 6;
            this.spectrumPictureBox.TabStop = false;
            // 
            // FeaturesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1183, 606);
            this.Controls.Add(this.spectrumPictureBox);
            this.Controls.Add(this.featuresListView);
            this.Controls.Add(this.featurePlotPanel);
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
        private LinePlot featurePlotPanel;
        private System.Windows.Forms.PictureBox spectrumPictureBox;
    }
}