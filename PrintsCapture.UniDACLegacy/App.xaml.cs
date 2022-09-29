using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using NLog;
using PrintsCapture.LocalAwSeqCheck;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Sequence;
using PrintsCapture.Ui.Class;
using PrintsCapture.Ui.Print;
using XL_ID.Utilities.Wpf.ViewModel;
using XL_ID.Utilities.Wpf.WindowHelper;

namespace PrintsCapture.UniDACLegacy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string fileNamePrefix;

        private static LocalSeqCheck sequenceCheckService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashWindowHelper.CreateSplash( new SplashLabels { Title = "UniDAC", SubTitle = "PrintsCapture " + PrintCaptureApp.AppVersion, Message = "...", CloseLabel = "X"}
                , new System.Uri("pack://application:,,,/Images/LogoPrintCapture4-300x300.png"));

            var bg = new BackgroundWorker();
            bg.DoWork += (s,a) => this.StartApp(e.Args);
            bg.RunWorkerAsync();
        }

        private void StartApp(string[] args)
        {
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

            foreach (var arg in args)
            {
                var lowerArg = arg.ToLower();
                var posStart = arg.IndexOf(":", StringComparison.Ordinal);
                string argName = "";
                string argValue = "";
                if (posStart > 0)
                {
                    argName = lowerArg.Substring(0, posStart - 1).Trim();
                    argValue = lowerArg.Substring(posStart + 1);
                }


                if (lowerArg.StartsWith("prefix:") && posStart > 0)
                {
                    fileNamePrefix = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("lang:") && posStart > 0)
                {
                    langParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("key:") && posStart > 0)
                {
                    keyParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("name:") && posStart > 0)
                {
                    nameParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("mode:") && posStart > 0)
                {
                    modeParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("capture:") && posStart > 0)
                {
                    captureModeParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("endorsement:"))
                {
                    endorsementParameter = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("icdversion:"))
                {
                    icdVersion = arg.Substring(posStart + 1);
                }
                else if (lowerArg.StartsWith("wizard:"))
                {
                    isWizardMode = arg.Substring(posStart + 1) == "1";
                }
                else if (lowerArg.StartsWith("topmost:"))
                {
                    topMostWindow = arg.Substring(posStart + 1) == "1";
                }
                else if (lowerArg.StartsWith("debug:"))
                {
                    debug = arg.Substring(posStart + 1) == "1";
                }
                else if (lowerArg.StartsWith("login"))
                {
                    isLoginMode = arg.Substring(posStart + 1) == "1";
                } else if (lowerArg.StartsWith("singlecaptureprompt:"))
                {
                    singleFingerCaptureLabel = arg.Substring(posStart + 1);
                }
            }

            // get language
            var lang = ConfigurationManager.AppSettings["lang"];

            if (!string.IsNullOrEmpty(lang))
            {
                langParameter = lang;
            }

            if (string.IsNullOrEmpty(langParameter))
            {
                langParameter = "fr";
            }
            else if (langParameter.Length > 2)
            {
                langParameter = langParameter.Substring(0, 2).ToLowerInvariant();
            }

            try
            {
                // DotNet 4.0
                PrintCaptureApp.ApplicationCulture = new CultureInfo(langParameter);

                // DotNet 4.5
                //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(langParameter);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Cannot set culture to '{0}'", langParameter);                
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
                    IsOptionAvailable = modeParameter == "card",
                    CaptureModeAllowed = (PrintCaptureGroup)captureMode,
                    IsEndorsementAllowed = string.IsNullOrEmpty(endorsementParameter) || endorsementParameter == "1",
                    IsDebugAvailable = debug,
                    IsLoginMode = isLoginMode,
                    SingleFingerCapturePrompt = singleFingerCaptureLabel,
                    TopMostWindow = topMostWindow
                };

                sequenceCheckService =
                    new LocalSeqCheck(
                        modeParameter == "card" ? CaptureKind.Cardscan : CaptureKind.Livescan,
                        param.PrintList);

                param.SeqCheckService = sequenceCheckService;                

                Current.Dispatcher.Invoke(new Action(() =>
                {
                    var app = PrintCaptureApp.Start(param);
                    if (!PrintCaptureApp.OpenMainFormDialog())
                    {
                        Application.Current.Shutdown();
                        return;
                    }

                    if (app != null)
                    {
                        app.ScanCompleted += this.AppOnScanCompleted;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                    //splash.Close();
                }));
            }
            catch (Exception ex)
            {
                SplashWindowHelper.SetErrorMessage("Cannot start : " + ex.Message, 1);
            }           
        }

        private void AppOnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            if (e.Success)
            {
                try
                {
                    WritePrintFiles(sequenceCheckService.GetCapturedPrints());

                    WriteFingerFile(sequenceCheckService.CaptureDevice, sequenceCheckService.GetCapturedPrints(), sequenceCheckService.GetCapturedSegments(), sequenceCheckService.CaptureMode);
                }
                catch (Exception ex)
                {
                    PrintCaptureAppLog.Logger.Error(ex, "Cannot save files file in the folder. Error: {0} ! Process terminated. ", ex.Message);
                }
                
            }
            
        }        

        private static void WritePrintFiles(IEnumerable<PrintInfo> prints)
        {
            var allImages = Directory.GetFiles(PrintAppPath.AppPath, "*.bmp");
            foreach (var imagePath in allImages)
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    PrintCaptureAppLog.Logger.Warn("Cannot delete file '{0}' in the folder. Error: {1}. \nProcess continue ! ", Path.GetFileName(imagePath), ex.Message);
                }
                
            }            

            foreach (var printInfo in prints)
            {
                var name = printInfo.NistPosition.ToString();

                var baseFileName = PrintAppPath.AppPath + fileNamePrefix + name;

                if (printInfo.Image != null)
                {
                    if (printInfo.IsEndorsement)
                    {
                        var newIndex = printInfo.EndorsementFinger.EndorsementIndex;
                        if (newIndex==1)
                        {
                            newIndex = 11;
                        }
                        else if (newIndex == 6)
                        {
                            newIndex = 12;
                        }
                        baseFileName += "-" + newIndex;
                    }

                    if (printInfo.IsOverriden)
                    {
                        baseFileName += "-R" + printInfo.OverrideCode;
                    }

                    try
                    {
                        printInfo.Image.Save(baseFileName + ".bmp", ImageFormat.Bmp);
                    }
                    catch (Exception ex)
                    {
                        PrintCaptureAppLog.Logger.Error(ex, "Cannot save print image file !");
                        MessageBox.Show( string.Format("Fatal Error : Cannot save print to drive ! Error occured on '{0}' file. \nError : {1}", 
                                            Path.GetFileName(baseFileName), ex.Message)
                            , "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                
                
            }

        }

        private static void WriteFingerFile(CaptureDeviceInfo deviceInfo, List<PrintInfo> prints, List<PrintInfo> segmentsList, string captureType)
        {
            const string FingerFile = "FingerInfo.info";

            var filePath = PrintAppPath.AppPath + fileNamePrefix + FingerFile;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var printsNoEndorsement = prints.Where(x => !x.IsEndorsement).ToList();

            using (var devInfoWriter = new StreamWriter(filePath))
            {
                devInfoWriter.WriteLine("[General]");
                devInfoWriter.WriteLine("PrintCount={0}", printsNoEndorsement.Count);
                devInfoWriter.WriteLine("SegmentCount={0}", segmentsList.Sum(x => x.Segments.Count));
                devInfoWriter.WriteLine("DeviceModel={0}", deviceInfo.ModelName.ToUpperInvariant());
                devInfoWriter.WriteLine("DeviceMake={0}", deviceInfo.Make.ToUpperInvariant());
                devInfoWriter.WriteLine("DeviceSerial={0}", deviceInfo.SerialNumber == null ? "NA" : deviceInfo.SerialNumber.ToUpperInvariant());
                devInfoWriter.WriteLine("CapturedDate={0}", DateTime.Now.ToString("yyyy-MM-dd"));

                devInfoWriter.WriteLine("CaptureType={0}", captureType);

                devInfoWriter.WriteLine();

                var index = 1;
                // Write prints info
                
                foreach (var print in printsNoEndorsement)
                {
                    devInfoWriter.WriteLine("[Print{0}]", index);
                    devInfoWriter.WriteLine("NistIndex={0}", print.NistPosition);
                    devInfoWriter.WriteLine("Quality={0}", print.QualityScore);
                    devInfoWriter.WriteLine("MinutiaCount={0}", print.MinutiaCount);
                    devInfoWriter.WriteLine("Sequence={0}", print.SequenceScore);
                    
                    devInfoWriter.WriteLine("OverrideCode={0}", print.OverrideCode);
                    devInfoWriter.WriteLine("OverrideText={0}", print.OverrideUserReason);
                    devInfoWriter.WriteLine("MissingCode={0}", print.PhysicalPart.MissingCode);
                    devInfoWriter.WriteLine("MissingDate={0}", print.PhysicalPart.MissingDate);                    
                    devInfoWriter.WriteLine();
                    index++;
                }


                // Write Segments
                index = 1;
                foreach (var printElement in segmentsList)
                {                    

                    foreach (var segment in printElement.Segments)
                    {
                        devInfoWriter.WriteLine("[Segment{0}]", index);
                        devInfoWriter.WriteLine("NistIndex={0}", segment.Part.EndorsementIndex);
                        devInfoWriter.WriteLine("ParentNistIndex={0}", printElement.NistPosition);
                        devInfoWriter.WriteLine("Quality={0}", segment.QualityScore);
                        devInfoWriter.WriteLine("MinutiaCount={0}", segment.MinutiaCount);
                        
                        devInfoWriter.WriteLine("MissingCode={0}", segment.MissingCode);
                        devInfoWriter.WriteLine("MissingDate={0}", segment.MissingDate);
                        devInfoWriter.WriteLine("OverrideCode={0}", segment.OverrideCode);
                        devInfoWriter.WriteLine("OverrideText={0}", segment.OverrideText);
                        devInfoWriter.WriteLine("Top={0}", segment.Position.Top);
                        devInfoWriter.WriteLine("Left={0}", segment.Position.Left);
                        devInfoWriter.WriteLine("Bottom={0}", segment.Position.Bottom);
                        devInfoWriter.WriteLine("Right={0}", segment.Position.Right);
                        devInfoWriter.WriteLine();
                        index++;
                    }
                }

                devInfoWriter.Flush();

                devInfoWriter.Close();
            }                                  
        }
    }
}
