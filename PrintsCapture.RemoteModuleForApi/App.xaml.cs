using System;
using System.Windows;

namespace PrintsCapture.RemoteModuleForApi
{
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;

    using CommandLine;

    using Microsoft.ServiceBus;

    using NLog;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.RemoteSeqCheck;
    using PrintsCapture.Ui.Class;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {        

        private string resultUrl;        

        private string referenceId;

        private Logger logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.logger = LogManager.GetCurrentClassLogger();

            var options = new CommandLineOptions();

            this.logger.Debug("Analyzing command line");
            var optionsOk = Parser.Default.ParseArguments(e.Args, options);

            optionsOk = optionsOk & options.Validate();

            if (!optionsOk)
            {
                this.logger.Error("Invalid command line arguments");
                Console.WriteLine("Wrong arguments. See application documentation for help");
                Current.Shutdown(1);
                return;
            }

            this.resultUrl = options.CallbackUrl;            
            
            this.referenceId = options.ReferenceId;

            this.logger.Debug("Initializing App parameter");
            try
            {
                var config = new PrintCaptureAppParameter
                {
                    CaptureModeAllowed = PrintCaptureGroup.FlatOnly,
                    Mode = "livescan",
                    CultureName = options.CultureIdentifier,
                    DescriptionLine1 = string.IsNullOrEmpty(options.Description1) ? options.ReferenceId : options.Description1,
                    DescriptionLine2 = options.Description2,
                    IsWizardMode = options.IsSimpleUi,
                    IsLoginMode = options.IsLoginMode
                };

                PrintCaptureApp.ApplicationCulture = new CultureInfo(config.CultureName);

                this.logger.Debug("Creating PrintSetService with service bus information");
                var cs = new ServiceBusConnectionStringBuilder();
                cs.Endpoints.Add(new Uri(options.EndPointAddress));

                cs.SharedSecretIssuerName = options.IsSharedAccessKey ? string.Empty : options.User;
                cs.SharedSecretIssuerSecret = options.Password;
                cs.SharedAccessKeyName = options.IsSharedAccessKey ? options.User : string.Empty;
                
                var service = new PrintSetService(options.Token, cs.ToString(), CaptureKind.Livescan, config.PrintList)
                {                    
                    ExternalReference = options.ReferenceId
                };

                config.SeqCheckService = service;

                this.logger.Info("Starting application");
                var app = PrintCaptureApp.Start(config);
                app.ScanCompleted += this.AppOnScanCompleted;

                this.logger.Info("Application started");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                this.logger.Error("Application stopped");
                Current.Shutdown(1);
            }
            
        }

        private void AppOnScanCompleted(object sender, Ui.Print.ScanCompletedEventArgs e)
        {
            this.logger.Debug("AppOnScanCompleted. Url : {0},  Id={1},  Success:{2}", this.resultUrl, this.referenceId, e.Success);
            var values = new NameValueCollection();
            values.Add("Reference", this.referenceId);

            // push data !            
            if (e.Success)
            {                
                values.Add("Success", "1");            
                values.Add("ResultSetId", e.ResultSetId);
            }
            else
            {
                values.Add("Success", "0");                
            }

            
            this.SendToWeb(values);
            

            Current.Dispatcher.Invoke(new Action(() => Current.Shutdown(0)));
        }

        //private void SendToPipe(NameValueCollection nameValues)
        //{
        //    logger.Debug("Sending information to Pipe {0}", this.namedPipe);
        //    try
        //    {
        //        var pipe = new NamedPipeClientStream(this.namedPipe);

        //        var sw = new StreamWriter(pipe);

        //        foreach (var key in nameValues.AllKeys)
        //        {
        //            var t = key + ":" + nameValues.Get(key);
        //            sw.WriteLine(t);
        //        }
        //        sw.Flush();
        //        sw.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Console.WriteLine("Cannot send result to pipe. Error : {0}", ex.Message);
        //        Application.Current.Dispatcher.Invoke(new Action(() => Application.Current.Shutdown(1)));                
        //    }           
        //}

        private void SendToWeb(NameValueCollection values)
        {
            this.logger.Debug("Sending information to Url {0}", this.resultUrl);
            try
            {
                using (var wc = new WebClient())
                {
                    wc.UploadValues(this.resultUrl, values);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                Console.WriteLine("Cannot send result to url. Error : {0}", ex.Message);
                Current.Dispatcher.Invoke(new Action(() => Current.Shutdown(1)));               
            }                     
        }

        //private static bool IsAdmin()
        //{
        //    //return true;

        //    var isAdmin = false;
        //    var windowsIdentity = WindowsIdentity.GetCurrent();

        //    if (windowsIdentity == null)
        //    {
        //        ShowError("WindowsIdentity is null.");
        //    }
        //    else
        //    {
        //        var pricipal = new WindowsPrincipal(windowsIdentity);
        //        var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

        //        if (hasAdministrativeRight)
        //        {
        //            isAdmin = true;
        //        }
        //        else
        //        {
        //            ShowError("Must be run as administrator.");
        //        }
        //    }

        //    return isAdmin;
        //}

        public static void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
