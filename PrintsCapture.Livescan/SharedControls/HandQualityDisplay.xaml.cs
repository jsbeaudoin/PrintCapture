using PrintsCapture.Livescan.Properties;

namespace PrintsCapture.Livescan.SharedControls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using PrintsCapture.Livescan.ViewModel;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.Wpf.Extension;

    /// <summary>
    /// Interaction logic for HandQualityControl.xaml
    /// </summary>
    public partial class HandQualityControl : UserControl, INotifyPropertyChanged
    {
        private Dictionary<HandPartStatus, Brush> conditionBrushes;        

        private PrintList printList;

        private List<ConditionElementViewModel> conditionViewModels;

        private ConditionPopupViewModel conditionPopupViewModel;

        public HandQualityControl()
        {
            InitializeComponent();
        }        

        public bool AllowConditionEdition { get; set; }

        public ConditionPopupViewModel ConditionPopupViewModel
        {
            get
            {
                return this.conditionPopupViewModel;
            }
            private set
            {
                if (Equals(value, this.conditionPopupViewModel))
                {
                    return;
                }
                this.conditionPopupViewModel = value;
                this.OnPropertyChanged("ConditionPopupViewModel");
            }
        }

        public void Initialize(PrintList printList)
        {
            var imgs = ConditionImage.GetConditionImages();
            this.printList = printList;
            this.conditionViewModels =
                    imgs.Select(
                        x =>
                            new ConditionElementViewModel()
                            {
                                Image = x.Image,
                                Status = x.Status,
                                Label = this.printList.GetCondition(x.Status).Text,
                                IsSelected = false
                            }).ToList();

           
           conditionBrushes = imgs.ToDictionary(x => x.Status, y => (Brush)new ImageBrush() { ImageSource = y.Image });            
        }        

        public void SetPartBrushAndTip(Hand hand, HandPart part, Brush brush, string tip="")
        {
            var rects = (hand == Hand.Left ? this.LeftHandQuality : this.RightHandQuality) 
                .FindVisualChildren<Rectangle>();
            var rect = rects.FirstOrDefault(x => x.Tag != null && (HandPart)x.Tag == part);

            if (rect == null)
            {
                return;
            }

            rect.Fill = brush;
            if (tip != "")
            {
                rect.ToolTip = tip;
            }
            
        }        

        private void FingerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            if (e.LeftButton != MouseButtonState.Pressed || rect == null || !this.AllowConditionEdition)
            {
                return;
            }

            var parent = rect.Parent as Canvas;
            if (parent == null)
            {
                return;
            }

            var hand = (Hand)parent.Tag;
            var part = (HandPart)rect.Tag;

            this.ShowConditionSelection(rect, hand, part);
        }

        private void ConditionMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var panel = sender as StackPanel;
            if (e.LeftButton != MouseButtonState.Pressed || this.conditionPopupViewModel == null || panel == null)
            {
                return;
            }

            var newCondition = (HandPartStatus)panel.Tag;
            var condition = this.printList.Conditions.FirstOrDefault(x => x.Status == newCondition);

            var vm = this.conditionPopupViewModel;
            var p = this.printList.PhysicalParts.FirstOrDefault(x => x.Hand == vm.Hand && x.HandPart == vm.Part);

            p.MissingCode = condition.Code;
            p.MissingDate = vm.ConditionDate;

            this.ConditionPopup.IsOpen = false;
            var tooltip = condition.Status != HandPartStatus.Present
                ? string.Format("{0} : {1}", p.MissingDate, condition.Text)
                : null;

            this.SetPartBrushAndTip(vm.Hand, vm.Part, this.conditionBrushes[condition.Status], tooltip);

            this.ConditionPopupViewModel = null;
        }

        private void ShowConditionSelection(Rectangle sender, Hand hand, HandPart part)
        {
            this.ConditionPopup.PlacementTarget = sender;
            var vm = new ConditionPopupViewModel();
            vm.Hand = hand;
            vm.Part = part;

            var p = this.printList.PhysicalParts.FirstOrDefault(x => x.Hand == hand && x.HandPart == part);

            if (p == null)
            {
                return;
            }

            var code = this.printList.GetCondition(p.MissingCode);
            vm.PrintName = PrintList.GetName(hand, part);
            vm.Status = code.Status;
            vm.ConditionDate = p.MissingDate;

            foreach (var conditionViewModel in this.conditionViewModels)
            {
                conditionViewModel.IsSelected = code.Status == conditionViewModel.Status;
            }

            vm.AllConditions = this.conditionViewModels;          

            this.ConditionPopupViewModel = vm;
              
            this.ConditionPopup.IsOpen = true;
            this.ConditionDateTextBox.Focus();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
