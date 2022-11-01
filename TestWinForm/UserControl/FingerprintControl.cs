using System;
using System.Windows.Forms;
using UniBIO.Services.Communication.BiometricService;

namespace TestWinForm.UserControl
{
    public partial class FingerprintControl : System.Windows.Forms.UserControl
    {
        private const string language = "fr";
        private const bool isDebug = true;
        private const bool isAdmin = true;

        public event OnDataValidatedHandler OnDataValidated;

        public delegate void OnDataValidatedHandler(bool success);

        public Object Individual { get; set; }

        public FingerprintControl()
        {
            this.InitializeComponent();
        }

        public FingerprintControl(Object individual)
        {
            this.Individual = individual;
            this.InitializeComponent();

            if (isAdmin)
            {
                this.OnDataValidated?.Invoke(true);
                this.pnlScanComplete.Visible = true;
            }
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            var rm = new RemoteModuleHelper(language, isDebug);

            rm.CaptureCompleted += (success, resultObject, exception) =>
            {
                this.Invoke((Action) (() =>
                {
                    this.Enabled = true;
                    var fingers = resultObject as CapturedPrintData;

                    if (exception != null)
                    {
                        MessageBox.Show(string.Format("Error {0}", exception.Message),
                            "Prints" ,MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }


                    if (fingers == null)
                    {
                        this.SetScanComplete(false);
                        return;
                    }

                    //this.Individual.TransactionInfo.ImageCaptureEquipment =
                    //    new ImageCaptureEquipment
                    //    {
                    //        Make = fingers.Device.Manufacturer,
                    //        Model = fingers.Device.ModelName,
                    //        SerialNumber = fingers.Device.SerialNumber
                    //    };

                    //this.Individual.TransactionInfo.FingerprintedDate = DateTime.Now;

                    //this.Individual.TransactionInfo.PrintsetInfo = new PrintSetInformation
                    //{
                    //    CaptureFlatOnly = fingers.CaptureFlatOnly
                    //};
                    //this.Individual.TransactionInfo.FingerprintData = fingers.Prints;
                    this.SetScanComplete(success);
                }));

                
            };

            //rm.GetLivePrints(this.get);            
        }



        public void SetLanguage()
        {
            //this.lblTitle.Text = UI.Fingerprints;
            //this.lblInstruction.Text = UI.FingerprintInstruction;
            //this.btnStartScan.Text = UI.StartFingerprintsCapture;
            //this.lblScanComplete.Text = UI.FingerprintScanCompleted;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                this.OnDataValidated?.Invoke(this.pnlScanComplete.Visible);
            }
        }

        private void SetScanComplete(bool isCompleted)
        {            
            this.OnDataValidated?.Invoke(isCompleted);
            this.pnlScanComplete.Visible = isCompleted;
        }
    }
}
