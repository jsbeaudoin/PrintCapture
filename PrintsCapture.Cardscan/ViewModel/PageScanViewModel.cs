namespace PrintsCapture.Cardscan.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;

    using Prints.Enum;
    using Prints.Language;

    public class PageScanViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PrintZoneInfo> printList = new ObservableCollection<PrintZoneInfo>();

        private ScanModelList modelList;

        private ZoneViewModel currentZone;

        private readonly int[] zoomLadder = new[] { 96, 125, 200, 250, 300, 400, 500, 750, 1000 };

        private string currentModelName;

        private int currentZoom;

        private int currentContrast;

        private int currentBrightness;

        private ScanModel currentModel;
        private string imageInformation;

        public PageScanViewModel(bool isFlat)
        {
            this.AllPrints = PrintZoneInfo.GetPrintList(isFlat);
            this.QuickPrints = new List<QuickPrintsViewModel>();

            foreach (var info in this.AllPrints)
            {
                this.PrintList.Add(info);
            }

            this.ModelList = CardScanSettings.Default.ScanModels; //ScanModelList.GetList();

            this.currentZoom = 0;
            this.MinZoom = 0;
            this.CurrentContrast = 100;
            this.currentBrightness = 100;
            this.MaxZoom = this.zoomLadder.Length - 1;

            if (isFlat)
            {

                this.AddQuickPrints(CommonText.PrintFlat,
                    x => x.HandPart == HandPart.FourFlats && x.Hand == Hand.Left,
                    x => x.HandPart == HandPart.TwoThumbs && x.Hand == Hand.None,
                    x => x.HandPart == HandPart.FourFlats && x.Hand == Hand.Right
                    
                    );
            }
            else
            {

                this.AddQuickPrints(
                    CommonText.CaptureStandard14,
                    x => x.ScanKind == HandScanKind.Rolled,
                    x => x.HandPart == HandPart.FourFlats && x.Hand == Hand.Left,
                    x => x.HandPart == HandPart.Thumb && x.Hand == Hand.Left && x.ScanKind == HandScanKind.Flat,
                    x => x.HandPart == HandPart.Thumb && x.Hand == Hand.Right && x.ScanKind == HandScanKind.Flat,
                    x => x.HandPart == HandPart.FourFlats && x.Hand == Hand.Right);

                this.AddQuickPrints(
                    $"{CommonText.Palms} ({CommonText.PrintRightHand})",
                    x => x.HandPart == HandPart.UpperPalm && x.Hand == Hand.Right,
                    x => x.HandPart == HandPart.LowerPalm && x.Hand == Hand.Right,
                    x => x.HandPart == HandPart.Hypothenar && x.Hand == Hand.Right);

                this.AddQuickPrints(
                    $"{CommonText.Palms} ({CommonText.PrintLeftHand})",
                    x => x.HandPart == HandPart.UpperPalm && x.Hand == Hand.Left,
                    x => x.HandPart == HandPart.LowerPalm && x.Hand == Hand.Left,
                    x => x.HandPart == HandPart.Hypothenar && x.Hand == Hand.Left);
            }
        }

        private void AddQuickPrints(string label, params Func<PrintZoneInfo, bool>[] predicates)
        {
            var list = new List<PrintZoneInfo>();
            foreach (var predicate in predicates)
            {
                list.AddRange(this.AllPrints.Where(predicate).OrderBy(x => x.SortOrder));
            }
            
            this.QuickPrints.Add(new QuickPrintsViewModel() {Label = label, Ids = list.Select(x => x.Id).ToList()});
        }

        public string ImageInformation
        {
            get { return imageInformation; }
            set
            {
                imageInformation = value;
                this.OnPropertyChanged("ImageInformation");
            }
        }

        public string CurrentModelName
        {
            get
            {
                return this.currentModelName;
            }
            private set
            {
                this.currentModelName = value;
                this.OnPropertyChanged("CurrentModelName");
            }
        }

        public ScanModel CurrentModel
        {
            get
            {
                return this.currentModel;
            }
            set
            {
                if (value == this.currentModel)
                {
                    return;
                }
                this.currentModel = value;
                this.OnPropertyChanged("CurrentModel");

                this.CurrentModelName = value == null ? CommonText.NoModel : value.Label;
            }
        }

        public ImageSource PageImage { get; set; }

        public int CurrentZoom
        {
            get
            {
                return this.currentZoom;
            }
            set
            {
                if (Equals(value, this.currentZoom))
                {
                    return;
                }

                if (value < this.MinZoom || value > this.MaxZoom)
                {
                    value = this.MinZoom;
                }

                this.currentZoom = value;

                var dpi = this.zoomLadder[value];

                this.OnPropertyChanged("CurrentZoom");

                ScanConstants.CurrentDpi = dpi;
                this.OnDpiChanged();
            }
        }

        public int CurrentContrast
        {
            get
            {
                return this.currentContrast;
            }
            set
            {
                if (Equals(value, this.currentContrast))
                {
                    return;
                }

                this.currentContrast = value;

                this.OnPropertyChanged("CurrentContrast");

                this.OnBrightnessContrastChanged();
            }
        }

        public int CurrentBrightness
        {
            get
            {
                return this.currentBrightness;
            }
            set
            {
                if (value == this.currentBrightness)
                {
                    return;
                }
                this.currentBrightness = value;
                this.OnPropertyChanged("CurrentBrightness");
                this.OnBrightnessContrastChanged();
            }
        }

        public event EventHandler DpiChanged;

        public event EventHandler BrightnessContrastChanged;

        public int MinZoom { get; private set; }

        public int MaxZoom { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ZoneViewModel CurrentZone
        {
            get
            {
                return this.currentZone;
            }
            set
            {
                this.currentZone = value;
                this.OnPropertyChanged("CurrentZone");
            }
        }

        public List<PrintZoneInfo> AllPrints { get; private set; }

        public List<QuickPrintsViewModel> QuickPrints { get; private set; }

        public ObservableCollection<PrintZoneInfo> PrintList
        {
            get
            {
                return this.printList;
            }
            set
            {
                this.printList = value;
                this.OnPropertyChanged("PrintList");
            }
        }

        public ScanModelList ModelList
        {
            get
            {
                return this.modelList;
            }
            private set
            {
                this.modelList = value;
                this.OnPropertyChanged("ModelList");
            }
        }        

        void OnDpiChanged()
        {
            var handler = this.DpiChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        void OnBrightnessContrastChanged()
        {
            var handler = this.BrightnessContrastChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
