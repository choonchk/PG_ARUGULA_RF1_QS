namespace TestPlanDriver
{
    partial class FormDoNOTTouch
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
            this.buttonStartTestPlan = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxDoLotArgString = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxTestArgString = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUnInitArgString = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxInitArgString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxCalFileInterpolate = new System.Windows.Forms.CheckBox();
            this.numericUpDownLoopDelay = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownLoopCnt = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listBoxRunResult = new System.Windows.Forms.ListBox();
            this.labelResultFilePath = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxBuddyFile = new System.Windows.Forms.CheckBox();
            this.buttonInit = new System.Windows.Forms.Button();
            this.buttonUnInit = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxTraceFile = new System.Windows.Forms.CheckBox();
            this.checkBoxAdaptiveSamplingOnOff = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonDoLot = new System.Windows.Forms.Button();
            this.comboBoxPackages = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.numericUpDownMaxSitesNum = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxCalHandlerSelector = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLoopDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLoopCnt)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSitesNum)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStartTestPlan
            // 
            this.buttonStartTestPlan.Enabled = false;
            this.buttonStartTestPlan.Location = new System.Drawing.Point(170, 592);
            this.buttonStartTestPlan.Name = "buttonStartTestPlan";
            this.buttonStartTestPlan.Size = new System.Drawing.Size(128, 23);
            this.buttonStartTestPlan.TabIndex = 0;
            this.buttonStartTestPlan.Text = "DoTest";
            this.buttonStartTestPlan.UseVisualStyleBackColor = true;
            this.buttonStartTestPlan.Click += new System.EventHandler(this.buttonStartTestPlan_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxDoLotArgString);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.textBoxTestArgString);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxUnInitArgString);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxInitArgString);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(9, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(405, 84);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Arguments String to Methods";
            // 
            // textBoxDoLotArgString
            // 
            this.textBoxDoLotArgString.Enabled = false;
            this.textBoxDoLotArgString.Location = new System.Drawing.Point(178, 23);
            this.textBoxDoLotArgString.Name = "textBoxDoLotArgString";
            this.textBoxDoLotArgString.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxDoLotArgString.Size = new System.Drawing.Size(86, 20);
            this.textBoxDoLotArgString.TabIndex = 10;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(127, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "CloseLot:";
            // 
            // textBoxTestArgString
            // 
            this.textBoxTestArgString.Enabled = false;
            this.textBoxTestArgString.Location = new System.Drawing.Point(47, 53);
            this.textBoxTestArgString.Name = "textBoxTestArgString";
            this.textBoxTestArgString.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxTestArgString.Size = new System.Drawing.Size(349, 20);
            this.textBoxTestArgString.TabIndex = 6;
            this.textBoxTestArgString.Text = "SimHW=1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "TEST:";
            // 
            // textBoxUnInitArgString
            // 
            this.textBoxUnInitArgString.Enabled = false;
            this.textBoxUnInitArgString.Location = new System.Drawing.Point(332, 23);
            this.textBoxUnInitArgString.Name = "textBoxUnInitArgString";
            this.textBoxUnInitArgString.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxUnInitArgString.Size = new System.Drawing.Size(64, 20);
            this.textBoxUnInitArgString.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Enabled = false;
            this.label2.Location = new System.Drawing.Point(275, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "UNINIT:";
            // 
            // textBoxInitArgString
            // 
            this.textBoxInitArgString.Enabled = false;
            this.textBoxInitArgString.Location = new System.Drawing.Point(47, 24);
            this.textBoxInitArgString.Name = "textBoxInitArgString";
            this.textBoxInitArgString.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxInitArgString.Size = new System.Drawing.Size(64, 20);
            this.textBoxInitArgString.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(8, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "INIT:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 14);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Select Package:";
            // 
            // checkBoxCalFileInterpolate
            // 
            this.checkBoxCalFileInterpolate.AutoSize = true;
            this.checkBoxCalFileInterpolate.Enabled = false;
            this.checkBoxCalFileInterpolate.Location = new System.Drawing.Point(6, 18);
            this.checkBoxCalFileInterpolate.Name = "checkBoxCalFileInterpolate";
            this.checkBoxCalFileInterpolate.Size = new System.Drawing.Size(135, 17);
            this.checkBoxCalFileInterpolate.TabIndex = 11;
            this.checkBoxCalFileInterpolate.Text = "CalFile Auto-Interpolate";
            this.checkBoxCalFileInterpolate.UseVisualStyleBackColor = true;
            this.checkBoxCalFileInterpolate.CheckedChanged += new System.EventHandler(this.checkBoxCalFileInterpolate_CheckedChanged);
            // 
            // numericUpDownLoopDelay
            // 
            this.numericUpDownLoopDelay.Enabled = false;
            this.numericUpDownLoopDelay.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownLoopDelay.Location = new System.Drawing.Point(125, 51);
            this.numericUpDownLoopDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownLoopDelay.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownLoopDelay.Name = "numericUpDownLoopDelay";
            this.numericUpDownLoopDelay.Size = new System.Drawing.Size(61, 20);
            this.numericUpDownLoopDelay.TabIndex = 10;
            this.numericUpDownLoopDelay.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Enabled = false;
            this.label6.Location = new System.Drawing.Point(7, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Inter-Loop Delay (ms):";
            // 
            // numericUpDownLoopCnt
            // 
            this.numericUpDownLoopCnt.Enabled = false;
            this.numericUpDownLoopCnt.Location = new System.Drawing.Point(59, 25);
            this.numericUpDownLoopCnt.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownLoopCnt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLoopCnt.Name = "numericUpDownLoopCnt";
            this.numericUpDownLoopCnt.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownLoopCnt.TabIndex = 8;
            this.numericUpDownLoopCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.Location = new System.Drawing.Point(9, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Loop #:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listBoxRunResult);
            this.groupBox2.Controls.Add(this.labelResultFilePath);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(3, 175);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(775, 411);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Result";
            // 
            // listBoxRunResult
            // 
            this.listBoxRunResult.FormattingEnabled = true;
            this.listBoxRunResult.Location = new System.Drawing.Point(8, 47);
            this.listBoxRunResult.Name = "listBoxRunResult";
            this.listBoxRunResult.Size = new System.Drawing.Size(759, 355);
            this.listBoxRunResult.TabIndex = 12;
            // 
            // labelResultFilePath
            // 
            this.labelResultFilePath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelResultFilePath.Location = new System.Drawing.Point(126, 17);
            this.labelResultFilePath.Name = "labelResultFilePath";
            this.labelResultFilePath.Size = new System.Drawing.Size(483, 20);
            this.labelResultFilePath.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Enabled = false;
            this.label5.Location = new System.Drawing.Point(7, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Raw Result File (.csv):";
            // 
            // checkBoxBuddyFile
            // 
            this.checkBoxBuddyFile.AutoSize = true;
            this.checkBoxBuddyFile.Location = new System.Drawing.Point(6, 71);
            this.checkBoxBuddyFile.Name = "checkBoxBuddyFile";
            this.checkBoxBuddyFile.Size = new System.Drawing.Size(94, 17);
            this.checkBoxBuddyFile.TabIndex = 13;
            this.checkBoxBuddyFile.Text = "Buddy File ON";
            this.checkBoxBuddyFile.UseVisualStyleBackColor = true;
            this.checkBoxBuddyFile.CheckedChanged += new System.EventHandler(this.checkBox2ndResultFile_CheckedChanged);
            // 
            // buttonInit
            // 
            this.buttonInit.Location = new System.Drawing.Point(26, 592);
            this.buttonInit.Name = "buttonInit";
            this.buttonInit.Size = new System.Drawing.Size(128, 23);
            this.buttonInit.TabIndex = 7;
            this.buttonInit.Text = "Init";
            this.buttonInit.UseVisualStyleBackColor = true;
            this.buttonInit.Click += new System.EventHandler(this.buttonInit_Click);
            // 
            // buttonUnInit
            // 
            this.buttonUnInit.Enabled = false;
            this.buttonUnInit.Location = new System.Drawing.Point(494, 591);
            this.buttonUnInit.Name = "buttonUnInit";
            this.buttonUnInit.Size = new System.Drawing.Size(128, 23);
            this.buttonUnInit.TabIndex = 8;
            this.buttonUnInit.Text = "Un-Init";
            this.buttonUnInit.UseVisualStyleBackColor = true;
            this.buttonUnInit.Click += new System.EventHandler(this.buttonUnInit_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxTraceFile);
            this.groupBox3.Controls.Add(this.checkBoxBuddyFile);
            this.groupBox3.Controls.Add(this.checkBoxAdaptiveSamplingOnOff);
            this.groupBox3.Controls.Add(this.checkBoxCalFileInterpolate);
            this.groupBox3.Location = new System.Drawing.Point(617, 47);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(161, 122);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Misc Options";
            // 
            // checkBoxTraceFile
            // 
            this.checkBoxTraceFile.AutoSize = true;
            this.checkBoxTraceFile.Location = new System.Drawing.Point(6, 95);
            this.checkBoxTraceFile.Name = "checkBoxTraceFile";
            this.checkBoxTraceFile.Size = new System.Drawing.Size(134, 17);
            this.checkBoxTraceFile.TabIndex = 14;
            this.checkBoxTraceFile.Text = "Trace File ON (Always)";
            this.checkBoxTraceFile.UseVisualStyleBackColor = true;
            // 
            // checkBoxAdaptiveSamplingOnOff
            // 
            this.checkBoxAdaptiveSamplingOnOff.AutoSize = true;
            this.checkBoxAdaptiveSamplingOnOff.Location = new System.Drawing.Point(6, 44);
            this.checkBoxAdaptiveSamplingOnOff.Name = "checkBoxAdaptiveSamplingOnOff";
            this.checkBoxAdaptiveSamplingOnOff.Size = new System.Drawing.Size(150, 17);
            this.checkBoxAdaptiveSamplingOnOff.TabIndex = 0;
            this.checkBoxAdaptiveSamplingOnOff.Text = "Enable Adaptive Sampling";
            this.checkBoxAdaptiveSamplingOnOff.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.numericUpDownLoopDelay);
            this.groupBox4.Controls.Add(this.numericUpDownLoopCnt);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(420, 38);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(192, 84);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Looping Options";
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(641, 592);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(128, 23);
            this.buttonExit.TabIndex = 11;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonDoLot
            // 
            this.buttonDoLot.Enabled = false;
            this.buttonDoLot.Location = new System.Drawing.Point(330, 592);
            this.buttonDoLot.Name = "buttonDoLot";
            this.buttonDoLot.Size = new System.Drawing.Size(128, 23);
            this.buttonDoLot.TabIndex = 12;
            this.buttonDoLot.Text = "Do Lot";
            this.buttonDoLot.UseVisualStyleBackColor = true;
            this.buttonDoLot.Click += new System.EventHandler(this.buttonDoLot_Click);
            // 
            // comboBoxPackages
            // 
            this.comboBoxPackages.FormattingEnabled = true;
            this.comboBoxPackages.Location = new System.Drawing.Point(102, 10);
            this.comboBoxPackages.Name = "comboBoxPackages";
            this.comboBoxPackages.Size = new System.Drawing.Size(229, 21);
            this.comboBoxPackages.TabIndex = 11;
            this.comboBoxPackages.SelectedIndexChanged += new System.EventHandler(this.comboBoxPackages_SelectedIndexChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.numericUpDownMaxSitesNum);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Location = new System.Drawing.Point(3, 123);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(411, 46);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Split Test Plan Input";
            // 
            // numericUpDownMaxSitesNum
            // 
            this.numericUpDownMaxSitesNum.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownMaxSitesNum.Location = new System.Drawing.Point(111, 17);
            this.numericUpDownMaxSitesNum.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numericUpDownMaxSitesNum.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownMaxSitesNum.Name = "numericUpDownMaxSitesNum";
            this.numericUpDownMaxSitesNum.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownMaxSitesNum.TabIndex = 11;
            this.numericUpDownMaxSitesNum.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Enabled = false;
            this.label12.Location = new System.Drawing.Point(21, 20);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(72, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "MaxSitesNum";
            // 
            // comboBoxCalHandlerSelector
            // 
            this.comboBoxCalHandlerSelector.FormattingEnabled = true;
            this.comboBoxCalHandlerSelector.Location = new System.Drawing.Point(512, 9);
            this.comboBoxCalHandlerSelector.Name = "comboBoxCalHandlerSelector";
            this.comboBoxCalHandlerSelector.Size = new System.Drawing.Size(258, 21);
            this.comboBoxCalHandlerSelector.TabIndex = 15;
            this.comboBoxCalHandlerSelector.SelectedIndexChanged += new System.EventHandler(this.comboBoxCalHandlerSelector_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(401, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(103, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Select CAL Handler:";
            // 
            // FormDoNOTTouch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 624);
            this.Controls.Add(this.comboBoxCalHandlerSelector);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.comboBoxPackages);
            this.Controls.Add(this.buttonDoLot);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonUnInit);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.buttonInit);
            this.Controls.Add(this.buttonStartTestPlan);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormDoNOTTouch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clotho ATF Test Plan Lite Driver";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDoNOTTouch_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLoopDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLoopCnt)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSitesNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartTestPlan;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxUnInitArgString;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxInitArgString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTestArgString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownLoopCnt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelResultFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownLoopDelay;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonInit;
        private System.Windows.Forms.Button buttonUnInit;
        private System.Windows.Forms.CheckBox checkBoxCalFileInterpolate;
        private System.Windows.Forms.CheckBox checkBoxAdaptiveSamplingOnOff;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListBox listBoxRunResult;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonDoLot;
        private System.Windows.Forms.TextBox textBoxDoLotArgString;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxPackages;
        private System.Windows.Forms.CheckBox checkBoxBuddyFile;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxSitesNum;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkBoxTraceFile;
        private System.Windows.Forms.ComboBox comboBoxCalHandlerSelector;
        private System.Windows.Forms.Label label9;
    }
}

