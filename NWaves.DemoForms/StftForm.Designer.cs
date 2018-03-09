namespace NWaves.DemoForms
{
    partial class StftForm
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
            this.processedSignalPanel = new System.Windows.Forms.Panel();
            this.signalPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spectrogramPanel = new System.Windows.Forms.Panel();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // processedSignalPanel
            // 
            this.processedSignalPanel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.processedSignalPanel.Location = new System.Drawing.Point(12, 213);
            this.processedSignalPanel.Name = "processedSignalPanel";
            this.processedSignalPanel.Size = new System.Drawing.Size(1081, 174);
            this.processedSignalPanel.TabIndex = 7;
            // 
            // signalPanel
            // 
            this.signalPanel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.signalPanel.Location = new System.Drawing.Point(12, 42);
            this.signalPanel.Name = "signalPanel";
            this.signalPanel.Size = new System.Drawing.Size(1081, 165);
            this.signalPanel.TabIndex = 6;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1105, 28);
            this.menuStrip1.TabIndex = 8;
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
            this.openToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // spectrogramPanel
            // 
            this.spectrogramPanel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.spectrogramPanel.Location = new System.Drawing.Point(12, 393);
            this.spectrogramPanel.Name = "spectrogramPanel";
            this.spectrogramPanel.Size = new System.Drawing.Size(1081, 231);
            this.spectrogramPanel.TabIndex = 7;
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(48, 24);
            this.playToolStripMenuItem.Text = "&Play";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // StftForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1105, 636);
            this.Controls.Add(this.spectrogramPanel);
            this.Controls.Add(this.processedSignalPanel);
            this.Controls.Add(this.signalPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StftForm";
            this.Text = "StftForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel processedSignalPanel;
        private System.Windows.Forms.Panel signalPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Panel spectrogramPanel;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
    }
}