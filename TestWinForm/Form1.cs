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

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Ui.Class;

    using UniBIO.Services.Communication.BiometricService;

    using XL_ID.Utilities.Image;

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

            var result = wz.ShowWizardDialog("fr",3, true);

            
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
                            //this.SetScanComplete(false);
                            return;
                        }
                        
                        //this.SetScanComplete(success);
                    }));

                
                };

                rm.GetScanPrints();    
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
            //var args = new Dictionary<string, string>();
            //args.Add("mode", "live");
            //args.Add("culture", "en");
            //args.Add("debug", "1");
            //args.Add("descriptionline1", "");
            //args.Add("descriptionline2", "");
            //args.Add("wizard", "1");
            //args.Add("icdversion", "178");


            //args.Add("lang", "en");
            ////args.Add("capturemode", "2");
            //args.Add("capturemode", "8");
            //args.Add("login", "1");
            //args.Add("singlecaptureprompt", "Single PrintCapture Title");
            //args.Add("topmost","1" );

            //var printCapture = new PrintsCapture.Direct.Wizard();

            try
            {
                //var result = printCapture.Capture(args);

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


                object[] arguments = { args };
                var assemblyPath = Application.StartupPath + "\\PrintsCapture.Direct.dll";
                var result = DynaInvoke.InvokeMethodSlow(assemblyPath, "Wizard", "Capture", arguments);

                if (result == null)
                {
                    MessageBox.Show("Null result received from DirectCapture", "Direct Capture");
                }
                else
                {
                    MessageBox.Show("Capture finnished without error", "Direct Capture");
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
                args.Add("capturemode", "5");
                args.Add("login", "0");
                args.Add("singlecaptureprompt", "PrintCapture Login Title");
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
                    MessageBox.Show("Capture finnished without error", "Direct Capture");
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
    }
}
