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
            Dictionary<string, string> commandArgs = new Dictionary<string, string>();
            // transfer command line args into Dictionary for unified processing of parameters
            foreach (var arg in args)
            {
                var lowerArg = arg.ToLower();
                var posStart = arg.IndexOf(":", StringComparison.Ordinal);
                string argName = "";
                string argValue = "";
                if (posStart > 0)
                {
                    argName = lowerArg.Substring(0, posStart).Trim();
                    argValue = lowerArg.Substring(posStart + 1);
                    commandArgs.Add(argName, argValue);
                }
            }

            var appParam = PrintCaptureAppParameter.FromParameters(commandArgs);
            try
            {
                if (commandArgs.ContainsKey("prefix")) fileNamePrefix = commandArgs["prefix"];

                sequenceCheckService =
                    new LocalSeqCheck(
                        appParam.Mode == "cardscan" ? CaptureKind.Cardscan : CaptureKind.Livescan,
                        appParam.PrintList);

                appParam.SeqCheckService = sequenceCheckService;

                Current.Dispatcher.Invoke(new Action(() =>
                {
                    var app = PrintCaptureApp.Start(appParam);
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
            var firstNonMissing = printsNoEndorsement.First(x => !x.IsMissing);
            var dpi = firstNonMissing.Resolution.ToDpi();

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
                devInfoWriter.WriteLine("Dpi={0}", dpi);

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
