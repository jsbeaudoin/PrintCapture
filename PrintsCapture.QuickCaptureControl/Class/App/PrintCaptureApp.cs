namespace PrintsCapture.QuickCaptureControl.Class.App
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Prints.Sequence;
    using PrintsCapture.QuickCaptureControl.Languages;
    using Device;

    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;

    public class PrintCaptureApp
    {
        public const string AppVersion = "1.0.1.0";

        public const string AppName = "QuickCaptureControl";

        public const string MainWindowTag = "Main";


        #region Static Members        

        private static CultureInfo culture;
        
        private static PrintCaptureApp instance;

        private static CaptureKind captureKind;

        private BaseSeqCheckService seqCheckService;

        //private MainViewModel mainViewModel;

        //private WizardProcessViewModel wizardViewModel;               

        private PrintValidationChangedDelayed validationChangedTrigger;

        public static CultureInfo ApplicationCulture
        {
            get { return culture; }
            set
            {
                if (culture != null && culture.Equals(value))
                {
                    return;
                }

                culture = value;
                Text.Culture = culture;
                CommonText.Culture = culture;

            }
        }

        public static PrintCaptureApp Instance => instance;

        //public static bool HasWizardAcceptedPrint => instance.wizardViewModel?.AcceptPrint != null && instance.wizardViewModel.AcceptPrint.Value;

        public static bool IsWizard => instance?.IsWizardMode ?? false;

        public static BaseSeqCheckService SequenceCheck => instance.seqCheckService;

        public static PrintRules Rules => instance.PrintList.Rules;

        public static bool IsDebugMode => instance.IsDebugAvailable;


        public static PrintCaptureApp Start(PrintCaptureAppParameter appParam)
        {
            //PrintCaptureAppLog.Logger.Debug("PrintCaptureApp Start");
            //PrintCaptureAppLog.Logger.Info($"Prints allowed:{appParam.CaptureModeAllowed}, Mode:{appParam.Mode}, Options:{appParam.IsOptionAvailable}");
            instance = GetInstance(appParam);
            if (instance == null)
            {
                return null;
            }
            instance.seqCheckService = appParam.SeqCheckService;

            // Setup prints and rules
            instance.PrintList = appParam.PrintList;
            var rules = PrintCaptureDriver.Settings.Rules;
            
            // make sure rules have a valid set
            rules.CaptureGroupAllowed = appParam.CaptureModeAllowed;

            if (!rules.CaptureGroupAllowed.HasFlag(rules.CaptureGroup))
            {
                
                var defaultValue = rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.FlatOnly) ? PrintCaptureGroup.FlatOnly
                    : PrintCaptureGroup.Standard14;
                if (!rules.CaptureGroupAllowed.HasFlag(defaultValue))
                {
                    var firstValue = rules.CaptureGroupAllowed.GetValues().Cast<PrintCaptureGroup>().FirstOrDefault();
                    //PrintCaptureAppLog.Logger.Info($"PrintCaptureApp No capture flag set, setting value {firstValue}");
                    rules.CaptureGroup = firstValue;
                }
                else
                {
                    //PrintCaptureAppLog.Logger.Info($"PrintCaptureApp No capture flag set, setting value {defaultValue}");
                    rules.CaptureGroup = defaultValue;
                }
                                
            }
            
            rules.IsEndorsementAllowed = appParam.IsEndorsementAllowed;

            instance.PrintList.Rules = rules;
            instance.validationChangedTrigger = new PrintValidationChangedDelayed(instance.PrintList);
            instance.validationChangedTrigger.DelayedRuleChanged += RulesOnDelayedRuleChanged;  
            PrintModificationDispatcher.AddWatch(PrintModified);            

            // Get Sdk list ...
            PrintCaptureDriver.LoadPluginAndDefaultDevice(instance);
            //SplashWindowHelper.Show();

            //PrintCaptureAppLog.Logger.Debug("PrintCaptureApp - Initialize Sequence service");
            InitializeService(appParam.SeqCheckService);                       

            if (instance.IsWizardMode)
            {
                //PrintCaptureAppLog.Logger.Debug("PrintCaptureApp - QuickFlat Mode");

                if (!PrintCaptureDriver.SelectFirstPluggedDevice())
                {
                    //LogDispatcher.DoLog("No Device found for Wizard", LogEventLevel.Warning);
                    //MessageBox.Show( CommonText.DeviceNotFound, Text.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);                    
                    //instance.CloseApplication(false); // This does not close window for Wizard
                    //Application.Current.Shutdown();
                    return null;
                }
                // Wizard is not shown immediately, the option window is shown first.
            }

            return instance;
        }

        /// <summary>
        ///  Wizard must open form manually
        /// </summary>
        public static bool OpenMainFormDialog()
        {
            if (instance.IsWizardMode)
            {
                return DisplayWizard();
            }

            return true;
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }


        public static void ConfigureDevice()
        {
            //PrintCaptureAppLog.Logger.Trace("Configure Device");
            var dv = new DeviceConfigurationViewModel
            {
                CaptureKind = instance.PrintList.Rules.CaptureKind,
                Sdks = instance.Sdks,
                SelectedDeviceKey = instance.SelectedDevice == null ? string.Empty : instance.SelectedDevice.InternalKey
            };


            var result = PrintCaptureDriver.Instance.Configure(dv);

            if (!(result.HasValue && result.Value))
            {
                return;
            }

            instance.SetSelectedDevice(dv.SelectedDeviceKey);
        }

        private static PrintCaptureApp GetInstance(PrintCaptureAppParameter appParam)
        {
            if (instance != null)
            {
                return instance;
            }

            if (appParam == null)
            {
                ShowMessage(CommonText.AppCantStartDirectly);
                //Application.Current.Shutdown(1);
                return null;
            }

            instance = new PrintCaptureApp(appParam);
            
            captureKind = appParam.Mode == @"livescan" ? CaptureKind.Livescan : CaptureKind.Cardscan;

            //PrintCaptureAppLog.Logger.Debug("PrintCaptureApp mode ({0})", captureKind);
            //instance.mainViewModel = new MainViewModel();

            return instance;
        }

        

        private static void InitializeService(BaseSeqCheckService seqService)
        {
            // Get Sequence check service !
            //PrintCaptureAppLog.Logger.Debug("Connecting to Biometric Service");

            var service = seqService;
            service.PrintModified += ServiceOnPrintModified;
            service.ServiceException += ServiceOnServiceException;            

            service.ServiceConnected += (s, e) =>
            {
                //instance.mainViewModel.ServiceConnected = e.Connected;                
            };
            service.ServiceDisconnected += (s, e) =>
            {
                //instance.mainViewModel.ServiceConnected = e.Connected;                
            };

            service.Connect();
        }

        private static bool DisplayWizard()
        {
            //var pl = instance.PrintList;
            ////PrintCaptureAppLog.Logger.Debug("Display Wizard");

            ////var wizardVm = new WizardProcessViewModel
            ////{
            ////    EndorsementVisibility = Rules.IsEndorsementAllowed
            ////        ? Visibility.Visible
            ////        : Visibility.Collapsed,
            ////    AppVersionLabel = string.Format(CommonText.AppVersionDisplay, AppVersion)
            ////};

            ////instance.wizardViewModel = wizardVm;
            //var dev = instance.SelectedDevice as ILivescanDevice;

            //if (dev == null)
            //{
            //    return false;
            //}
            
            //dev.FingerResolution = PrintResolution.Dpi500;            

            ////wizardVm.DeviceInformation = $@"Dpi : {dev.FingerResolution.ToDpi()}";
            ////wizardVm.DeviceName = dev.DisplayName;

            //var capture = PrintCaptureDriver.Instance;
            //capture.SetWizardMode(wizardVm);

            
            //// no option window for single finger
            //if (pl.Rules.CaptureGroup != PrintCaptureGroup.OneFingerOnly)
            //{
            //    SplashWindowHelper.Hide();
            //    // ------------------------------------------
            //    // show option window
            //    var opVm = new WizardOptionViewModel(pl);
            //    var optWin = new WizardOptionWindow(opVm);
            //    optWin.Loaded += (sender, args) => SplashWindowHelper.Hide();

            //    if (optWin.ShowDialog() != true)
            //    {
            //        return false;
            //    }
                
            //    pl.Rules.CaptureGroup = opVm.CaptureGroup;               
            //}
            

            //// ------------------------------------------

            ////var win = new QuickFlatProcessWindow(quick);
            //var win = new WizardCaptureWindow(wizardVm);
            //win.Loaded += (o, e) =>
            //{
            //    win.Topmost = false;
            //    SplashWindowHelper.Hide();
            //};
            //win.Closing += WinOnClosing;
            //win.Closed += (sender, args) => MainWindowOnClosed(instance, EventArgs.Empty);
            
            //// CaptureWindow owner = first window
            //var openForms = System.Windows.Forms.Application.OpenForms;

            //if (openForms.Count > 0)
            //{
            //    var form = openForms[0];
            //    new System.Windows.Interop.WindowInteropHelper(win).Owner = form.Handle;
            //}

            //if (!win.IsVisible)
            //{
            //    win.ShowDialog();
            //}
            

            return true;
        }

        private static void FillMainViewModel()
        {
            //PrintCaptureAppLog.Logger.Debug("Filling main view model");                          

            //var rules = new RulesViewModel(instance.PrintList.Rules);
            //instance.mainViewModel.Rules = rules;

            //rules.PropertyChanged += RulesOnPropertyChanged;

            //var printGridVm = new PrintGridViewModel(Rules.CaptureKind);
            //printGridVm.Update(instance.PrintList.GetAllViewModels());

            //instance.mainViewModel.PrintGridViewModel = printGridVm;
            //instance.mainViewModel.IdLine1 = instance.Line1;
            //instance.mainViewModel.IdLine2 = instance.Line2;

            //// Apply initial values
            //instance.mainViewModel.IsEndButtonAvailable = true;
            //SequenceCheck.IsDataCompressed = rules.IsDataCompressed;
            //printGridVm.ShowPalmPrint = rules.CaptureMode == PrintCaptureGroup.StandardAndPalm;
            //printGridVm.ShowRolledPrint = !rules.IsFlatCaptureMode;
            //printGridVm.ShowMissingLine = rules.IsFlatCaptureMode;
            //printGridVm.CaptureTwoThumbs = rules.IsFlatCaptureMode;

            //// endorsement
            //instance.mainViewModel.EndorsableFingers = instance.PrintList.GetEndorsableFingers();
        }

        private static void RulesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //var rulesVm = (RulesViewModel)sender;
            //var rules = instance.PrintList.Rules;
            //var validations = instance.validationChangedTrigger;

            //// quality and validations options are changed by a delayed class that allow some time before triggering the 
            ////  viewmodel updates.
            //bool changed = (validations.IsSequenceEnabled != rulesVm.IsSequenceEnabled) || (validations.IsSequencePositionEnabled != rulesVm.IsSequencePositionEnabled);

            //validations.IsSequenceEnabled = rulesVm.IsSequenceEnabled;
            //validations.SequenceThreshold = rulesVm.SequenceThreshold;

            //validations.IsSequencePositionEnabled = rulesVm.IsSequencePositionEnabled;
            //validations.IsQualityEnabled = rulesVm.IsQualityEnabled;
            //validations.QualityThreshold = rulesVm.QualityThreshold;

            //rules.IsOverrideFlatForbidden = rulesVm.IsOverrideFlatForbidden;
            //rules.IsOverrideRolledForbidden = rulesVm.IsOverrideRolledForbidden;
            //rules.RetryNeededForOverride = rulesVm.RetryNeededForOverride;
            //rules.IsOverrideAlwaysShown = rulesVm.IsOverrideAlwaysShown;
            //rules.IsDataCompressed = rulesVm.IsDataCompressed;
            //rules.CropTolerance = rulesVm.CropTolerance;

            //SequenceCheck.MinimumMinutiaCount = rulesVm.MinimumMinutiaCount;

            //SequenceCheck.IsDataCompressed = rulesVm.IsDataCompressed;
            //instance.PrintList.Rules.CaptureGroup = rulesVm.CaptureMode;

            //instance.mainViewModel.PrintGridViewModel.CaptureTwoThumbs = rules.CaptureTwoThumbs;

            //instance.mainViewModel.PrintGridViewModel.ShowPalmPrint = rulesVm.CaptureMode
            //                                                          == PrintCaptureGroup.StandardAndPalm;
            //instance.mainViewModel.PrintGridViewModel.ShowRolledPrint = rulesVm.CaptureMode
            //                                                            != PrintCaptureGroup.FlatOnly;

            //instance.mainViewModel.PrintGridViewModel.ShowMissingLine = rulesVm.IsFlatCaptureMode;

            //if (changed)
            //{
            //    SequenceCheck.SequencePositionRuleChanged();
            //}

        }

        //private static void OpenMainForm()
        //{
        //    //PrintCaptureAppLog.Logger.Debug("Opening main form");
        //    var win = new MainWindow(instance.mainViewModel)
        //    {
        //        FlatToggleButton = {IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.FlatOnly)},
        //        StdAndPalmToggleButton =
        //        {
        //            IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.StandardAndPalm)
        //        },
        //        Std14ToggleButton = {IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.Standard14)},
        //        OptionRibbonTab = {IsEnabled = instance.IsOptionAvailable},
        //        DebugRibbonTab = {IsEnabled = instance.IsDebugAvailable},
        //        EndorsementRibbonGroup =
        //        {
        //            Visibility = Rules.IsEndorsementAllowed
        //                ? Visibility.Visible
        //                : Visibility.Collapsed
        //        }
        //    };

        //    win.Closing += WinOnClosing;
        //    win.Closed += MainWindowOnClosed;
        //    win.Loaded += (o, e) =>
        //    {
        //        win.Topmost = false;
        //        SplashWindowHelper.Hide();
        //        PrintCaptureDriver.Instance.AdaptForCaptureKind(win);
        //    };
        //    win.Show();
        //}

        private static void RulesOnDelayedRuleChanged(object sender, EventArgs eventArgs)
        {
            //instance.mainViewModel.PrintGridViewModel.Update(instance.PrintList.GetAllViewModels());
        }

        private static void ServiceOnServiceException(object sender, SequenceCheckExceptionEventArgs e)
        {           
            //PrintCaptureAppLog.Logger.Error(e.Exception, "Service exception");
            var msg = (@"Service Exception. ") + e.Exception.Message;

            var innerEx = e.Exception.InnerException;
            while (innerEx != null)
            {
                msg += Environment.NewLine + innerEx.Message;
                innerEx = innerEx.InnerException;
            }

            ShowMessage(msg);           
        }        

        private static void ServiceOnPrintModified(object sender, PrintModifiedEventArgs e)
        {            
            PrintModificationDispatcher.PrintModified(e.Info);
        }

        private static void PrintModified(object sender, PrintModifiedEventArgs e)
        {
            if (instance.IsWizardMode) return;

            var vm = instance.PrintList.GetViewModel(e.Info);
            //instance.mainViewModel.PrintGridViewModel.Update(vm);
            //instance.mainViewModel.IsEndButtonAvailable = instance.PrintList.Prints.All(x => !x.IsInError);
        }

        #endregion

        #region Non static Members

        private PrintCaptureApp(PrintCaptureAppParameter appParameter)
        {
            this.IcdVersion = appParameter.IcdVersion;
            this.IsWizardMode = appParameter.IsWizardMode;
            this.IsDebugAvailable = appParameter.IsDebugAvailable;            
            this.IsOptionAvailable = appParameter.IsOptionAvailable;
            this.Line1 = appParameter.DescriptionLine1;
            this.Line2 = appParameter.DescriptionLine2;
            //LogDispatcher.DoLog($"PrintCaptureApp instance created.\nIcd:{this.IcdVersion}, IsWizard:{this.IsWizardMode}, IsDebug:{this.IsDebugAvailable}");
        }

        bool IsOptionAvailable { get;  }        

        bool IsDebugAvailable { get; }

        bool IsWizardMode { get; }

        string Line1 { get; }

        string Line2 { get; }

        

        [ImportMany]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public List<ICaptureSdk> Sdks { get; private set; }

        public PrintList PrintList { get; private set; }

        public ICaptureDevice SelectedDevice { get; private set; }

        public string IcdVersion { get; private set; }

        public void SetSelectedDevice(string internalKey)
        {
            var defaultDevice = instance.Sdks.GetDeviceFromKey(internalKey);

            this.SelectedDevice = defaultDevice;
            if (this.SelectedDevice == null)
            {
                //this.mainViewModel.ScannerName = Text.NoDevice;
                //this.mainViewModel.ScannerInfo = string.Empty;
                //this.mainViewModel.ScannerImage = new BitmapImage(new Uri(@"pack://application:,,,/PrintsCapture.Ui;component/Images/Setting32x32.png"));
            }
            else
            {
                //this.mainViewModel.ScannerName = this.SelectedDevice.DisplayName;
                //this.mainViewModel.ScannerInfo = PrintCaptureDriver.Instance.GetDeviceSecondaryInfo();
                //this.mainViewModel.ScannerImage = new BitmapImage(new Uri(this.SelectedDevice.ImageUri));
                this.seqCheckService.CaptureDevice = new CaptureDeviceInfo
                {
                    Make = defaultDevice.HardwareMake,
                    ModelName = defaultDevice.ModelName,
                    SerialNumber = defaultDevice.SerialNumber,
                    Kind = defaultDevice.Sdk.DeviceKind
                };
            }


        }

        public event ScanCompletedHandler ScanCompleted;


        private void CloseApplication(bool success)
        {
            //PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication. Success={0}", success);            
            //SplashWindowHelper.SetMessage(@". . .", false);
            //SplashWindowHelper.Show();            

            PrintModificationDispatcher.ClearAll();
            string setId = string.Empty;
            if (this.seqCheckService != null)
            {
                //PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - EndSession");
                setId = this.seqCheckService.EndSession(success);
                this.seqCheckService = null;
            }

            var result = new ScanCompletedEventArgs(success, setId);

            if (instance.SelectedDevice != null && instance.SelectedDevice.IsOpened)
            {
                //PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - SelectedDevice.Close");
                instance.SelectedDevice.Close();
                instance.SelectedDevice = null;
            }

            //PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - ScanCompleted handler");
            this.ScanCompleted?.Invoke(this, result);            

            // No App closing if on Wizard mode
            //if (Application.Current != null && !this.IsWizardMode)
            //{
            //    Application.Current.Shutdown();
            //}
            //PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - Done.");
        }

        #endregion
    }
}
