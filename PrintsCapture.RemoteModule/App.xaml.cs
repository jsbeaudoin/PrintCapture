using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using XL_ID.Utilities.Wpf.ViewModel;
using XL_ID.Utilities.Wpf.WindowHelper;
using XL_ID.Utilities.XML;

namespace PrintsCapture.RemoteModule
{
    using System.Text;

    using Microsoft.ServiceBus;

    using NLog;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.RemoteSeqCheck;
    using PrintsCapture.Ui.Class;
    using PrintsCapture.Ui.Print;

    using RemoteModules;
    using System.Globalization;

    using PrintsCapture.Prints.Extension;

    /// <summary>
        /// Interaction logic for App.xaml
        /// </summary>
        public partial class App
        {
            private RemoteModuleCallback remoteModule;

            private Logger logger;

            private string referenceId;

            private bool isLocalRemote;

            private PrintCaptureAppParameter appParameter;

            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);

                logger = LogManager.GetCurrentClassLogger();
                this.logger.Info("*****App PrintsCapture.RemoteModule starting*****");

                var startTime = DateTime.Now;
                SplashWindowHelper.CreateSplash( 
                    new SplashLabels
                    {                        
                        Title = "UniDAC", 
                        SubTitle = "PrintsCapture " + PrintCaptureApp.AppVersion,
                        Message = "..."

                    },
                    new Uri("pack://application:,,,/Images/LogoPrintCapture4-300x300.png"));
                var duration = DateTime.Now.Subtract(startTime);
                this.logger.Trace($"Splash Screen creation process time: {duration.ToString("mm\\:ss\\:ff")}");

                if (!RemoteModuleCallback.IsLaunchedByModuleHost)
                {
                    this.logger.Trace(RemoteModuleCallback.IsLaunchedByModuleHost);
                    if (e.Args.Length > 0 && e.Args[0] == "debug")
                    {
                        var config = new PrintCaptureAppParameter
                                     {
                                         CultureName = "fr",
                                         DescriptionLine1 = "Line1",
                                         DescriptionLine2 = "Line2",
                                         Mode = "livescan",
                                         CaptureModeAllowed = PrintCaptureGroup.FlatOnly
                                     };

                        // DotNet 4.0 ...
                        PrintCaptureApp.ApplicationCulture = new CultureInfo(config.CultureName);

                    // DotNet 4.5
                    //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(config.CultureName);
                    //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureName);

                    // Base64-2 : RW5kcG9pbnQ9c2I6Ly91bmlkYWMuc2VydmljZWJ1cy53aW5kb3dzLm5ldC9EZXYtUHJpbnRTZXRTZXJ2aWNlLztTaGFyZWRBY2Nlc3NLZXlOYW1lPVVuaWRhYy5DaXZpbC5XZWI7U2hhcmVkQWNjZXNzS2V5PVBpeTlKUUh5cm9YVjVBWGVVdGNXUVRwbmIydk8xY0t1dU5Uc2JydTJlTVE9
                    // Base64 : RW5kcG9pbnQ9c2I6Ly91bmlkYWMuc2VydmljZWJ1cy53aW5kb3dzLm5ldC9EZXYtUHJpbnRTZXRTZXJ2aWNlLztTaGFyZWRTZWNyZXRJc3N1ZXI9VW5pZGFjLkNpdmlsLldlYjtTaGFyZWRTZWNyZXRWYWx1ZT1QaXk5SlFIeXJvWFY1QVhlVXRjV1FUcG5iMnZPMWNLdXVOVHNicnUyZU1RPQ==


                    var s =
                            Convert.FromBase64String(
                                "RW5kcG9pbnQ9c2I6Ly91bmlkYWMuc2VydmljZWJ1cy53aW5kb3dzLm5ldC9EZXYtUHJpbnRTZXRTZXJ2aWNlLztTaGFyZWRBY2Nlc3NLZXlOYW1lPVVuaWRhYy5DaXZpbC5XZWI7U2hhcmVkQWNjZXNzS2V5PVBpeTlKUUh5cm9YVjVBWGVVdGNXUVRwbmIydk8xY0t1dU5Uc2JydTJlTVE9");
                        var c = Encoding.UTF8.GetString(s);
                        var cs = new ServiceBusConnectionStringBuilder(); //c
                        cs.SharedAccessKeyName = "Unidac.Civil.Web"; //"UniBIO.Services";
                        cs.SharedAccessKey = "Piy9JQHyroXV5AXeUtcWQTpnb2vO1cKuuNTsbru2eMQ="; // "mwmivuTKm1yaBPsTUSmD1BdM4puTrzlkW4uaRwCarwM=";
                        cs.Endpoints.Add(new Uri("sb://unidac.servicebus.windows.net/dev-PrintSetService/"));
                        
                        //"Endpoint=sb://unidac.servicebus.windows.net/dev-PrintDataService/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=yourKey";


                        var seq = new PrintSetService(
                            "9ca5c736-acbc-4946-a143-e085684deb30",
                            cs.ToString(),                           
                            config.Mode == "cardscan" ? CaptureKind.Cardscan : CaptureKind.Livescan,
                            config.PrintList);
                        
                        //seq.ServiceIssuerName = "UniBIO.Services";
                        //seq.ServiceIssuerSecret = "mwmivuTKm1yaBPsTUSmD1BdM4puTrzlkW4uaRwCarwM=";
                        //seq.IsSharedAccessKey = false;

                        config.SeqCheckService = seq;
                        this.LaunchApp(config);
                        return;
                    }

                    this.logger.Debug("App was not launched by module host. App Shutdown.");

                    SplashWindowHelper.SetErrorMessage("App was not launched by module host.", 1);                    
                    return;
                }

