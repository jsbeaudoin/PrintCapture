using System.Diagnostics;
using XL_ID.Utilities.Log;

namespace TestWinForm
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picRemoteModule = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.StartDebugButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.btnRemoteModuleLive = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.LoadPreviousPrintsGroup = new System.Windows.Forms.GroupBox();
            this.printSerializeRadio = new System.Windows.Forms.RadioButton();
            this.PrintBmpRadio = new System.Windows.Forms.RadioButton();
            this.previousPrintCheckBox = new System.Windows.Forms.CheckBox();
            this.txtQualityThreshold = new System.Windows.Forms.TextBox();
            this.chkEnableQualityCheck = new System.Windows.Forms.CheckBox();
            this.chkForceQualityCheck = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dpi1000 = new System.Windows.Forms.RadioButton();
            this.dpi500 = new System.Windows.Forms.RadioButton();
            this.captureModeGroup = new System.Windows.Forms.GroupBox();
            this.criminalNoPalmsCaptureRadio = new System.Windows.Forms.RadioButton();
            this.criminalPalmCaptureRadio = new System.Windows.Forms.RadioButton();
            this.civilCaptureRadio = new System.Windows.Forms.RadioButton();
            this.chkSqMode = new System.Windows.Forms.CheckBox();
            this.ResultGroupBox = new System.Windows.Forms.GroupBox();
            this.ImageResultPanel = new System.Windows.Forms.Panel();
            this.ResultLabel = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRemoteModule)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.LoadPreviousPrintsGroup.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.captureModeGroup.SuspendLayout();
            this.ResultGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(76, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 45);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(64, 121);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(98, 88);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(183, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(98, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Login";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picRemoteModule);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Location = new System.Drawing.Point(174, 42);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(139, 198);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remote Module";
            // 
            // picRemoteModule
            // 
            this.picRemoteModule.Location = new System.Drawing.Point(7, 53);
            this.picRemoteModule.Name = "picRemoteModule";
            this.picRemoteModule.Size = new System.Drawing.Size(100, 139);
            this.picRemoteModule.TabIndex = 1;
            this.picRemoteModule.TabStop = false;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(7, 20);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 23);
            this.button3.TabIndex = 0;
            this.button3.Text = "Start";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Controls.Add(this.LoadPreviousPrintsGroup);
            this.groupBox2.Controls.Add(this.txtQualityThreshold);
            this.groupBox2.Controls.Add(this.chkEnableQualityCheck);
            this.groupBox2.Controls.Add(this.chkForceQualityCheck);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.captureModeGroup);
            this.groupBox2.Controls.Add(this.chkSqMode);
            this.groupBox2.Location = new System.Drawing.Point(319, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(607, 293);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Test Livescan with parameters";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.StartDebugButton);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.btnRemoteModuleLive);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Location = new System.Drawing.Point(15, 176);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 100);
            this.panel1.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Start Options";
            // 
            // StartDebugButton
            // 
            this.StartDebugButton.Location = new System.Drawing.Point(16, 28);
            this.StartDebugButton.Name = "StartDebugButton";
            this.StartDebugButton.Size = new System.Drawing.Size(137, 23);
            this.StartDebugButton.TabIndex = 9;
            this.StartDebugButton.Text = "Live - Direct Start (Debug)";
            this.StartDebugButton.UseVisualStyleBackColor = true;
            this.StartDebugButton.Click += new System.EventHandler(this.StartDebugButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(18, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 34);
            this.label2.TabIndex = 13;
            this.label2.Text = "Will crash if launched twice without stopping the process.";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(421, 28);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(95, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "DynaInvok Start";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnRemoteModuleLive
            // 
            this.btnRemoteModuleLive.Location = new System.Drawing.Point(182, 28);
            this.btnRemoteModuleLive.Name = "btnRemoteModuleLive";
            this.btnRemoteModuleLive.Size = new System.Drawing.Size(137, 20);
            this.btnRemoteModuleLive.TabIndex = 16;
            this.btnRemoteModuleLive.Text = "Remote Module - Live";
            this.btnRemoteModuleLive.UseVisualStyleBackColor = true;
            this.btnRemoteModuleLive.Click += new System.EventHandler(this.btnRemoteModuleLive_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(421, 54);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(95, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Direct Start";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // LoadPreviousPrintsGroup
            // 
            this.LoadPreviousPrintsGroup.Controls.Add(this.printSerializeRadio);
            this.LoadPreviousPrintsGroup.Controls.Add(this.PrintBmpRadio);
            this.LoadPreviousPrintsGroup.Controls.Add(this.previousPrintCheckBox);
            this.LoadPreviousPrintsGroup.Location = new System.Drawing.Point(235, 74);
            this.LoadPreviousPrintsGroup.Name = "LoadPreviousPrintsGroup";
            this.LoadPreviousPrintsGroup.Size = new System.Drawing.Size(200, 100);
            this.LoadPreviousPrintsGroup.TabIndex = 8;
            this.LoadPreviousPrintsGroup.TabStop = false;
            this.LoadPreviousPrintsGroup.Text = "Previous Prints Options";
            // 
            // printSerializeRadio
            // 
            this.printSerializeRadio.AutoSize = true;
            this.printSerializeRadio.Checked = true;
            this.printSerializeRadio.Location = new System.Drawing.Point(22, 71);
            this.printSerializeRadio.Name = "printSerializeRadio";
            this.printSerializeRadio.Size = new System.Drawing.Size(70, 17);
            this.printSerializeRadio.TabIndex = 2;
            this.printSerializeRadio.TabStop = true;
            this.printSerializeRadio.Text = "Serialized";
            this.printSerializeRadio.UseVisualStyleBackColor = true;
            // 
            // PrintBmpRadio
            // 
            this.PrintBmpRadio.AutoSize = true;
            this.PrintBmpRadio.Location = new System.Drawing.Point(22, 48);
            this.PrintBmpRadio.Name = "PrintBmpRadio";
            this.PrintBmpRadio.Size = new System.Drawing.Size(46, 17);
            this.PrintBmpRadio.TabIndex = 1;
            this.PrintBmpRadio.Text = "Bmp";
            this.PrintBmpRadio.UseVisualStyleBackColor = true;
            // 
            // previousPrintCheckBox
            // 
            this.previousPrintCheckBox.AutoSize = true;
            this.previousPrintCheckBox.Location = new System.Drawing.Point(11, 20);
            this.previousPrintCheckBox.Name = "previousPrintCheckBox";
            this.previousPrintCheckBox.Size = new System.Drawing.Size(134, 17);
            this.previousPrintCheckBox.TabIndex = 0;
            this.previousPrintCheckBox.Text = "Load Previous print set";
            this.previousPrintCheckBox.UseVisualStyleBackColor = true;
            // 
            // txtQualityThreshold
            // 
            this.txtQualityThreshold.Location = new System.Drawing.Point(405, 51);
            this.txtQualityThreshold.Name = "txtQualityThreshold";
            this.txtQualityThreshold.Size = new System.Drawing.Size(27, 20);
            this.txtQualityThreshold.TabIndex = 19;
            this.txtQualityThreshold.Text = "3";
            // 
            // chkEnableQualityCheck
            // 
            this.chkEnableQualityCheck.AutoSize = true;
            this.chkEnableQualityCheck.Location = new System.Drawing.Point(257, 51);
            this.chkEnableQualityCheck.Name = "chkEnableQualityCheck";
            this.chkEnableQualityCheck.Size = new System.Drawing.Size(142, 17);
            this.chkEnableQualityCheck.TabIndex = 18;
            this.chkEnableQualityCheck.Text = "Enable Quality validation";
            this.chkEnableQualityCheck.UseVisualStyleBackColor = true;
            // 
            // chkForceQualityCheck
            // 
            this.chkForceQualityCheck.AutoSize = true;
            this.chkForceQualityCheck.Location = new System.Drawing.Point(246, 28);
            this.chkForceQualityCheck.Name = "chkForceQualityCheck";
            this.chkForceQualityCheck.Size = new System.Drawing.Size(211, 17);
            this.chkForceQualityCheck.TabIndex = 17;
            this.chkForceQualityCheck.Text = "Overide quality validation and threshold";
            this.chkForceQualityCheck.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dpi1000);
            this.groupBox3.Controls.Add(this.dpi500);
            this.groupBox3.Location = new System.Drawing.Point(15, 132);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(142, 38);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Dpi";
            // 
            // dpi1000
            // 
            this.dpi1000.AutoSize = true;
            this.dpi1000.Location = new System.Drawing.Point(76, 18);
            this.dpi1000.Name = "dpi1000";
            this.dpi1000.Size = new System.Drawing.Size(49, 17);
            this.dpi1000.TabIndex = 1;
            this.dpi1000.TabStop = true;
            this.dpi1000.Text = "1000";
            this.dpi1000.UseVisualStyleBackColor = true;
            // 
            // dpi500
            // 
            this.dpi500.AutoSize = true;
            this.dpi500.Checked = true;
            this.dpi500.Location = new System.Drawing.Point(11, 19);
            this.dpi500.Name = "dpi500";
            this.dpi500.Size = new System.Drawing.Size(43, 17);
            this.dpi500.TabIndex = 0;
            this.dpi500.TabStop = true;
            this.dpi500.Text = "500";
            this.dpi500.UseVisualStyleBackColor = true;
            // 
            // captureModeGroup
            // 
            this.captureModeGroup.Controls.Add(this.criminalNoPalmsCaptureRadio);
            this.captureModeGroup.Controls.Add(this.criminalPalmCaptureRadio);
            this.captureModeGroup.Controls.Add(this.civilCaptureRadio);
            this.captureModeGroup.Location = new System.Drawing.Point(15, 47);
            this.captureModeGroup.Name = "captureModeGroup";
            this.captureModeGroup.Size = new System.Drawing.Size(142, 78);
            this.captureModeGroup.TabIndex = 12;
            this.captureModeGroup.TabStop = false;
            this.captureModeGroup.Text = "Capture Mode";
            // 
            // criminalNoPalmsCaptureRadio
            // 
            this.criminalNoPalmsCaptureRadio.AutoSize = true;
            this.criminalNoPalmsCaptureRadio.Location = new System.Drawing.Point(18, 61);
            this.criminalNoPalmsCaptureRadio.Name = "criminalNoPalmsCaptureRadio";
            this.criminalNoPalmsCaptureRadio.Size = new System.Drawing.Size(112, 17);
            this.criminalNoPalmsCaptureRadio.TabIndex = 2;
            this.criminalNoPalmsCaptureRadio.Text = "Criminal (no palms)";
            this.criminalNoPalmsCaptureRadio.UseVisualStyleBackColor = true;
            // 
            // criminalPalmCaptureRadio
            // 
            this.criminalPalmCaptureRadio.AutoSize = true;
            this.criminalPalmCaptureRadio.Checked = true;
            this.criminalPalmCaptureRadio.Location = new System.Drawing.Point(18, 44);
            this.criminalPalmCaptureRadio.Name = "criminalPalmCaptureRadio";
            this.criminalPalmCaptureRadio.Size = new System.Drawing.Size(122, 17);
            this.criminalPalmCaptureRadio.TabIndex = 1;
            this.criminalPalmCaptureRadio.TabStop = true;
            this.criminalPalmCaptureRadio.Text = "Criminal (With palms)";
            this.criminalPalmCaptureRadio.UseVisualStyleBackColor = true;
            // 
            // civilCaptureRadio
            // 
            this.civilCaptureRadio.AutoSize = true;
            this.civilCaptureRadio.Location = new System.Drawing.Point(18, 21);
            this.civilCaptureRadio.Name = "civilCaptureRadio";
            this.civilCaptureRadio.Size = new System.Drawing.Size(44, 17);
            this.civilCaptureRadio.TabIndex = 0;
            this.civilCaptureRadio.Text = "Civil";
            this.civilCaptureRadio.UseVisualStyleBackColor = true;
            // 
            // chkSqMode
            // 
            this.chkSqMode.AutoSize = true;
            this.chkSqMode.Checked = true;
            this.chkSqMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSqMode.Location = new System.Drawing.Point(15, 24);
            this.chkSqMode.Name = "chkSqMode";
            this.chkSqMode.Size = new System.Drawing.Size(69, 17);
            this.chkSqMode.TabIndex = 10;
            this.chkSqMode.Text = "Sq Mode";
            this.chkSqMode.UseVisualStyleBackColor = true;
            // 
            // ResultGroupBox
            // 
            this.ResultGroupBox.Controls.Add(this.ImageResultPanel);
            this.ResultGroupBox.Controls.Add(this.ResultLabel);
            this.ResultGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ResultGroupBox.Location = new System.Drawing.Point(0, 324);
            this.ResultGroupBox.Name = "ResultGroupBox";
            this.ResultGroupBox.Size = new System.Drawing.Size(997, 450);
            this.ResultGroupBox.TabIndex = 8;
            this.ResultGroupBox.TabStop = false;
            this.ResultGroupBox.Text = "Capture Results";
            // 
            // ImageResultPanel
            // 
            this.ImageResultPanel.AutoScroll = true;
            this.ImageResultPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ImageResultPanel.Location = new System.Drawing.Point(3, 47);
            this.ImageResultPanel.Name = "ImageResultPanel";
            this.ImageResultPanel.Size = new System.Drawing.Size(991, 400);
            this.ImageResultPanel.TabIndex = 1;
            // 
            // ResultLabel
            // 
            this.ResultLabel.AutoSize = true;
            this.ResultLabel.Location = new System.Drawing.Point(9, 26);
            this.ResultLabel.Name = "ResultLabel";
            this.ResultLabel.Size = new System.Drawing.Size(35, 13);
            this.ResultLabel.TabIndex = 0;
            this.ResultLabel.Text = "label3";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(174, 246);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(139, 23);
            this.button6.TabIndex = 9;
            this.button6.Text = "Single Finger Capture";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(997, 774);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.ResultGroupBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picRemoteModule)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.LoadPreviousPrintsGroup.ResumeLayout(false);
            this.LoadPreviousPrintsGroup.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.captureModeGroup.ResumeLayout(false);
            this.captureModeGroup.PerformLayout();
            this.ResultGroupBox.ResumeLayout(false);
            this.ResultGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.PictureBox picRemoteModule;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button StartDebugButton;
        private System.Windows.Forms.GroupBox captureModeGroup;
        private System.Windows.Forms.RadioButton criminalNoPalmsCaptureRadio;
        private System.Windows.Forms.RadioButton criminalPalmCaptureRadio;
        private System.Windows.Forms.RadioButton civilCaptureRadio;
        private System.Windows.Forms.CheckBox chkSqMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton dpi1000;
        private System.Windows.Forms.RadioButton dpi500;
        private System.Windows.Forms.Button btnRemoteModuleLive;
        private System.Windows.Forms.TextBox txtQualityThreshold;
        private System.Windows.Forms.CheckBox chkEnableQualityCheck;
        private System.Windows.Forms.CheckBox chkForceQualityCheck;
        private System.Windows.Forms.GroupBox LoadPreviousPrintsGroup;
        private System.Windows.Forms.RadioButton printSerializeRadio;
        private System.Windows.Forms.RadioButton PrintBmpRadio;
        private System.Windows.Forms.CheckBox previousPrintCheckBox;
        private System.Windows.Forms.GroupBox ResultGroupBox;
        private System.Windows.Forms.Label ResultLabel;
        private System.Windows.Forms.Panel ImageResultPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button6;
    }
}

