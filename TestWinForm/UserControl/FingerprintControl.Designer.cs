namespace TestWinForm.UserControl
{
    partial class FingerprintControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.pnlScanComplete = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lblScanComplete = new System.Windows.Forms.Label();
            this.btnStartScan = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlScanComplete.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.pictureBox1);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(702, 108);
            this.pnlHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(127, 33);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(569, 35);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Fingerprints";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            //this.pictureBox1.Image = global::XL_ID.UniDAC.Civil.Wizard.WinGui.Properties.Resources.Fingerprint_Scanner_Gris;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(117, 101);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // lblInstruction
            // 
            this.lblInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.Location = new System.Drawing.Point(3, 132);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(693, 52);
            this.lblInstruction.TabIndex = 3;
            this.lblInstruction.Text = "Click on the button below to scan your fingerprints";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlScanComplete
            // 
            this.pnlScanComplete.Controls.Add(this.pictureBox2);
            this.pnlScanComplete.Controls.Add(this.lblScanComplete);
            this.pnlScanComplete.Location = new System.Drawing.Point(4, 251);
            this.pnlScanComplete.Name = "pnlScanComplete";
            this.pnlScanComplete.Size = new System.Drawing.Size(692, 100);
            this.pnlScanComplete.TabIndex = 6;
            this.pnlScanComplete.Visible = false;
            // 
            // pictureBox2
            // 
            //this.pictureBox2.Image = global::XL_ID.UniDAC.Civil.Wizard.WinGui.Properties.Resources.symbol_check;
            this.pictureBox2.Location = new System.Drawing.Point(601, 9);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(88, 83);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 7;
            this.pictureBox2.TabStop = false;
            // 
            // lblScanComplete
            // 
            this.lblScanComplete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblScanComplete.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScanComplete.Location = new System.Drawing.Point(4, 9);
            this.lblScanComplete.Name = "lblScanComplete";
            this.lblScanComplete.Size = new System.Drawing.Size(591, 83);
            this.lblScanComplete.TabIndex = 6;
            this.lblScanComplete.Text = "All fingerprints have been captured";
            this.lblScanComplete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnStartScan
            // 
            this.btnStartScan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartScan.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.btnStartScan.Image = global::XL_ID.UniDAC.Civil.Wizard.WinGui.Properties.Resources.fingerprint_reade_32x32r1;
            this.btnStartScan.Location = new System.Drawing.Point(217, 199);
            this.btnStartScan.Name = "btnStartScan";
            this.btnStartScan.Size = new System.Drawing.Size(259, 44);
            this.btnStartScan.TabIndex = 4;
            this.btnStartScan.Text = "Start Fingerprints Capture";
            this.btnStartScan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnStartScan.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnStartScan.UseVisualStyleBackColor = true;
            this.btnStartScan.Click += new System.EventHandler(this.btnStartScan_Click);
            // 
            // FingerprintControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlScanComplete);
            this.Controls.Add(this.btnStartScan);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.pnlHeader);
            this.Name = "FingerprintControl";
            this.Size = new System.Drawing.Size(702, 500);
            this.pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlScanComplete.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnStartScan;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.Panel pnlScanComplete;
        private System.Windows.Forms.Label lblScanComplete;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
