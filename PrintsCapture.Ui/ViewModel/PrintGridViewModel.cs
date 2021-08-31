// -----------------------------------------------------------------------
// <copyright file="PrintListViewModel.cs" company="Solutions XL-ID inc">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PrintsCapture.Ui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.ViewModel;  

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PrintGridViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, Action<PrintElementViewModel>> fingerProperties = new Dictionary<string, Action<PrintElementViewModel>>();
        
        private bool showPalmPrint;        

        private bool showRolledPrint;

        private PrintElementViewModel palmRightHypothenar;

        private PrintElementViewModel palmRightLower;

        private PrintElementViewModel palmRightUpper;

        private PrintElementViewModel palmLeftHypothenar;

        private PrintElementViewModel palmLeftLower;

        private PrintElementViewModel palmLeftUpper;

        private PrintElementViewModel flatRightFingers;

        private PrintElementViewModel flatLeftFingers;

        private PrintElementViewModel flatRightThumb;

        private PrintElementViewModel flatLeftThumb;

        private PrintElementViewModel rolledRightThumb;

        private PrintElementViewModel rolledRightIndex;

        private PrintElementViewModel rolledRightMiddle;

        private PrintElementViewModel rolledRightRing;

        private PrintElementViewModel rolledRightLittle;

        private PrintElementViewModel rolledLeftThumb;

        private PrintElementViewModel rolledLeftIndex;

        private PrintElementViewModel rolledLeftMiddle;

        private PrintElementViewModel rolledLeftRing;

        private PrintElementViewModel rolledLeftLittle;

        private PrintElementViewModel endorsementFinger;        

        private PrintElementViewModel flatTwoThumbs;

        private bool captureTwoThumbs;

        private Visibility twoThumbsVisibility;

        private bool showMissingLine;

        private Visibility leftThumbVisibility;

        private Visibility rightThumbVisibility;

        private bool isCaptureInProgress;

        public PrintGridViewModel(CaptureKind kind)
        {
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Hypothenar, HandScanKind.Flat), vm => this.PalmRightHypothenar = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.LowerPalm, HandScanKind.Flat), vm => this.PalmRightLower = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.UpperPalm, HandScanKind.Flat), vm => this.PalmRightUpper = vm);
            
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Hypothenar, HandScanKind.Flat), vm => this.PalmLeftHypothenar = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.LowerPalm, HandScanKind.Flat), vm => this.PalmLeftLower = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.UpperPalm, HandScanKind.Flat), vm => this.PalmLeftUpper = vm);
            
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.FourFlats, HandScanKind.Flat), vm => this.FlatRightFingers = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.FourFlats, HandScanKind.Flat), vm => this.FlatLeftFingers = vm);
            
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Thumb, HandScanKind.Flat), vm => this.FlatRightThumb = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Thumb, HandScanKind.Flat), vm => this.FlatLeftThumb = vm);

            this.fingerProperties.Add(PrintInfo.GetKey(Hand.None, HandPart.Endorsement, HandScanKind.Flat), vm => this.EndorsementFinger = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.None, HandPart.TwoThumbs, HandScanKind.Flat), vm => this.FlatTwoThumbs = vm);

            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Thumb, HandScanKind.Rolled), vm => this.RolledRightThumb = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Index, HandScanKind.Rolled), vm => this.RolledRightIndex = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Middle, HandScanKind.Rolled), vm => this.RolledRightMiddle = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Ring, HandScanKind.Rolled), vm => this.RolledRightRing = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Right, HandPart.Little, HandScanKind.Rolled), vm => this.RolledRightLittle = vm);


            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Thumb, HandScanKind.Rolled), vm => this.RolledLeftThumb = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Index, HandScanKind.Rolled), vm => this.RolledLeftIndex = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Middle, HandScanKind.Rolled), vm => this.RolledLeftMiddle = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Ring, HandScanKind.Rolled), vm => this.RolledLeftRing = vm);
            this.fingerProperties.Add(PrintInfo.GetKey(Hand.Left, HandPart.Little, HandScanKind.Rolled), vm => this.RolledLeftLittle = vm);

            this.DefineThumbsVisibility();

            this.DefineMenuVisibility(kind);
        }

        

        public bool IsCaptureInProgress
        {
            get
            {
                return this.isCaptureInProgress;
            }
            set
            {
                if (this.isCaptureInProgress == value)
                {
                    return;
                }
                this.isCaptureInProgress = value;
                this.OnPropertyChanged("IsCaptureInProgress");
            }
        }

        public Visibility ScanVisibility { get; private set; }

        public Visibility EditVisibility { get; private set; }

        public PrintElementViewModel EndorsementFinger
        {
            get
            {
                return this.endorsementFinger;
            }
            set
            {
                if (Equals(value, this.endorsementFinger))
                {
                    return;
                }
                this.endorsementFinger = value;
                this.OnPropertyChanged("EndorsementFinger");
            }
        }

        /// <summary>
        /// Gets or sets the rolled left little.
        /// </summary>
        public PrintElementViewModel RolledLeftLittle
        {
            get
            {
                return this.rolledLeftLittle;
            }
            set
            {
                if (Equals(value, this.rolledLeftLittle))
                {
                    return;
                }
                this.rolledLeftLittle = value;
                this.OnPropertyChanged("RolledLeftLittle");

                
            }
        }        

        /// <summary>
        /// Gets or sets the rolled left ring.
        /// </summary>
        public PrintElementViewModel RolledLeftRing
        {
            get
            {
                return this.rolledLeftRing;
            }
            set
            {
                if (Equals(value, this.rolledLeftRing))
                {
                    return;
                }
                this.rolledLeftRing = value;
                this.OnPropertyChanged("RolledLeftRing");
            }
        }

        /// <summary>
        /// Gets or sets the rolled left middle.
        /// </summary>
        public PrintElementViewModel RolledLeftMiddle
        {
            get
            {
                return this.rolledLeftMiddle;
            }
            set
            {
                if (Equals(value, this.rolledLeftMiddle))
                {
                    return;
                }
                this.rolledLeftMiddle = value;
                this.OnPropertyChanged("RolledLeftMiddle");
            }
        }

        /// <summary>
        /// Gets or sets the rolled left index.
        /// </summary>
        public PrintElementViewModel RolledLeftIndex
        {
            get
            {
                return this.rolledLeftIndex;
            }
            set
            {
                if (Equals(value, this.rolledLeftIndex))
                {
                    return;
                }
                this.rolledLeftIndex = value;
                this.OnPropertyChanged("RolledLeftIndex");
            }
        }

        /// <summary>
        /// Gets or sets the rolled left thumb.
        /// </summary>
        public PrintElementViewModel RolledLeftThumb
        {
            get
            {
                return this.rolledLeftThumb;
            }
            set
            {
                if (Equals(value, this.rolledLeftThumb))
                {
                    return;
                }
                this.rolledLeftThumb = value;
                this.OnPropertyChanged("RolledLeftThumb");
            }
        }

        /// <summary>
        /// Gets or sets the rolled right little.
        /// </summary>
        public PrintElementViewModel RolledRightLittle
        {
            get
            {
                return this.rolledRightLittle;
            }
            set
            {
                if (Equals(value, this.rolledRightLittle))
                {
                    return;
                }
                this.rolledRightLittle = value;
                this.OnPropertyChanged("RolledRightLittle");
            }
        }

        /// <summary>
        /// Gets or sets the rolled right ring.
        /// </summary>
        public PrintElementViewModel RolledRightRing
        {
            get
            {
                return this.rolledRightRing;
            }
            set
            {
                if (Equals(value, this.rolledRightRing))
                {
                    return;
                }
                this.rolledRightRing = value;
                this.OnPropertyChanged("RolledRightRing");
            }
        }

        /// <summary>
        /// Gets or sets the rolled right middle.
        /// </summary>
        public PrintElementViewModel RolledRightMiddle
        {
            get
            {
                return this.rolledRightMiddle;
            }
            set
            {
                if (Equals(value, this.rolledRightMiddle))
                {
                    return;
                }
                this.rolledRightMiddle = value;
                this.OnPropertyChanged("RolledRightMiddle");
            }
        }

        /// <summary>
        /// Gets or sets the rolled right index.
        /// </summary>
        public PrintElementViewModel RolledRightIndex
        {
            get
            {
                return this.rolledRightIndex;
            }
            set
            {
                if (Equals(value, this.rolledRightIndex))
                {
                    return;
                }
                this.rolledRightIndex = value;
                this.OnPropertyChanged("RolledRightIndex");
            }
        }

        /// <summary>
        /// Gets or sets the rolled right thumb.
        /// </summary>
        public PrintElementViewModel RolledRightThumb
        {
            get
            {
                return this.rolledRightThumb;
            }
            set
            {
                if (Equals(value, this.rolledRightThumb))
                {
                    return;
                }
                this.rolledRightThumb = value;
                this.OnPropertyChanged("RolledRightThumb");
            }
        }

        /// <summary>
        /// Gets or sets the flat left thumb.
        /// </summary>
        public PrintElementViewModel FlatLeftThumb
        {
            get
            {
                return this.flatLeftThumb;
            }
            set
            {
                if (Equals(value, this.flatLeftThumb))
                {
                    return;
                }
                this.flatLeftThumb = value;
                this.DefineThumbsVisibility();
                this.OnPropertyChanged("FlatLeftThumb");
            }
        }

        /// <summary>
        /// Gets or sets the flat right thumb.
        /// </summary>
        public PrintElementViewModel FlatRightThumb
        {
            get
            {
                return this.flatRightThumb;
            }
            set
            {
                if (Equals(value, this.flatRightThumb))
                {
                    return;
                }
                this.flatRightThumb = value;
                this.DefineThumbsVisibility();
                this.OnPropertyChanged("FlatRightThumb");
            }
        }

        /// <summary>
        /// Gets or sets the flat right thumb.
        /// </summary>
        public PrintElementViewModel FlatTwoThumbs
        {
            get
            {
                return this.flatTwoThumbs;
            }
            set
            {
                if (Equals(value, this.flatTwoThumbs))
                {
                    return;
                }
                this.flatTwoThumbs = value;
                this.DefineThumbsVisibility();
                this.OnPropertyChanged("FlatTwoThumbs");
            }
        }

        /// <summary>
        /// Gets or sets the flat left fingers.
        /// </summary>
        public PrintElementViewModel FlatLeftFingers
        {
            get
            {
                return this.flatLeftFingers;
            }
            set
            {
                if (Equals(value, this.flatLeftFingers))
                {
                    return;
                }
                this.flatLeftFingers = value;
                this.OnPropertyChanged("FlatLeftFingers");
            }
        }

        /// <summary>
        /// Gets or sets the flat right fingers.
        /// </summary>
        public PrintElementViewModel FlatRightFingers
        {
            get
            {
                return this.flatRightFingers;
            }
            set
            {
                if (Equals(value, this.flatRightFingers))
                {
                    return;
                }
                this.flatRightFingers = value;
                this.OnPropertyChanged("FlatRightFingers");
            }
        }

        /// <summary>
        /// Gets or sets the palm left upper.
        /// </summary>
        public PrintElementViewModel PalmLeftUpper
        {
            get
            {
                return this.palmLeftUpper;
            }
            set
            {
                if (Equals(value, this.palmLeftUpper))
                {
                    return;
                }
                this.palmLeftUpper = value;
                this.OnPropertyChanged("PalmLeftUpper");
            }
        }

        /// <summary>
        /// Gets or sets the palm left lower.
        /// </summary>
        public PrintElementViewModel PalmLeftLower
        {
            get
            {
                return this.palmLeftLower;
            }
            set
            {
                if (Equals(value, this.palmLeftLower))
                {
                    return;
                }
                this.palmLeftLower = value;
                this.OnPropertyChanged("PalmLeftLower");
            }
        }

        /// <summary>
        /// Gets or sets the palm left hypothenar.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public PrintElementViewModel PalmLeftHypothenar
        {
            get
            {
                return this.palmLeftHypothenar;
            }
            set
            {
                if (Equals(value, this.palmLeftHypothenar))
                {
                    return;
                }
                this.palmLeftHypothenar = value;
                this.OnPropertyChanged("PalmLeftHypothenar");
            }
        }

        /// <summary>
        /// Gets or sets the palm right upper.
        /// </summary>
        public PrintElementViewModel PalmRightUpper
        {
            get
            {
                return this.palmRightUpper;
            }
            set
            {
                if (Equals(value, this.palmRightUpper))
                {
                    return;
                }
                this.palmRightUpper = value;
                this.OnPropertyChanged("PalmRightUpper");
            }
        }

        /// <summary>
        /// Gets or sets the palm right lower.
        /// </summary>
        public PrintElementViewModel PalmRightLower
        {
            get
            {
                return this.palmRightLower;
            }
            set
            {
                if (Equals(value, this.palmRightLower))
                {
                    return;
                }
                this.palmRightLower = value;
                this.OnPropertyChanged("PalmRightLower");
            }
        }

        /// <summary>
        /// Gets or sets the palm right hypothenar.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public PrintElementViewModel PalmRightHypothenar
        {
            get
            {
                return this.palmRightHypothenar;
            }
            set
            {
                if (Equals(value, this.palmRightHypothenar))
                {
                    return;
                }
                this.palmRightHypothenar = value;
                this.OnPropertyChanged("PalmRightHypothenar");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether include rolled print.
        /// </summary>
        public bool ShowRolledPrint
        {
            get
            {
                return this.showRolledPrint;
            }
            set
            {
                if (value.Equals(this.showRolledPrint))
                {
                    return;
                }
                this.showRolledPrint = value;
                this.OnPropertyChanged("ShowRolledPrint");
            }
        }

        public bool ShowMissingLine
        {
            get
            {
                return this.showMissingLine;
            }
            set
            {
                if (value == this.showMissingLine)
                {
                    return;
                }
                this.showMissingLine = value;
                this.OnPropertyChanged("ShowMissingLine");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether include palm print.
        /// </summary>
        public bool ShowPalmPrint
        {
            get
            {
                return this.showPalmPrint;
            }
            set
            {
                if (value.Equals(this.showPalmPrint))
                {
                    return;
                }
                this.showPalmPrint = value;
                this.OnPropertyChanged("ShowPalmPrint");
            }
        }        

        public bool CaptureTwoThumbs
        {
            get
            {
                return this.captureTwoThumbs;
            }
            set
            {
                if (value.Equals(this.captureTwoThumbs))
                {
                    return;
                }

                this.captureTwoThumbs = value;

                this.OnPropertyChanged("CaptureTwoThumbs");     
           
                this.DefineThumbsVisibility();             
            }
        }

        public Visibility LeftThumbVisibility
        {
            get
            {
                return this.leftThumbVisibility;
            }
            private set
            {
                if (value != this.leftThumbVisibility)
                {
                    this.leftThumbVisibility = value;
                    this.OnPropertyChanged("LeftThumbVisibility");
                }                
            }
        }

        public Visibility RightThumbVisibility
        {
            get
            {
                return this.rightThumbVisibility;
            }
            private set
            {
                if (value != this.rightThumbVisibility)
                {
                    this.rightThumbVisibility = value;
                    this.OnPropertyChanged("RightThumbVisibility");
                }
            }
        }

        public Visibility TwoThumbsVisibility
        {
            get
            {
                return this.twoThumbsVisibility;
            }
            private set
            {
                if (value != this.twoThumbsVisibility)
                {
                    this.twoThumbsVisibility = value;
                    this.OnPropertyChanged("TwoThumbsVisibility");
                }                
            }
        }

        public void Update(List<PrintElementViewModel> viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                this.Update(viewModel);
            }
        }

        public void Update(PrintElementViewModel viewModel)
        {
            var print = viewModel.LinkedPrint;
            var key = string.IsNullOrEmpty(print.MatchedKey) ? print.Key : print.MatchedKey;
            
            if (!this.fingerProperties.ContainsKey(key))
            {
                return;
            }

            this.fingerProperties[key](viewModel);
        }        

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void DefineMenuVisibility(CaptureKind kind)
        {
            this.ScanVisibility = kind == CaptureKind.Livescan ? Visibility.Visible : Visibility.Collapsed;
            this.EditVisibility = kind == CaptureKind.Cardscan ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DefineThumbsVisibility()
        {
            if (this.captureTwoThumbs)
            {
                this.LeftThumbVisibility = Visibility.Collapsed;
                this.RightThumbVisibility = Visibility.Collapsed;
                if (this.flatTwoThumbs == null)
                {
                    return;
                }
                //this.TwoThumbsVisibility = this.flatTwoThumbs.IsMissing ? Visibility.Collapsed : Visibility.Visible;
                this.TwoThumbsVisibility = Visibility.Visible;
            }
            else
            {
                this.TwoThumbsVisibility = Visibility.Collapsed;
                if (this.flatLeftThumb == null || this.flatRightThumb == null)
                {
                    return;
                }
                //this.LeftThumbVisibility = this.flatLeftThumb.IsMissing ? Visibility.Collapsed : Visibility.Visible;
                //this.RightThumbVisibility = this.flatRightThumb.IsMissing ? Visibility.Collapsed : Visibility.Visible;

                this.LeftThumbVisibility = Visibility.Visible;
                this.RightThumbVisibility = Visibility.Visible;
            }
        }
    }
}
