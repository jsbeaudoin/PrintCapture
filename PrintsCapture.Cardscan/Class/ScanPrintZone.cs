namespace PrintsCapture.Cardscan
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Xml.Serialization;

    public class ScanPrintZone
    {
        private int rotation;        

        private PrintZoneInfo printZone;

        private ScanSize size;

        private ScanLocation location;

        //[XmlIgnore]
        //public Bitmap ZoneImage { get; set; }

        [XmlIgnore]
        public PrintZoneInfo PrintZone
        {
            get
            {
                return this.printZone;
            }
            set
            {
                this.printZone = value;

                this.Id = value == null ? string.Empty : value.Id;
            }
        }

        public ScanPrintZone()
        {
            this.location = new ScanLocation(0,0);
            this.size = new ScanSize(0,0);
        }

        internal ScanPrintZone(string id, double left, double top, double width, double height)
        {

            this.Id = id;
            this.Location = new ScanLocation {Left = left, Top = top, Unit = ScanUnit.Inches};
            this.Size = new ScanSize {Width = width, Height = height, Unit = ScanUnit.Inches};
            this.Rotation = 0;
        }

        /// <summary>
        /// Property to load/save PrintZone when serialization occurs
        /// </summary>
        public string Id { get; set; }

        public ScanLocation Location
        {
            get
            {
                return this.location;
            }
            set
            {
                this.location = value.ToInches();                
            }
        }

        public ScanSize Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value.ToInches();
            }
        }

        public int Rotation
        {
            get
            {
                return this.rotation;
            }
            set
            {
                var invertedBefore = this.AreWidthHeightInverted;
                this.rotation = value;
                if (this.Thumb == null)
                {
                    return;
                }

                if (invertedBefore != this.AreWidthHeightInverted)
                {
                    // change width and Height !
                    var oldHeight = this.Size.Height;
                    this.Size.Height = this.Size.Width;
                    this.Size.Width = oldHeight;
                }

                this.Adapt(); // Resize to current size to take into account min and max values
                this.Thumb.UpdateLayout();
            }
        }

        [XmlIgnore]
        public bool AreWidthHeightInverted
        {
            get
            {
                // 0 and 180 degrees means no inversion
                return !(this.Rotation == 0 || this.Rotation == 180);
            }
        }

        public void CommitLocationFromControl()
        {
            var pixLoc = new ScanLocation(ScanUnit.Pixels, this.Thumb.Margin.Top, this.Thumb.Margin.Left);
            this.Location = pixLoc;
            this.Adapt();
        }

        /// <summary>
        /// Move the zone by the offset amount of pixels
        /// </summary>
        /// <param name="horizontal">Horizontal changes</param>
        /// <param name="vertical">Vertical changes</param>
        /// <param name="fromLocation">Changes are made from the current location(Inches) and saved In location. Otherwise, change are from control boundary and not saved to location</param>
        public void MoveByOffsetPixel(double horizontal, double vertical, bool fromLocation = true)
        {
            ScanLocation pixLoc = fromLocation ? this.Location.ToPixels() : new ScanLocation(ScanUnit.Pixels, this.Thumb.Margin.Top, this.Thumb.Margin.Left);
            pixLoc.Left += horizontal;
            pixLoc.Top += vertical;

            if (fromLocation)
            {
                this.Location = pixLoc;
            }
            

            this.Thumb.Margin = new Thickness(pixLoc.Left, pixLoc.Top, 0, 0); 
        }

        public void Move(ScanLocation newLocation)
        {            
            this.Location = newLocation;
            var pixLoc = this.Location.ToPixels();

            this.Thumb.Margin = new Thickness(pixLoc.Left, pixLoc.Top, 0,0);            
        }


        public void ResizeByPixelChange(double horizontal, double vertical)
        {
            ScanSize newSize = this.size.ToPixels();

            newSize.Width += horizontal;
            newSize.Height += vertical;
            this.Resize(newSize);
        }

        public void Resize(ScanSize newSize)
        {
            if (this.PrintZone == null)
            {
                return;
            }

            var newInchSize = newSize.ToInches();
            var maxSize = this.PrintZone.Size.ToInches();           
            //var changedSize = new ScanSize(newSize.Unit, newpixSize.Width, newpixSize.Height);

            var height = newInchSize.Height;
            var width = newInchSize.Width;
            if (this.AreWidthHeightInverted)
            {
                height = newInchSize.Width;
                width = newInchSize.Height;
            }            

            if (width < ScanConstants.MinInchWidth)
            {
                width = ScanConstants.MinInchWidth;                
            }
            else if (width > maxSize.Width)
            {
                width = maxSize.Width;
            }

            if (height < ScanConstants.MinInchHeight)
            {
                height = ScanConstants.MinInchHeight;
            }
            else if (height > maxSize.Height)
            {
                height = maxSize.Height;
            }

            var changed = this.AreWidthHeightInverted ? new ScanSize(ScanUnit.Inches, height, width) : new ScanSize(ScanUnit.Inches, width, height);

            this.Size = changed;


            this.ResizeThumb(this.Size);      

        }

        /// <summary>
        /// Adapt location and size to current information and Dpi !
        /// </summary>
        public void Adapt()
        {
            this.ResizeThumb(this.Size);
            this.Move(this.Location);
        }

        [XmlIgnore]
        public Thumb Thumb { get; set; }
        
        private void ResizeThumb(ScanSize newSize)
        {
            if (newSize.Unit != ScanUnit.Pixels)
            {
                newSize = newSize.ToPixels();
            }

            var grid = this.Thumb.Template.FindName("Grid", this.Thumb) as Grid;
            if (grid == null)
            {
                return;
            }

            grid.Height = newSize.Height;
            grid.Width = newSize.Width;  
        }

        
    }
}
