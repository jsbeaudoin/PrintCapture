using System.Drawing.Imaging;
using Microsoft.Win32;
using NLog;

namespace PrintsCapture.Cardscan.Window
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;    
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
   
    using ViewModel;
    using Prints.Enum;
    using Prints.ViewModel;


    using Rectangle = System.Windows.Shapes.Rectangle;

    /// <summary>
    /// Interaction logic for PageScanWindow.xaml
    /// </summary>
    public partial class PageScanWindow : Window, INotifyPropertyChanged
    {
        private readonly PrintResolution resolution;

        private readonly List<EndorsableFingerViewModel> endorsableFingers;        

        private readonly Bitmap sourceImage;

        private PageScanViewModel viewModel;

        public PageScanWindow(Bitmap bmp, PrintResolution resolution, List<EndorsableFingerViewModel> endorsableFingers, string associatedModelName, bool isFlat)
        {            
            this.resolution = resolution;
            this.endorsableFingers = endorsableFingers;
            this.sourceImage = bmp;            
            InitializeComponent();

            this.pageScan = new PageScan(bmp, this.resolution.ToDpi(), isFlat);
            
            this.ViewModel = this.pageScan.ViewModel;                                                

            this.GlobalZoneThumb.Margin = new Thickness(0,0,0,0);
            
            PageScan.DisplayedImage = this.ScannedImage;

            this.ViewModel.CurrentZoom = CardScanSettings.Default.ZoomLevel;

            this.viewModel.DpiChanged += this.ViewModelOnDpiChanged;

            this.viewModel.BrightnessContrastChanged += ViewModelOnBrightnessContrastChanged;

            this.ChangeImageZone();

            this.RestoreModel(associatedModelName);                                    

            this.ViewModel.ImageInformation = $"{bmp.Width} x {bmp.Height}  @ {resolution}";
        }

        private void ViewModelOnBrightnessContrastChanged(object sender, EventArgs eventArgs)
        {
            var brightness = this.viewModel.CurrentBrightness / 100F;
            var contrast = this.viewModel.CurrentContrast / 100F;
            
            this.pageScan.AdjustContrastBrightness(brightness, contrast);
        }

        private void ViewModelOnDpiChanged(object sender, EventArgs eventArgs)
        {
            this.ChangeImageZone();

            foreach (var zone in this.pageScan.ScanZones)
            {
                zone.Adapt();
            }
        }

        public List<PrintZoneInfo> GetScannedFingerList()
        {
            return this.pageScan.ScanZones.Select(zone => zone.PrintZone).ToList();
        }

        public PageScanResultViewModel GetResults()
        {
            var result = new PageScanResultViewModel();            

            foreach (var zone in this.pageScan.ScanZones.OrderBy(x => x.PrintZone.SortOrder))
            {
                var zoneResult = new PageScanZoneResultViewModel();
                zoneResult.Image = this.pageScan.CaptureZone(zone);
                zoneResult.ZoneInfo = zone.PrintZone;
                zoneResult.Resolution = this.resolution;
                result.Results.Add(zoneResult);
            }

            return result;
        }

        private readonly PageScan pageScan;        

        public PageScanViewModel ViewModel
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

        

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PrintLabelDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lbl = sender as Label;

            if (lbl == null)
            {
                return;
            }

            var id = lbl.Tag.ToString();

            var printZoneInfo = this.ViewModel.PrintList.SingleOrDefault(x => x.Id == id);

            this.ViewModel.PrintList.Remove(printZoneInfo);

            this.AddNewZone(printZoneInfo);
        }



        private void ApplyModel(ScanModel model)
        {

            this.ClearZones();

            foreach (var printZone in model.Zones)
            {
                this.AddScanZone(printZone);
            }

            this.viewModel.CurrentModel = model;
        }

        private void ClearZones()
        {
            // clear zones
            foreach (var printZone in this.pageScan.ScanZones)
            {
                this.DeleteZone(printZone);
            }

            this.pageScan.ScanZones.Clear();
        }

        private void AddNewZone(PrintZoneInfo zone)
        {                        
            var pz = new ScanPrintZone { PrintZone = zone, Location = new ScanLocation(0, 0), Size = zone.Size };

            this.AddScanZone(pz);
        }        

        private void AddScanZone(ScanPrintZone scanPrintZoneOriginal)
        {
            var o = scanPrintZoneOriginal;
            var scanPrintZone = new ScanPrintZone
                                {
                                    Id = o.Id,
                                    Location =
                                        new ScanLocation(
                                        o.Location.Unit,
                                        o.Location.Top,
                                        o.Location.Left),
                                    PrintZone = o.PrintZone,
                                    Rotation = o.Rotation,
                                    Size = new ScanSize(o.Size.Unit, o.Size.Width, o.Size.Height),
                                    Thumb = o.Thumb
                                };

            var templateName = scanPrintZone.PrintZone.HandPart != HandPart.Endorsement ?
                "FingerZone" : "EndorsementFingerZone";
            this.ViewModel.PrintList.Remove(scanPrintZone.PrintZone);            

            var thumb = new Thumb();
            thumb.Cursor = Cursors.ScrollAll;
            thumb.ForceCursor = true;
            thumb.VerticalAlignment = VerticalAlignment.Top;
            thumb.HorizontalAlignment = HorizontalAlignment.Left;

            thumb.Template = this.Resources[templateName] as ControlTemplate;
            thumb.ApplyTemplate();
            thumb.DragDelta += this.MoveZone;
            thumb.DragCompleted += this.MoveZoneCompleted;
            //thumb.MouseLeftButtonUp += ZoneClick;

            if (scanPrintZone.PrintZone.HandPart == HandPart.Endorsement)
            {
                var zone= new EndorsementZoneInfo(scanPrintZone.PrintZone, this.endorsableFingers)
                          {
                              EndorsementIndex = this.endorsableFingers.First().Index
                          };
                scanPrintZone.PrintZone = zone;                
                var cbo = thumb.Template.FindName("ComboBox", thumb) as ComboBox;
                cbo.ItemsSource = zone.EndorsableFingers;
                cbo.SelectedValuePath = "Index";
                cbo.DisplayMemberPath = "Name";

                var binding = new Binding("EndorsementIndex") { Source = zone };
                cbo.SetBinding(Selector.SelectedValueProperty, binding);
                //cbo.SelectedValue
                //ItemsSource="{Binding EndorsableFingers}" DisplayMemberPath="Name" 
                //          SelectedValuePath="Index" SelectedValue="{Binding EndorsementIndex}"
            }

            var lbl = thumb.Template.FindName("Label", thumb) as TextBlock; 
            lbl.Text = scanPrintZone.PrintZone.Label;

            var rect = thumb.Template.FindName("ColourRectangle", thumb) as Rectangle;
            if (scanPrintZone.PrintZone.Hand == Hand.Left || scanPrintZone.PrintZone.Hand == Hand.Right)
            {
                rect.Fill = scanPrintZone.PrintZone.Hand == Hand.Left ? ScanConstants.LeftFingerBrush : ScanConstants.RightFingerBrush;
            }
            
            rect.MouseLeftButtonDown += this.ZoneClick;
            rect.Tag = scanPrintZone;
            //rect.MouseDown += this.ZoneClick;
            this.ImageZoneGrid.Children.Add(thumb);

            scanPrintZone.Thumb = thumb;            

            this.pageScan.ScanZones.Add(scanPrintZone);

            scanPrintZone.Adapt();

            thumb.UpdateLayout();

            //this.pageScan.CaptureZone(scanPrintZone);
        }

        private void MoveZoneCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
            {
                return;
            }

            // Else, move that thing !
            var masterThumb = this.GetMasterThumb(thumb);            
            if (masterThumb.Tag != null && string.Equals(masterThumb.Tag, "resize"))
            {
                return;
            }
            
            this.DisplayCroppedZone();
        }

        private void DeleteCurrentZone()
        {
            var zn = this.pageScan.ViewModel.CurrentZone.LinkedZone;

            this.DeleteZone(zn);

            this.pageScan.ScanZones.Remove(zn);
            this.UnselectZone();
        }

        private void DeleteZone(ScanPrintZone zn)
        {
            this.ImageZoneGrid.Children.Remove(zn.Thumb);

            // remove events
            zn.Thumb.DragDelta -= this.MoveZone;
            zn.Thumb.DragCompleted -= this.MoveZoneCompleted;

            var rect = zn.Thumb.Template.FindName("ColourRectangle", zn.Thumb) as Rectangle;            
            rect.MouseLeftButtonDown -= this.ZoneClick;

            // reinsert print info !

            var list = this.pageScan.ViewModel.PrintList;
            list.Add(zn.PrintZone);
            this.pageScan.ViewModel.PrintList = new ObservableCollection<PrintZoneInfo>(list.OrderBy(x => x.SortOrder));

        }


        private void UnselectZone()
        {
            this.pageScan.ViewModel.CurrentZone = null;
        }

        private void MoveZone(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
            {
                return;
            }            

            // Else, move that thing !
            var masterThumb = this.GetMasterThumb(thumb);
            if (masterThumb.Tag != null && string.Equals(masterThumb.Tag, "resize"))
            {
                return;
            }

            var zone = this.pageScan.ScanZones.Single(x => x.Thumb == thumb);
            zone.MoveByOffsetPixel(e.HorizontalChange, e.VerticalChange);                      
        }       

        private Thumb GetMasterThumb(Thumb thumb)
        {
            var control = thumb;
            // find Master thumb, then change zone
            if (thumb.TemplatedParent != null)
            {
                var parent = thumb.TemplatedParent as Thumb;

                control = parent;
            }

            return control;

        }        

        private ScanPrintZone SelectCurrentZone(ScanPrintZone zone)
        {
            if (zone == null || (this.ViewModel.CurrentZone != null && this.ViewModel.CurrentZone.LinkedZone == zone))
            {
                return zone;
            }

            if (this.viewModel.CurrentZone != null)
            {
                Grid.SetZIndex(this.viewModel.CurrentZone.LinkedZone.Thumb, 0);
            }          

            this.ViewModel.CurrentZone = new ZoneViewModel(zone);

            Grid.SetZIndex(zone.Thumb, 1);

            return zone;
        }

        private void ZoneClick(object sender, RoutedEventArgs e)
        {
            var rect = sender as Rectangle;
            if (rect == null)
            {
                return;
            }                          
            
            var scanPrintZone = rect.Tag as ScanPrintZone;
            this.SelectCurrentZone(scanPrintZone);
        }        

        private void ResizeStarted(object sender, DragStartedEventArgs e)
        {
            var masterThumb = this.GetMasterThumb(sender as Thumb);

            masterThumb.Tag = "resize";
        }

        private void ResizeInProgress(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var masterThumb = this.GetMasterThumb(thumb);
            var linkedPageScan = this.pageScan.ScanZones.Single(x => x.Thumb == masterThumb);            

            linkedPageScan.ResizeByPixelChange(e.HorizontalChange, e.VerticalChange);
        }        

        void DisplayCroppedZone()
        {
            PageScan.CaptureCurrentZone();            
        }

        private void ResizeCompleted(object sender, DragCompletedEventArgs e)
        {
            var masterThumb = this.GetMasterThumb(sender as Thumb);

            masterThumb.Tag = null;
            
            this.DisplayCroppedZone();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteCurrentZone();
        }

        private void PivotButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || this.ViewModel.CurrentZone == null)
            {
                return;
            }

            int pivot = 0;
            if (!int.TryParse(btn.Tag.ToString(), out pivot))
            {
                return;
            }

            this.ViewModel.CurrentZone.Rotation += pivot;
            //this.DisplayCroppedZone();
        }

        

        private void ModelManageButton_Click(object sender, RoutedEventArgs e)
        {                                    
            var currentModel = this.viewModel.ModelList.Manage(this.viewModel.CurrentModel, this.pageScan.ScanZones);

            this.viewModel.CurrentModel = currentModel;
        }

        private void RememberLastModel()
        {
            var lastModel = this.viewModel.ModelList.SingleOrDefault(x => x.Name == "@");
            if (lastModel == null)
            {
                lastModel = new ScanModel { Name = "@", IsStarred = true, IsReadOnly = true };
                this.viewModel.ModelList.Add(lastModel);
            }

            lastModel.CreationDateTime = DateTime.Now;
            lastModel.Zones = this.CopyZones(this.pageScan.ScanZones);

            CardScanSettings.Default.ScanModels = this.viewModel.ModelList;
            CardScanSettings.Save();

            //Environment.GetFolderPath(Environment.SpecialFolder.MyComputer, Environment.SpecialFolderOption.DoNotVerify);
        }

        private List<ScanPrintZone> CopyZones(IEnumerable<ScanPrintZone> zones)
        {
            var newList = new List<ScanPrintZone>();
            newList.AddRange(zones);

            return newList;
        }

        private void RestoreModel(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                modelName = "@";
            }

            var lastModel = this.viewModel.ModelList.SingleOrDefault(x => x.Name == modelName);
            if (lastModel == null)
            {
                return;
            }
            LoadAndApplyModel(lastModel);
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            CardScanSettings.Default.ZoomLevel = this.viewModel.CurrentZoom;

            this.RememberLastModel();                       

            this.DialogResult = true;
            this.Close();
        }

        private void ModelLabelClick(object sender, MouseButtonEventArgs e)
        {            
            var selectedModel = (ScanModel)this.ModelListBox.SelectedValue;

            if (selectedModel == null)
            {
                this.ViewModel.CurrentModel = selectedModel;
                return;
            }
            LoadAndApplyModel(selectedModel);            
        }

        private void LoadAndApplyModel(ScanModel model)
        {
            try
            {                
                foreach (var zone in model.Zones)
                {
                    var printInfo = this.viewModel.AllPrints.SingleOrDefault(x => x.Id == zone.Id);
                    zone.PrintZone = printInfo;
                }
                this.ApplyModel(model);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void ModelStarMouseDown(object sender, MouseButtonEventArgs e)
        {
            // filter on double click only
            if (e.ClickCount != 2 || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            // get element
            var modelSelected = (ScanModel)this.ModelListBox.SelectedItem;

            modelSelected.IsStarred = !modelSelected.IsStarred;

        }

        private void GlobalZoneMoved(object sender, DragDeltaEventArgs e)
        {
            var marg = this.GlobalZoneThumb.Margin;
            var newMarg = new Thickness(marg.Left + e.HorizontalChange, marg.Top + e.VerticalChange,0,0);            

            foreach (var zone in this.pageScan.ScanZones)
            {
                zone.MoveByOffsetPixel(e.HorizontalChange, e.VerticalChange, false);                
            }

            this.GlobalZoneThumb.Margin = newMarg;
        }

        private void GlobalZoneMoveCompleted(object sender, DragCompletedEventArgs e)
        {
            this.GlobalZoneThumb.Margin = new  Thickness(0,0,0,0);

            foreach (var zone in this.pageScan.ScanZones)
            {
                zone.CommitLocationFromControl();
            }

            Mouse.OverrideCursor = null;
        }

        private void ThumbResizeBottomRightMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeNWSE;
        }

        private void ThumbResizeBottomRightMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void ThumbMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.ScrollAll;
        }

        private void ThumbMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void GlobalZoneStartedMove(object sender, DragStartedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.ScrollAll;
        }

        /// <summary>
        /// Adapt image according to source Bitmap and display Dpi
        /// </summary>
        private  void ChangeImageZone()
        {
            var dpi = ScanConstants.CurrentDpi;
            this.ScannedImage.Width = (this.sourceImage.Width / this.sourceImage.HorizontalResolution) * (double)dpi;
            this.ScannedImage.Height = (this.sourceImage.Height / this.sourceImage.VerticalResolution) * (double)dpi;

            this.GlobalZoneThumb.Width = this.ScannedImage.Width;
            this.GlobalZoneThumb.Height = this.ScannedImage.Height;
        }

        private void ZoomButtonClick(object sender, RoutedEventArgs e)
        {            
            var btn = (Button)sender;
           
            var newIndex = this.viewModel.CurrentZoom + (btn.Tag.ToString() == "out" ? -1 : 1);

            if (newIndex < this.viewModel.MinZoom || newIndex > this.viewModel.MaxZoom)
            {
                return;
            }

            this.viewModel.CurrentZoom = newIndex;                        
        }

        private void QuickPrintSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            var item = e.AddedItems[0] as QuickPrintsViewModel;

            if (item == null)
            {
                return;
            }

            var currentTop = 0D;
            var currentLeft = 0D;
            var count = 0;

            this.ClearZones();

            foreach (var s in item.Ids)
            {
                count += 1;
                string s1 = s;
                var printZoneInfo = this.ViewModel.PrintList.SingleOrDefault(x => x.Id == s1);

                this.ViewModel.PrintList.Remove(printZoneInfo);

                var pz = new ScanPrintZone { PrintZone = printZoneInfo, Location = new ScanLocation( ScanUnit.Pixels, currentTop, currentLeft), Size = printZoneInfo.Size };

                if (count % 5 == 0)
                {
                    // change row
                    currentLeft = 0;
                    currentTop += printZoneInfo.Size.ToPixels().Height;
                }
                else
                {
                    currentLeft += printZoneInfo.Size.ToPixels().Width;
                }

                this.AddScanZone(pz);
            }

            this.QuickPartComboBox.SelectedItem = null;
        }

        private void ImageInformationDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left))
            {
                return;
            }

            // save dialog !
            var sfd = new SaveFileDialog();
            sfd.Filter = "Bmp file | *.bmp";
            sfd.AddExtension = true;
            sfd.CheckPathExists = true;
            sfd.DefaultExt = "bmp";

            if (!sfd.ShowDialog() == true)
            {
                return;
            }

            try
            {
                // save the bmp !
                this.sourceImage.Save(sfd.FileName, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Could not save Bitmap to path : '{0}'", sfd.FileName);
                MessageBox.Show("Cannot save Image : " + ex.Message, "Save error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
            }
        }
    }
}
