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
            this.label2 = new System.Windows.Forms.Label();
            this.captureModeGroup = new System.Windows.Forms.GroupBox();
            this.criminalNoPalmsCaptureRadio = new System.Windows.Forms.RadioButton();
            this.criminalPalmCaptureRadio = new System.Windows.Forms.RadioButton();
            this.civilCaptureRadio = new System.Windows.Forms.RadioButton();
            this.chkSqMode = new System.Windows.Forms.CheckBox();
            this.button5 = new System.Windows.Forms.Button();
            this.StartDebugButton = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.fingerprintControl1 = new TestWinForm.UserControl.FingerprintControl();
            this.previousPrintCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dpi500 = new System.Windows.Forms.RadioButton();
            this.dpi1000 = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRemoteModule)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.captureModeGroup.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            this.button2.Location = new System.Drawing.Point(174, 12);
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
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.previousPrintCheckBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.captureModeGroup);
            this.groupBox2.Controls.Add(this.chkSqMode);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.StartDebugButton);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Location = new System.Drawing.Point(365, 42);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(386, 219);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Direct Capture";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(153, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(190, 34);
            this.label2.TabIndex = 13;
            this.label2.Text = "Will crash if launched twice without stopping the process.";
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
            this.criminalPalmCaptureRadio.Location = new System.Drawing.Point(18, 44);
            this.criminalPalmCaptureRadio.Name = "criminalPalmCaptureRadio";
            this.criminalPalmCaptureRadio.Size = new System.Drawing.Size(122, 17);
            this.criminalPalmCaptureRadio.TabIndex = 1;
            this.criminalPalmCaptureRadio.Text = "Criminal (With palms)";
            this.criminalPalmCaptureRadio.UseVisualStyleBackColor = true;
            // 
            // civilCaptureRadio
            // 
            this.civilCaptureRadio.AutoSize = true;
            this.civilCaptureRadio.Checked = true;
            this.civilCaptureRadio.Location = new System.Drawing.Point(18, 21);
            this.civilCaptureRadio.Name = "civilCaptureRadio";
            this.civilCaptureRadio.Size = new System.Drawing.Size(44, 17);
            this.civilCaptureRadio.TabIndex = 0;
            this.civilCaptureRadio.TabStop = true;
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
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(179, 120);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(95, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Direct Start";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // StartDebugButton
            // 
            this.StartDebugButton.Location = new System.Drawing.Point(156, 20);
            this.StartDebugButton.Name = "StartDebugButton";
            this.StartDebugButton.Size = new System.Drawing.Size(160, 23);
            this.StartDebugButton.TabIndex = 9;
            this.StartDebugButton.Text = "Direct Start (With Debug)";
            this.StartDebugButton.UseVisualStyleBackColor = true;
            this.StartDebugButton.Click += new System.EventHandler(this.StartDebugButton_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(179, 91);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(95, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "DynaInvok Start";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // fingerprintControl1
            // 
            this.fingerprintControl1.Individual = null;
            this.fingerprintControl1.Location = new System.Drawing.Point(64, 267);
            this.fingerprintControl1.Name = "fingerprintControl1";
            this.fingerprintControl1.Size = new System.Drawing.Size(702, 479);
            this.fingerprintControl1.TabIndex = 5;
            this.fingerprintControl1.Load += new System.EventHandler(this.fingerprintControl1_Load);
            // 
            // previousPrintCheckBox
            // 
            this.previousPrintCheckBox.AutoSize = true;
            this.previousPrintCheckBox.Checked = true;
            this.previousPrintCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.previousPrintCheckBox.Location = new System.Drawing.Point(15, 150);
            this.previousPrintCheckBox.Name = "previousPrintCheckBox";
            this.previousPrintCheckBox.Size = new System.Drawing.Size(122, 17);
            this.previousPrintCheckBox.TabIndex = 14;
            this.previousPrintCheckBox.Text = "Load Previous prints";
            this.previousPrintCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dpi1000);
            this.groupBox3.Controls.Add(this.dpi500);
            this.groupBox3.Location = new System.Drawing.Point(15, 173);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(142, 38);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Dpi";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(997, 574);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.fingerprintControl1);
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
            this.captureModeGroup.ResumeLayout(false);
            this.captureModeGroup.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
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
        private UserControl.FingerprintControl fingerprintControl1;
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
        private System.Windows.Forms.CheckBox previousPrintCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton dpi1000;
        private System.Windows.Forms.RadioButton dpi500;
    }
}

