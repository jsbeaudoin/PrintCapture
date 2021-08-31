using System.Drawing;
using System.IO;
using System.Windows.Media;
using Aware.AwSequence;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Ui.Class.Wizard;
using UniBIO.Services.Communication.BiometricService;
using XL_ID.Utilities.Wpf.Extension;
using XL_ID.Utilities.XML;
using Color = System.Drawing.Color;

namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using RemoteModules;
    using PrintsCapture.Ui.Extension;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.TestList = new List<ComboItemVm>
            {
                new ComboItemVm {Label = "Choose an option", Value = 0, IsEnabled = true},
                new ComboItemVm {Label = "Scan again", Value = -1, IsEnabled = true},
                new ComboItemVm {Label = "Ignore error with reason", Value = 0, IsEnabled = false},
                new ComboItemVm {Label = "Reason1", Value = 1, IsEnabled = true},

            };

            this.SelectedItem = this.TestList[0];

            InitializeComponent();

            

        }
        
        private RemoteModule remote;
        private RemoteModulesHost manager = new RemoteModulesHost();

        private Dictionary<string, string> commandConfig;

        

        private void RemoteTestButtonClick(object sender, RoutedEventArgs e)
        {

            this.manager.ServiceHost.Start();

            try
            {
                var foundModules = RemoteModulesFinder.InDirectory(this.AppPathTextBox.Text);

                if (foundModules.Count == 0)
                {
                    throw new ApplicationException("No Remote module found in folder !");                    
                }

                this.remote = this.manager.InitializeRemoteModule(foundModules.First());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not get the remote module. " + ex.Message);
                return;
            }
            
            
            this.remote.Connected += RmOnConnected;
            this.remote.Disconnected += RmDisconnected;
            this.remote.Exited += RmOnExited;
            this.remote.NewResult += RmNewResult;


            this.commandConfig = new Dictionary<string, string>();
            this.commandConfig.Add("culture", this.CultureNameTextBox.Text);
            //this.commandConfig.Add("Token", this.TokenTextBox.Text);
            //this.commandConfig.Add("ServiceAddress", this.AddressTextBox.Text);
            //this.commandConfig.Add("ServiceName", this.ServiceNameTextBox.Text);
            this.commandConfig.Add("descriptionline1", "Ligne 1 de description");
            this.commandConfig.Add("descriptionline2", "Ligne 2 de description");
            this.commandConfig.Add("wizard", "0");
            this.commandConfig.Add("debug", "1");
            this.commandConfig.Add("cmd", "driverlicence");
            this.commandConfig.Add("login", "0");
            this.commandConfig.Add("debug-process", "1");

            //this.commandConfig.Add("ReferenceId", this.IdTextBox.Text);

            if (this.LivescanModeRadioButton.IsChecked == true)
            {
                this.commandConfig.Add("mode","livescan");
                this.commandConfig.Add("capturemode", "8");
            }
            else if (this.chkCardScan.IsChecked == true)
            {
                this.commandConfig.Add("mode", "cardscan");
                this.commandConfig.Add("capturemode", "3");
            }
            else
            {
                this.commandConfig.Add("capturemode", "8");
            }
            //this.commandConfig.Add("capturemode", this.LivescanModeRadioButton.IsChecked == true ? "7" : "5");

            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        this.remote.Start();
                        this.DisplayInfo("Remote started locally");
                    }
                    catch (Exception ex)
                    {
                        this.DisplayInfo("Could not start remote : " + ex.Message);
                    }
                });
        }

        private void RmNewResult(Dictionary<string, string> result)
        {
            this.DisplayInfo("Remote  New Result");

            if (result.ContainsKey("success") && result["success"] == "0" && result.ContainsKey("message"))
            {
                this.DisplayInfo(result["message"]);
            }

            if (result.ContainsKey("data") && result["data"] != null)
            {
                CapturedPrintData data = null;
                data = XmlSerializer.Deserialize<CapturedPrintData>(result["data"]);

                ImageSource img = null;
                var firstImage = data.Prints.First();
                using (var ms = new MemoryStream(firstImage.ImageData))
                {
                    var b = new System.Drawing.Bitmap(ms);
                    img = b.ToImageSource(true, true);
                }

                this.DisplayInfo("minutias : " + firstImage.MinutiaCount.ToString());

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.ResultImage.Source = img;
                }));
            }

        }

        private void RmOnExited(int exitcode)
        {
            this.DisplayInfo("Remote  exited");
        }

        private void RmDisconnected(object sender, EventArgs e)
        {
            this.DisplayInfo("Remote disconnected");
        }

        private void DisplayInfo(string msg)
        {
            Application.Current.Dispatcher.Invoke(
                () => { this.ResulTextBlock.Text += Environment.NewLine + DateTime.Now.ToString("HH:mm:ss  ") + msg; });
        }

        private void RmOnConnected(object sender, EventArgs e)
        {
            this.DisplayInfo("Remote connected");
            //MessageBox.Show("Connected. Now is the time to attach the debug process.");
            this.remote.SendCommand(this.commandConfig);

            this.DisplayInfo("Command sent");
        }

        private void SeqTestButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var seq = new Aware.AwSequence.awSequenceCheck();

                //for (var index = 1; index <= 10; index++)
                //{
                //    seq.SetFingerMissing((awSequenceCheck.AwareFingerType) index,
                //        awSequenceCheck.AwareFingerMissingCode.AW_FNG_MISSING);
                //}

                //seq.SetFingerMissing(awSequenceCheck.AwareFingerType.AW_LEFT_INDEX_FINGER,
                //    awSequenceCheck.AwareFingerMissingCode.AW_FNG_PRESENT);


                for (var index = 1; index <= 10; index++)
                {
                    var awIndex=(awSequenceCheck.AwareFingerType)index;
                    var state = seq.FingerMissing(awIndex);
                    Console.WriteLine("Finger : '{0}'({1}) : {2}", awIndex,index, state);
                }

                var bmp = (Bitmap)Image.FromFile("C:\\temp\\Image2.bmp");
                var bytes = XL_ID.Utilities.Image.ImageUtilities.ConvertToByteArray(bmp);

                var imgCrop = new XL_ID.Utilities.Image.AutoCropper(bmp, Color.White, 80);
                var rect = imgCrop.GetAutoCropRectangle();
                Console.WriteLine("Cropped finger size : ({2},{3}) {0}x{1}", rect.Width, rect.Height, rect.Left, rect.Top);

                var awareError = seq.SetFingerRes(awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_INDEX_FINGER,
                    bytes,
                    bmp.Width,
                    bmp.Height,
                    awSequenceCheck.AwareImageResolution.AW_500PPI);
                Console.WriteLine("Loaded data into left index");

                var finger = XL_ID.Utilities.Image.ImageUtilities.AutoCrop(bmp);
                Console.WriteLine("Cropped finger size : {0}x{1}", finger.Width, finger.Height);

                awareError = seq.SetFingerRes(awSequenceCheck.AwareFingerType. AW_PLAIN_LEFT_FOUR_FINGERS, 
                    bytes,
                    bmp.Width,
                    bmp.Height,
                    awSequenceCheck.AwareImageResolution.AW_500PPI);
                Console.WriteLine("Loaded data into left slap");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }
        }

        private void WizardTestButtonClick(object sender, RoutedEventArgs e)
        {
            var captureGroupTest = PrintCaptureGroup.OneFingerOnly;

            var val1 = captureGroupTest.GetValues().Cast<PrintCaptureGroup>().FirstOrDefault();
            var cultureName = this.CultureNameTextBox.Text;
            Console.WriteLine(val1);

            var wz = new PrintCaptureWizard();

            var result = wz.ShowWizardDialog(cultureName, 7, true);


            ImageSource img = null;
            var firstImage = result?.Prints?.FirstOrDefault();

            if (firstImage == null)
            {
                this.DisplayInfo("No result to display");
                return;
            }

            this.DisplayInfo("minutias : " + firstImage.MinutiaCount.ToString());

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //this.ResultImage.Source = firstImage.ImageData. .ToImageSource();
            }));
        }

        public ComboItemVm SelectedItem { get; set; }

        public List<ComboItemVm> TestList { get; set; }
    }
}
