using System.Globalization;
using NLog;
using PrintsCapture.Ui.Extension;
using PrintsCapture.Ui.View;
using PrintsCapture.Ui.ViewModel.Wizard;
using XL_ID.Utilities.Log;
using XL_ID.Utilities.Wpf.WindowHelper;

namespace PrintsCapture.Ui.Class
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using Device;
    using Device.Extension;
    using Device.Interface;
    using Livescan.View;
    using Livescan.ViewModel;
    using Prints;
    using Prints.Enum;
    using Prints.Language;
    using Prints.Sequence;
    using Print;        
    using ViewModel;
    using Language;

    
    

    public class PrintCaptureApp
    {
        public const string AppVersion = "1.0.55.7";

        public const string AppName = "PrintsCapture";

        public const string MainWindowTag = "Main";


        #region Static Members        

        private static CultureInfo culture;
        
        private static PrintCaptureApp instance;

        private static CaptureKind captureKind;

        private BaseSeqCheckService seqCheckService;

        private MainViewModel mainViewModel;

        private WizardProcessViewModel wizardViewModel;               

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

        public static bool HasWizardAcceptedPrint { get; private set; } // => instance.wizardViewModel?.AcceptPrint != null && instance.wizardViewModel.AcceptPrint.Value;

        public static bool IsWizard => instance?.IsWizardMode ?? false;

        public static BaseSeqCheckService SequenceCheck => instance.seqCheckService;

        public static PrintRules Rules => instance.PrintList.Rules;

        public static bool IsDebugMode => instance.IsDebugAvailable;

        public static PrintCaptureApp Start(PrintCaptureAppParameter appParam)
        {
            HasWizardAcceptedPrint = false;
            PrintCaptureAppLog.Logger.Debug("***********PrintCaptureApp Start***********");
            PrintCaptureAppLog.Logger.Info($"Prints allowed:{appParam.CaptureModeAllowed}, Mode:{appParam.Mode}, Options:{appParam.IsOptionAvailable}");

            PrintCaptureAppLog.Logger.Info($"GetInstance Start (hh:mm:ss:ff): {DateTime.Now.ToString("HH:mm:ss:ff")}");
            var startTime = DateTime.Now;
            instance = GetInstance(appParam);
            if (instance == null)
            {
                return null;
            }

            var duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info($"SeqCheckService Process Time (mm:ss:ff): {duration.ToString("mm\\:ss\\:ff")}");
            instance.seqCheckService = appParam.SeqCheckService;

            // Setup prints and rules
            instance.PrintList = appParam.PrintList;
            var rules = PrintCaptureDriver.Settings.Rules;
            rules.OrderMode = appParam.CaptureOrder;
            rules.Labels.SingleFingerCapturePrompt = appParam.SingleFingerCapturePrompt;
            
            // make sure rules have a valid set
            rules.CaptureGroupAllowed = appParam.CaptureModeAllowed;            
            var defaultValue = PrintCaptureGroup.FlatOnly; // default: flats. Else rolled and flats EXCEPT for Sq, which include palms by default
            if (!rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.FlatOnly))
            {
                defaultValue = appParam.CaptureOrder == CaptureOrderMode.Sq ? PrintCaptureGroup.StandardAndPalm : PrintCaptureGroup.Standard14;
            }

            if (!rules.CaptureGroupAllowed.HasFlag(defaultValue))
            {
                var firstValue = rules.CaptureGroupAllowed.GetValues().Cast<PrintCaptureGroup>().FirstOrDefault();
                PrintCaptureAppLog.Logger.Info($"PrintCaptureApp, setting CaptureGroup value {firstValue}");
                rules.CaptureGroup = firstValue;
            }
            else
            {
                PrintCaptureAppLog.Logger.Info($"PrintCaptureApp, setting CaptureGroup value {defaultValue}");
                rules.CaptureGroup = defaultValue;
            } 
            rules.IsEndorsementAllowed = appParam.IsEndorsementAllowed;
            if (appParam.ForceQualityValidationEnabled.HasValue)
            {
                rules.IsQualityEnabled = appParam.ForceQualityValidationEnabled.Value;
            }
            if (appParam.ForceQualityThreshold.HasValue)
            {
                rules.QualityThreshold = appParam.ForceQualityThreshold.Value;
            }

            instance.PrintList.Rules = rules;
            instance.validationChangedTrigger = new PrintValidationChangedDelayed(instance.PrintList);
            instance.validationChangedTrigger.DelayedRuleChanged += RulesOnDelayedRuleChanged;  
            PrintModificationDispatcher.AddWatch(PrintModified);

            // Get Sdk list ...
            PrintCaptureDriver.LoadPluginAndDefaultDevice(instance);
            SplashWindowHelper.Show();

            PrintCaptureAppLog.Logger.Debug($"PrintCaptureApp - Initialize Sequence service. (hh:mm:ss:ff) ={DateTime.Now.ToString("HH:mm:ss:ff")}");
            InitializeService(appParam.SeqCheckService);

            if (instance.IsWizardMode || instance.IsLoginMode )
            {
                PrintCaptureAppLog.Logger.Debug("PrintCaptureApp - QuickFlat Mode");

                PrintCaptureAppLog.Logger.Info($"SelectFirstPluggedDevice Start (hh:mm:ss:ff):{DateTime.Now.ToString("HH:mm:ss:ff")}");
                startTime = DateTime.Now;
                if (!PrintCaptureDriver.SelectFirstPluggedDevice())
                {
                    
                    LogDispatcher.DoLog("No Device found for Wizard", LogEventLevel.Warning);
                    MessageBox.Show( CommonText.DeviceNotFound, Text.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);                    
                    instance.CloseApplication(false); // This does not close window for Wizard
                    Application.Current.Shutdown();
                    return null;
                }
                duration = DateTime.Now.Subtract(startTime);
                PrintCaptureAppLog.Logger.Info($"SelectFirstPluggedDevice Process time(mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");
                // Wizard is not shown immediately, the option window is shown first.
            }
            else
            {
                PrintCaptureAppLog.Logger.Debug("PrintCaptureApp - Regular Mode");
                try
                {
                    FillMainViewModel();                                    
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex, "Could not Fill Main View Model");
                    return null;
                }

                PrintCaptureAppLog.Logger.Debug("Opening mainform");
                Application.Current.Dispatcher.Invoke(new Action(() => OpenMainForm(appParam)));
            }            

            return instance;
        }

        /// <summary>
        ///  Wizard must open form manually
        /// </summary>
        public static bool OpenMainFormDialog()
        {
            if (instance.IsWizardMode || instance.IsLoginMode)
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
            PrintCaptureAppLog.Logger.Trace("Configure Device");
            var dv = new DeviceConfigurationViewModel
            {
                CaptureKind = instance.PrintList.Rules.CaptureKind,
                Sdks = instance.Sdks,
                SelectedDeviceKey = instance.SelectedDevice == null ? string.Empty : instance.SelectedDevice.InternalKey
            };
            PrintCaptureAppLog.Logger.Trace($"SDKs Count: {dv.Sdks.Count}");
            foreach (var captureSdk in dv.Sdks)
            {
                PrintCaptureAppLog.Logger.Trace($"{captureSdk} Supported Device List Count: {captureSdk.SupportedDeviceList.Count()}");
            }

            var result = PrintCaptureDriver.Instance.Configure(dv);

            if (!(result.HasValue && result.Value))
            {
                return;
            }

            instance.SetSelectedDevice(dv.SelectedDeviceKey);
        }

        public static Window GetWindowByTag(string tag)
        {
            var a = Application.Current;
            if (a.Dispatcher.CheckAccess())
            {
                return a.Windows.Cast<Window>().FirstOrDefault(x => x.Tag != null && x.Tag.Equals(tag));
            }

            Window result = null;

            a.Dispatcher.Invoke(
                new Action(
                    () => result = a.Windows.Cast<Window>().FirstOrDefault(x => x.Tag != null && x.Tag.Equals(tag))));

            return result;
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
                Application.Current.Shutdown(1);
                return null;
            }

            instance = new PrintCaptureApp(appParam);
            
            captureKind = appParam.Mode == @"livescan" ? CaptureKind.Livescan : CaptureKind.Cardscan;

            PrintCaptureAppLog.Logger.Debug("PrintCaptureApp mode ({0})", captureKind);
            instance.mainViewModel = new MainViewModel();

            return instance;
        }

        

        private static void InitializeService(BaseSeqCheckService seqService)
        {
            // Get Sequence check service !
            PrintCaptureAppLog.Logger.Debug("Connecting to Biometric Service");

            var service = seqService;
            service.PrintModified += ServiceOnPrintModified;
            service.ServiceException += ServiceOnServiceException;            

            service.ServiceConnected += (s, e) =>
            {
                instance.mainViewModel.ServiceConnected = e.Connected;                
            };
            service.ServiceDisconnected += (s, e) =>
            {
                instance.mainViewModel.ServiceConnected = e.Connected;                
            };

            service.Connect();
        }

        private static bool DisplayWizard()
        {
            var startTime = DateTime.Now;
            var pl = instance.PrintList;
            PrintCaptureAppLog.Logger.Debug("Display Wizard");
            

            var wizardVm = new WizardProcessViewModel
            {
                EndorsementVisibility = Rules.IsEndorsementAllowed
                    ? Visibility.Visible
                    : Visibility.Collapsed,
                AppVersionLabel = string.Format(CommonText.AppVersionDisplay, AppVersion),
                IsLoginMode = instance.IsLoginMode,
                TopMostWindow = instance.TopMostWindow,
                AlwaysCanOverride = instance.AlwaysCanOverrideWizard
            };

            var duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info(
                $"WizardProcessViewModel Creation Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");

            instance.wizardViewModel = wizardVm;

            startTime = DateTime.Now;
            var dev = instance.SelectedDevice as ILivescanDevice;
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info(
                $"SelectedDevice Creation (ILivescanDevice) Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");


            if (dev == null)
            {
                return false;
            }
            
            dev.FingerResolution = PrintResolution.Dpi500;            

            wizardVm.DeviceInformation = $@"Dpi : {dev.FingerResolution.ToDpi()}";
            wizardVm.DeviceName = dev.DisplayName;

            var capture = PrintCaptureDriver.Instance;

            startTime = DateTime.Now;
            capture.SetWizardMode(wizardVm);
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info(
                $"Setting Wizard Mode Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");
            
            // no option window for single finger
            if (pl.Rules.CaptureGroup != PrintCaptureGroup.OneFingerOnly)
            {
                startTime = DateTime.Now;
                SplashWindowHelper.Hide();
                // ------------------------------------------
                // show option window
                var opVm = new WizardOptionViewModel(pl){TopMostWindow = wizardVm.TopMostWindow};
                var optWin = new WizardOptionWindow(opVm);
                optWin.Loaded += (sender, args) => SplashWindowHelper.Hide();

                duration = DateTime.Now.Subtract(startTime);
                PrintCaptureAppLog.Logger.Info(
                    $"Setting Wizard Option From. Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");

                if (optWin.ShowDialog() != true)
                {
                    return false;
                }
                
                pl.Rules.CaptureGroup = opVm.CaptureGroup;               
            }
            

            // ------------------------------------------

            startTime = DateTime.Now;
            var win = new WizardCaptureWindow(wizardVm);
            win.Loaded += (o, e) =>
            {
                win.Topmost = false;
                SplashWindowHelper.Hide();
                win.Topmost = wizardVm.TopMostWindow;
            };
            win.Closing += WinOnClosing;
            win.Closed += (sender, args) => MainWindowOnClosed(instance, EventArgs.Empty);
            
            // CaptureWindow owner = first window
            var openForms = System.Windows.Forms.Application.OpenForms;

            if (openForms.Count > 0)
            {
                var index = 0;
                var tryAgain = true;
                while (tryAgain) {
                    tryAgain = false;
                    try
                    {
                        var form = openForms[0];
                        new System.Windows.Interop.WindowInteropHelper(win).Owner = form.Handle;
                    }
                    catch (Exception ex)
                    {
                        index++;
                        tryAgain = index < openForms.Count;
                    }
                }
                
                
            }

            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info(
                $"Setting Wizard From. Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");

            if (!win.IsVisible)
            {
                win.ShowDialog();
            }
            

            return true;
        }

        private static void FillMainViewModel()
        {
            PrintCaptureAppLog.Logger.Debug("Filling main view model");                          

            var rules = new RulesViewModel(instance.PrintList.Rules);
            instance.mainViewModel.Rules = rules;            
            
            rules.PropertyChanged += RulesOnPropertyChanged;

            var printGridVm = new PrintGridViewModel(Rules.CaptureKind);            
            printGridVm.Update(instance.PrintList.GetAllViewModels());

            instance.mainViewModel.PrintGridViewModel = printGridVm;
            instance.mainViewModel.IdLine1 = instance.Line1;
            instance.mainViewModel.IdLine2 = instance.Line2;

            // Apply initial values
            instance.mainViewModel.IsEndButtonAvailable = true;
            SequenceCheck.IsDataCompressed = rules.IsDataCompressed;
            printGridVm.ShowPalmPrint = rules.CaptureMode == PrintCaptureGroup.StandardAndPalm;
            printGridVm.ShowRolledPrint = ! rules.IsFlatCaptureMode;
            printGridVm.ShowMissingLine = rules.IsFlatCaptureMode;

            // endorsement
            instance.mainViewModel.EndorsableFingers = instance.PrintList.GetEndorsableFingers();            
        }        

        private static void RulesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var rulesVm = (RulesViewModel)sender;
            var rules = instance.PrintList.Rules;
            var validations = instance.validationChangedTrigger;

            // quality and validations options are changed by a delayed class that allow some time before triggering the 
            //  viewmodel updates.
            bool changed = (validations.IsSequenceEnabled != rulesVm.IsSequenceEnabled) || (validations.IsSequencePositionEnabled != rulesVm.IsSequencePositionEnabled);

            validations.IsSequenceEnabled = rulesVm.IsSequenceEnabled;
            validations.SequenceThreshold = rulesVm.SequenceThreshold;
            
            validations.IsSequencePositionEnabled = rulesVm.IsSequencePositionEnabled;
            validations.IsQualityEnabled = rulesVm.IsQualityEnabled;
            validations.QualityThreshold = rulesVm.QualityThreshold;

            rules.IsOverrideFlatForbidden = rulesVm.IsOverrideFlatForbidden;
            rules.IsOverrideRolledForbidden = rulesVm.IsOverrideRolledForbidden;
            rules.RetryNeededForOverride = rulesVm.RetryNeededForOverride;
            rules.IsOverrideAlwaysShown = rulesVm.IsOverrideAlwaysShown;
            rules.IsDataCompressed = rulesVm.IsDataCompressed;
            rules.CropTolerance = rulesVm.CropTolerance;

            SequenceCheck.MinimumMinutiaCount = rulesVm.MinimumMinutiaCount;

            SequenceCheck.IsDataCompressed = rulesVm.IsDataCompressed;
            instance.PrintList.Rules.CaptureGroup = rulesVm.CaptureMode;

            instance.mainViewModel.PrintGridViewModel.ShowPalmPrint = rulesVm.CaptureMode
                                                                      == PrintCaptureGroup.StandardAndPalm;
            instance.mainViewModel.PrintGridViewModel.ShowRolledPrint = rulesVm.CaptureMode
                                                                        != PrintCaptureGroup.FlatOnly;

            instance.mainViewModel.PrintGridViewModel.ShowMissingLine = rulesVm.IsFlatCaptureMode;

            if (changed)
            {
                SequenceCheck.SequencePositionRuleChanged();
            }

        }

        private static void OpenMainForm(PrintCaptureAppParameter appParams)
        {
            PrintCaptureAppLog.Logger.Debug("Opening main form");
            var win = new MainWindow(instance.mainViewModel)
            {
                FlatToggleButton = {IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.FlatOnly)},
                StdAndPalmToggleButton =
                {
                    IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.StandardAndPalm)
                },
                Std14ToggleButton = {IsEnabled = Rules.CaptureGroupAllowed.HasFlag(PrintCaptureGroup.Standard14)},
                OptionRibbonTab = {IsEnabled = instance.IsOptionAvailable},
                DebugRibbonTab = {IsEnabled = instance.IsDebugAvailable},
                EndorsementRibbonGroup =
                {
                    Visibility = Rules.IsEndorsementAllowed
                        ? Visibility.Visible
                        : Visibility.Collapsed
                }
            };

            win.Closing += WinOnClosing;
            win.Closed += MainWindowOnClosed;
            win.Loaded += (o, e) =>
            {
                win.Topmost = false;
                SplashWindowHelper.Hide();
                PrintCaptureDriver.Instance.AdaptForCaptureKind(win);
                win.LoadPreviousPrints(appParams.ImportedPrints);
            };
            win.Show();
        }

        private static void WinOnClosing(object sender, CancelEventArgs e)
        {
            var win = sender as MainWindow;
            bool? acceptPrint = win == null ? instance.wizardViewModel.AcceptPrint : win.AcceptPrint;                        

            if (acceptPrint == null)
            {                
                if (
                    MessageBox.Show(Text.CloseConfirmationPrompt, Text.ApplicationTitle, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    e.Cancel = true;
                }                
            }
            if (! e.Cancel && acceptPrint == true)
            {
                HasWizardAcceptedPrint = true;
            }
            
        }

        private static void MainWindowOnClosed(object sender, EventArgs eventArgs)
        {
            
            var win = sender as MainWindow;
            if (win == null)
            {
                instance.CloseApplication(instance.wizardViewModel.AcceptPrint ?? false);
            }
            else
            {
                instance.CloseApplication(win.AcceptPrint ?? false);
            }
            SplashWindowHelper.Close();
        }

        private static void RulesOnDelayedRuleChanged(object sender, EventArgs eventArgs)
        {
            instance.mainViewModel.PrintGridViewModel.Update(instance.PrintList.GetAllViewModels());
        }

        private static void ServiceOnServiceException(object sender, SequenceCheckExceptionEventArgs e)
        {           
            PrintCaptureAppLog.Logger.Error(e.Exception, "Service exception");
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
            if (instance.IsWizardMode || instance.IsLoginMode) return;
            LogDispatcher.DoLog("PrintCaptureApp(refresh main viewmodel) PrintModified");
            var vm = instance.PrintList.GetViewModel(e.Info);
            instance.mainViewModel.PrintGridViewModel.Update(vm);
            instance.mainViewModel.IsEndButtonAvailable = instance.PrintList.Prints.All(x => !x.IsInError);
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
            this.IsLoginMode = appParameter.IsLoginMode;
            this.TopMostWindow = appParameter.TopMostWindow;
            this.AlwaysCanOverrideWizard = appParameter.AlwaysCanOverrideWizard;
            LogDispatcher.DoLog($"PrintCaptureApp instance created.\nIcd:{this.IcdVersion}, IsWizard:{this.IsWizardMode}, IsDebug:{this.IsDebugAvailable}");
        }

        bool IsOptionAvailable { get;  }        

        bool IsDebugAvailable { get; }

        bool IsWizardMode { get; }

        string Line1 { get; }

        string Line2 { get; }

        private bool TopMostWindow { get; }

        private bool AlwaysCanOverrideWizard { get; set; }

        [ImportMany]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public List<ICaptureSdk> Sdks { get; private set; }

        public PrintList PrintList { get; private set; }

        public ICaptureDevice SelectedDevice { get; private set; }

        public string IcdVersion { get; private set; }

        public bool IsLoginMode { get; private set; }

        public void SetSelectedDevice(string internalKey)
        {
            var defaultDevice = instance.Sdks.GetDeviceFromKey(internalKey);

            this.SelectedDevice = defaultDevice;
            if (this.SelectedDevice == null)
            {
                PrintCaptureAppLog.Logger.Info($"Device : {Text.NoDevice}");
                this.mainViewModel.ScannerName = Text.NoDevice;
                this.mainViewModel.ScannerInfo = string.Empty;
                this.mainViewModel.ScannerImage = new BitmapImage(new Uri(@"pack://application:,,,/PrintsCapture.Ui;component/Images/Setting32x32.png"));
            }
            else
            {
                PrintCaptureAppLog.Logger.Info($"Device : {this.SelectedDevice.DisplayName}");
                this.mainViewModel.ScannerName = this.SelectedDevice.DisplayName;
                this.mainViewModel.ScannerInfo = PrintCaptureDriver.Instance.GetDeviceSecondaryInfo();
                this.mainViewModel.ScannerImage = new BitmapImage(new Uri(this.SelectedDevice.ImageUri));
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
            PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication. Success={0}", success);            
            SplashWindowHelper.SetMessage(@". . .", false);
            SplashWindowHelper.Show();            

            PrintModificationDispatcher.ClearAll();
            string setId = string.Empty;
            if (this.seqCheckService != null)
            {
                PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - EndSession");
                setId = this.seqCheckService.EndSession(success);
                this.seqCheckService = null;
            }

            var result = new ScanCompletedEventArgs(success, setId);

            if (instance.SelectedDevice != null && instance.SelectedDevice.IsOpened)
            {
                PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - SelectedDevice.Close");
                instance.SelectedDevice.Close();
                instance.SelectedDevice = null;
            }

            PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - ScanCompleted handler");
            this.ScanCompleted?.Invoke(this, result);            

            // No App closing if on Wizard mode
            if (Application.Current != null && !this.IsWizardMode)
            {
                Application.Current.Shutdown();
            }
            PrintCaptureAppLog.Logger.Debug(@"PrintCaptureApp - CloseApplication - Done.");
        }

        #endregion
    }
}
