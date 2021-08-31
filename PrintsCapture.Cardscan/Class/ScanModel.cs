using XL_ID.Utilities.Setting;

namespace PrintsCapture.Cardscan
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Prints.Language;

    public class ScanModel : INotifyPropertyChanged, ISettingFile
    {
        private bool isStarred;

        private string name;

        public ScanModel()
        {
            this.Zones = new List<ScanPrintZone>();
        }        

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == this.name)
                {
                    return;
                }
                this.name = value;
                
                var translated =
                    CommonText.ResourceManager.GetString(
                        "FixedModelLabel_" + this.name.Replace("_","").Replace("@", "").Replace(" ", ""),
                        CommonText.Culture);
                this.Label = string.IsNullOrEmpty(translated) ? value : translated;                

                this.OnPropertyChanged("Name");
            }
        }

        

        [XmlIgnore]
        public string Label { get; private set; }

        public bool IsStarred
        {
            get
            {
                return this.isStarred;
            }
            set
            {
                if (value.Equals(this.isStarred))
                {
                    return;
                }
                this.isStarred = value;
                this.OnPropertyChanged("IsStarred");
            }
        }

        public DateTime CreationDateTime { get; set; }

        public bool IsReadOnly { get; set; }
               
        public List<ScanPrintZone> Zones { get; set; }                       

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlIgnore]
        public string SettingFileName { get; set; }
    }


}
