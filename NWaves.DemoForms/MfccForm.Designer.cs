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
            this.melFilterBankPanel.Location = new System.Drawing.Point(12, 404);
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
            this.mfccPanel.Location = new System.Drawing.Point(606, 404);
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
            this.mfccListView.Size = new System.Drawing.Size(915, 366);
            this.mfccListView.TabIndex = 3;
            this.mfccListView.UseCompatibleStateImageBehavior = false;
            this.mfccListView.View = System.Windows.Forms.View.Details;
            this.mfccListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.mfccListView_ItemSelectionChanged);
            // 
            // MfccForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 576);
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
    }
}