                Task.Factory.StartNew(this.SetupRemote);
            }

            private void SetupRemote()
            {
                try
                {
                    var startTime = DateTime.Now;
                    this.remoteModule = new RemoteModuleCallback();
                    var duration = DateTime.Now.Subtract(startTime);
                    this.logger.Trace($"New RemoteModuleCallback process time: {duration.ToString("mm\\:ss\\:ff")}");

                    startTime = DateTime.Now;
                    this.remoteModule.Connected += this.RemoteModuleOnConnected;
                    this.remoteModule.Disconnected += this.RemoteModuleOnDisconnected;
                    this.remoteModule.NewCommand += this.RemoteModuleOnNewCommand;
                    duration = DateTime.Now.Subtract(startTime);
                    this.logger.Trace($"Setting remote module events. Process time: {duration.ToString("mm\\:ss\\:ff")}");

                    startTime = DateTime.Now;
                    this.Connect();
                    duration = DateTime.Now.Subtract(startTime);
                    this.logger.Trace($"Connect function process time: {duration.ToString("mm\\:ss\\:ff")}");

                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Can't initialize remote module.");

                    SplashWindowHelper.SetErrorMessage("Can't initialize remote module.", 1);                    
                }
            }

            private void Connect()
            {

                try
                {
                    this.remoteModule.Connect();
                    this.logger.Info("Remote Module Connected");
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Can't connect to remote module.");
                    SplashWindowHelper.SetErrorMessage("Can't connect to remote module", 1);                    
                }

            }


            private void LaunchApp(PrintCaptureAppParameter config)
            {                           
                var app = PrintCaptureApp.Start(config);
                if (app != null)
                {
                    app.ScanCompleted += AppOnScanCompleted;                   
                    PrintCaptureApp.OpenMainFormDialog();
                    
                }
                else
                {
                    Application.Current.Shutdown(1);
                }
            }

            private void AppOnScanCompleted(object sender, ScanCompletedEventArgs e)
            {
                var dicResult = new Dictionary<string, string>();
                if (!isLocalRemote)
                {
                    dicResult["success"] = e.Success ? "1" : "0";
                    dicResult["referenceid"] = this.referenceId;
                    dicResult["resultsetid"] = e.ResultSetId;
                }
                else
                {
                    dicResult["success"] = e.Success ? "1" : "0";
                    WritePrintsInformation(dicResult);                
                }

                remoteModule?.SendResult(dicResult);
            }

        private void WritePrintsInformation(Dictionary<string, string> dicResult)
        {
            var seq = this.appParameter.SeqCheckService as LocalAwSeqCheck.LocalSeqCheck;
            if (seq == null)
            {
                return;
            }

            try
            {
                if (!PrintCaptureApp.IsWizard || PrintCaptureApp.HasWizardAcceptedPrint)
                {
                    this.logger.Info("WritePrintsInformation : Adding serialized GetCapturedPrintData in DATA");
                    dicResult.Add("data", XmlSerializer.Serialize(seq.GetCapturedPrintData()));
                }
                else
                {
                    this.logger.Info("WritePrintsInformation : Adding null in DATA");
                    dicResult.Add("data", null);
                }
                
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Could not create all results");
                dicResult["success"] = "0";
                dicResult["message"] = "WritePrintsInformation Error: " + ex.Message;
            }
            
        }        
        

        private void RemoteModuleOnNewCommand(Dictionary<string, string> moduleArgs)
            {

                this.logger.Debug("Remote Received command");

                string log = "command received";                

                try
                {
                    if (!moduleArgs.ContainsKey("web") || moduleArgs["web"] == "0")
                    {
                        if (!ConfigureLocal(moduleArgs))
                        {
                            return;
                        }
                    }
                    else
                    {
                        ConfigureWeb(moduleArgs);
                    }
                    
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Received an incomplete/incorrect command. {0}", log);
                    SplashWindowHelper.SetErrorMessage("Received an incomplete/incorrect command", 1);
                    return;
                }

                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => this.LaunchApp(this.appParameter)));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "App could not be launched");
                    SplashWindowHelper.SetErrorMessage("App could not be launched : " + ex.Message, 1);
                    return;
                }
            }

        private void ConfigureWeb(Dictionary<string, string> moduleArguments)
        {
            string log = "command received";

            var config = new PrintCaptureAppParameter();

            try
            {
                this.CheckArguments(moduleArguments, true, "Mode", "Culture", "Token", "ServiceBusConnectString", "ReferenceId", "DescriptionLine1", "DescriptionLine2");
                config.Mode = moduleArguments["Mode"];
                log += "  mode:" + config.Mode;
                config.CultureName = moduleArguments["Culture"];
                log += "  culture:" + config.CultureName;
                config.DescriptionLine1 = moduleArguments["DescriptionLine1"];
                log += " Decription Line1:" + config.DescriptionLine1;
                config.DescriptionLine2 = moduleArguments["DescriptionLine2"];
                log += "  Decription Line 2:" + config.DescriptionLine2;

                var token = moduleArguments["Token"];
                log += "  token:" + token;
                var serviceBusConnectString = moduleArguments["ServiceBusConnectString"];
                log += "  service bus:" + serviceBusConnectString;

                this.referenceId = moduleArguments["ReferenceId"];
                log += "  Reference Id:" + this.referenceId;

                // Optional parameters 
                //  CaptureMode : as an int (enum flag)
                // DebugTab : 1 = Shown
                // OptionTab : 1 = Shown
                config.CaptureModeAllowed = PrintCaptureGroup.FlatOnly;
                if (moduleArguments.ContainsKey("CaptureMode"))
                {
                    var capt = 0;
                    if (int.TryParse(moduleArguments["CaptureMode"], out capt))
                    {
                        config.CaptureModeAllowed = (PrintCaptureGroup)capt;
                    }
                }

                config.IsEndorsementAllowed = true;
                if (moduleArguments.ContainsKey("NoEndorsement"))
                {
                    if (moduleArguments["NoEndorsement"] == "1")
                    {
                        config.IsEndorsementAllowed = false;
                    }
                }

                if (moduleArguments.ContainsKey("SimpleUi"))
                {
                    if (moduleArguments["SimpleUi"] == "1")
                    {
                        config.IsWizardMode = true;
                    }
                }

                if (moduleArguments.ContainsKey("TopMost"))
                {
                    if (moduleArguments["TopMost"] == "1")
                    {
                        config.TopMostWindow = true;
                    }
                }

                config.SeqCheckServiceConnection = Encoding.UTF8.GetString(Convert.FromBase64String(serviceBusConnectString));
                config.IsDebugAvailable = this.CheckOptionalBool(moduleArguments, "DebugTab", false);
                config.IsOptionAvailable = this.CheckOptionalBool(moduleArguments, "OptionTab", false);

                // DotNet 4.0 ...
                PrintCaptureApp.ApplicationCulture = new CultureInfo(config.CultureName);

                // DotNet 4.5
                //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(config.CultureName);
                //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureName);

                var seq = new RemoteSeqCheck.PrintSetService(
                    token,
                    config.SeqCheckServiceConnection,
                    config.Mode == "live" ? CaptureKind.Livescan : CaptureKind.Cardscan,
                    config.PrintList);                

                config.SeqCheckService = seq;

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Received an incomplete/incorrect command. {0}", log);
                SplashWindowHelper.SetErrorMessage("Received an incomplete/incorrect command",1);                
            }
        }

        private bool ConfigureLocal(Dictionary<string, string> moduleArguments)
        {
            string log = "command received";

            this.appParameter = PrintCaptureAppParameter.FromParameters(moduleArguments);            

            try
            {
                this.isLocalRemote = true;
                SplashWindowHelper.Hide();
                if (moduleArguments.ContainsKey("debug-process") && moduleArguments["debug-process"] == "1")
                {
                    MessageBox.Show("Attach debugger now");
                }


                // DotNet 4.0 ...
                PrintCaptureApp.ApplicationCulture = new CultureInfo(this.appParameter.CultureName);

                // DotNet 4.5
                //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(config.CultureName);
                //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureName);

                var seq = new LocalAwSeqCheck.LocalSeqCheck(
                    this.appParameter.Mode == "live" ? CaptureKind.Livescan : CaptureKind.Cardscan,
                    this.appParameter.PrintList);

                this.appParameter.SeqCheckService = seq;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Received an incomplete/incorrect command. {0}", log);
                SplashWindowHelper.SetErrorMessage("Received an incomplete/incorrect command.", 1);
                return false;
            }
        }

        private bool CheckOptionalBool(Dictionary<string, string> dict, string name, bool defaultValue)
        {
            if (!dict.ContainsKey(name))
            {
                return defaultValue;
            }

            return dict[name] == "1";
        }

        private bool CheckArguments(Dictionary<string, string> dict, bool throwException, params string[] fields)
            {
                var complete = true;
                var missings = string.Empty;

                foreach (var field in fields)
                {
                    if (!dict.ContainsKey(field))
                    {
                        complete = false;
                        missings += field + Environment.NewLine;
                    }
                }

                if (!complete && throwException)
                {
                    throw new ApplicationException("Command incomplete, some fields are missing :" + Environment.NewLine + missings);
                }

                return complete;
            }

            private void RemoteModuleOnDisconnected(object sender, EventArgs eventArgs)
            {
                Console.WriteLine("Disconnected");
            }

            private void RemoteModuleOnConnected(object sender, EventArgs eventArgs)
            {
                Console.WriteLine("Remote connected");
            }

        }
    }

