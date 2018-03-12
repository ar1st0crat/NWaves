using NWaves.DemoForms.UserControls;

namespace NWaves.DemoForms
{
    partial class LpcForm
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
            this.lpcListView = new System.Windows.Forms.ListView();
            this.lpcPanel = new LinePlot();
            this.spectrumPanel = new LinePlot();
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
            this.menuStrip1.Size = new System.Drawing.Size(1030, 28);
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
            // lpcListView
            // 
            this.lpcListView.FullRowSelect = true;
            this.lpcListView.GridLines = true;
            this.lpcListView.Location = new System.Drawing.Point(12, 41);
            this.lpcListView.Name = "lpcListView";
            this.lpcListView.Size = new System.Drawing.Size(1004, 366);
            this.lpcListView.TabIndex = 6;
            this.lpcListView.UseCompatibleStateImageBehavior = false;
            this.lpcListView.View = System.Windows.Forms.View.Details;
            this.lpcListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lpcListView_ItemSelectionChanged);
            // 
            // lpcPanel
            // 
            this.lpcPanel.BackColor = System.Drawing.Color.White;
            this.lpcPanel.Location = new System.Drawing.Point(605, 413);
            this.lpcPanel.Name = "lpcPanel";
            this.lpcPanel.Size = new System.Drawing.Size(411, 210);
            this.lpcPanel.TabIndex = 5;
            // 
            // spectrumPanel
            // 
            this.spectrumPanel.BackColor = System.Drawing.Color.White;
            this.spectrumPanel.Location = new System.Drawing.Point(11, 413);
            this.spectrumPanel.Name = "spectrumPanel";
            this.spectrumPanel.Size = new System.Drawing.Size(588, 210);
            this.spectrumPanel.TabIndex = 4;
            // 
            // LpcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1030, 635);
            this.Controls.Add(this.lpcListView);
            this.Controls.Add(this.lpcPanel);
            this.Controls.Add(this.spectrumPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LpcForm";
            this.Text = "LpcForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ListView lpcListView;
        private LinePlot lpcPanel;
        private LinePlot spectrumPanel;
    }
}