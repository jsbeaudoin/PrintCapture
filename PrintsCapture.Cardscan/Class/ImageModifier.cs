namespace PrintsCapture.Cardscan
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using NLog;

    using XL_ID.Utilities.Image;

    using Brushes = System.Drawing.Brushes;
    using Color = System.Drawing.Color;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using Point = System.Windows.Point;

    public enum ModifierTool
    {
        Move,
        Select
    }

    public class ImageModifier
    {
        private readonly Bitmap imageBeforeAnyEdit; // original 8 bit image

        private readonly Bitmap imageToEdit; // original 8 bit image

        private readonly System.Windows.Controls.Image displayImageControl;

        private readonly Panel parentPanel;

        private Bitmap qualityImage; // 24 bit. never any rotation in quality image, so no quality loss for non 90 degrees rotation

        private Bitmap modifiedImage; // 24 bits with all modifications.

        private int currentAngle = 0; // cumulative rotation angle        

        const int DefaultSelectSize = 28;

        private float contrast = 1.0F;

        private float brightness = 1.0F;        

        private Thumb moveZone;

        private Thumb selectZone;

        private ControlTemplate moveTemplate;

        private ControlTemplate selectTemplate;

        private ModifierTool currentTool;

        private int cropTolerance;

        private WriteableBitmap writeableBitmap;

        public ImageModifier(Bitmap imageToEdit, Bitmap imageBeforeAnyEdit, System.Windows.Controls.Image displayImageZone, int cropTolerance)
        {
            this.imageBeforeAnyEdit = imageBeforeAnyEdit;
            this.imageToEdit = imageToEdit;
            this.displayImageControl = displayImageZone;
            this.cropTolerance = cropTolerance;

            this.parentPanel = this.displayImageControl.Parent as Panel;
            this.parentPanel.MouseLeftButtonDown += this.ParentLeftButtonDown;


            //this.parentCanvas.MouseLeftButtonUp += CanvasLeftButtonClick;
           // this.parentCanvas.MouseDown += this.CanvasLeftButtonDown;
            this.writeableBitmap = new WriteableBitmap(imageToEdit.Width, imageToEdit.Height, imageToEdit.HorizontalResolution, imageToEdit.VerticalResolution, PixelFormats.Rgb24, null );
            this.displayImageControl.Source = this.writeableBitmap;
            this.CreateOriginalImages();

            this.DisplayImage();
            this.CreateMoveZone();
            this.CreateSelectZone();
        }

        private void ParentLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var el = sender as IInputElement;
            if (this.currentTool != ModifierTool.Select || el == null)
            {
                return;
            }

            Point position = Mouse.GetPosition(el);
            this.selectZone.Margin = new Thickness(position.X, position.Y, 0, 0);

            this.selectZone.Width = DefaultSelectSize;
            this.selectZone.Height = DefaultSelectSize;

            this.selectZone.Visibility = Visibility.Visible;
            // code below was used to initiate a resize when select rectangle was created.

            //if (this.selectZone.Template == null)
            //{
            //    return;
            //}

            var resizeBr = this.selectZone.Template.FindName("ResizeBottomRight", this.selectZone) as Thumb;

            if (resizeBr == null)
            {
                return;
            }

            resizeBr.CaptureMouse();

            var e2 = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
            e2.RoutedEvent = e.RoutedEvent;
            resizeBr.RaiseEvent(e2);

            //this.selectZone.RaiseEvent(mouseButtonEventArgs);

            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new Action(() => this.selectZone.RaiseEvent(mouseButtonEventArgs)));
        }

        public Bitmap GetResultImage()
        {
            var img8Bits = ImageUtilities.ConvertToIndexedFormat(
                this.modifiedImage,
                ConvertBitmapFormat.Format8bppIndexed);
            img8Bits.SetResolution(this.imageBeforeAnyEdit.HorizontalResolution, this.imageBeforeAnyEdit.VerticalResolution);
            return img8Bits;
        }

        public bool IsMoveEnabled
        {
            get
            {
                return this.moveZone.IsEnabled;
            }
            set
            {
                this.moveZone.IsEnabled = value;
            }
        }

        public ModifierTool CurrentTool
        {
            get
            {
                return this.currentTool;
            }
            set
            {
                this.currentTool = value;
                if (value == ModifierTool.Move)
                {
                    this.selectZone.Visibility = Visibility.Hidden;
                    this.moveZone.Visibility = Visibility.Visible;
                }
                else
                {
                    this.selectZone.Visibility = Visibility.Visible;
                    this.moveZone.Visibility = Visibility.Hidden;
                }
            }
        }

        public ControlTemplate MoveTemplate
        {
            get
            {
                return this.moveTemplate;
            }
            set
            {
                this.moveTemplate = value;
                this.moveZone.Template = value;
                this.moveZone.ApplyTemplate();
            }
        }

        public ControlTemplate SelectTemplate
        {
            get
            {
                return this.selectTemplate;
            }
            set
            {
                this.selectTemplate = value;
                this.selectZone.Template = value;
                this.selectZone.ApplyTemplate();

                var resizer = this.selectZone.Template.FindName("ResizeBottomRight", this.selectZone) as Thumb;
                if (resizer != null)
                {
                    resizer.Tag = "br";
                    resizer.DragDelta += this.SelectZoneResized;
                    resizer.DragCompleted += this.SelectZoneResizeCompleted;
                }
            }
        }

        public void MirrorHorizontal()
        {
            this.Flip(RotateFlipType.RotateNoneFlipX);            
        }

        public void MirrorVertical()
        {
            this.Flip(RotateFlipType.RotateNoneFlipY);            
        }

        /// <summary>
        /// Adjust image contrast
        /// </summary>
        /// <param name="value">Value of 1 means no change. Range 0 - 2</param>
        public void SetContrast(float value)
        {
            this.contrast = value;
            this.AdjustModifiedImage();
        }

        /// <summary>
        /// Adjust image brightness
        /// </summary>
        /// <param name="value">Value of 1 means no change. Range 0 - 2</param>
        public void SetBrightness(float value)
        {
            this.brightness = value;
            this.AdjustModifiedImage();
        }

        public void RestoreImage()
        {
            this.currentAngle = 0;
            this.CreateOriginalImages(false);
            this.DisplayImage();
        }

        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="angle">Angle to pivot image. Values between -360 to 360</param>
        public void Pivot(int angle)
        {
            this.currentAngle += angle;            

            this.AdjustModifiedImage();            
        }

        public void DeleteSectedZone()
        {
            this.DeleteSelected();
        }

        public void CropAndCenter()
        {
            this.SetModifiedImage(XL_ID.Utilities.Image.ImageUtilities.AutoCropAndCenter(this.modifiedImage, Color.White, cropTolerance, Color.White, this.imageToEdit.Size));
            this.modifiedImage.SetResolution(this.qualityImage.HorizontalResolution, this.qualityImage.VerticalResolution);
            this.SetModifiedImageAsQuality();
            this.DisplayImage();
        }


        private void CreateOriginalImages(bool useImageToEdit = true)
        {
            var imgTemp = new Bitmap(this.imageToEdit.Width, this.imageToEdit.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);

            using (var g = Graphics.FromImage(imgTemp))
            {
                g.DrawImage(useImageToEdit ? this.imageToEdit : this.imageBeforeAnyEdit, new System.Drawing.Point(0, 0));
                g.Flush();
            }            

            this.SetQualityImage(imgTemp);
            this.SetModifiedImage(imgTemp);
        }

        /// <summary>
        /// Adjust to angle of rotation, brightness and contrast settings
        /// </summary>
        private void AdjustModifiedImage()
        {                    
            float gamma = 1.0f; // no change in gamma

            float adjustedBrightness = (float)this.brightness - 1.0F;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
                new float[] {this.contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, this.contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, this.contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

            var imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.qualityImage.HorizontalResolution, this.qualityImage.VerticalResolution);

            using (var g = Graphics.FromImage(imgTemp))
            {
                g.Clear(Color.White);

                g.TranslateTransform((float)this.imageToEdit.Width / 2, (float)this.imageToEdit.Height / 2);
                //rotate
                g.RotateTransform(this.currentAngle); // angleRotation
                //move image back            

                g.TranslateTransform(-(float)this.imageToEdit.Width / 2, -(float)this.imageToEdit.Height / 2);

                g.DrawImage(this.qualityImage, new Rectangle(0, 0, this.qualityImage.Width, this.qualityImage.Height)
                    , 0, 0, this.imageToEdit.Width, this.imageToEdit.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }            
            
            this.SetModifiedImage(imgTemp);
            this.DisplayImage();        
        }

        private void Flip(RotateFlipType flipMode)
        {
            this.qualityImage.RotateFlip(flipMode);
            this.AdjustModifiedImage();
        }

        private void CreateMoveZone()
        {
            this.moveZone = new Thumb();
            this.parentPanel.Children.Add(this.moveZone);
            this.moveZone.VerticalAlignment = VerticalAlignment.Top;
            this.moveZone.HorizontalAlignment = HorizontalAlignment.Left;

            this.moveZone.DragDelta += this.ImageMoved;
            this.moveZone.DragCompleted += this.ImageMoveCompleted;
            this.moveZone.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.ScrollAll;
            this.moveZone.MouseLeave += (s, e) => Mouse.OverrideCursor = null;

            var heightBinding = new Binding("ActualHeight");
            heightBinding.Source = this.displayImageControl;
            this.moveZone.SetBinding(FrameworkElement.HeightProperty, heightBinding);

            var widthBinding = new Binding("ActualWidth");
            widthBinding.Source = this.displayImageControl;
            this.moveZone.SetBinding(FrameworkElement.WidthProperty, widthBinding);

            this.IsMoveEnabled = true;
        }

        private void CreateSelectZone()
        {
            this.selectZone = new Thumb();

            this.parentPanel.Children.Add(this.selectZone);

            this.selectZone.Margin = new Thickness(0,0,0,0);
            
            this.selectZone.HorizontalAlignment = HorizontalAlignment.Left;
            this.selectZone.VerticalAlignment = VerticalAlignment.Top;
            this.selectZone.Width = 100;
            this.selectZone.Height = 100;

            this.selectZone.Visibility = Visibility.Hidden;
            this.selectZone.Template = this.selectTemplate;
            this.selectZone.ApplyTemplate();
            this.selectZone.DragDelta += this.SelectZoneOnDragDelta;
        }

        private void SelectZoneOnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var moveThumb = sender as Thumb;
            if (moveThumb == null  || moveThumb.Tag != null)
            {
                return;
            }
            var mar = this.selectZone.Margin;
            this.selectZone.Margin = new Thickness(mar.Left + e.HorizontalChange, mar.Top + e.VerticalChange,0,0);            
        }

        private void ImageMoveCompleted(object sender, DragCompletedEventArgs e)
        {
            // calculate value for move ! Translate WPF DPU into pixels
            var horRatio = this.imageToEdit.Width / this.displayImageControl.ActualWidth;
            var verRatio = this.imageToEdit.Height / this.displayImageControl.ActualHeight;

            var x = (int)(e.HorizontalChange * horRatio);
            var y = (int)(e.VerticalChange * verRatio);

            Console.WriteLine("x:{0} / y:{1}  - hor:{2}  / ver:{3}", x, y, horRatio, verRatio);

            this.MoveImage(x, y);        
        }

        private void ImageMoved(object sender, DragDeltaEventArgs e)
        {
            // calculate value for move ! Translate WPF DPU into pixels
            var horRatio = this.imageToEdit.Width / this.displayImageControl.ActualWidth;
            var verRatio = this.imageToEdit.Height / this.displayImageControl.ActualHeight;

            var x = (int)(e.HorizontalChange * horRatio);
            var y = (int)(e.VerticalChange * verRatio);

            Console.WriteLine("x:{0} / y:{1}  - hor:{2}  / ver:{3}",x,y,horRatio, verRatio);

            this.PreviewMove(x, y);           
        }
        

        private void PreviewMove(int xoffset, int yoffset)
        {            
            // do the same operation to the already modified image
            var imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);
            using (var g = Graphics.FromImage(imgTemp))
            {
                g.Clear(Color.White);
                g.DrawImage(this.modifiedImage, new System.Drawing.Point(xoffset, yoffset));
            }            
           
            this.DisplayPreviewImage(imgTemp);            
        }

        private void MoveImage(int xoffset, int yoffset)
        {
            var imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);
            using (var g = Graphics.FromImage(imgTemp))
            {
                g.Clear(Color.White);
                g.DrawImage(this.qualityImage, new System.Drawing.Point(xoffset, yoffset));
            }            

            //imgTemp = currentImage = ImageHelper.MoveImage(currentImage, imgTemp, g, imageSize, modifiedImageRect, currentImageRect);
            this.SetQualityImage(imgTemp);

            // do the same operation to the already modified image
            imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);
            
            using (var g = Graphics.FromImage(imgTemp))
            {
                g.Clear(Color.White);
                g.DrawImage(this.modifiedImage, new System.Drawing.Point(xoffset, yoffset));
            }
            
            //imgTemp = currentImage = ImageHelper.MoveImage(currentImage, imgTemp, g, imageSize, modifiedImageRect, currentImageRect);
            this.SetModifiedImage(imgTemp);

           this.DisplayImage();

           
        }

        private void SelectZoneResizeCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            var resizeThumb = sender as Thumb;
            if (resizeThumb == null || resizeThumb.TemplatedParent == null)
            {
                return;
            }

            var parentThumb = resizeThumb.TemplatedParent as Thumb;
            parentThumb.Tag = null;
        }

        private void SelectZoneResized(object sender, DragDeltaEventArgs e)
        {
            var resizeThumb = sender as Thumb;
            if (resizeThumb == null || resizeThumb.TemplatedParent == null)
            {
                return;
            }

            var parentThumb = resizeThumb.TemplatedParent as Thumb;
            parentThumb.Tag = "resize";

            var newWidth = parentThumb.Width + e.HorizontalChange;
            var newHeight = parentThumb.Height + e.VerticalChange;

            if (newWidth < 0 || newHeight < 0)
            {
                return;
            }

            parentThumb.Width += e.HorizontalChange;
            parentThumb.Height += e.VerticalChange;
        }

        private void DisplayImage()
        {
            this.DisplayImageEx(this.modifiedImage);                        
        }

        private void DisplayPreviewImage(Bitmap img)
        {
            this.DisplayImageEx(img);
            img.Dispose();                      
        }

        private void DisplayImageEx(Bitmap img)
        {
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, img.PixelFormat);

            try
            {
                this.writeableBitmap.WritePixels(
                    new Int32Rect(0, 0, img.Width, img.Height),
                    data.Scan0,
                    Math.Abs(data.Stride) * img.Height,
                    data.Stride);
            }
            finally
            {
                img.UnlockBits(data);
            }            
        }

        private void DeleteSelected()
        {
            double ratioH = this.qualityImage.Width / this.displayImageControl.ActualWidth;
            double ratioV = this.qualityImage.Height / this.displayImageControl.ActualHeight;

            var x = (int)(this.selectZone.Margin.Left * ratioH); //(int)(Canvas.GetLeft(this.selectZone) * ratioH);
            var y = (int)(this.selectZone.Margin.Top * ratioV);  //(int)(Canvas.GetTop(this.selectZone) * ratioV);
            var width = (int)(this.selectZone.ActualWidth * ratioH);
            var height = (int)(this.selectZone.ActualHeight * ratioV);

            var imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);
            Graphics g = Graphics.FromImage(imgTemp);
            g.Clear(Color.White);
            g.DrawImage(this.qualityImage, 0, 0);
            g.FillRectangle(Brushes.White, x, y, width, height);
            this.SetQualityImage(imgTemp);

            imgTemp = new Bitmap(this.qualityImage.Width, this.qualityImage.Height, PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.imageToEdit.HorizontalResolution, this.imageToEdit.VerticalResolution);
            g = Graphics.FromImage(imgTemp);
            g.Clear(Color.White);
            g.DrawImage(this.modifiedImage, 0, 0);
            g.FillRectangle(Brushes.White, x, y, width, height);

            this.SetModifiedImage(imgTemp);

            this.DisplayImage();
        }

        private void SetQualityImage(Bitmap newImage)
        {
            var old = this.qualityImage;
            this.qualityImage = newImage;
            if (newImage != old && old != null && old != this.modifiedImage)
            {
                old.Dispose();
            }
        }

        private void SetModifiedImage(Bitmap newImage)
        {
            var old = this.modifiedImage;
            this.modifiedImage = newImage;
            if (old != newImage && old != null && old != this.qualityImage)
            {
                old.Dispose();
            }
        }

        private void SetModifiedImageAsQuality()
        {
            SetQualityImage(this.modifiedImage);
            this.contrast = 1;
            this.brightness = 1;
            this.currentAngle = 0;            
        }

        public void CompletePivot()
        {            
            this.SetModifiedImageAsQuality();            

            this.DisplayImage();
        }

        public void BackgroundWhitening()
        {
            var replacer = new ColorReplacer(); 
            replacer.ReplaceColor(Color.FromArgb(245,245,245), 10, Color.White, this.modifiedImage);

            this.SetModifiedImageAsQuality();            

            this.DisplayImage();
        }
    }
}
