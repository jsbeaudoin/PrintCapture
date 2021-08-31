namespace PrintsCapture.Livescan
{
    using System.ComponentModel;
    using System.Windows;

    using PrintsCapture.Device;
    using PrintsCapture.Device.Enum;

    public class DeviceProperty: INotifyPropertyChanged
    {
        private CustomProperty prop;

        private Visibility textBoxVisibility;

        private Visibility checkBoxVisibility;

        public DeviceProperty(CustomProperty prop)
        {
            this.prop = prop;
            this.TextBoxVisibility = prop.ValueKind != CustomPropertyValueKind.Bool ? Visibility.Visible : Visibility.Collapsed;
            this.CheckBoxVisibility = prop.ValueKind == CustomPropertyValueKind.Bool ? Visibility.Visible : Visibility.Collapsed;
        }

        public string Key
        {
            get { return this.prop.InternalKey; }
        }

        public string Label
        {
            get
            {
                if (!string.IsNullOrEmpty(this.prop.DisplayGroup))
                {
                    return string.Format(@"{0} : {1}", this.prop.DisplayGroup, this.prop.DisplayName);
                }
                else
                {
                    return this.prop.DisplayName;
                }
                
            }
        }       

        public string Value
        {
            get
            {
                return this.prop.GetValue();
            }

            set
            {
                this.prop.SetValue(value);

                this.OnPropertyChanged(@"Value");
                this.OnPropertyChanged(@"IsChecked");
            }
        }

        public bool IsChecked
        {
            get
            {
                if (this.prop.ValueKind == CustomPropertyValueKind.Bool && this.prop.GetBoolValue())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (this.prop.ValueKind == CustomPropertyValueKind.Bool)
                {
                    this.prop.SetValue( (value ? CustomProperty.BoolTrue : CustomProperty.BoolFalse).ToString());
                    this.OnPropertyChanged(@"IsChecked");
                }
            }
        }

        public Visibility TextBoxVisibility
        {
            get
            {
                return this.textBoxVisibility;
            }
            private set
            {
                if (value == this.textBoxVisibility)
                {
                    return;
                }
                this.textBoxVisibility = value;
                this.OnPropertyChanged(@"TextBoxVisibility");
            }
        }

        public Visibility CheckBoxVisibility
        {
            get
            {
                return this.checkBoxVisibility;
            }
            private set
            {
                if (value == this.checkBoxVisibility)
                {
                    return;
                }
                this.checkBoxVisibility = value;
                this.OnPropertyChanged(@"CheckBoxVisibility");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}