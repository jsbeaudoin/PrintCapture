using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using PrintsCapture.Ui.Language;
using XL_ID.Utilities.Wpf.ViewModel;

namespace PrintsCapture.Ui.ViewModel
{
    public class EndorsementConsentViewModel : INotifyPropertyChanged
    {
        private const string VsTextName = "VulnerableSectorConsent";
        private const string WaiverTextName = "WaiverConsent";

        private string currentCulture;

        private string consentText;

        private string textName = WaiverTextName;

        private bool isWaiverSelected;

        private bool isVulnerableSelected;

        public EndorsementConsentViewModel()
        {
            var cultures = new List<ListElementViewModel<string>>();

            cultures.Add(new ListElementViewModel<string>("fr", "Français"));
            cultures.Add(new ListElementViewModel<string>("en", "English"));

            this.isWaiverSelected = true;

            this.CultureList = cultures;
        }

        public string CurrentCulture
        {
            get
            {
                return this.currentCulture;
            }
            set
            {
                if (value == this.currentCulture)
                {
                    return;
                }
                this.currentCulture = value;
                this.OnPropertyChanged("CurrentCulture");
                
                this.SetText();
            }
        }

        public string IndividualName { get; set; }

        public List<ListElementViewModel<string>> CultureList { get; private set; }

        public bool IsWaiverSelected
        {
            get
            {
                return this.isWaiverSelected;
            }
            set
            {
                if (value == this.isWaiverSelected)
                {
                    return;
                }
                this.isWaiverSelected = value;
                this.OnPropertyChanged("IsWaiverSelected");

                if (value)
                {
                    this.IsVulnerableSelected = false;
                    textName = WaiverTextName;
                    this.SetText();
                }
            }
        }

        public bool IsVulnerableSelected
        {
            get
            {
                return this.isVulnerableSelected;
            }
            set
            {
                if (value == this.isVulnerableSelected)
                {
                    return;
                }
                this.isVulnerableSelected = value;
                this.OnPropertyChanged("IsVulnerableSelected");

                if (value)
                {
                    this.IsWaiverSelected = false;
                    textName = VsTextName;
                    this.SetText();
                }
            }
        }

        public string ConsentText
        {
            get
            {
                return this.consentText;
            }
            set
            {
                if (value == this.consentText)
                {
                    return;
                }
                this.consentText = value;
                this.OnPropertyChanged("ConsentText");                
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SetText()
        {
            var cultureInfo = new CultureInfo(this.currentCulture);
            var text = Text.ResourceManager.GetString(textName, cultureInfo);
            if (text != null)
            {
                this.ConsentText = string.Format(text, cultureInfo, "", IndividualName);
            }
            
        }
    }
}
