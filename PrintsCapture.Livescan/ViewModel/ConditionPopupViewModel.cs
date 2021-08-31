using System;
using System.Collections.Generic;
using PrintsCapture.Livescan.Properties;

namespace PrintsCapture.Livescan.ViewModel
{
    using System.ComponentModel;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;

    public class ConditionPopupViewModel : INotifyPropertyChanged
    {
        private string conditionDate;

        private string formatExceptionText;

        public string PrintName { get; set; }

        public Hand Hand { get; set; }

        public HandPart Part { get; set; }

        public HandPartStatus Status { get; set; }

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

        public List<ConditionElementViewModel> AllConditions { get; set; }

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
