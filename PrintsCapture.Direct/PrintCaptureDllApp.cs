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
            PrintCaptureAppLog.Init();
            var instanceExists = instance != null;
            var codeToExecute = new Action(() =>
            {
                instance = instance ?? new PrintCaptureDllApp();
                instance.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                instance.Result = null;
                instance.StartCapture(args, instanceExists);                
            });
            
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

        public static void Reset()
        {
            instance = null;
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

                try
                {
                    var param = PrintCaptureAppParameter.FromParameters(startArguments);

                    sequenceCheckService =
                        new LocalSeqCheck(
                            param.Mode == "card" ? CaptureKind.Cardscan : CaptureKind.Livescan,
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
                this.Result = CapturedPrintDataBuilder.GetCapturedPrintData(sequenceCheckService);
            }
            
            this.ShutdownWPF();

            CaptureWindowClosed?.Invoke(sender, null);
        }

    }
}
