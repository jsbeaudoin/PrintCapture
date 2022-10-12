using PrintsCapture.Ui.Class.Wizard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestWinForm
{
    using System.Globalization;
    using System.IO;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Ui.Class;

    using UniBIO.Services.Communication.BiometricService;

    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.XML;

    //using PrintsCapture.Login.Class;

    public partial class Form1 : Form
    {
        private Dictionary<string, string> GetPrintsCaptureArguments(string cmdType)
        {
            var d = new Dictionary<string, string>();
            d.Add("mode", cmdType);
            d.Add("debug", "1");
            d.Add("descriptionline1", "");
            d.Add("descriptionline2", "");
            d.Add("wizard", "1");
            return d;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var wz = new PrintCaptureWizard();

            var result = wz.ShowWizardDialog("fr", 3, true);


            var firstImage = result?.Prints?.FirstOrDefault();



            if (firstImage == null)
            {
                this.label1.Text = "No result to display";
                return;
            }

            this.label1.Text = "minutias : " + firstImage.MinutiaCount.ToString();

            //this.pictureBox1.Image = firstImage.Image;            
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

                //var config = new PrintCaptureAppParameter
                //{
                //    CultureName = "en",
                //    Mode = "livescan",
                //    DescriptionLine1 = "description line 1",
                //    DescriptionLine2 = "description line 2",
                //    CaptureModeAllowed = PrintCaptureGroup.FlatOnly,
                //    IsWizardMode = false,
                //    IcdVersion = "178",
                //    IsDebugAvailable = true,
                //};


                //PrintCaptureApp.ApplicationCulture = new CultureInfo(config.CultureName);

                //var result = PrintCaptureApp.Start(config);

                this.Enabled = false;
                var rm = new RemoteModuleHelper("fr", true);

                rm.CaptureCompleted += (success, resultObject, exception) =>
                {
                    this.Invoke((Action)(() =>
                   {
                       this.Enabled = true;
                       var fingers = resultObject as CapturedPrintData;

                       if (exception != null)
                       {
                           MessageBox.Show(string.Format("Error {0}", exception.Message),
                               "Prints", MessageBoxButtons.OK, MessageBoxIcon.Error);
                       }


                       if (fingers == null)
                       {
                            //this.SetScanComplete(false);
                            return;
                       }

                        //this.SetScanComplete(success);
                    }));


                };

                rm.GetLivePrints(GetCaptureParameters());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "TEST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
            }

        }

        private void fingerprintControl1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            var args = GetCaptureParameters();
            args.Add("singlecaptureprompt", "PrintCapture Login Title");

            try
            {
                /* var result = printCapture.Capture(args);

                var args = new Dictionary<string, string>();
                args.Add("mode", "live");
                args.Add("culture", "en");
                args.Add("debug", "1");
                args.Add("descriptionline1", "");
                args.Add("descriptionline2", "");
                args.Add("wizard", "1");
                args.Add("lang", "en");
                args.Add("capturemode", "8");
                args.Add("login", "1");
                args.Add("singlecaptureprompt", "PrintCapture Login Title");
                args.Add("topmost", "1");
                args.Add("alwayscanoverride", "1");
                */

                object[] arguments = { args };
                var assemblyPath = Application.StartupPath + "\\PrintsCapture.Direct.dll";
                //PrintsCapture.Direct.PrintCaptureDllApp.DoCapture(args);
                //PrintsCapture.Direct.PrintCaptureDllApp.CaptureWindowClosed += PrintCaptureCompleted;

                // printCapture.Capture(args); // new PrintsCapture.Direct.Wizard();

                var result = DynaInvoke.InvokeMethodSlow(assemblyPath, "Wizard", "Capture", arguments);

                if (result == null)
                {
                    MessageBox.Show("Null result received from DirectCapture", "Direct Capture");
                }
                else
                {
                    MessageBox.Show("Capture finished without error", "Direct Capture");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            var printCapture = new PrintsCapture.Direct.Wizard();

            try
            {

                var args = new Dictionary<string, string>();
                args.Add("mode", "live");
                args.Add("culture", "en");
                args.Add("debug", "1");
                args.Add("descriptionline1", "");
                args.Add("descriptionline2", "");
                args.Add("wizard", "0");
                args.Add("lang", "en");
                args.Add("capturemode", "4");
                args.Add("noendorsement", "1"); // no endorsement finger
                args.Add("captureorder", "sq");

                //args.Add("login", "0");
                //args.Add("singlecaptureprompt", "PrintCapture Login Title");
                args.Add("topmost", "0");
                args.Add("alwayscanoverride", "1");

                var result = printCapture.Capture(args);
                printCapture.Dispose();

                if (result == null)
                {
                    MessageBox.Show("Null result received from DirectCapture", "Direct Capture");
                }
                else
                {
                    MessageBox.Show("Capture finished without error", "Direct Capture");
                    if (MessageBox.Show("Do you want to save bmp prints?", "Saving Prints", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        for (int x = 0; x < result.Prints.Count; x++)
                        {
                            var fileName = $"D://Finger{result.Prints[x].Position}.bmp";
                            var bmp = XL_ID.Utilities.Image.ImageUtilities.ConvertFromBinary(result.Prints[x].ImageData);
                            bmp.Save(fileName);
                            bmp.Dispose();
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private Dictionary<string, string> GetCaptureParameters()
        {
            bool isSqMode = this.chkSqMode.Checked;
            string captureGroup = "0";
            bool endorsement = false;
            if (this.civilCaptureRadio.Checked)
            {
                endorsement = true;
                captureGroup = "3"; // flats only or standard 14
            }
            else if (this.criminalNoPalmsCaptureRadio.Checked)
            {
                captureGroup = "1"; // standard 14 only
            }
            else if (this.criminalPalmCaptureRadio.Checked)
            {
                captureGroup = "5"; // palms + standard 14
            }

            var args = new Dictionary<string, string>();
            args.Add("mode", "live");
            args.Add("culture", "en");
            args.Add("debug", "1");
            args.Add("descriptionline1", "");
            args.Add("descriptionline2", "");
            args.Add("wizard", "0");
            args.Add("lang", "en");
            args.Add("capture", captureGroup);
            args.Add("endorsement", endorsement ? "1" : "0"); // no endorsement finger
            args.Add("sqmode", isSqMode ? "1" : "");

            //args.Add("login", "0");
            //args.Add("singlecaptureprompt", "PrintCapture Login Title");
            args.Add("topmost", "0");
            args.Add("alwayscanoverride", "1");

            if (this.previousPrintCheckBox.Checked)
            {
                args.Add("prints", GetPreviousPrints());
            }

            return args;
        }

        private string GetPreviousPrints()
        {
            List<int> printIndexes = null;
            if (this.civilCaptureRadio.Checked)
            {
                printIndexes = new List<int> { 13,14,15 };
            }
            else if (this.criminalNoPalmsCaptureRadio.Checked)
            {
                printIndexes = new List<int> { 1,2,3,4,5,6,7,8,9,10,11,12, 13, 14 };
            }
            else if (this.criminalPalmCaptureRadio.Checked)
            {
                printIndexes = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 22,24,25,26,27,28 };
            }
            var imageDpi = this.dpi500.Checked ? 500 : 1000;

            var allPrints = new List<FingerprintData>();

            

            var dlg = new OpenFileDialog { Filter = "Bitmap|*.bmp" };
            dlg.Title = string.Format("Select print bmp in folder");
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                throw new ApplicationException("Dialog canceled");
            }

            if (dlg.FileName.EndsWith(".txt"))
            {
                return File.ReadAllText(dlg.FileName);
            }

            var folder = Path.GetDirectoryName(dlg.FileName);

            foreach (var imageIndex in printIndexes)
            {
                var printData = new FingerprintData();
                printData.Position = imageIndex;

                var filePath = $"{folder}\\{imageIndex}.bmp";
                printData.ImageDataFormat = PrintDataFormat.Bmp;
                if (File.Exists(filePath))
                {
                    var bmp = (Bitmap)Image.FromFile(filePath);
                    if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                    {
                        bmp = ImageUtilities.ConvertToIndexedFormat(bmp, ConvertBitmapFormat.Format8bppIndexed);
                    }
                    printData.ImageData = ImageUtilities.ConvertToByteArray(bmp);
                    printData.ImageInfo = new ImageInformation { DPI = imageDpi, ImpressionType = UniBIO.Services.Communication.TransactionService.CaptureType.LiveScan, HLL = bmp.Width, VLL = bmp.Height };
                    
                    if (imageIndex == 6)
                    {
                        printData.Override = new FingerprintOverride { Description = "TEST OVERRIDE", Position = imageIndex, ReasonCode = 99 };
                    } else if (imageIndex == 7)
                    {
                        printData.Override = new FingerprintOverride { Position = imageIndex, ReasonCode = 2 };
                    }
                } else
                {
                    printData.Missing = new MissingPrint { Date = "1990-01-01", Position = (PrintPosition) imageIndex, NistCode = "MI" };
                }
                allPrints.Add(printData);
            }

            var capturedData = new CapturedPrintData
            {
                CaptureFlatOnly = this.civilCaptureRadio.Checked,
                CapturedTime = DateTime.Now,
                Device = new DeviceInformation { Kind = DeviceKind.LiveScan, Manufacturer = "A", ModelName = "B", SerialNumber = "C" },
                Dpi = imageDpi,
                Prints = allPrints
            };

            return ObjectSerializer.SaveIntanceToString(capturedData);
    }
    

        private void StartDebugButton_Click(object sender, EventArgs e)
        {
            try
            {
                PrintsCapture.Direct.PrintCaptureDllApp.Reset();
                var args = GetCaptureParameters();
                
                PrintsCapture.Direct.PrintCaptureDllApp.DoCapture(args);
                PrintsCapture.Direct.PrintCaptureDllApp.CaptureWindowClosed += PrintCaptureCompleted;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void PrintCaptureCompleted(object sender, EventArgs e)
        {
            var result = PrintsCapture.Direct.PrintCaptureDllApp.GetResults();

            if (result == null)
            {
                MessageBox.Show("Null result received from DirectCapture", "Direct Capture");
            }
            else
            {
                MessageBox.Show("Capture finished without error", "Direct Capture");
                if (MessageBox.Show("Do you want to save bmp prints?", "Saving Prints", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    for (int x = 0; x < result.Prints.Count; x++)
                    {
                        var fileName = $"D://Finger{result.Prints[x].Position}.bmp";
                        var bmp = XL_ID.Utilities.Image.ImageUtilities.ConvertFromBinary(result.Prints[x].ImageData);
                        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        bmp.Dispose();
                    }

                }
            }
        }

        private void btnRemoteModuleLive_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                var rm = new RemoteModuleHelper("fr", true);

                rm.CaptureCompleted += (success, resultObject, exception) =>
                {
                    this.Invoke((Action)(() =>
                    {
                        this.Enabled = true;
                        var fingers = resultObject as CapturedPrintData;

                        if (exception != null)
                        {
                            MessageBox.Show(string.Format("Error {0}", exception.Message),
                                "Prints", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }


                        if (fingers == null)
                        {
                            //this.SetScanComplete(false);
                            return;
                        }

                        //this.SetScanComplete(success);
                    }));


                };
                var args = GetCaptureParameters();
                args.Add("debug-process", "1");
                rm.GetLivePrints(args);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "TEST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
            }
        }
    }
}
