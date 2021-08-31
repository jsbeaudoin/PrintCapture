
namespace PrintsCapture.LivescanWinForm.Forms
{
    partial class PreviewWindow
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
            this.BottomPanel = new System.Windows.Forms.Panel();
            this.btnClosePreview = new System.Windows.Forms.Button();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.MessagePanel = new System.Windows.Forms.Panel();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.PreviewImage = new System.Windows.Forms.PictureBox();
            this.LastPrintImage = new System.Windows.Forms.PictureBox();
            this.lblPreviousPrint = new System.Windows.Forms.Label();
            this.btnScanAgain = new System.Windows.Forms.Button();
            this.DeviceMessagePanel = new System.Windows.Forms.Panel();
            this.btnRestart = new System.Windows.Forms.Button();
            this.lblDeviceMessage = new System.Windows.Forms.Label();
            this.BottomPanel.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.MessagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastPrintImage)).BeginInit();
            this.DeviceMessagePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.btnClosePreview);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Location = new System.Drawing.Point(0, 531);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = new System.Drawing.Size(866, 45);
            this.BottomPanel.TabIndex = 0;
            // 
            // btnClosePreview
            // 
            this.btnClosePreview.Location = new System.Drawing.Point(348, 6);
            this.btnClosePreview.Name = "btnClosePreview";
            this.btnClosePreview.Size = new System.Drawing.Size(156, 32);
            this.btnClosePreview.TabIndex = 0;
            this.btnClosePreview.Text = "Fermer";
            this.btnClosePreview.UseVisualStyleBackColor = true;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.btnScanAgain);
            this.leftPanel.Controls.Add(this.lblPreviousPrint);
            this.leftPanel.Controls.Add(this.LastPrintImage);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(214, 531);
            this.leftPanel.TabIndex = 1;
            // 
            // TopPanel
            // 
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(214, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(652, 152);
            this.TopPanel.TabIndex = 2;
            // 
            // MessagePanel
            // 
            this.MessagePanel.Controls.Add(this.MessageLabel);
            this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MessagePanel.Location = new System.Drawing.Point(214, 152);
            this.MessagePanel.Name = "MessagePanel";
            this.MessagePanel.Size = new System.Drawing.Size(652, 46);
            this.MessagePanel.TabIndex = 3;
            // 
            // MessageLabel
            // 
            this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageLabel.Location = new System.Drawing.Point(0, 0);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(652, 46);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.Text = "@Message";
            this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PreviewImage
            // 
            this.PreviewImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewImage.Location = new System.Drawing.Point(214, 198);
            this.PreviewImage.Name = "PreviewImage";
            this.PreviewImage.Size = new System.Drawing.Size(652, 333);
            this.PreviewImage.TabIndex = 4;
            this.PreviewImage.TabStop = false;
            // 
            // LastPrintImage
            // 
            this.LastPrintImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.LastPrintImage.Location = new System.Drawing.Point(0, 0);
            this.LastPrintImage.Name = "LastPrintImage";
            this.LastPrintImage.Size = new System.Drawing.Size(214, 198);
            this.LastPrintImage.TabIndex = 0;
            this.LastPrintImage.TabStop = false;
            // 
            // lblPreviousPrint
            // 
            this.lblPreviousPrint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPreviousPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPreviousPrint.Location = new System.Drawing.Point(4, 209);
            this.lblPreviousPrint.Name = "lblPreviousPrint";
            this.lblPreviousPrint.Size = new System.Drawing.Size(204, 23);
            this.lblPreviousPrint.TabIndex = 1;
            this.lblPreviousPrint.Text = "Empreinte précédente";
            this.lblPreviousPrint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnScanAgain
            // 
            this.btnScanAgain.Location = new System.Drawing.Point(3, 447);
            this.btnScanAgain.Name = "btnScanAgain";
            this.btnScanAgain.Size = new System.Drawing.Size(208, 23);
            this.btnScanAgain.TabIndex = 2;
            this.btnScanAgain.Text = "Numériser à nouveau";
            this.btnScanAgain.UseVisualStyleBackColor = true;
            // 
            // DeviceMessagePanel
            // 
            this.DeviceMessagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DeviceMessagePanel.Controls.Add(this.lblDeviceMessage);
            this.DeviceMessagePanel.Controls.Add(this.btnRestart);
            this.DeviceMessagePanel.Location = new System.Drawing.Point(220, 318);
            this.DeviceMessagePanel.Name = "DeviceMessagePanel";
            this.DeviceMessagePanel.Size = new System.Drawing.Size(639, 81);
            this.DeviceMessagePanel.TabIndex = 5;
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(225, 51);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(187, 23);
            this.btnRestart.TabIndex = 0;
            this.btnRestart.Text = "Reprendre";
            this.btnRestart.UseVisualStyleBackColor = true;
            // 
            // lblDeviceMessage
            // 
            this.lblDeviceMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeviceMessage.Location = new System.Drawing.Point(4, 15);
            this.lblDeviceMessage.Name = "lblDeviceMessage";
            this.lblDeviceMessage.Size = new System.Drawing.Size(630, 23);
            this.lblDeviceMessage.TabIndex = 1;
            this.lblDeviceMessage.Text = "@DeviceMessage";
            this.lblDeviceMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PreviewWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 576);
            this.Controls.Add(this.DeviceMessagePanel);
            this.Controls.Add(this.PreviewImage);
            this.Controls.Add(this.MessagePanel);
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.BottomPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreviewWindow";
            this.ShowIcon = false;
            this.Text = "Preview Window";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.BottomPanel.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.MessagePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastPrintImage)).EndInit();
            this.DeviceMessagePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BottomPanel;
        private System.Windows.Forms.Button btnClosePreview;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Panel MessagePanel;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.PictureBox PreviewImage;
        private System.Windows.Forms.PictureBox LastPrintImage;
        private System.Windows.Forms.Label lblPreviousPrint;
        private System.Windows.Forms.Button btnScanAgain;
        private System.Windows.Forms.Panel DeviceMessagePanel;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Label lblDeviceMessage;
    }
}