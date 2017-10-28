namespace NWaves.DemoForms
{
    partial class FiltersForm
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
            this.magnitudeResponsePanel = new System.Windows.Forms.Panel();
            this.phaseResponsePanel = new System.Windows.Forms.Panel();
            this.poleZeroPanel = new System.Windows.Forms.Panel();
            this.signalBeforeFilteringPanel = new System.Windows.Forms.Panel();
            this.spectrogramBeforeFilteringPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlapAddToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlapSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.differenceEquationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyzeFilterButton = new System.Windows.Forms.Button();
            this.filterTypesComboBox = new System.Windows.Forms.ComboBox();
            this.filterParamsDataGrid = new System.Windows.Forms.DataGridView();
            this.Param = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.signalAfterFilteringPanel = new System.Windows.Forms.Panel();
            this.spectrogramAfterFilteringPanel = new System.Windows.Forms.Panel();
            this.numeratorListBox = new System.Windows.Forms.ListBox();
            this.denominatorListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.playSignalButton = new System.Windows.Forms.Button();
            this.playFilteredSignalButton = new System.Windows.Forms.Button();
            this.orderNumeratorTextBox = new System.Windows.Forms.TextBox();
            this.orderDenominatorTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.changeOrderButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterParamsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // magnitudeResponsePanel
            // 
            this.magnitudeResponsePanel.BackColor = System.Drawing.SystemColors.Window;
            this.magnitudeResponsePanel.Location = new System.Drawing.Point(219, 50);
            this.magnitudeResponsePanel.Name = "magnitudeResponsePanel";
            this.magnitudeResponsePanel.Size = new System.Drawing.Size(512, 157);
            this.magnitudeResponsePanel.TabIndex = 0;
            // 
            // phaseResponsePanel
            // 
            this.phaseResponsePanel.BackColor = System.Drawing.SystemColors.Window;
            this.phaseResponsePanel.Location = new System.Drawing.Point(219, 217);
            this.phaseResponsePanel.Name = "phaseResponsePanel";
            this.phaseResponsePanel.Size = new System.Drawing.Size(512, 157);
            this.phaseResponsePanel.TabIndex = 1;
            // 
            // poleZeroPanel
            // 
            this.poleZeroPanel.BackColor = System.Drawing.SystemColors.Window;
            this.poleZeroPanel.Location = new System.Drawing.Point(738, 50);
            this.poleZeroPanel.Name = "poleZeroPanel";
            this.poleZeroPanel.Size = new System.Drawing.Size(217, 202);
            this.poleZeroPanel.TabIndex = 2;
            // 
            // signalBeforeFilteringPanel
            // 
            this.signalBeforeFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalBeforeFilteringPanel.Location = new System.Drawing.Point(13, 427);
            this.signalBeforeFilteringPanel.Name = "signalBeforeFilteringPanel";
            this.signalBeforeFilteringPanel.Size = new System.Drawing.Size(461, 153);
            this.signalBeforeFilteringPanel.TabIndex = 3;
            // 
            // spectrogramBeforeFilteringPanel
            // 
            this.spectrogramBeforeFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.spectrogramBeforeFilteringPanel.Location = new System.Drawing.Point(13, 586);
            this.spectrogramBeforeFilteringPanel.Name = "spectrogramBeforeFilteringPanel";
            this.spectrogramBeforeFilteringPanel.Size = new System.Drawing.Size(461, 128);
            this.spectrogramBeforeFilteringPanel.TabIndex = 4;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.filterToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(968, 28);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(142, 26);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(142, 26);
            this.saveAsToolStripMenuItem.Text = "&Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(139, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(142, 26);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.overlapAddToolStripMenuItem,
            this.overlapSaveToolStripMenuItem,
            this.differenceEquationToolStripMenuItem});
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(54, 24);
            this.filterToolStripMenuItem.Text = "Fi&lter";
            // 
            // overlapAddToolStripMenuItem
            // 
            this.overlapAddToolStripMenuItem.Name = "overlapAddToolStripMenuItem";
            this.overlapAddToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.overlapAddToolStripMenuItem.Text = "Overlap-&Add";
            this.overlapAddToolStripMenuItem.Click += new System.EventHandler(this.overlapAddToolStripMenuItem_Click);
            // 
            // overlapSaveToolStripMenuItem
            // 
            this.overlapSaveToolStripMenuItem.Name = "overlapSaveToolStripMenuItem";
            this.overlapSaveToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.overlapSaveToolStripMenuItem.Text = "Overlap-&Save";
            this.overlapSaveToolStripMenuItem.Click += new System.EventHandler(this.overlapSaveToolStripMenuItem_Click);
            // 
            // differenceEquationToolStripMenuItem
            // 
            this.differenceEquationToolStripMenuItem.Name = "differenceEquationToolStripMenuItem";
            this.differenceEquationToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.differenceEquationToolStripMenuItem.Text = "Difference &Equation";
            this.differenceEquationToolStripMenuItem.Click += new System.EventHandler(this.differenceEquationToolStripMenuItem_Click);
            // 
            // analyzeFilterButton
            // 
            this.analyzeFilterButton.Location = new System.Drawing.Point(13, 322);
            this.analyzeFilterButton.Name = "analyzeFilterButton";
            this.analyzeFilterButton.Size = new System.Drawing.Size(189, 52);
            this.analyzeFilterButton.TabIndex = 6;
            this.analyzeFilterButton.Text = "Analyze filter";
            this.analyzeFilterButton.UseVisualStyleBackColor = true;
            this.analyzeFilterButton.Click += new System.EventHandler(this.buttonAnalyzeFilter_Click);
            // 
            // filterTypesComboBox
            // 
            this.filterTypesComboBox.FormattingEnabled = true;
            this.filterTypesComboBox.Items.AddRange(new object[] {
            "Custom IIR",
            "Custom FIR",
            "BiQuad LP",
            "BiQuad HP",
            "BiQuad BP",
            "BiQuad notch",
            "BiQuad allpass",
            "BiQuad peaking",
            "BiQuad lowshelf",
            "BiQuad highshelf",
            "Moving average",
            "Moving average recursive",
            "Pre-emphasis",
            "Butterworth",
            "Chebyshev"});
            this.filterTypesComboBox.Location = new System.Drawing.Point(12, 50);
            this.filterTypesComboBox.Name = "filterTypesComboBox";
            this.filterTypesComboBox.Size = new System.Drawing.Size(190, 24);
            this.filterTypesComboBox.TabIndex = 7;
            this.filterTypesComboBox.Text = "Custom IIR";
            this.filterTypesComboBox.SelectedIndexChanged += new System.EventHandler(this.filterTypesComboBox_SelectedIndexChanged);
            // 
            // filterParamsDataGrid
            // 
            this.filterParamsDataGrid.AllowUserToAddRows = false;
            this.filterParamsDataGrid.AllowUserToDeleteRows = false;
            this.filterParamsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.filterParamsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Param,
            this.Value});
            this.filterParamsDataGrid.Location = new System.Drawing.Point(13, 108);
            this.filterParamsDataGrid.Name = "filterParamsDataGrid";
            this.filterParamsDataGrid.RowTemplate.Height = 24;
            this.filterParamsDataGrid.Size = new System.Drawing.Size(189, 208);
            this.filterParamsDataGrid.TabIndex = 8;
            // 
            // Param
            // 
            this.Param.HeaderText = "Param";
            this.Param.Name = "Param";
            this.Param.ReadOnly = true;
            this.Param.Width = 80;
            // 
            // Value
            // 
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.Width = 65;
            // 
            // signalAfterFilteringPanel
            // 
            this.signalAfterFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalAfterFilteringPanel.Location = new System.Drawing.Point(480, 427);
            this.signalAfterFilteringPanel.Name = "signalAfterFilteringPanel";
            this.signalAfterFilteringPanel.Size = new System.Drawing.Size(475, 153);
            this.signalAfterFilteringPanel.TabIndex = 4;
            // 
            // spectrogramAfterFilteringPanel
            // 
            this.spectrogramAfterFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.spectrogramAfterFilteringPanel.Location = new System.Drawing.Point(480, 587);
            this.spectrogramAfterFilteringPanel.Name = "spectrogramAfterFilteringPanel";
            this.spectrogramAfterFilteringPanel.Size = new System.Drawing.Size(475, 128);
            this.spectrogramAfterFilteringPanel.TabIndex = 9;
            // 
            // numeratorListBox
            // 
            this.numeratorListBox.FormattingEnabled = true;
            this.numeratorListBox.ItemHeight = 16;
            this.numeratorListBox.Location = new System.Drawing.Point(763, 258);
            this.numeratorListBox.Name = "numeratorListBox";
            this.numeratorListBox.Size = new System.Drawing.Size(75, 116);
            this.numeratorListBox.TabIndex = 10;
            // 
            // denominatorListBox
            // 
            this.denominatorListBox.FormattingEnabled = true;
            this.denominatorListBox.ItemHeight = 16;
            this.denominatorListBox.Location = new System.Drawing.Point(879, 258);
            this.denominatorListBox.Name = "denominatorListBox";
            this.denominatorListBox.Size = new System.Drawing.Size(76, 116);
            this.denominatorListBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(399, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Frequency Response";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(796, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Pole-Zero Plot";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(741, 258);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "b";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(857, 258);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "a";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 407);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 17);
            this.label5.TabIndex = 16;
            this.label5.Text = "Before filtering";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(477, 407);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 17);
            this.label6.TabIndex = 17;
            this.label6.Text = "After filtering";
            // 
            // playSignalButton
            // 
            this.playSignalButton.Location = new System.Drawing.Point(118, 402);
            this.playSignalButton.Name = "playSignalButton";
            this.playSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playSignalButton.TabIndex = 18;
            this.playSignalButton.Text = "Play";
            this.playSignalButton.UseVisualStyleBackColor = true;
            this.playSignalButton.Click += new System.EventHandler(this.playSignalButton_Click);
            // 
            // playFilteredSignalButton
            // 
            this.playFilteredSignalButton.Location = new System.Drawing.Point(571, 402);
            this.playFilteredSignalButton.Name = "playFilteredSignalButton";
            this.playFilteredSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playFilteredSignalButton.TabIndex = 19;
            this.playFilteredSignalButton.Text = "Play";
            this.playFilteredSignalButton.UseVisualStyleBackColor = true;
            this.playFilteredSignalButton.Click += new System.EventHandler(this.playFilteredSignalButton_Click);
            // 
            // orderNumeratorTextBox
            // 
            this.orderNumeratorTextBox.Location = new System.Drawing.Point(36, 80);
            this.orderNumeratorTextBox.Name = "orderNumeratorTextBox";
            this.orderNumeratorTextBox.Size = new System.Drawing.Size(29, 22);
            this.orderNumeratorTextBox.TabIndex = 20;
            this.orderNumeratorTextBox.Text = "2";
            // 
            // orderDenominatorTextBox
            // 
            this.orderDenominatorTextBox.Location = new System.Drawing.Point(92, 80);
            this.orderDenominatorTextBox.Name = "orderDenominatorTextBox";
            this.orderDenominatorTextBox.Size = new System.Drawing.Size(30, 22);
            this.orderDenominatorTextBox.TabIndex = 21;
            this.orderDenominatorTextBox.Text = "2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(70, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 17);
            this.label7.TabIndex = 22;
            this.label7.Text = "a";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(16, 17);
            this.label8.TabIndex = 23;
            this.label8.Text = "b";
            // 
            // changeOrderButton
            // 
            this.changeOrderButton.Location = new System.Drawing.Point(135, 79);
            this.changeOrderButton.Name = "changeOrderButton";
            this.changeOrderButton.Size = new System.Drawing.Size(67, 23);
            this.changeOrderButton.TabIndex = 24;
            this.changeOrderButton.Text = "Change";
            this.changeOrderButton.UseVisualStyleBackColor = true;
            this.changeOrderButton.Click += new System.EventHandler(this.changeOrderButton_Click);
            // 
            // FiltersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 729);
            this.Controls.Add(this.changeOrderButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.orderDenominatorTextBox);
            this.Controls.Add(this.orderNumeratorTextBox);
            this.Controls.Add(this.playFilteredSignalButton);
            this.Controls.Add(this.playSignalButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.analyzeFilterButton);
            this.Controls.Add(this.denominatorListBox);
            this.Controls.Add(this.numeratorListBox);
            this.Controls.Add(this.spectrogramAfterFilteringPanel);
            this.Controls.Add(this.signalAfterFilteringPanel);
            this.Controls.Add(this.filterParamsDataGrid);
            this.Controls.Add(this.filterTypesComboBox);
            this.Controls.Add(this.spectrogramBeforeFilteringPanel);
            this.Controls.Add(this.signalBeforeFilteringPanel);
            this.Controls.Add(this.poleZeroPanel);
            this.Controls.Add(this.phaseResponsePanel);
            this.Controls.Add(this.magnitudeResponsePanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FiltersForm";
            this.Text = "FiltersForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterParamsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel magnitudeResponsePanel;
        private System.Windows.Forms.Panel phaseResponsePanel;
        private System.Windows.Forms.Panel poleZeroPanel;
        private System.Windows.Forms.Panel signalBeforeFilteringPanel;
        private System.Windows.Forms.Panel spectrogramBeforeFilteringPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button analyzeFilterButton;
        private System.Windows.Forms.ComboBox filterTypesComboBox;
        private System.Windows.Forms.DataGridView filterParamsDataGrid;
        private System.Windows.Forms.Panel signalAfterFilteringPanel;
        private System.Windows.Forms.Panel spectrogramAfterFilteringPanel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Param;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.ListBox numeratorListBox;
        private System.Windows.Forms.ListBox denominatorListBox;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overlapAddToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overlapSaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem differenceEquationToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button playSignalButton;
        private System.Windows.Forms.Button playFilteredSignalButton;
        private System.Windows.Forms.TextBox orderNumeratorTextBox;
        private System.Windows.Forms.TextBox orderDenominatorTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button changeOrderButton;
    }
}