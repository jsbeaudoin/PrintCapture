using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NLog;
using PrintsCapture.LocalAwSeqCheck;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Ui.Class;
using PrintsCapture.Ui.Print;
using UniBIO.Services.Communication.BiometricService;
using XL_ID.Utilities.Wpf.ViewModel;
using XL_ID.Utilities.Wpf.WindowHelper;


namespace PrintsCapture.Direct
{
    /// <summary>
    /// Interaction logic for PrintCaptureDllApp in WPF class. IMportant, start in a STA Thread
    /// </summary>

    public partial class PrintCaptureDllApp : Application
    {
        private static PrintCaptureDllApp instance;
        private static LocalSeqCheck sequenceCheckService;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();       

        public CapturedPrintData Result { get; private set; }

        public static event EventHandler CaptureWindowClosed;

        private PrintCaptureDllApp()
        {
            // make it a singleton then
        }

        public static void DoCapture(Dictionary<string, string> args)
        {
            var instanceExists = instance != null;
            var codeToExecute = new Action(() =>
            {
                instance = instance ?? new PrintCaptureDllApp();
                instance.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                instance.Result = null;
                instance.StartCapture(args, instanceExists);                
            });

            //if (!instanceExists)
            //{
            //    Thread t = new Thread(new ThreadStart(codeToExecute));
            //    t.SetApartmentState(ApartmentState.STA);
            //    t.Start();


                
            //} else
            //{
            //    //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            //    //Application.Current.Shutdown();
            //}
            
            if (instanceExists)
            {
                try
                {
                    instance.Dispatcher.Invoke(codeToExecute);
                    Console.WriteLine("Aouch");
                }
                catch (Exception ex)
                {

                    throw;
                }
                 // make sure we run on same Thread in subsequent calls
            } else
            {
                codeToExecute();
            }
            
        }

        public static void AppShutDown()
        {
            //SplashWindowHelper.Close();
            instance?.Shutdown();
        }

        public static CapturedPrintData GetResults()
        {
            return instance.Result;
        }        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            instance = this;
            
        }

