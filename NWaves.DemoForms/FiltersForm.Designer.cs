using NWaves.DemoForms.UserControls;

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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlapAddToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlapSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.differenceEquationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.framebyFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interpolateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decimateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resampleTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.analyzeFilterButton = new System.Windows.Forms.Button();
            this.filterTypesComboBox = new System.Windows.Forms.ComboBox();
            this.filterParamsDataGrid = new System.Windows.Forms.DataGridView();
            this.Param = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.phaseViewComboBox = new System.Windows.Forms.ComboBox();
            this.spectrogramAfterFilteringPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.signalAfterFilteringPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.spectrogramBeforeFilteringPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.signalBeforeFilteringPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.poleZeroPanel = new NWaves.DemoForms.UserControls.PoleZeroPlot();
            this.phaseResponsePanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.magnitudeResponsePanel = new NWaves.DemoForms.UserControls.LinePlot();
            this.label9 = new System.Windows.Forms.Label();
            this.zpIterationsTextBox = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterParamsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.filterToolStripMenuItem,
            this.resampleToolStripMenuItem,
            this.resampleTextBox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1001, 31);
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
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 27);
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
            this.autoToolStripMenuItem,
            this.overlapAddToolStripMenuItem,
            this.overlapSaveToolStripMenuItem,
            this.differenceEquationToolStripMenuItem,
            this.framebyFrameToolStripMenuItem});
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(54, 27);
            this.filterToolStripMenuItem.Text = "Fi&lter";
            // 
            // autoToolStripMenuItem
            // 
            this.autoToolStripMenuItem.Name = "autoToolStripMenuItem";
            this.autoToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.autoToolStripMenuItem.Text = "Auto";
            this.autoToolStripMenuItem.Click += new System.EventHandler(this.autoToolStripMenuItem_Click);
            // 
            // overlapAddToolStripMenuItem
            // 
            this.overlapAddToolStripMenuItem.Name = "overlapAddToolStripMenuItem";
            this.overlapAddToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
            this.overlapAddToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.overlapAddToolStripMenuItem.Text = "Overlap-&Add";
            this.overlapAddToolStripMenuItem.Click += new System.EventHandler(this.overlapAddToolStripMenuItem_Click);
            // 
            // overlapSaveToolStripMenuItem
            // 
            this.overlapSaveToolStripMenuItem.Name = "overlapSaveToolStripMenuItem";
            this.overlapSaveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.overlapSaveToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.overlapSaveToolStripMenuItem.Text = "Overlap-&Save";
            this.overlapSaveToolStripMenuItem.Click += new System.EventHandler(this.overlapSaveToolStripMenuItem_Click);
            // 
            // differenceEquationToolStripMenuItem
            // 
            this.differenceEquationToolStripMenuItem.Name = "differenceEquationToolStripMenuItem";
            this.differenceEquationToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
            this.differenceEquationToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.differenceEquationToolStripMenuItem.Text = "Difference &Equation";
            this.differenceEquationToolStripMenuItem.Click += new System.EventHandler(this.differenceEquationToolStripMenuItem_Click);
            // 
            // framebyFrameToolStripMenuItem
            // 
            this.framebyFrameToolStripMenuItem.Name = "framebyFrameToolStripMenuItem";
            this.framebyFrameToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.framebyFrameToolStripMenuItem.Text = "Frame-by-Frame";
            this.framebyFrameToolStripMenuItem.Click += new System.EventHandler(this.framebyFrameToolStripMenuItem_Click);
            // 
            // resampleToolStripMenuItem
            // 
            this.resampleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.interpolateToolStripMenuItem,
            this.decimateToolStripMenuItem,
            this.customToolStripMenuItem});
            this.resampleToolStripMenuItem.Name = "resampleToolStripMenuItem";
            this.resampleToolStripMenuItem.Size = new System.Drawing.Size(86, 27);
            this.resampleToolStripMenuItem.Text = "&Resample";
            // 
            // interpolateToolStripMenuItem
            // 
            this.interpolateToolStripMenuItem.Name = "interpolateToolStripMenuItem";
            this.interpolateToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
            this.interpolateToolStripMenuItem.Text = "&Interpolate";
            this.interpolateToolStripMenuItem.Click += new System.EventHandler(this.interpolateToolStripMenuItem_Click);
            // 
            // decimateToolStripMenuItem
            // 
            this.decimateToolStripMenuItem.Name = "decimateToolStripMenuItem";
            this.decimateToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
            this.decimateToolStripMenuItem.Text = "&Decimate";
            this.decimateToolStripMenuItem.Click += new System.EventHandler(this.decimateToolStripMenuItem_Click);
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
            this.customToolStripMenuItem.Text = "&Custom";
            this.customToolStripMenuItem.Click += new System.EventHandler(this.customToolStripMenuItem_Click);
            // 
            // resampleTextBox
            // 
            this.resampleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resampleTextBox.Name = "resampleTextBox";
            this.resampleTextBox.Size = new System.Drawing.Size(50, 27);
            this.resampleTextBox.Text = "2";
            // 
            // analyzeFilterButton
            // 
            this.analyzeFilterButton.Location = new System.Drawing.Point(13, 338);
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
            "One-pole LP",
            "One-pole HP",
            "Comb feed-forward",
            "Comb feed-back",
            "Moving average",
            "Moving average recursive",
            "Savitzky-Golay",
            "Pre-emphasis",
            "De-emphasis",
            "DC removal",
            "RASTA",
            "Butterworth",
            "Chebyshev-I",
            "Chebyshev-II",
            "Elliptic",
            "Bessel",
            "Thiran",
            "Equiripple LP",
            "Equiripple BS",
            "Custom LP/HP",
            "Custom BP/BR"});
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
            this.filterParamsDataGrid.Size = new System.Drawing.Size(189, 224);
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
            // numeratorListBox
            // 
            this.numeratorListBox.FormattingEnabled = true;
            this.numeratorListBox.ItemHeight = 16;
            this.numeratorListBox.Location = new System.Drawing.Point(797, 290);
            this.numeratorListBox.Name = "numeratorListBox";
            this.numeratorListBox.Size = new System.Drawing.Size(75, 100);
            this.numeratorListBox.TabIndex = 10;
            // 
            // denominatorListBox
            // 
            this.denominatorListBox.FormattingEnabled = true;
            this.denominatorListBox.ItemHeight = 16;
            this.denominatorListBox.Location = new System.Drawing.Point(913, 290);
            this.denominatorListBox.Name = "denominatorListBox";
            this.denominatorListBox.Size = new System.Drawing.Size(76, 100);
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
            this.label2.Location = new System.Drawing.Point(830, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Pole-Zero Plot";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(775, 290);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "b";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(891, 290);
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
            this.label6.Location = new System.Drawing.Point(511, 407);
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
            this.playFilteredSignalButton.Location = new System.Drawing.Point(605, 402);
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
            // phaseViewComboBox
            // 
            this.phaseViewComboBox.FormattingEnabled = true;
            this.phaseViewComboBox.Items.AddRange(new object[] {
            "Phase",
            "Phase unwrapped",
            "Group delay",
            "Phase delay"});
            this.phaseViewComboBox.Location = new System.Drawing.Point(219, 217);
            this.phaseViewComboBox.Name = "phaseViewComboBox";
            this.phaseViewComboBox.Size = new System.Drawing.Size(538, 24);
            this.phaseViewComboBox.TabIndex = 25;
            this.phaseViewComboBox.Text = "Phase unwrapped";
            this.phaseViewComboBox.SelectedIndexChanged += new System.EventHandler(this.phaseViewComboBox_SelectedIndexChanged);
            // 
            // spectrogramAfterFilteringPanel
            // 
            this.spectrogramAfterFilteringPanel.AutoScroll = true;
            this.spectrogramAfterFilteringPanel.BackColor = System.Drawing.Color.White;
            this.spectrogramAfterFilteringPanel.ColorMapName = "magma";
            this.spectrogramAfterFilteringPanel.Location = new System.Drawing.Point(499, 566);
            this.spectrogramAfterFilteringPanel.Markline = null;
            this.spectrogramAfterFilteringPanel.MarklineThickness = 0;
            this.spectrogramAfterFilteringPanel.Name = "spectrogramAfterFilteringPanel";
            this.spectrogramAfterFilteringPanel.Size = new System.Drawing.Size(490, 149);
            this.spectrogramAfterFilteringPanel.Spectrogram = null;
            this.spectrogramAfterFilteringPanel.TabIndex = 9;
            // 
            // signalAfterFilteringPanel
            // 
            this.signalAfterFilteringPanel.AutoScroll = true;
            this.signalAfterFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalAfterFilteringPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalAfterFilteringPanel.Gain = 1F;
            this.signalAfterFilteringPanel.Location = new System.Drawing.Point(499, 427);
            this.signalAfterFilteringPanel.Name = "signalAfterFilteringPanel";
            this.signalAfterFilteringPanel.PaddingX = 24;
            this.signalAfterFilteringPanel.PaddingY = 5;
            this.signalAfterFilteringPanel.Signal = null;
            this.signalAfterFilteringPanel.Size = new System.Drawing.Size(490, 133);
            this.signalAfterFilteringPanel.Stride = 256;
            this.signalAfterFilteringPanel.TabIndex = 4;
            // 
            // spectrogramBeforeFilteringPanel
            // 
            this.spectrogramBeforeFilteringPanel.AutoScroll = true;
            this.spectrogramBeforeFilteringPanel.BackColor = System.Drawing.Color.White;
            this.spectrogramBeforeFilteringPanel.ColorMapName = "magma";
            this.spectrogramBeforeFilteringPanel.Location = new System.Drawing.Point(13, 566);
            this.spectrogramBeforeFilteringPanel.Markline = null;
            this.spectrogramBeforeFilteringPanel.MarklineThickness = 0;
            this.spectrogramBeforeFilteringPanel.Name = "spectrogramBeforeFilteringPanel";
            this.spectrogramBeforeFilteringPanel.Size = new System.Drawing.Size(480, 148);
            this.spectrogramBeforeFilteringPanel.Spectrogram = null;
            this.spectrogramBeforeFilteringPanel.TabIndex = 4;
            // 
            // signalBeforeFilteringPanel
            // 
            this.signalBeforeFilteringPanel.AutoScroll = true;
            this.signalBeforeFilteringPanel.BackColor = System.Drawing.SystemColors.Window;
            this.signalBeforeFilteringPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalBeforeFilteringPanel.Gain = 1F;
            this.signalBeforeFilteringPanel.Location = new System.Drawing.Point(13, 427);
            this.signalBeforeFilteringPanel.Name = "signalBeforeFilteringPanel";
            this.signalBeforeFilteringPanel.PaddingX = 24;
            this.signalBeforeFilteringPanel.PaddingY = 5;
            this.signalBeforeFilteringPanel.Signal = null;
            this.signalBeforeFilteringPanel.Size = new System.Drawing.Size(480, 133);
            this.signalBeforeFilteringPanel.Stride = 256;
            this.signalBeforeFilteringPanel.TabIndex = 3;
            // 
            // poleZeroPanel
            // 
            this.poleZeroPanel.AutoScroll = true;
            this.poleZeroPanel.BackColor = System.Drawing.SystemColors.Window;
            this.poleZeroPanel.Location = new System.Drawing.Point(772, 50);
            this.poleZeroPanel.Name = "poleZeroPanel";
            this.poleZeroPanel.Poles = null;
            this.poleZeroPanel.Size = new System.Drawing.Size(217, 202);
            this.poleZeroPanel.TabIndex = 2;
            this.poleZeroPanel.Zeros = null;
            // 
            // phaseResponsePanel
            // 
            this.phaseResponsePanel.AutoScroll = true;
            this.phaseResponsePanel.BackColor = System.Drawing.SystemColors.Window;
            this.phaseResponsePanel.ForeColor = System.Drawing.Color.Blue;
            this.phaseResponsePanel.Location = new System.Drawing.Point(219, 247);
            this.phaseResponsePanel.Name = "phaseResponsePanel";
            this.phaseResponsePanel.PaddingX = 30;
            this.phaseResponsePanel.PaddingY = 20;
            this.phaseResponsePanel.Size = new System.Drawing.Size(538, 143);
            this.phaseResponsePanel.Stride = 1;
            this.phaseResponsePanel.TabIndex = 1;
            this.phaseResponsePanel.Thickness = 1;
            // 
            // magnitudeResponsePanel
            // 
            this.magnitudeResponsePanel.AutoScroll = true;
            this.magnitudeResponsePanel.BackColor = System.Drawing.SystemColors.Window;
            this.magnitudeResponsePanel.ForeColor = System.Drawing.Color.Blue;
            this.magnitudeResponsePanel.Location = new System.Drawing.Point(219, 50);
            this.magnitudeResponsePanel.Name = "magnitudeResponsePanel";
            this.magnitudeResponsePanel.PaddingX = 30;
            this.magnitudeResponsePanel.PaddingY = 20;
            this.magnitudeResponsePanel.Size = new System.Drawing.Size(538, 149);
            this.magnitudeResponsePanel.Stride = 1;
            this.magnitudeResponsePanel.TabIndex = 0;
            this.magnitudeResponsePanel.Thickness = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(773, 257);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(131, 17);
            this.label9.TabIndex = 26;
            this.label9.Text = "Zero/pole iterations";
            // 
            // zpIterationsTextBox
            // 
            this.zpIterationsTextBox.Location = new System.Drawing.Point(910, 256);
            this.zpIterationsTextBox.Name = "zpIterationsTextBox";
            this.zpIterationsTextBox.Size = new System.Drawing.Size(79, 22);
            this.zpIterationsTextBox.TabIndex = 27;
            // 
            // FiltersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 729);
            this.Controls.Add(this.zpIterationsTextBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.phaseViewComboBox);
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

        private LinePlot magnitudeResponsePanel;
        private LinePlot phaseResponsePanel;
        private PoleZeroPlot poleZeroPanel;
        private SignalPlot signalBeforeFilteringPanel;
        private SpectrogramPlot spectrogramBeforeFilteringPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button analyzeFilterButton;
        private System.Windows.Forms.ComboBox filterTypesComboBox;
        private System.Windows.Forms.DataGridView filterParamsDataGrid;
        private SignalPlot signalAfterFilteringPanel;
        private SpectrogramPlot spectrogramAfterFilteringPanel;
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
        private System.Windows.Forms.ToolStripMenuItem resampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem interpolateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decimateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox resampleTextBox;
        private System.Windows.Forms.ComboBox phaseViewComboBox;
        private System.Windows.Forms.ToolStripMenuItem framebyFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoToolStripMenuItem;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox zpIterationsTextBox;
    }
}