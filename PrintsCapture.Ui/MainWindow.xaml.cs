using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PrintsCapture.Cardscan;
using PrintsCapture.Livescan;
using XL_ID.Utilities.Setting;
using XL_ID.Utilities.Wpf.WindowHelper;

namespace PrintsCapture.Ui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;    
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls.Ribbon;
    using System.Windows.Forms;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.Ui.Class;
    using PrintsCapture.Ui.Language;    
    using PrintsCapture.Ui.Properties;
    using PrintsCapture.Ui.UserControls;
    using PrintsCapture.Ui.ViewModel;
    using PrintsCapture.Ui.View;

    using MessageBox = System.Windows.MessageBox;
    using System.Windows.Interop;
    using XL_ID.Utilities.Wpf.ViewModel;
    using XL_ID.Utilities.Log;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private PrintOperationService printOp;
      
        private readonly ToggleGroupHandler captureToggleGroup = new ToggleGroupHandler();
        
        private MainViewModel viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow(MainViewModel viewModel) : base()
        {
            this.viewModel = viewModel;
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "InitializeComponent");
                MessageBox.Show("WTF ?" + ex.Message);
                throw;
            }
            
            this.VersionTextBlock.Text = $"Version {PrintCaptureApp.AppVersion}";
            
            this.captureToggleGroup.Add(this.Std14ToggleButton);
            this.captureToggleGroup.Add(this.FlatToggleButton);
            this.captureToggleGroup.Add(this.StdAndPalmToggleButton);

            this.captureToggleGroup.CheckByValue(this.viewModel.Rules.CaptureMode);

            this.printOp = new PrintOperationService();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

            base.OnSourceInitialized(e);
        }

        public MainViewModel ViewModel
        {
            get
            {
                return this.viewModel;
            }
            private set
            {
                if (Equals(value, this.ViewModel))
                {
                    return;
                }

                this.viewModel = value;
                this.OnPropertyChanged("ViewModel");                
            }
        }

        public bool? AcceptPrint { get; set; }

        internal void LoadPreviousPrints(List<ImportedPrint> importedPrints)
        {
            LogDispatcher.DoLog("LoadPreviousPrints -- called-b");
            if (importedPrints == null)
            {
                LogDispatcher.DoLog("LoadPreviousPrints -- ended - no previous prints sent");
                return;
            }
            LogDispatcher.DoLog("Loading previous prints");
            PrintCaptureApp.SequenceCheck.StartSession();
            LogDispatcher.DoLog("Sequence check started");
            int splashWindowId = 0;
            try
            {
                splashWindowId = SplashWindowHelper.CreateSplash(new SplashLabels { Title = "UniDAC", SubTitle = "PrintsCapture " + PrintCaptureApp.AppVersion, Message = Text.LoadingPreviousPrints, CloseLabel = "X" }
                        , new System.Uri("pack://application:,,,/PrintsCapture.Direct;component/Images/LogoPrintCapture4-300x300.png"));
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Error creating Splash screen!", LogEventLevel.Error, ex);
                throw;
            }
            LogDispatcher.DoLog("Splash #2 created");
            SplashWindowHelper.Show(splashWindowId);
            LogDispatcher.DoLog("Splash #2 showed");

            var printList = PrintCaptureApp.Instance.PrintList;
            var missings = importedPrints.Where(x => !string.IsNullOrEmpty(x.MissingDate));
            LogDispatcher.DoLog("Validating missing");
            foreach (var missingPrint in missings)
            {
                var correspondingPrint = printList.Prints.First(x => x.NistPosition == missingPrint.NistPosition);
                // use logic of service, as it handles segments and such
                this.printOp.UpdateMissingInfo(correspondingPrint.PhysicalPart, missingPrint.MissingCode, missingPrint.MissingDate);
            }
            LogDispatcher.DoLog("Missings set");

            var batch = new List<PrintInfo>();
            var printIndex = 0;
            foreach (var print in importedPrints)
            {
                printIndex += 1;
                LogDispatcher.DoLog($"Loading prints {printIndex} / {importedPrints.Count}");
                SplashWindowHelper.SetMessage($"Loading prints {printIndex} / {importedPrints.Count}", false, splashWindowId);
                var correspondingPrint = printList.Prints.First(x => x.NistPosition == print.NistPosition && x.IsEndorsement == print.IsEndorsement);
                correspondingPrint.Image = print.Image;
                correspondingPrint.Resolution = print.Dpi.ToResolution();
                correspondingPrint.OriginalImage = print.Image;
                if (! string.IsNullOrEmpty( print.OverrideCode))
                {
                    int overrideCode = 0;
                    int.TryParse(print.OverrideCode, out overrideCode);
                    correspondingPrint.OverrideCode = overrideCode;
                    correspondingPrint.OverrideUserReason = print.OverrideReason;
                    
                }
                if (print.Image != null)
                {
                    batch.Add(correspondingPrint);
                }
            }
            LogDispatcher.DoLog("Running sequence check");
            SplashWindowHelper.SetMessage($"Running Sequence check", false, splashWindowId);
            PrintCaptureApp.SequenceCheck.AddPrintRange(batch);

            foreach (var print in batch)
            {
                PrintModificationDispatcher.PrintModified(print);
            }

            LogDispatcher.DoLog("Previous prints loaded");
            SplashWindowHelper.Hide(splashWindowId);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (PrintCaptureApp.Instance.SelectedDevice == null)
            {
                this.ShowConfigurationWindow();
            }
        }

        private void EndCaptureButtonClick(object sender, RoutedEventArgs e)
        {
            PrintCaptureAppSettings.Save();
            var printList = PrintCaptureApp.Instance.PrintList;

            if (!printList.CheckAllStatuses(true))
            {
                return;
            }

            var integrity = PrintCaptureApp.SequenceCheck.CheckSessionIntegrity(true);

            if (integrity == null || integrity.Count > 0)
            {
                return;
            }


            if (printList.IsEndorsementEmpty() && printList.Rules.IsEndorsementAllowed)
            {
                if (MessageBox.Show(
                    Text.EndorsementNotSetPrompt,
                    Text.CloseApplicationTitle,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            else if (MessageBox.Show(
                Text.EndConfirmationPrompt,
                Text.CloseApplicationTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) != MessageBoxResult.OK)
            {
                return;
            }

            this.AcceptPrint = true;            
            this.Close();
        }                        

        public void ShowConfigurationWindow()
        {
            PrintCaptureApp.ConfigureDevice();                          
        }        

        private void PrintGridControlOnPrintImageClick(object sender, PrintClickEventArgs e)
        {
            if (e.ViewModel.LinkedPrint.Image == null && !this.ViewModel.InCaptureMode)
            {
                // Print not captured yet
                this.printOp.ProcessPresenceWindow(e.ViewModel);
                
                this.ViewModel.EndorsableFingers = e.ViewModel.LinkedPrint.PrintList.GetEndorsableFingers();

                if (this.viewModel.PrintGridViewModel.EndorsementFinger.IsMissing)
                {
                    var endorsement = this.viewModel.PrintGridViewModel.EndorsementFinger.LinkedPrint;
                    endorsement.EndorsementFinger = null;
                    endorsement.ProcessStatus = PrintProcessStatus.InProcess;
                    PrintModificationDispatcher.PrintModified(endorsement);
                    PrintCaptureApp.SequenceCheck.AddPrint(endorsement);
                }
            }
            else
            {
                // Print already captured
                this.printOp.ProcessZoomWindow(e.ViewModel);                
            }
        }       
        
        private void ScanRibbonButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.InCaptureMode = true;
            var scanCommandResult = PrintCaptureDriver.Instance.CaptureAuto();

            if (scanCommandResult == ScanResult.NoDevice || scanCommandResult == ScanResult.Canceled)
            {
                this.ViewModel.InCaptureMode = false;
            }
        }

        private void ConfigureButtonClick(object sender, RoutedEventArgs e)
        {
            this.ShowConfigurationWindow();
        }

        private void ResetCaptureRibbonButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Text.ResetConfirmationPrompt, Text.ConfirmationTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            PrintCaptureApp.SequenceCheck.ResetSession();
            this.viewModel.InCaptureMode = false;
            var data = PrintCaptureApp.Instance.PrintList;

            foreach (var print in data.Prints)
            {
                print.Reset();
                print.PhysicalPart.MissingCode = null;
                print.PhysicalPart.MissingDate = null;
                PrintModificationDispatcher.PrintModified(print);
                //var vm = data.GetViewModel(print);
                //this.ViewModel.PrintGridViewModel.Update(vm);
            }
        }

        private void ScanEndorsementButtonClick(object sender, RoutedEventArgs e)
        {
            var endorsementPrint = this.viewModel.PrintGridViewModel.EndorsementFinger.LinkedPrint;
            
            PrintCaptureDriver.Instance.CaptureSingle(endorsementPrint);            
        }

        private void ScanImageMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.printOp.ProcessZoomWindow(this.viewModel.PrintGridViewModel.EndorsementFinger);            
        }

        private void ConsentButtonClick(object sender, RoutedEventArgs e)
        {
            var vm = new EndorsementConsentViewModel();
            vm.CurrentCulture = PrintCaptureApp.ApplicationCulture.TwoLetterISOLanguageName;
            vm.IndividualName = this.viewModel.IdLine1;

            var win = new EndorsementConsentWindow(vm);
            win.ShowDialog();
        }

        private void RibbonWindowClosing(object sender, CancelEventArgs e)
        {
            if (!PrintCaptureApp.SequenceCheck.Connected)
            {
                return;
            }

            if (!this.AcceptPrint.HasValue)
            {

                if (MessageBox.Show(Text.CloseConfirmationPrompt, Text.ConfirmationTitle, MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)                
                {
                    e.Cancel = true;
                }
                else
                {
                    this.AcceptPrint = false;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CaptureModeToggleChecked(object sender, RoutedEventArgs e)
        {
            var toggle = (RibbonToggleButton)sender;
            var captureMode = (PrintCaptureGroup)toggle.Tag;            
            this.ViewModel.Rules.CaptureMode = captureMode;
            var list = PrintCaptureApp.Instance.PrintList;

            // reset missing reasons for slaps only !!
            var slaps = list.Prints.Where(x => x.IsSlap);
            foreach (var printInfo in slaps)
            {
                if (printInfo.IsMissing)
                {
                    printInfo.PhysicalPart.MissingCode = null;
                    printInfo.PhysicalPart.MissingDate = null;
                    PrintModificationDispatcher.PrintModified(printInfo);                    
                }
            }

            //foreach (var part in list.PhysicalParts )
            //{
            //    if (part.IsMissing)
            //    {
            //        this.UpdateMissingInfo(part, null, null);
            //    }
            //}            

        }        

        private void RestartServiceButtonClick(object sender, RoutedEventArgs e)
        {
            SplashWindowHelper.Show();
            SplashWindowHelper.SetMessage("Restarting services", false);

            var action = new Action(
                () =>
                {
                    
                    PrintCaptureApp.SequenceCheck.ResetSession();
                    PrintCaptureApp.SequenceCheck.StartSession();

                    var list = PrintCaptureApp.Instance.PrintList.Prints.Where(x => x.Image != null).ToList();

                    foreach (var printInfo in list)
                    {
                        printInfo.ProcessStatus = PrintProcessStatus.InProcess;
                        PrintModificationDispatcher.PrintModified(printInfo);
                    }
                    PrintCaptureApp.SequenceCheck.AddPrintRange(list);
                    SplashWindowHelper.Hide();
                });

            Task.Factory.StartNew(action);            
        }

        private void SavePrintsButtonClick(object sender, RoutedEventArgs e)
        {
            var od = new FolderBrowserDialog();
            var result = od.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var folder = od.SelectedPath;
            if (!folder.EndsWith("\\"))
            {
                folder += "\\";
            }

            foreach (var printInfo in PrintCaptureApp.Instance.PrintList.Prints)
            {
                if (!printInfo.IsMissing && printInfo.ImageForProcessing != null)
                {
                    var path = folder + printInfo.NistPosition + ".bmp";
                    printInfo.ImageForProcessing.Save(path, ImageFormat.Bmp);
                }
            }

            MessageBox.Show("Prints exported");
        }        

        private void RepositionRibbonButtonClick(object sender, RoutedEventArgs e)
        {
            PrintCaptureDriver.Instance.CapturePositions();
        }

        private void ServiceStatusItemDoubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.ViewModel.ServiceConnected)
            {
                if (MessageBox.Show(
                    CommonText.ResetConnectionMessage,
                    Text.ApplicationTitle,
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return;
                }

                this.RestartServiceButtonClick(sender, null);
            }
        }

        private void MainWindowActivated(object sender, EventArgs e)
        {
            var win = System.Windows.Application.Current.Windows.Cast<System.Windows.Window>().FirstOrDefault(x => x.Owner == this);

            if (win == null)
            {
                return;
            }            

            win.Activate();
        }

        private void VersionTextMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && e.ClickCount == 2))
            {
                return;
            }

            this.OptionRibbonTab.IsEnabled = !this.OptionRibbonTab.IsEnabled;
            this.DebugRibbonTab.IsEnabled = !this.DebugRibbonTab.IsEnabled;
        }

        private void RibbonLoaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        private void ResetSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (PrintCaptureApp.Instance.PrintList.Rules.CaptureKind == CaptureKind.Cardscan)
            {
                AppSettings.DeleteSettings<CardScanSettings>(PrintCaptureApp.AppName);
            }
            else
            {
                AppSettings.DeleteSettings<LivescanSettings>(PrintCaptureApp.AppName);
            }

            MessageBox.Show(string.Format("Settings for '{0}' have been deleted.", PrintCaptureApp.Rules.CaptureKind));
        }

        private void PrintGridControlOnContextClick(object sender, PrintContextMenuClickArgs e)
        {
            PrintCaptureDriver.Instance.DoOperation(e.Print.LinkedPrint, e.Action);
        }

        private void TestSequenceButtonClick(object sender, RoutedEventArgs e)
        {
            PrintCaptureApp.SequenceCheck.Test();
        }

        private void ResumeRibbonButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.InCaptureMode = true;
            var scanCommandResult = PrintCaptureDriver.Instance.CapturePositions();

            if (scanCommandResult == ScanResult.NoDevice || scanCommandResult == ScanResult.Canceled)
            {
                this.ViewModel.InCaptureMode = false;
            }
        }
    }
}