        private void StartCapture(Dictionary<string, string> startArguments, bool noNewSplash = false)
        {
            try
            {
                // Really important to start the splash screen, as it initializes all WPF and the app Needs an owner window to display properly. And to end properly too.
                this.logger.Trace("Showing splash screen");
                if (! noNewSplash)
                {
                    SplashWindowHelper.CreateSplash(new SplashLabels { Title = "UniDAC", SubTitle = "PrintsCapture " + PrintCaptureApp.AppVersion, Message = "...", CloseLabel = "X" }
                        , new System.Uri("pack://application:,,,/PrintsCapture.Direct;component/Images/LogoPrintCapture4-300x300.png"));
                } else
                {
                    SplashWindowHelper.Show();
                    
                }
                this.logger.Trace("Starting PrintCapture from DirectCapture class");
                string fileNamePrefix;
                string langParameter = string.Empty;
                string keyParameter = string.Empty;
                string modeParameter = string.Empty;
                string nameParameter = string.Empty;
                string captureModeParameter = string.Empty;
                string endorsementParameter = string.Empty;
                string singleFingerCaptureLabel = string.Empty;
                string icdVersion = string.Empty;
                bool isWizardMode = false;
                bool debug = false;
                bool isLoginMode = false;
                bool topMostWindow = false;
                bool alwaysCanOverride = false;
                bool sqMode = false;

                this.logger.Trace("Setting PrintCapture parameters");
                if (startArguments.ContainsKey("prefix")) fileNamePrefix = startArguments["prefix"];
                if (startArguments.ContainsKey("lang")) langParameter = startArguments["lang"];
                if (startArguments.ContainsKey("culture")) langParameter = startArguments["culture"];
                if (startArguments.ContainsKey("key")) keyParameter = startArguments["key"];
                if (startArguments.ContainsKey("name")) nameParameter = startArguments["name"];
                if (startArguments.ContainsKey("mode")) modeParameter = startArguments["mode"];
                if (startArguments.ContainsKey("capture")) captureModeParameter = startArguments["capture"];
                if (startArguments.ContainsKey("endorsement")) endorsementParameter = startArguments["endorsement"];
                if (startArguments.ContainsKey("icdversion")) icdVersion = startArguments["icdversion"];
                if (startArguments.ContainsKey("wizard")) isWizardMode = startArguments["wizard"] == "1";
                if (startArguments.ContainsKey("topmost")) topMostWindow = startArguments["topmost"] == "1";
                if (startArguments.ContainsKey("debug")) debug = startArguments["debug"] == "1";
                if (startArguments.ContainsKey("login")) isLoginMode = startArguments["login"] == "1";
                if (startArguments.ContainsKey("captureorder")) sqMode = startArguments["captureorder"] == "sq";
                if (startArguments.ContainsKey("singlecapturemessage")) singleFingerCaptureLabel = startArguments["singlecapturemessage"];
                if (startArguments.ContainsKey("alwayscanoverride"))
                    alwaysCanOverride = startArguments["alwayscanoverride"] == "1";

                if (string.IsNullOrEmpty(langParameter))
                {
                    langParameter = "en"; //English is the default language
                }
                else if (langParameter.Length > 2)
                {
                    langParameter = langParameter.Substring(0, 2).ToLowerInvariant();
                }

                try
                {
                    PrintCaptureApp.ApplicationCulture = new CultureInfo(langParameter);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Cannot set culture to '{0}'", langParameter);
                }

                int captureMode = 3;
                if (!string.IsNullOrEmpty(captureModeParameter))
                {
                    int.TryParse(captureModeParameter, out captureMode);
                }

                try
                {
                    var param = new PrintCaptureAppParameter
                    {
                        CultureName = langParameter,
                        IsWizardMode = isWizardMode,
                        DescriptionLine1 = nameParameter,
                        DescriptionLine2 = keyParameter,
                        IcdVersion = icdVersion,
                        Mode = modeParameter == "card" ? "cardscan" : "livescan",
                        CaptureOrder = sqMode ? CaptureOrderMode.Sq : CaptureOrderMode.Standard,
                        IsOptionAvailable = modeParameter == "card",
                        CaptureModeAllowed = (PrintCaptureGroup)captureMode,
                        IsEndorsementAllowed = string.IsNullOrEmpty(endorsementParameter) || endorsementParameter == "1",
                        IsDebugAvailable = debug,
                        IsLoginMode = isLoginMode,
                        SingleFingerCapturePrompt = singleFingerCaptureLabel,
                        TopMostWindow = topMostWindow,
                        AlwaysCanOverrideWizard = alwaysCanOverride
                    };

                    sequenceCheckService =
                        new LocalSeqCheck(
                            modeParameter == "card" ? CaptureKind.Cardscan : CaptureKind.Livescan,
                            param.PrintList);

                    param.SeqCheckService = sequenceCheckService;


                    var app = PrintCaptureApp.Start(param);

                    if (app != null)
                    {
                        app.ScanCompleted += this.AppOnScanCompleted;
                    }
                    else
                    {
                        this.logger.Warn("PrintCaptureApp.Start Failed!");
                    }

                    if (!PrintCaptureApp.OpenMainFormDialog())
                    {
                        this.logger.Warn("OpenMainFormDialog Failed!");
                    }


                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Wizard.Capture");
                    Console.WriteLine(ex);
                    this.ShutdownWPF();
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Wizard.Capture");
                Console.WriteLine(e);
                this.ShutdownWPF();
            }
        }        

        private void ShutdownWPF()
        {
            Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        private void AppOnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            if (PrintCaptureApp.HasWizardAcceptedPrint)
            {
                this.Result = sequenceCheckService.GetCapturedPrintData();
            }
            
            this.ShutdownWPF();

            CaptureWindowClosed?.Invoke(sender, null);
        }

    }
}
