using NWaves.DemoForms.UserControls;

namespace NWaves.DemoForms
{
    partial class EffectsForm
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
            this.playFilteredSignalButton = new System.Windows.Forms.Button();
            this.playSignalButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.spectrogramAfterFilteringPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.spectrogramBeforeFilteringPanel = new NWaves.DemoForms.UserControls.SpectrogramPlot();
            this.stopButton = new System.Windows.Forms.Button();
            this.stopFilteredButton = new System.Windows.Forms.Button();
            this.applyEffectButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.widthTextBox = new System.Windows.Forms.TextBox();
            this.flangerRadioButton = new System.Windows.Forms.RadioButton();
            this.label18 = new System.Windows.Forms.Label();
            this.dryTextBox = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.wetTextBox = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.hopSizeTextBox = new System.Windows.Forms.TextBox();
            this.winSizeTextBox = new System.Windows.Forms.TextBox();
            this.tsmComboBox = new System.Windows.Forms.ComboBox();
            this.pitchShiftCheckBox = new System.Windows.Forms.CheckBox();
            this.delayRadioButton = new System.Windows.Forms.RadioButton();
            this.label15 = new System.Windows.Forms.Label();
            this.pitchShiftTextBox = new System.Windows.Forms.TextBox();
            this.pitchShiftRadioButton = new System.Windows.Forms.RadioButton();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lfoQTextBox = new System.Windows.Forms.TextBox();
            this.lfoFreqTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.maxFreqTextBox = new System.Windows.Forms.TextBox();
            this.minFreqTextBox = new System.Windows.Forms.TextBox();
            this.phaserRadioButton = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tremoloIndexTextBox = new System.Windows.Forms.TextBox();
            this.tremoloFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.echoDecayTextBox = new System.Windows.Forms.TextBox();
            this.echoDelayTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.distTextBox = new System.Windows.Forms.TextBox();
            this.qTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.distortionGainTextBox = new System.Windows.Forms.TextBox();
            this.wahwahRadioButton = new System.Windows.Forms.RadioButton();
            this.echoRadioButton = new System.Windows.Forms.RadioButton();
            this.tubeDistortionRadioButton = new System.Windows.Forms.RadioButton();
            this.distortionRadioButton = new System.Windows.Forms.RadioButton();
            this.overdriveRadioButton = new System.Windows.Forms.RadioButton();
            this.tremoloRadioButton = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signalAfterFilteringPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.signalBeforeFilteringPanel = new NWaves.DemoForms.UserControls.SignalPlot();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // playFilteredSignalButton
            // 
            this.playFilteredSignalButton.Location = new System.Drawing.Point(831, 291);
            this.playFilteredSignalButton.Name = "playFilteredSignalButton";
            this.playFilteredSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playFilteredSignalButton.TabIndex = 27;
            this.playFilteredSignalButton.Text = "Play";
            this.playFilteredSignalButton.UseVisualStyleBackColor = true;
            this.playFilteredSignalButton.Click += new System.EventHandler(this.playFilteredSignalButton_Click);
            // 
            // playSignalButton
            // 
            this.playSignalButton.Location = new System.Drawing.Point(347, 291);
            this.playSignalButton.Name = "playSignalButton";
            this.playSignalButton.Size = new System.Drawing.Size(59, 26);
            this.playSignalButton.TabIndex = 26;
            this.playSignalButton.Text = "Play";
            this.playSignalButton.UseVisualStyleBackColor = true;
            this.playSignalButton.Click += new System.EventHandler(this.playSignalButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(477, 296);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 17);
            this.label6.TabIndex = 25;
            this.label6.Text = "After effect";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 296);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 17);
            this.label5.TabIndex = 24;
            this.label5.Text = "Before effect";
            // 
            // spectrogramAfterFilteringPanel
            // 
            this.spectrogramAfterFilteringPanel.AutoScroll = true;
            this.spectrogramAfterFilteringPanel.BackColor = System.Drawing.Color.White;
            this.spectrogramAfterFilteringPanel.ColorMapName = "magma";
            this.spectrogramAfterFilteringPanel.Location = new System.Drawing.Point(480, 476);
            this.spectrogramAfterFilteringPanel.Markline = null;
            this.spectrogramAfterFilteringPanel.MarklineThickness = 0;
            this.spectrogramAfterFilteringPanel.Name = "spectrogramAfterFilteringPanel";
            this.spectrogramAfterFilteringPanel.Size = new System.Drawing.Size(475, 152);
            this.spectrogramAfterFilteringPanel.Spectrogram = null;
            this.spectrogramAfterFilteringPanel.TabIndex = 23;
            // 
            // spectrogramBeforeFilteringPanel
            // 
            this.spectrogramBeforeFilteringPanel.AutoScroll = true;
            this.spectrogramBeforeFilteringPanel.BackColor = System.Drawing.Color.White;
            this.spectrogramBeforeFilteringPanel.ColorMapName = "magma";
            this.spectrogramBeforeFilteringPanel.Location = new System.Drawing.Point(13, 475);
            this.spectrogramBeforeFilteringPanel.Markline = null;
            this.spectrogramBeforeFilteringPanel.MarklineThickness = 0;
            this.spectrogramBeforeFilteringPanel.Name = "spectrogramBeforeFilteringPanel";
            this.spectrogramBeforeFilteringPanel.Size = new System.Drawing.Size(461, 153);
            this.spectrogramBeforeFilteringPanel.Spectrogram = null;
            this.spectrogramBeforeFilteringPanel.TabIndex = 22;
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(412, 291);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(59, 26);
            this.stopButton.TabIndex = 28;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // stopFilteredButton
            // 
            this.stopFilteredButton.Location = new System.Drawing.Point(896, 291);
            this.stopFilteredButton.Name = "stopFilteredButton";
            this.stopFilteredButton.Size = new System.Drawing.Size(59, 26);
            this.stopFilteredButton.TabIndex = 29;
            this.stopFilteredButton.Text = "Stop";
            this.stopFilteredButton.UseVisualStyleBackColor = true;
            this.stopFilteredButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // applyEffectButton
            // 
            this.applyEffectButton.Location = new System.Drawing.Point(15, 247);
            this.applyEffectButton.Name = "applyEffectButton";
            this.applyEffectButton.Size = new System.Drawing.Size(940, 38);
            this.applyEffectButton.TabIndex = 30;
            this.applyEffectButton.Text = "Apply!";
            this.applyEffectButton.UseVisualStyleBackColor = true;
            this.applyEffectButton.Click += new System.EventHandler(this.applyEffectButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.widthTextBox);
            this.groupBox1.Controls.Add(this.flangerRadioButton);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.dryTextBox);
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.wetTextBox);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.hopSizeTextBox);
            this.groupBox1.Controls.Add(this.winSizeTextBox);
            this.groupBox1.Controls.Add(this.tsmComboBox);
            this.groupBox1.Controls.Add(this.pitchShiftCheckBox);
            this.groupBox1.Controls.Add(this.delayRadioButton);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.pitchShiftTextBox);
            this.groupBox1.Controls.Add(this.pitchShiftRadioButton);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.lfoQTextBox);
            this.groupBox1.Controls.Add(this.lfoFreqTextBox);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.maxFreqTextBox);
            this.groupBox1.Controls.Add(this.minFreqTextBox);
            this.groupBox1.Controls.Add(this.phaserRadioButton);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.tremoloIndexTextBox);
            this.groupBox1.Controls.Add(this.tremoloFrequencyTextBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.echoDecayTextBox);
            this.groupBox1.Controls.Add(this.echoDelayTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.distTextBox);
            this.groupBox1.Controls.Add(this.qTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.distortionGainTextBox);
            this.groupBox1.Controls.Add(this.wahwahRadioButton);
            this.groupBox1.Controls.Add(this.echoRadioButton);
            this.groupBox1.Controls.Add(this.tubeDistortionRadioButton);
            this.groupBox1.Controls.Add(this.distortionRadioButton);
            this.groupBox1.Controls.Add(this.overdriveRadioButton);
            this.groupBox1.Controls.Add(this.tremoloRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(13, 31);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(942, 210);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sound Effects";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(597, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 47;
            this.label2.Text = "Width";
            // 
            // widthTextBox
            // 
            this.widthTextBox.Location = new System.Drawing.Point(647, 93);
            this.widthTextBox.Name = "widthTextBox";
            this.widthTextBox.Size = new System.Drawing.Size(41, 22);
            this.widthTextBox.TabIndex = 46;
            this.widthTextBox.Text = "0,003";
            // 
            // flangerRadioButton
            // 
            this.flangerRadioButton.AutoSize = true;
            this.flangerRadioButton.Location = new System.Drawing.Point(387, 90);
            this.flangerRadioButton.Name = "flangerRadioButton";
            this.flangerRadioButton.Size = new System.Drawing.Size(77, 21);
            this.flangerRadioButton.TabIndex = 45;
            this.flangerRadioButton.Text = "Flanger";
            this.flangerRadioButton.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label18.Location = new System.Drawing.Point(835, 56);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(42, 25);
            this.label18.TabIndex = 44;
            this.label18.Text = "Dry";
            // 
            // dryTextBox
            // 
            this.dryTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dryTextBox.Location = new System.Drawing.Point(883, 56);
            this.dryTextBox.Name = "dryTextBox";
            this.dryTextBox.Size = new System.Drawing.Size(49, 30);
            this.dryTextBox.TabIndex = 43;
            this.dryTextBox.Text = "0,2";
            this.dryTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label19.Location = new System.Drawing.Point(827, 19);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(48, 25);
            this.label19.TabIndex = 42;
            this.label19.Text = "Wet";
            // 
            // wetTextBox
            // 
            this.wetTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.wetTextBox.Location = new System.Drawing.Point(883, 19);
            this.wetTextBox.Name = "wetTextBox";
            this.wetTextBox.Size = new System.Drawing.Size(49, 30);
            this.wetTextBox.TabIndex = 41;
            this.wetTextBox.Text = "0,8";
            this.wetTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(851, 174);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(34, 17);
            this.label17.TabIndex = 40;
            this.label17.Text = "Hop";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(766, 174);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(32, 17);
            this.label16.TabIndex = 39;
            this.label16.Text = "Win";
            // 
            // hopSizeTextBox
            // 
            this.hopSizeTextBox.Location = new System.Drawing.Point(891, 172);
            this.hopSizeTextBox.Name = "hopSizeTextBox";
            this.hopSizeTextBox.Size = new System.Drawing.Size(41, 22);
            this.hopSizeTextBox.TabIndex = 38;
            this.hopSizeTextBox.Text = "128";
            // 
            // winSizeTextBox
            // 
            this.winSizeTextBox.Location = new System.Drawing.Point(804, 172);
            this.winSizeTextBox.Name = "winSizeTextBox";
            this.winSizeTextBox.Size = new System.Drawing.Size(41, 22);
            this.winSizeTextBox.TabIndex = 37;
            this.winSizeTextBox.Text = "512";
            // 
            // tsmComboBox
            // 
            this.tsmComboBox.FormattingEnabled = true;
            this.tsmComboBox.Items.AddRange(new object[] {
            "Phase Vocoder",
            "Phase Vocoder (Phase Locking)",
            "WSOLA",
            "Paul stretch"});
            this.tsmComboBox.Location = new System.Drawing.Point(687, 142);
            this.tsmComboBox.Name = "tsmComboBox";
            this.tsmComboBox.Size = new System.Drawing.Size(245, 24);
            this.tsmComboBox.TabIndex = 36;
            this.tsmComboBox.Text = "Phase Vocoder";
            // 
            // pitchShiftCheckBox
            // 
            this.pitchShiftCheckBox.AutoSize = true;
            this.pitchShiftCheckBox.Location = new System.Drawing.Point(583, 159);
            this.pitchShiftCheckBox.Name = "pitchShiftCheckBox";
            this.pitchShiftCheckBox.Size = new System.Drawing.Size(91, 21);
            this.pitchShiftCheckBox.TabIndex = 35;
            this.pitchShiftCheckBox.Text = "Pitch shift";
            this.pitchShiftCheckBox.UseVisualStyleBackColor = true;
            // 
            // delayRadioButton
            // 
            this.delayRadioButton.AutoSize = true;
            this.delayRadioButton.Location = new System.Drawing.Point(7, 172);
            this.delayRadioButton.Name = "delayRadioButton";
            this.delayRadioButton.Size = new System.Drawing.Size(65, 21);
            this.delayRadioButton.TabIndex = 34;
            this.delayRadioButton.Text = "Delay";
            this.delayRadioButton.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(493, 163);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(23, 17);
            this.label15.TabIndex = 33;
            this.label15.Text = "by";
            // 
            // pitchShiftTextBox
            // 
            this.pitchShiftTextBox.Location = new System.Drawing.Point(530, 158);
            this.pitchShiftTextBox.Name = "pitchShiftTextBox";
            this.pitchShiftTextBox.Size = new System.Drawing.Size(41, 22);
            this.pitchShiftTextBox.TabIndex = 32;
            this.pitchShiftTextBox.Text = "1,2";
            // 
            // pitchShiftRadioButton
            // 
            this.pitchShiftRadioButton.AutoSize = true;
            this.pitchShiftRadioButton.Location = new System.Drawing.Point(387, 161);
            this.pitchShiftRadioButton.Name = "pitchShiftRadioButton";
            this.pitchShiftRadioButton.Size = new System.Drawing.Size(107, 21);
            this.pitchShiftRadioButton.TabIndex = 31;
            this.pitchShiftRadioButton.Text = "Time stretch";
            this.pitchShiftRadioButton.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(598, 68);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(19, 17);
            this.label13.TabIndex = 30;
            this.label13.Text = "Q";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(486, 68);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(37, 17);
            this.label14.TabIndex = 29;
            this.label14.Text = "Freq";
            // 
            // lfoQTextBox
            // 
            this.lfoQTextBox.Location = new System.Drawing.Point(647, 65);
            this.lfoQTextBox.Name = "lfoQTextBox";
            this.lfoQTextBox.Size = new System.Drawing.Size(41, 22);
            this.lfoQTextBox.TabIndex = 28;
            this.lfoQTextBox.Text = "0,5";
            // 
            // lfoFreqTextBox
            // 
            this.lfoFreqTextBox.Location = new System.Drawing.Point(530, 65);
            this.lfoFreqTextBox.Name = "lfoFreqTextBox";
            this.lfoFreqTextBox.Size = new System.Drawing.Size(54, 22);
            this.lfoFreqTextBox.TabIndex = 27;
            this.lfoFreqTextBox.Text = "1,2";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(598, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(33, 17);
            this.label11.TabIndex = 26;
            this.label11.Text = "Max";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(486, 40);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(30, 17);
            this.label12.TabIndex = 25;
            this.label12.Text = "Min";
            // 
            // maxFreqTextBox
            // 
            this.maxFreqTextBox.Location = new System.Drawing.Point(647, 37);
            this.maxFreqTextBox.Name = "maxFreqTextBox";
            this.maxFreqTextBox.Size = new System.Drawing.Size(41, 22);
            this.maxFreqTextBox.TabIndex = 24;
            this.maxFreqTextBox.Text = "2000";
            // 
            // minFreqTextBox
            // 
            this.minFreqTextBox.Location = new System.Drawing.Point(530, 37);
            this.minFreqTextBox.Name = "minFreqTextBox";
            this.minFreqTextBox.Size = new System.Drawing.Size(54, 22);
            this.minFreqTextBox.TabIndex = 23;
            this.minFreqTextBox.Text = "300";
            // 
            // phaserRadioButton
            // 
            this.phaserRadioButton.AutoSize = true;
            this.phaserRadioButton.Location = new System.Drawing.Point(387, 38);
            this.phaserRadioButton.Name = "phaserRadioButton";
            this.phaserRadioButton.Size = new System.Drawing.Size(74, 21);
            this.phaserRadioButton.TabIndex = 22;
            this.phaserRadioButton.Text = "Phaser";
            this.phaserRadioButton.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(257, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 17);
            this.label9.TabIndex = 21;
            this.label9.Text = "Index";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(145, 40);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 17);
            this.label10.TabIndex = 20;
            this.label10.Text = "Freq";
            // 
            // tremoloIndexTextBox
            // 
            this.tremoloIndexTextBox.Location = new System.Drawing.Point(306, 37);
            this.tremoloIndexTextBox.Name = "tremoloIndexTextBox";
            this.tremoloIndexTextBox.Size = new System.Drawing.Size(41, 22);
            this.tremoloIndexTextBox.TabIndex = 19;
            this.tremoloIndexTextBox.Text = "0,5";
            // 
            // tremoloFrequencyTextBox
            // 
            this.tremoloFrequencyTextBox.Location = new System.Drawing.Point(189, 37);
            this.tremoloFrequencyTextBox.Name = "tremoloFrequencyTextBox";
            this.tremoloFrequencyTextBox.Size = new System.Drawing.Size(54, 22);
            this.tremoloFrequencyTextBox.TabIndex = 18;
            this.tremoloFrequencyTextBox.Text = "10";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(256, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 17);
            this.label7.TabIndex = 17;
            this.label7.Text = "Decay";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(145, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 16;
            this.label8.Text = "Delay";
            // 
            // echoDecayTextBox
            // 
            this.echoDecayTextBox.Location = new System.Drawing.Point(306, 160);
            this.echoDecayTextBox.Name = "echoDecayTextBox";
            this.echoDecayTextBox.Size = new System.Drawing.Size(41, 22);
            this.echoDecayTextBox.TabIndex = 15;
            this.echoDecayTextBox.Text = "0,5";
            // 
            // echoDelayTextBox
            // 
            this.echoDelayTextBox.Location = new System.Drawing.Point(189, 160);
            this.echoDelayTextBox.Name = "echoDelayTextBox";
            this.echoDelayTextBox.Size = new System.Drawing.Size(54, 22);
            this.echoDelayTextBox.TabIndex = 14;
            this.echoDelayTextBox.Text = "0,18";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Dist";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(145, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Q";
            // 
            // distTextBox
            // 
            this.distTextBox.Location = new System.Drawing.Point(306, 118);
            this.distTextBox.Name = "distTextBox";
            this.distTextBox.Size = new System.Drawing.Size(41, 22);
            this.distTextBox.TabIndex = 11;
            this.distTextBox.Text = "5";
            // 
            // qTextBox
            // 
            this.qTextBox.Location = new System.Drawing.Point(189, 118);
            this.qTextBox.Name = "qTextBox";
            this.qTextBox.Size = new System.Drawing.Size(54, 22);
            this.qTextBox.TabIndex = 10;
            this.qTextBox.Text = "-0,2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Gain";
            // 
            // distortionGainTextBox
            // 
            this.distortionGainTextBox.Location = new System.Drawing.Point(189, 91);
            this.distortionGainTextBox.Name = "distortionGainTextBox";
            this.distortionGainTextBox.Size = new System.Drawing.Size(54, 22);
            this.distortionGainTextBox.TabIndex = 6;
            this.distortionGainTextBox.Text = "20";
            // 
            // wahwahRadioButton
            // 
            this.wahwahRadioButton.AutoSize = true;
            this.wahwahRadioButton.Location = new System.Drawing.Point(387, 64);
            this.wahwahRadioButton.Name = "wahwahRadioButton";
            this.wahwahRadioButton.Size = new System.Drawing.Size(87, 21);
            this.wahwahRadioButton.TabIndex = 5;
            this.wahwahRadioButton.Text = "WahWah";
            this.wahwahRadioButton.UseVisualStyleBackColor = true;
            // 
            // echoRadioButton
            // 
            this.echoRadioButton.AutoSize = true;
            this.echoRadioButton.Location = new System.Drawing.Point(7, 145);
            this.echoRadioButton.Name = "echoRadioButton";
            this.echoRadioButton.Size = new System.Drawing.Size(61, 21);
            this.echoRadioButton.TabIndex = 4;
            this.echoRadioButton.Text = "Echo";
            this.echoRadioButton.UseVisualStyleBackColor = true;
            // 
            // tubeDistortionRadioButton
            // 
            this.tubeDistortionRadioButton.AutoSize = true;
            this.tubeDistortionRadioButton.Location = new System.Drawing.Point(7, 118);
            this.tubeDistortionRadioButton.Name = "tubeDistortionRadioButton";
            this.tubeDistortionRadioButton.Size = new System.Drawing.Size(126, 21);
            this.tubeDistortionRadioButton.TabIndex = 3;
            this.tubeDistortionRadioButton.Text = "Tube Distortion";
            this.tubeDistortionRadioButton.UseVisualStyleBackColor = true;
            // 
            // distortionRadioButton
            // 
            this.distortionRadioButton.AutoSize = true;
            this.distortionRadioButton.Location = new System.Drawing.Point(7, 91);
            this.distortionRadioButton.Name = "distortionRadioButton";
            this.distortionRadioButton.Size = new System.Drawing.Size(89, 21);
            this.distortionRadioButton.TabIndex = 2;
            this.distortionRadioButton.Text = "Distortion";
            this.distortionRadioButton.UseVisualStyleBackColor = true;
            // 
            // overdriveRadioButton
            // 
            this.overdriveRadioButton.AutoSize = true;
            this.overdriveRadioButton.Location = new System.Drawing.Point(7, 64);
            this.overdriveRadioButton.Name = "overdriveRadioButton";
            this.overdriveRadioButton.Size = new System.Drawing.Size(91, 21);
            this.overdriveRadioButton.TabIndex = 1;
            this.overdriveRadioButton.Text = "Overdrive";
            this.overdriveRadioButton.UseVisualStyleBackColor = true;
            // 
            // tremoloRadioButton
            // 
            this.tremoloRadioButton.AutoSize = true;
            this.tremoloRadioButton.Checked = true;
            this.tremoloRadioButton.Location = new System.Drawing.Point(7, 37);
            this.tremoloRadioButton.Name = "tremoloRadioButton";
            this.tremoloRadioButton.Size = new System.Drawing.Size(81, 21);
            this.tremoloRadioButton.TabIndex = 0;
            this.tremoloRadioButton.TabStop = true;
            this.tremoloRadioButton.Text = "Tremolo";
            this.tremoloRadioButton.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(970, 28);
            this.menuStrip1.TabIndex = 32;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.saveAsToolStripMenuItem.Text = "&Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // signalAfterFilteringPanel
            // 
            this.signalAfterFilteringPanel.AutoScroll = true;
            this.signalAfterFilteringPanel.BackColor = System.Drawing.Color.White;
            this.signalAfterFilteringPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalAfterFilteringPanel.Gain = 1F;
            this.signalAfterFilteringPanel.Location = new System.Drawing.Point(480, 316);
            this.signalAfterFilteringPanel.Name = "signalAfterFilteringPanel";
            this.signalAfterFilteringPanel.PaddingX = 24;
            this.signalAfterFilteringPanel.PaddingY = 5;
            this.signalAfterFilteringPanel.Signal = null;
            this.signalAfterFilteringPanel.Size = new System.Drawing.Size(475, 153);
            this.signalAfterFilteringPanel.Stride = 256;
            this.signalAfterFilteringPanel.TabIndex = 21;
            // 
            // signalBeforeFilteringPanel
            // 
            this.signalBeforeFilteringPanel.AutoScroll = true;
            this.signalBeforeFilteringPanel.BackColor = System.Drawing.Color.White;
            this.signalBeforeFilteringPanel.ForeColor = System.Drawing.Color.Blue;
            this.signalBeforeFilteringPanel.Gain = 1F;
            this.signalBeforeFilteringPanel.Location = new System.Drawing.Point(13, 316);
            this.signalBeforeFilteringPanel.Name = "signalBeforeFilteringPanel";
            this.signalBeforeFilteringPanel.PaddingX = 24;
            this.signalBeforeFilteringPanel.PaddingY = 5;
            this.signalBeforeFilteringPanel.Signal = null;
            this.signalBeforeFilteringPanel.Size = new System.Drawing.Size(461, 153);
            this.signalBeforeFilteringPanel.Stride = 256;
            this.signalBeforeFilteringPanel.TabIndex = 20;
            // 
            // EffectsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 640);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.applyEffectButton);
            this.Controls.Add(this.stopFilteredButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.playFilteredSignalButton);
            this.Controls.Add(this.playSignalButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.spectrogramAfterFilteringPanel);
            this.Controls.Add(this.signalAfterFilteringPanel);
            this.Controls.Add(this.spectrogramBeforeFilteringPanel);
            this.Controls.Add(this.signalBeforeFilteringPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EffectsForm";
            this.Text = "EffectsForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playFilteredSignalButton;
        private System.Windows.Forms.Button playSignalButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private SpectrogramPlot spectrogramAfterFilteringPanel;
        private SpectrogramPlot spectrogramBeforeFilteringPanel;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button stopFilteredButton;
        private System.Windows.Forms.Button applyEffectButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.RadioButton wahwahRadioButton;
        private System.Windows.Forms.RadioButton echoRadioButton;
        private System.Windows.Forms.RadioButton tubeDistortionRadioButton;
        private System.Windows.Forms.RadioButton distortionRadioButton;
        private System.Windows.Forms.RadioButton overdriveRadioButton;
        private System.Windows.Forms.RadioButton tremoloRadioButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox distortionGainTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox distTextBox;
        private System.Windows.Forms.TextBox qTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tremoloIndexTextBox;
        private System.Windows.Forms.TextBox tremoloFrequencyTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox echoDecayTextBox;
        private System.Windows.Forms.TextBox echoDelayTextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox lfoQTextBox;
        private System.Windows.Forms.TextBox lfoFreqTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox maxFreqTextBox;
        private System.Windows.Forms.TextBox minFreqTextBox;
        private System.Windows.Forms.RadioButton phaserRadioButton;
        private System.Windows.Forms.RadioButton delayRadioButton;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox pitchShiftTextBox;
        private System.Windows.Forms.RadioButton pitchShiftRadioButton;
        private System.Windows.Forms.CheckBox pitchShiftCheckBox;
        private SignalPlot signalAfterFilteringPanel;
        private SignalPlot signalBeforeFilteringPanel;
        private System.Windows.Forms.ComboBox tsmComboBox;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox hopSizeTextBox;
        private System.Windows.Forms.TextBox winSizeTextBox;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox dryTextBox;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox wetTextBox;
        private System.Windows.Forms.RadioButton flangerRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox widthTextBox;
    }
}