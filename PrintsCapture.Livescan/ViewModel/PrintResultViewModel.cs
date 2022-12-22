using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using PrintsCapture.Livescan.Properties;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Prints.Language;
using XL_ID.Utilities.XML;

namespace PrintsCapture.Livescan.ViewModel
{
    public class PrintResultViewModel : INotifyPropertyChanged
    {
        private ComboBoxItemViewModel selectedOption;

        public PrintResultViewModel(PrintInfo print)
        {
            var lang = CommonText.Culture.TwoLetterISOLanguageName;
            var overrideReasonList = OverrideReason.GetBaseList().Where(x => x.Language == lang).ToList();
            var choiceItems = new List<ComboBoxItemViewModel>();
            choiceItems.Add(new ComboBoxItemViewModel {Label = CommonText.ScanAgain, IsEnabled = true, Value = -1});

            this.Name = PrintList.GetShortName(print);
            this.selectedOption = choiceItems.First();

            if (print.NistPosition != print.MatchedNistPosition && print.MatchedNistPosition != 0)
            {
                this.OriginalPositionText = string.Format(CommonText.OriginalPosition, PrintList.GetShortName(print.PrintList, print.MatchedNistPosition)) ;
                this.OriginalPositionVisibility = Visibility.Visible;
            }
            else
            {
                this.OriginalPositionText = string.Empty;
                this.OriginalPositionVisibility = Visibility.Collapsed;
            }

            bool allowOverride = false;

            switch (print.Status)
            {
                case PrintStatus.Empty:
                    this.StatusText = CommonText.NoPrint;
                    this.StatusBrush = Brushes.Black;
                    break;

                case PrintStatus.InError:
                case PrintStatus.InErrorSwapped:
                    this.StatusText = CommonText.NotGood;
                    this.StatusBrush = Brushes.Red;
                    allowOverride = true;
                    break;

                case PrintStatus.Missing:
                    this.StatusText = CommonText.MissingFinger;
                    this.StatusBrush = Brushes.Green;
                    break;

                case PrintStatus.Overriden:
                    var reason = overrideReasonList.FirstOrDefault(x => x.Code == print.OverrideCode);
                    this.StatusText = string.Format(CommonText.ErrorIgnored, reason?.Text);                    
                    this.StatusBrush = Brushes.Green;
                    allowOverride = true;
                    break;

                case PrintStatus.Validated:
                case PrintStatus.ValidatedSwapped:
                    this.StatusText = CommonText.PrintsStatusOk;
                    this.StatusBrush = Brushes.Green;                    
                    break;

            }

            if (allowOverride)
            {
                choiceItems.Add(new ComboBoxItemViewModel
                {
                    Label = CommonText.IgnoreWithReason,
                    IsEnabled = false,
                    Value = 0
                });

                foreach (var overrideReason in overrideReasonList)
                {
                    choiceItems.Add(new ComboBoxItemViewModel
                    {
                        Label = overrideReason.Text,
                        Value = overrideReason.Code,
                        IsEnabled = true
                    });
                }

                if (print.IsOverriden)
                {
                    this.selectedOption = choiceItems.FirstOrDefault(x => x.Value == print.OverrideCode);
                }
            }
            else
            {
                choiceItems.Add(new ComboBoxItemViewModel { Label=CommonText.Accept, IsEnabled = true, Value = -2});
                this.selectedOption = choiceItems.Last();
            }

            this.PrintImage = print.PrintList.GetImage(print);
            this.Options = choiceItems;
        }

        public string Name { get; set; }

        public string OriginalPositionText { get; set; }

        public Visibility OriginalPositionVisibility { get; set; }

        public string StatusText { get; set; }

        public Brush StatusBrush { get; set; }

        public ImageSource PrintImage { get; set; }


        public List<ComboBoxItemViewModel> Options { get; set; }

        public ComboBoxItemViewModel SelectedOption
        {
            get { return selectedOption; }
            set
            {
                selectedOption = value; 
                this.OnPropertyChanged(nameof(SelectedOption));
                this.OnPropertyChanged(nameof(IsSelectedToOverride));
                this.OnPropertyChanged(nameof(IsSelectedToScanAgain));
                this.OnPropertyChanged(nameof(IsSelectedToBeAccepted));
            }
        }

        public bool IsSelectedToOverride => this.SelectedOption != null && this.SelectedOption.Value > 0;

        public bool IsSelectedToScanAgain => this.SelectedOption != null && this.SelectedOption.Value == -1;

        public bool IsSelectedToBeAccepted => this.SelectedOption != null && this.SelectedOption.Value == -2;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
