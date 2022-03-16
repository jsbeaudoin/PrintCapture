using PrintsCapture.Ui.Service;

namespace PrintsCapture.Ui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Documents;

    using JetBrains.Annotations;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Ui.Class;
    using PrintsCapture.Ui.Language;

    using UniBIO.Services.Communication.TransactionService;

    public class FingerPresenceViewModel : INotifyPropertyChanged
    {
        private string missingDate;

        private string missingCode;

        public FingerPresenceViewModel(string printName, string missingCode, string missingDate, PrintInfo print)
        {
            var serv = new MissingReasonService(print);

            this.Reasons = serv.GetList();
            this.MissingCode = missingCode;
            this.MissingDate = missingDate;
            this.PrintName = printName;
        }

        public Visibility ShowMissingdate => Properties.Settings.Default.ShowMissingDate ? Visibility.Visible : Visibility.Collapsed;

        public string PrintName { get; set; }

        public List<MissingReasonViewModel> Reasons { get; private set; }

        public string MissingCode
        {
            get
            {
                return this.missingCode;
            }
            set
            {
                if (value == this.missingCode) return;
                this.missingCode = value;
                this.OnPropertyChanged("MissingCode");
            }
        }

        public string MissingDate
        {
            get
            {
                return this.missingDate;
            }
            set
            {
                // validate non null date
                if (!string.IsNullOrEmpty(value))
                {
                    if (!(value.Length == 7 || value.Length == 10))
                    {
                        throw new ApplicationException(CommonText.ExceptionDateFormat);
                    }

                    int year, month, day;
                    if (!int.TryParse(value.Substring(0,4), out year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        throw new ApplicationException(CommonText.ExceptionDateYear);
                    }

                    if (value.Length >= 7)
                    {
                        if (!int.TryParse(value.Substring(5, 2), out month) || month < 1 || month > 12
                            || new DateTime(year, month, 1) > DateTime.Now)
                        {
                            throw new ApplicationException(CommonText.ExceptionDatePast);
                        }

                        if (value.Length == 10)
                        {                            
                            if (!int.TryParse(value.Substring(8, 2), out day) || day < 1 || day > DateTime.DaysInMonth(year, month)
                                || new DateTime(year, month, day) > DateTime.Now)
                            {
                                throw new ApplicationException(CommonText.ExceptionDatePast);
                            }
                        }
                    }
                }

                this.missingDate = value == string.Empty ? null : value;
            }
        }
       

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
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