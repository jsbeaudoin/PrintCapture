namespace PrintsCapture.Ui.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using PrintsCapture.Livescan.Properties;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.Wpf.Extension;
    using Prints.Language;

    /// <summary>
    /// Interaction logic for HandQualityControl.xaml
    /// </summary>
    public partial class HandConditionControl : UserControl, INotifyPropertyChanged
    {
        private Dictionary<HandPartStatus, Brush> conditionBrushes;        

        private PrintList printList;

        private bool showPalm;
        private bool isAdvancedMode;
        private string currentDate;
        private string currentFinger;
        
        private HandPartStatus currentCondition;

        private string currentDateMessage;

        public HandConditionControl()
        {
            InitializeComponent();

            this.IsAdvancedMode = true;
        }        

        public bool AllowConditionEdition { get; set; }

        public string CurrentFinger
        {
            get { return this.currentFinger; }
            set
            {
                this.currentFinger = value;
                this.OnPropertyChanged(nameof(this.CurrentFinger));
            }
        }

        public string CurrentDate
        {
            get { return this.currentDate; }
            set
            {
                this.currentDate = value;
                this.OnPropertyChanged(nameof(this.CurrentDate));
                this.ValidateDate();
            }
        }

        public string CurrentDateMessage
        {
            get
            {
                return this.currentDateMessage;
            }
            private set
            {
                if (this.currentDateMessage == value)
                {
                    return;
                }
                this.currentDateMessage = value;
                this.OnPropertyChanged(nameof(this.currentDateMessage));
            }
        }

        public HandPartStatus CurrentCondition
        {
            get { return this.currentCondition; }
            set
            {
                if (value == this.currentCondition)
                {
                    return;
                }
                this.currentCondition = value;
                this.OnPropertyChanged(nameof(this.CurrentCondition));

                this.RefreshCondition();
            }
        }

        public void Initialize(PrintList printList)
        {
            //var imgs = ConditionImage.GetConditionImages();
            this.printList = printList;
            this.conditionBrushes = new Dictionary<HandPartStatus, Brush>
            {
                {HandPartStatus.Present, new SolidColorBrush(Colors.Transparent)},
                {HandPartStatus.Amputated, new SolidColorBrush(Colors.Red)},
                {HandPartStatus.Bandaged, new SolidColorBrush(Colors.Yellow)},
                {HandPartStatus.ForeignReason, new SolidColorBrush(Colors.Purple)},
                {HandPartStatus.PhysicalLimitation, new SolidColorBrush(Colors.Blue)}
            };



            //this.UpdateStatus();
        }

        public void UpdateStatus()
        {
            if (this.printList == null)
            {
                return;
            }

            foreach (var physicalHandPart in this.printList.PhysicalParts)
            {
                this.ApplyBrush(physicalHandPart);
            }
        }


        private void ValidateDate()
        {
            var value = this.currentDate;
            var msg = string.Empty;
            // validate non null date
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length != 10)
                //if (!(value.Length == 4 || value.Length == 7 || value.Length == 10))
                {
                    msg = CommonText.ExceptionDateFormat;
                }
                else
                {
                    int year, month, day;
                    if (!int.TryParse(value.Substring(0, 4), out year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        msg = CommonText.ExceptionDateYear;
                    }
                    else if (value.Length >= 7)
                    {
                        if (!int.TryParse(value.Substring(5, 2), out month) || month < 1 || month > 12
                            || new DateTime(year, month, 1) > DateTime.Now)
                        {
                            msg = CommonText.ExceptionDatePast;
                        }
                        else if (value.Length == 10)
                        {
                            if (!int.TryParse(value.Substring(8, 2), out day) || day < 1 || day > DateTime.DaysInMonth(year, month)
                                || new DateTime(year, month, day) > DateTime.Now)
                            {
                                msg = CommonText.ExceptionDatePast;
                            }
                        }
                    }
                }

                
            }

            this.CurrentDateMessage = msg;
        }

        private void ApplyBrush(PhysicalHandPart physical)
        {
            Brush brushToApply;

            if (!this.IsAdvancedMode)
            {
                brushToApply = physical.IsMissing
                    ? this.conditionBrushes[HandPartStatus.Amputated]
                    : this.conditionBrushes[HandPartStatus.Present];
            }
            else
            {
                var cond = this.printList.Conditions.FirstOrDefault(x => x.Code == physical.MissingCode);

                brushToApply = cond == null ?
                    this.conditionBrushes.First().Value :
                    this.conditionBrushes[cond.Status];
            }

           

            //var brush = physical.IsMissing ? this.missingBrush : this.presentBrush;

            
            var pathObj = this.GetPath(physical.HandAndPart);

            if (pathObj != null)
            {
                pathObj.Fill = brushToApply;
            }            
            
            //border.ToolTip = cond?.Text;            
        }

        private Path GetPath(HandAndPart part)
        {
            var paths = this.MissingGrid.FindVisualChildren<Path>();
            var path = paths.FirstOrDefault(x => x.Tag != null && (HandAndPart)x.Tag == part);

            return path;
        }

        private PhysicalHandPart GetPhysical(HandAndPart part)
        {            

            var p = this.printList.PhysicalParts.FirstOrDefault(x => x.Hand == part.GetHand() && x.HandPart == part.GetPart());

            return p;
        }

        private void FingerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = sender as Path;

            if (path == null || !this.AllowConditionEdition)
            {
                return;
            }

            var part = (HandAndPart) path.Tag;

            var p = this.GetPhysical(part);

            if (this.DatePopup.IsOpen)
            {
                this.DatePopup.IsOpen = false;
            }
            this.EndDateEdit();

            this.CurrentDate = p.MissingDate;
            this.CurrentFinger = PrintList.GetShortName(p);

            this.DatePopup.Tag = p;


            this.DatePopup.IsOpen = true;
            this.MissingSinceTextbox.Focus();
            this.DatePopup.StaysOpen = false;
            this.CurrentCondition =
                this.printList.Conditions.First(x => x.Code == (p.MissingCode ?? String.Empty)).Status;         
        }

        private void ChangeCondition(HandAndPart part)
        {
            var p = this.GetPhysical(part);

            var missingCondition = this.printList.Conditions.FirstOrDefault(x => x.Code == p.MissingCode);

            var status = missingCondition?.Status ?? HandPartStatus.Present;            
            var newStatus = HandPartStatus.Present;

            switch (status)
            {
                case HandPartStatus.Present:
                    newStatus = HandPartStatus.Amputated;
                    break;
                
                case HandPartStatus.Amputated:
                    newStatus = !this.IsAdvancedMode ? HandPartStatus.Present : HandPartStatus.Bandaged;
                     
                    break;

                case HandPartStatus.Bandaged:
                    newStatus = HandPartStatus.PhysicalLimitation;
                    break;

                case HandPartStatus.PhysicalLimitation:
                    newStatus = HandPartStatus.Present;
                    break;
            }

            var newCondition = this.printList.Conditions.FirstOrDefault(x => x.Status == newStatus);

            p.MissingCode = newCondition?.Code ?? string.Empty;

            this.ApplyBrush(p);

        }

        public bool ShowPalm
        {
            get
            {
                return this.showPalm;
            }
            set
            {
                if (Equals(value, this.showPalm))
                {
                    return;
                }
                this.showPalm = value;
                this.OnPropertyChanged(nameof(this.ShowPalm));
                this.OnPropertyChanged(nameof(this.PalmVisibility));
            }
        }

        public Visibility PalmVisibility
        {
            get
            {
                return this.ShowPalm ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsAmputated
        {
            get { return this.currentCondition == HandPartStatus.Amputated; }
            set
            {
                if (value) this.ReasonChanged(HandPartStatus.Amputated);
            }            
        }

        public bool IsPhysicalLimitation
        {
            get { return this.currentCondition == HandPartStatus.PhysicalLimitation; }
            set { if (value) this.ReasonChanged(HandPartStatus.PhysicalLimitation); }
        }

        public bool IsBandaged
        {
            get { return this.currentCondition == HandPartStatus.Bandaged; }
            set { if (value) this.ReasonChanged(HandPartStatus.Bandaged); }
        }


        public bool IsForeignReason
        {
            get { return this.currentCondition == HandPartStatus.ForeignReason; }
            set { if (value) this.ReasonChanged(HandPartStatus.ForeignReason); }
        }

        public bool IsPresent
        {
            get { return this.currentCondition == HandPartStatus.Present; }
            set { if (value) this.ReasonChanged(HandPartStatus.Present); }
        }


        public bool IsAdvancedMode
        {
            get { return this.isAdvancedMode; }
            set
            {
                this.isAdvancedMode = value;
                this.UpdateStatus();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {            
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReasonChanged(HandPartStatus newStatus)
        {
            var p = (PhysicalHandPart) this.DatePopup.Tag;
            var newCondition = this.printList.Conditions.FirstOrDefault(x => x.Status == newStatus);

            var code = newCondition?.Code ?? string.Empty;

            if ((p.MissingCode ?? string.Empty) == code)
            {
                return;
            }

            p.MissingCode = code;

            this.CurrentCondition = newStatus;            
        }

        private void RefreshCondition()
        {
            this.OnPropertyChanged(nameof(this.IsAmputated));
            this.OnPropertyChanged(nameof(this.IsBandaged));
            this.OnPropertyChanged(nameof(this.IsPhysicalLimitation));
            this.OnPropertyChanged(nameof(this.IsForeignReason));
            this.OnPropertyChanged(nameof(this.IsPresent));

            this.ApplyBrush((PhysicalHandPart)this.DatePopup.Tag);
        }

        private void EndDateEdit()
        {
            this.BindingGroup.CommitEdit();
            var pop = this.DatePopup;
            var p = pop.Tag as PhysicalHandPart;
            if (p == null)
            {
                return;
            }

            p.MissingDate = (!string.IsNullOrEmpty(p.MissingCode) && string.IsNullOrEmpty(this.CurrentDateMessage)) ? this.CurrentDate : null;

            pop.Tag = null;
            this.CurrentDate = string.Empty;

            //var path = this.GetPath(p.HandAndPart);
            //path.ToolTip = p.MissingDate;
        }

        private void DatePopupClosed(object sender, System.EventArgs e)
        {
            this.EndDateEdit();
            
        }

        private void PopupXClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.ClosePopup();
            }
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            this.ClosePopup();
        }

        private void ClosePopup()
        {            
            this.DatePopup.IsOpen = false;            
        }
    }
}
