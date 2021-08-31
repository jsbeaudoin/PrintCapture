namespace PrintsCapture.Cardscan.Window
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for FingerEditor.xaml
    /// </summary>
    public partial class FingerEditorWindow
    {
        private ImageModifier imgModifier;

        private bool isZoomed;

        private readonly Dictionary<string, ModifierTool> toolTags;        

        public FingerEditorWindow()
        {
            InitializeComponent();
            this.toolTags = new Dictionary<string, ModifierTool>();
            this.toolTags.Add("select", ModifierTool.Select);
            this.toolTags.Add("move", ModifierTool.Move);

            this.ChangeZoom();
        }

        public bool? EditFinger(Bitmap image, Bitmap originalImage, string printName, int cropTolerance)
        {
            this.FingerNameLabel.Text = printName;
            this.imgModifier = new ImageModifier(image, originalImage, this.EditedImage, cropTolerance);
            this.imgModifier.MoveTemplate = this.Resources["ImageDragger"] as ControlTemplate;
            this.imgModifier.SelectTemplate = this.Resources["ImageSelector"] as ControlTemplate;
            return this.ShowDialog();
        }                

        public Bitmap ResultImage 
        {
            get
            {
                return this.imgModifier.GetResultImage();
            }
        }

        private void PivotButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn == null || btn.Tag == null)
            {
                return;
            }

            int angle;
            if (!int.TryParse(btn.Tag.ToString(), out angle))
            {
                return;
            }

            this.imgModifier.Pivot(angle);
        }        

        private void ToolCheckedChange(object sender, RoutedEventArgs e)
        {
            var tg = sender as RadioButton;

            if (tg == null || tg.Tag == null || ! (tg.IsChecked.HasValue && tg.IsChecked.Value))
            {
                return;
            }

            var key = tg.Tag.ToString();
            if (this.toolTags.ContainsKey(key))
            {
                this.imgModifier.CurrentTool = this.toolTags[key];
            }

            Mouse.OverrideCursor = null;
        }       

        private void MirrorHorizontalButtonClick(object sender, RoutedEventArgs e)
        {
            this.imgModifier.MirrorHorizontal();
        }

        private void MirrorVerticalButtonClick(object sender, RoutedEventArgs e)
        {
            this.imgModifier.MirrorVertical();
        }

        private void BrightnessSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.imgModifier == null)
            {
                return;
            }

            var value = this.BrightnessSlider.Value;
            var ratio = (float)(value / 100F);

            this.imgModifier.SetBrightness(ratio);
        }

        private void ContrastSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.imgModifier == null)
            {
                return;
            }

            var value = this.ContrastSlider.Value;
            var ratio = (float)(value / 100F);

            this.imgModifier.SetContrast(ratio);
        }

        private void RestoreButtonClick(object sender, RoutedEventArgs e)
        {
            this.BrightnessSlider.Value = 100;
            this.ContrastSlider.Value = 100;

            this.imgModifier.RestoreImage();
        }        

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void EmptyZoneClick(object sender, RoutedEventArgs e)
        {
            this.imgModifier.DeleteSectedZone();
        }

        

        private void BackgroundToWhiteButtonClick(object sender, RoutedEventArgs e)
        {
            this.imgModifier.BackgroundWhitening();
            this.ResetBrightnessContrast();
        }

        private void CropCenterClick(object sender, RoutedEventArgs e)
        {
            this.imgModifier.CropAndCenter();
            this.ResetBrightnessContrast();
        }        

        private void ZoomButtonClick(object sender, RoutedEventArgs e)
        {
            this.ChangeZoom();
        }

        private void ChangeZoom()
        {
            if (this.isZoomed)
            {
                // zoom out !
                BindingOperations.ClearBinding(this.EditedImage, HeightProperty);
                BindingOperations.ClearBinding(this.EditedImage, WidthProperty);

                this.EditedImage.Width = Double.NaN;
                this.EditedImage.Height = Double.NaN;

                this.ImageViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                this.ImageViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                this.ZoomInButton.IsEnabled = false;
                this.ZoomOutButton.IsEnabled = true;

                this.isZoomed = false;
            }
            else
            {
                var widthBinding = new Binding("ActualWidth");
                widthBinding.Source = this.ImageViewer;

                var heightBinding = new Binding("ActualHeight");
                heightBinding.Source = this.ImageViewer;

                this.EditedImage.SetBinding(WidthProperty, widthBinding);
                this.EditedImage.SetBinding(HeightProperty, heightBinding);
                

                this.ImageViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                this.ImageViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                this.ZoomInButton.IsEnabled = true;
                this.ZoomOutButton.IsEnabled = false;

                this.isZoomed = true;
            }
        }

        private void ResetBrightnessContrast()
        {
            this.BrightnessSlider.Value = 100;
            this.ContrastSlider.Value = 100;
        }

        private void ModeCheckedChange(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;

            if (!(rb.IsChecked.HasValue && rb.IsChecked.Value))
            {
                return;
            }

            // when called before control initialization is completed
            if (rb.Tag == null)
            {
                return;
            }

            var isEdit = rb.Tag.ToString() == "edit";

            if (isEdit)
            {
                this.PivotOperationGrid.Visibility = Visibility.Collapsed;
                this.OperationGrid.Visibility = Visibility.Visible;
                this.imgModifier.IsMoveEnabled = true;
            }
            else
            {
                this.PivotOperationGrid.Visibility = Visibility.Visible;
                this.OperationGrid.Visibility = Visibility.Collapsed;
                this.imgModifier.IsMoveEnabled = false;
            }

            this.imgModifier.CompletePivot();
            this.ResetBrightnessContrast();
        }        

        private void ThumbResizerMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void ThumbResizerMouseEnter(object sender, MouseEventArgs e)
        {
            if (this.imgModifier.IsMoveEnabled)
            {
                Mouse.OverrideCursor = Cursors.SizeNWSE;
            }
            
        }

        private void ImageSelectorMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void ImageSelectorMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.ScrollAll;
        }        

    }
}
