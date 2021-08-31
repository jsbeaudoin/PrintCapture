namespace PrintsCapture.QuickCaptureControl.Class
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using PrintsCapture.Device;
    using PrintsCapture.Device.Class;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Livescan;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Prints.Sequence;
    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.QuickCaptureControl.Class.App;
    using PrintsCapture.QuickCaptureControl.Languages;

    /// <summary>
    /// Provides the abstraction layer between the application and cardscan and livescan
    /// </summary>
    public class PrintCaptureDriver 
    {
        public PrintList PrintList { get; set; }

        private static PrintCaptureDriver instance;        

        private PrintCaptureDriver(PrintList printList)

        {            
            this.PrintList = printList;
        }

        private DeviceCaptureDriver baseDriver;

        private readonly List<PrintInfo> batch = new List<PrintInfo>();

        private bool isWizardMode;

        /// <summary>
        /// Print to zoom if capturing a single print, after the capture has ended
        /// </summary>
        private PrintInfo printToZoom = null;

        protected void OnPrintCaptured(object sender, PrintCapturedEventArgs e)
        {
            //PrintCaptureAppLog.Logger.Trace(@"OnPrintCaptured ({0})", this.PrintList.GetName(e.Print));

            if (e.Print != null)
            {
                if (!e.KeepOriginalImage)
                {
                    e.Print.OriginalImage = e.Print.Image;
                }
                
                if (!e.SendBatch.HasValue && PrintCaptureApp.SequenceCheck != null)
                {
                    PrintCaptureApp.SequenceCheck.AddPrint(e.Print);
                }
                else
                {
                    this.batch.Add(e.Print);
                }
            }

            if (e.SendBatch == true)
            {
                PrintCaptureApp.SequenceCheck.AddPrintRange(this.batch);
                this.batch.Clear();
            }
        }

        public static PrintSettings Settings
        {
            get
            {
                var setting = PrintCaptureAppSettings.Default.LivescanSetting;

                if (setting == null)
                {
                    
                    setting = new PrintSettings();
                    setting.Rules.CaptureKind = PrintCaptureApp.Rules.CaptureKind;
                    setting.Rules.IsSequenceChangingPosition = false;
                    PrintCaptureAppSettings.Default.LivescanSetting = setting;
                }

                return setting;
            }
        }

        public static void LoadPluginAndDefaultDevice(PrintCaptureApp appInstance)
        {
            //PrintCaptureAppLog.Logger.Trace("LoadPluginAndDefaultDevice");
            var path = PrintCaptureApp.Rules.CaptureKind == CaptureKind.Cardscan
                ? PrintAppPath.CardscanPluginPath
                : PrintAppPath.LivescanPluginPath;
            try
            {
                var loader = new PluginLoader(path);
                loader.ResolvePlugins(appInstance);
            }
            catch (Exception ex)
            {
                //PrintCaptureAppLog.Logger.Error(ex, "Plugins could not be loaded.");
                
                PrintCaptureApp.ShowMessage("Plugins could not be loaded. " + ex.Message);
                //MessageBox.Show("Plugins could not be loaded. " + ex.Message);
                if (!instance.isWizardMode)
                {
                    //Application.Current.Shutdown(1);
                }
                    
                return;
            }
            

            var selectedKey = Settings.SelectedDeviceKey;
            if (selectedKey == DeviceCaptureDriver.GenericDeviceKey)
            {
                selectedKey = null;
            }
            var selectedDevice = appInstance.Sdks.GetDeviceFromKey(selectedKey);

            instance = new PrintCaptureDriver(PrintCaptureApp.Instance.PrintList);
            instance.baseDriver = GetDriverForDevice(selectedDevice);
            
            instance.baseDriver.ConfigureDefaultValue(appInstance.Sdks, selectedKey);

            appInstance.SetSelectedDevice(selectedKey);
        }

        public static bool SelectFirstPluggedDevice()
        {
            //PrintCaptureAppLog.Logger.Trace("SelectFirstPluggedDevice");
            var sel = PrintCaptureApp.Instance.SelectedDevice;
            if (sel != null)
            {
                // verify if plugged !
                var sdk = sel.Sdk;
                if (!sdk.IsOpened)
                {
                    sdk.Open();
                }
                var plugged = sdk.GetPluggedDevices();
                if (plugged?.FirstOrDefault(x => x.InternalKey == sel.InternalKey) != null)
                {
                    // Device set correctly, available and plugged !
                    return true;
                }                
            }

            // device not set, try to autodetect it !!
            var sdks = PrintCaptureApp.Instance.Sdks;
            foreach (var captureSdk in sdks)
            {                
                if (!captureSdk.IsOpened)
                {
                    captureSdk.Open();
                }
                var allPlugged = captureSdk.GetPluggedDevices();
                var plugged = allPlugged?.FirstOrDefault();

                if (plugged != null)
                {                        
                    PrintCaptureApp.Instance.SetSelectedDevice(plugged.InternalKey);
                    return true;
                }

                captureSdk.Close();
                
            }

            if (PrintCaptureApp.IsDebugMode)
            {
                var device = sdks.FirstOrDefault(x => x.IsVirtual)?.SupportedDeviceList.First();
                if (device != null)
                {
                    PrintCaptureApp.Instance.SetSelectedDevice(device.InternalKey);
                    return true;
                }
                
            }

            return false;            
        }

        public static PrintCaptureDriver Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = new PrintCaptureDriver(PrintCaptureApp.Instance.PrintList);

                var capture = GetDriverForDevice(PrintCaptureApp.Instance.SelectedDevice);

                instance.baseDriver = capture;

                return instance;
            }
        }

        //public void SetWizardMode(WizardProcessViewModel wizardViewModel)
        //{
        //    //PrintCaptureAppLog.Logger.Trace("SetWizardMode");
        //    var printOp = new PrintOperationService();
        //    this.isWizardMode = true;

        //    var wizCapture = new WizardCapture(
        //        PrintCaptureApp.Instance.SelectedDevice,
        //        PrintCaptureApp.Instance.PrintList,
        //        wizardViewModel);

        //    wizardViewModel.CaptureRequired += (sender, args) => {this.StartSession();
        //                                                                this.HandleScanResult(wizCapture.CaptureAuto());
        //    };

        //    wizardViewModel.ConfigurationRequired += (sender, args) =>
        //    {
        //        this.baseDriver.CaptureDevice.Close();
        //        //this.baseDriver.WaitForstate();
        //        var conf = new ConfigurationWindow();
        //        if (conf.ShowDialog() == true)
        //        {
        //            // set the new Device
        //            PrintCaptureApp.Instance.SetSelectedDevice(conf.DeviceConfiguration.SelectedDevice.Key.InternalKey);
        //            this.baseDriver.SetDevice(conf.DeviceConfiguration.SelectedDevice.Key);
        //        }

        //        wizardViewModel.TriggerConfigurationCompleted();
        //    };

        //    //wizardViewModel.CaptureStopped += (sender, args) => wizCapture.StopCapture();            

        //    this.baseDriver = wizCapture;
        //    this.baseDriver.PrintCaptured += instance.OnPrintCaptured;
        //    this.baseDriver.DeviceInitialized += instance.OnDeviceInitialized;
        //    this.baseDriver.CaptureCompleted += (o, e) =>
        //    {
                
        //        // When completed capture on Wizard, if there are errors, show Window to choose actions.
        //        var completed = e as CaptureCompletedEventArgs;
        //        if (completed?.HasError == true)
        //        {
        //            wizardViewModel.TriggerHideWindow();
        //            var conVm = new WizardConclusionViewModel(PrintCaptureApp.Instance.PrintList);
        //            var conWin = new WizardConclusionWindow(conVm);
        //            if (conWin.ShowDialog() == true)
        //            {
        //                wizardViewModel.Instructions = CommonText.ValidationTitle;
        //                wizardViewModel.TriggerShowWindow();
        //            }
        //            else
        //            {
        //                wizardViewModel.AcceptPrint = false;
        //                wizardViewModel.TriggerCloseWindowRequired();
        //            }
                    
        //            return;
        //        }
        //        wizardViewModel.AcceptPrint = true;
        //       instance.OnCaptureCompleted();
        //        wizardViewModel.CaptureCompleted();
        //    };
            
        //}

        private static DeviceCaptureDriver GetDriverForDevice(ICaptureDevice device)
        {
            //PrintCaptureAppLog.Logger.Trace("GetDriverForDevice");
            if (instance != null && instance.baseDriver != null)
            {
                instance.baseDriver.DeviceInitialized -= instance.OnDeviceInitialized;
                instance.baseDriver.PrintCaptured -= instance.OnPrintCaptured;
                instance.baseDriver.SettingsCorrupted -= instance.OnSettingsCorrupted;
            }

            DeviceCaptureDriver capture= new LivescanCapture(device, PrintCaptureApp.Instance.PrintList); ;
            
            capture.PrintCaptured += instance.OnPrintCaptured;
            capture.DeviceInitialized += instance.OnDeviceInitialized;
            capture.CaptureCompleted += (o, e) => instance.OnCaptureCompleted();
            capture.SettingsCorrupted += instance.OnSettingsCorrupted;
            return capture;
        }

        public ScanResult CaptureAuto()
        {
            //PrintCaptureAppLog.Logger.Trace("CaptureAuto");
            this.batch.Clear();
            this.printToZoom = null;

            if (PrintCaptureApp.Instance.SelectedDevice == null)
            {
                MessageBox.Show(Text.NoDevice);
                return ScanResult.NoDevice;                
            }

            try
            {
                //this.SetWindowEnabled(false);
                this.StartSession();
                return this.HandleScanResult(this.baseDriver.CaptureAuto());
            }
            catch (Exception ex)
            {
                //this.SetWindowEnabled(true);
                MessageBox.Show(CommonText.DeviceInternalError + "\n" + ex.Message);
                return ScanResult.Canceled;
            }
        }

        public ScanResult CaptureSingle(PrintInfo print)
        {
            //PrintCaptureAppLog.Logger.Trace(@"CaptureSingle ({0})", PrintList.GetName(print));
            this.batch.Clear();
            if (PrintCaptureApp.Instance.SelectedDevice == null)
            {
                MessageBox.Show(Text.NoDevice);
                return ScanResult.NoDevice;
            }

            this.printToZoom = print;

            //this.SetWindowEnabled(false);
            //PrintCaptureAppLog.Logger.Trace(@"Start session called");
            this.StartSession();

            //PrintCaptureAppLog.Logger.Trace(@"Calling base Driver  CaptureSingle ({0})", PrintList.GetName(print));
            return this.HandleScanResult(this.baseDriver.CaptureSingle(print));            
        }

        /// <summary>
        /// Capture positions OR Rescan in error prints
        /// </summary>
        /// <returns></returns>
        public ScanResult CapturePositions()
        {
            //PrintCaptureAppLog.Logger.Trace("CapturePosition");

            try
            {
                this.printToZoom = null;
                //this.SetWindowEnabled(false);                
                return this.HandleScanResult(this.baseDriver.CapturePositions());
            }
            catch (Exception ex)
            {
                //this.SetWindowEnabled(true);
                MessageBox.Show(CommonText.DeviceInternalError + "\n" + ex.Message);
                return ScanResult.Canceled;
            }
            
        }

        public void ConfigureDefaultValue(List<ICaptureSdk> sdkList, string selectedKey)
        {
            //PrintCaptureAppLog.Logger.Trace("ConfigureDefaultValues");
            this.baseDriver.ConfigureDefaultValue(sdkList, selectedKey);
        }

        public bool? Configure(DeviceConfigurationViewModel viewModel)
        {
            //PrintCaptureAppLog.Logger.Trace("Configure");
            var result = this.baseDriver.Configure(viewModel);
            if (result.HasValue && result.Value)
            {
                //if (this.CaptureDevice == null || viewModel.SelectedDeviceKey != this.CaptureDevice.InternalKey)
                //{
                    var newDevice = viewModel.Sdks.GetDeviceFromKey(viewModel.SelectedDeviceKey);
                //    this.CaptureDevice = newDevice;
                    this.baseDriver = GetDriverForDevice(newDevice);
                    Settings.SelectedDeviceKey = viewModel.SelectedDeviceKey;
               // }
            }

            return result;
        }

        public bool? DoOperation(PrintInfo print, PrintZoomAction op)
        {
            //PrintCaptureAppLog.Logger.Trace("DoOperation ({0} - {1}", PrintList.GetName(print), op);
            var result = this.baseDriver.DoOperation(print, op);            

            return result;
        }

        private void StartSession()
        {
            //PrintCaptureAppLog.Logger.Trace("StartSession");
            if (PrintCaptureApp.SequenceCheck.InSession)
            {
                return;
            }

            var dev = PrintCaptureApp.Instance.SelectedDevice;

            //if (dev == null)
            //{
            //    MessageBox.Show("Cannot start session without any device selected");
            //}

            //if (!dev.IsOpened)
            //{
            //    MessageBox.Show("Device must be opened before starting session");
            //}            

            
            try
            {
                PrintCaptureApp.SequenceCheck.StartSession();
            }
            catch (Exception ex)
            {
                //PrintCaptureAppLog.Logger.Error(ex, "PrintCaptureApp.SequenceCheck.StartSession");
                MessageBox.Show(CommonText.CannotStartPrintSession);
            }
            
        }

        public void RestartSession()
        {
            //PrintCaptureAppLog.Logger.Trace("RestartSession");
            var action = new Action(
                () =>
                {
                    var service = PrintCaptureApp.SequenceCheck;
                    service.ResetSession();
                    service.StartSession();

                    var list = this.PrintList.Prints.Where(x => x.Image != null).ToList();

                    foreach (var printInfo in list)
                    {
                        printInfo.ProcessStatus = PrintProcessStatus.InProcess;
                        PrintModificationDispatcher.PrintModified(printInfo);
                    }

                    service.AddPrintRange(list);
                });

            Task.Factory.StartNew(action);
        }

        //public void AdaptForCaptureKind(MainWindow main)
        //{
        //    //PrintCaptureAppLog.Logger.Trace("AdaptForCaptureKind");
        //    var subTitle = string.Empty;
            
        //    if (PrintCaptureApp.Rules.CaptureKind == CaptureKind.Livescan)
        //    {
        //        subTitle = Text.Livescan;
        //        main.SeqPositionCheckBox.Visibility = Visibility.Collapsed;
        //        main.ScanEndorsementButton.Visibility = Visibility.Visible;
        //        main.ResumeRibbonButton.Visibility = Visibility.Visible;
        //        main.ScannerInfoTitle.Text = Text.Resolutions;
        //        main.ConsentTextButton.Visibility = Visibility.Visible;
        //        main.RepositionRibbonButton.Visibility = Visibility.Collapsed;
        //        //main.ConfigureCaptureRibbonButton.LargeImageSource = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/LivescanDevice.png"));
        //        // Livescan
        //        // No Positon sequencing
        //        // Displays resolution inf
        //        // no set info
        //    }
        //    else
        //    {
        //        subTitle = Text.CardScan;                
        //        main.SeqPositionCheckBox.Visibility = Visibility.Visible;
        //        main.ScanEndorsementButton.Visibility = Visibility.Collapsed;
        //        main.ResumeRibbonButton.Visibility = Visibility.Collapsed;
        //        main.ScannerInfoTitle.Text = Text.SerialNumber;
        //        main.ConsentTextButton.Visibility = Visibility.Collapsed;
        //        main.RepositionRibbonButton.Visibility = Visibility.Visible;
        //        //main.ConfigureCaptureRibbonButton.LargeImageSource = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/CardscanDevice.png"));
        //        // Cardscan
        //        // No COnsent text
        //        // No Resume button
        //        // displays serial number info
        //    }

        //    main.Title = Text.ApplicationTitle + " - " + subTitle;
        //}

        private ScanResult HandleScanResult(ScanResult result)
        {
            //PrintCaptureAppLog.Logger.Trace(@"HandleScanResult called");
            if (!(result == ScanResult.Completed || result == ScanResult.InProgress))
            {
                //this.SetWindowEnabled(true);
            }            

            return result;
        }

        private void OnSettingsCorrupted(object sender, EventArgs e)
        {
            //AppSettings.DeleteSettings<LivescanSettings>(PrintCaptureApp.AppName);
        }

        private void OnDeviceInitialized(object sender, EventArgs e)
        {
            //PrintCaptureAppLog.Logger.Trace("OnDeviceInitialized");
            var dev = PrintCaptureApp.Instance.SelectedDevice;

            var captureDevice = new CaptureDeviceInfo
            {
                Make = dev.HardwareMake,
                ModelName = dev.ModelName,
                SerialNumber = dev.SerialNumber,
                Kind = dev.Sdk.DeviceKind
            };

            PrintCaptureApp.SequenceCheck.CaptureDevice = captureDevice;
        }

        private void OnCaptureCompleted()
        {
            //this.SetWindowEnabled(true);
            //var validation = new PalmUserValidationService();
            //validation.VerifyQuality(this.PrintList);

            //if (this.printToZoom != null)
            //{
            //    var printOp = new PrintOperationService();
            //    printOp.ProcessZoomWindow(new PrintElementViewModel(this.printToZoom));                
            //}
        }

        //private void SetWindowEnabled(bool isEnabled)
        //{
        //    //PrintCaptureAppLog.Logger.Trace(@"SetWindowEnabled {0}", isEnabled);
        //    if (this.isWizardMode)
        //    {
        //        return;
        //    }

        //    var w = PrintCaptureApp.GetWindowByTag(PrintCaptureApp.MainWindowTag);
            
        //    if (w == null)
        //    {
        //        return;
        //    }

        //    w.IsEnabled = isEnabled;
        //}

        public string GetDeviceSecondaryInfo()
        {
            return this.baseDriver.GetDeviceSecondaryInfo();
        }
    }
}