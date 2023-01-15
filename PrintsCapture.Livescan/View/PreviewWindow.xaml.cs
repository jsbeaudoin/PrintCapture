using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Shapes;
using NLog;
using PrintsCapture.Device;
using PrintsCapture.Device.Enum;
using PrintsCapture.Livescan.ViewModel;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using XL_ID.Utilities.Wpf.WindowHelper;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace PrintsCapture.Livescan.View
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : INotifyPropertyChanged
    {
        private LivePreviewViewModel viewModel;
        protected Logger Logger { get; private set; }

        public PreviewWindow(LivePreviewViewModel viewModel) : base()
        {
            this.Logger = LogManager.GetCurrentClassLogger();
            this.Owner = WindowHelper.GetWindowByTag("Main");
            this.viewModel = viewModel;
            InitializeComponent();

            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

            this.PreviewBox = new PictureBox { BackColor = Color.Gray };
            
            this.Win32Window.Child = this.PreviewBox;
            this.PreviewBox.Location = new Point(0, 0);
            this.PreviewBox.BackColor = Color.LightGray;
            
            PrintModificationDispatcher.AddWatch(this.RefreshIfPrintModified);

            this.Closed += (sender, e) => PrintModificationDispatcher.RemoveWatch(this.RefreshIfPrintModified);

#if DEBUG
            this.Topmost = false;
#endif 
        }

        public System.Windows.Controls.Image GetDisplayImage()
        {
            return this.DisplayImage;
        }

        public WindowsFormsHost GetWin32Window()
        {
            return this.Win32Window;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

            base.OnSourceInitialized(e);
        }

        public PictureBox PreviewBox { get; private set; }
        
        public LivePreviewViewModel ViewModel
        {
            get
            {
                return this.viewModel;
            }
            set
            {
                if (Equals(value, this.viewModel))
                {
                    return;
                }
                this.viewModel = value;
                this.OnPropertyChanged("ViewModel");
            }
        }        

        private void PreviewCancelClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.KeepWindowOpen = false;
            this.Close();
        }        

        public int GetPreviewHandle()
        {
            return this.PreviewBox.Handle.ToInt32();
        }

        public void ResetQuality()
        {
            this.ResetHandQuality(this.LeftHandQuality.Children);
            this.ResetHandQuality(this.RightHandQuality.Children);
        }
        

        private void RefreshIfPrintModified(object sender, PrintModifiedEventArgs e)
        {
            this.Logger.Trace("PreviewWindow RefreshIfPrintModified");
            var previousViewModel = this.ViewModel.PreviousPrint;

            if (previousViewModel != null && e.Info == previousViewModel.LinkedPrint)
            {
                var vm = e.Info.PrintList.GetViewModel(e.Info);
                this.ViewModel.PreviousPrint = vm;
            }
        }

        private void ResetHandQuality(UIElementCollection children)
        {
            foreach (Shape item in children)
            {
                if (item.Tag != null)
                {
                    item.Fill = Brushes.Gray;
                }
            }
        }

        public void RefreshQualityInformation(IEnumerable<PrintCaptureQuality> qualityList)
        {
            var msg = string.Empty;
            if (qualityList != null)
            {
                foreach (var printCaptureQuality in qualityList)
                {
                    msg += printCaptureQuality.HandPart.HandPart.ToString() + ": " + printCaptureQuality.Quality.ToString() + "  , ";
                    this.DisplayQuality(printCaptureQuality.HandPart.Hand, printCaptureQuality.HandPart.HandPart, printCaptureQuality.Quality);
                }

            }
            
            this.QualityDebugLabel.Text = msg;

            //this.QualityLabel.Content = msg;            
        }        

        private void DisplayQuality(Hand hand, HandPart part, PrintQualityLevel quality)
        {
            var handCanvas = hand == Hand.Left ? this.LeftHandQuality : this.RightHandQuality;                        

            Brush fillBrush = null;
            switch (quality)
            {
                case PrintQualityLevel.Bad:
                    fillBrush = Brushes.Red;
                    break;
                case PrintQualityLevel.NotGoodEnough:
                    fillBrush = Brushes.Yellow;
                    break;
                case PrintQualityLevel.Good:
                    fillBrush = Brushes.Green;
                    break;
                case PrintQualityLevel.NotPresent:
                    fillBrush = Brushes.Black;
                    break;
                case PrintQualityLevel.Unknown:
                    fillBrush = Brushes.DarkOrange;
                    break;
            }

            foreach (Shape item in handCanvas.Children)
            {
                if (part.Equals(item.Tag))
                {
                    item.Fill = fillBrush;
                    return;
                }
            }            
        }

        private void BorderLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;

            // wait for handle creation, because display of preview usually depends on that
            // Since Win32 Handle can be created before the window is loaded, but that nothing can't be displayed
            //  when window is not loaded, 
            var waitIteration = 0;
            while (!this.PreviewBox.IsHandleCreated && waitIteration < 100)
            {
                Thread.Sleep(25);
                waitIteration++;
            }

            this.ViewModel.OnWin32HandleCreated();            
        }                

        private void ResumeCaptureButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OnResume();
        }

        public event PropertyChangedEventHandler PropertyChanged;
      
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ScanAgainClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.OnScanPreviousPrint();
        }
    }
}
