using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Livescan.ViewModel
{
    using System.ComponentModel;
    using System.Windows;

    using PrintsCapture.Livescan.Properties;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;

    public class FingerStatusViewModel : INotifyPropertyChanged
    {
        private string conditionDate;

        private string formatExceptionText;

        private bool isAvailable;

        private string missingReasonCode;

        public FingerStatusViewModel(HandAndPart part)
        {
            this.Part = part;
        }

        public HandAndPart Part { get; }

        public bool IsMissing => string.IsNullOrEmpty(this.MissingReasonCode);

        public bool IsAvailable
        {
            get
            {
                return this.isAvailable;
            }
            set
            {
                if (value == this.isAvailable) return;
                this.isAvailable = value;
                this.OnPropertyChanged(nameof(this.IsAvailable));
                this.OnPropertyChanged(nameof(this.PartVisibility));
            }
        }

        public Visibility PartVisibility
            => (this.IsMissing && this.IsAvailable) ? Visibility.Visible : Visibility.Collapsed;

        public string FingerName { get; set; }

        public string MissingReasonLabel { get; private set; }

        public string MissingReasonCode
        {
            get
            {
                return this.missingReasonCode;
            }
            set
            {
                if (value == this.missingReasonCode) return;
                this.missingReasonCode = value;
                this.OnPropertyChanged(nameof(this.MissingReasonCode));
                this.OnPropertyChanged(nameof(this.IsMissing));
                this.OnPropertyChanged(nameof(this.PartVisibility));
            }
        }

        public bool HasDate => !string.IsNullOrEmpty(this.ConditionDate);

        public string ConditionDate
        {
            get
            {
                return this.conditionDate;
            }
            set
            {
                // validate non null date
                if (!string.IsNullOrEmpty(value))
                {
                    if (!(value.Length == 4 || value.Length == 7 || value.Length == 10))
                    {
                        this.FormatExceptionText = CommonText.ExceptionDateFormat;
                        return;
                    }

                    int year, month, day;
                    if (!int.TryParse(value.Substring(0, 4), out year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        this.FormatExceptionText = CommonText.ExceptionDateYear;
                        return;
                    }

                    if (value.Length >= 7)
                    {
                        if (!int.TryParse(value.Substring(5, 2), out month) || month < 1 || month > 12
                            || new DateTime(year, month, 1) > DateTime.Now)
                        {
                            this.FormatExceptionText = CommonText.ExceptionDatePast;
                            return;
                        }

                        if (value.Length == 10)
                        {
                            if (!int.TryParse(value.Substring(8, 2), out day) || day < 1 || day > DateTime.DaysInMonth(year, month)
                                || new DateTime(year, month, day) > DateTime.Now)
                            {
                                this.FormatExceptionText = CommonText.ExceptionDatePast;
                                return;
                            }
                        }
                    }
                }

                this.conditionDate = value == string.Empty ? null : value;
                this.FormatExceptionText = string.Empty;
                this.OnPropertyChanged(nameof(HasDate));
            }
        }

        public string FormatExceptionText
        {
            get
            {
                return this.formatExceptionText;
            }
            set
            {
                if (value == this.formatExceptionText)
                {
                    return;
                }
                this.formatExceptionText = value;
                this.OnPropertyChanged(@"FormatExceptionText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
