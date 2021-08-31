namespace PrintsCapture.QuickCaptureControl
{
    partial class QuickCapture
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
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.btnStartCapture = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // picPreview
            // 
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(108, 98);
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // picStatus
            // 
            this.picStatus.Location = new System.Drawing.Point(1, 104);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(19, 22);
            this.picStatus.TabIndex = 1;
            this.picStatus.TabStop = false;
            // 
            // btnStartCapture
            // 
            this.btnStartCapture.Location = new System.Drawing.Point(4, 34);
            this.btnStartCapture.Name = "btnStartCapture";
            this.btnStartCapture.Size = new System.Drawing.Size(101, 23);
            this.btnStartCapture.TabIndex = 2;
            this.btnStartCapture.Text = "Start";
            this.btnStartCapture.UseVisualStyleBackColor = true;
            this.btnStartCapture.Visible = false;
            // 
            // QuickCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnStartCapture);
            this.Controls.Add(this.picStatus);
            this.Controls.Add(this.picPreview);
            this.Name = "QuickCapture";
            this.Size = new System.Drawing.Size(108, 132);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.PictureBox picStatus;
        private System.Windows.Forms.Button btnStartCapture;
    }
}
