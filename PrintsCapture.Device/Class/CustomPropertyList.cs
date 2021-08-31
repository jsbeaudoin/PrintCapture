namespace PrintsCapture.Device
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using PrintsCapture.Device.Enum;

    public class CustomPropertyList
    {
        private Dictionary<string, CustomProperty> properties;

        public CustomPropertyList()
        {
            this.properties = new Dictionary<string, CustomProperty>();            
            this.PropertyValues = new List<ValueElement>();
        }        

        internal List<ValueElement> PropertyValues { get; private set; }

        public CustomProperty GetProperty(string key)
        {
            if (!this.properties.ContainsKey(key)) {
                return null;
            }

            var prop = this.properties[key];
            
            return prop;
        }

        public ReadOnlyCollection<CustomProperty> GetPropertyList()
        {
            return this.properties.Select(x => x.Value).ToList().AsReadOnly();
        }

        public void AddTextProperty(string key, string displayName, string defaultValue, string groupName)
        {
            var prop = new CustomProperty();
            prop.InternalKey = key;
            prop.DisplayName = displayName;
            prop.DisplayGroup = groupName;
            prop.DefaultValue = defaultValue;
            prop.ValueKind = CustomPropertyValueKind.Text;

            this.AddProperty(prop);
        }

        public void AddBoolProperty(string key, string displayName, bool defaultValue, string groupName)
        {            
            var prop = new CustomProperty();            
            prop.InternalKey = key;
            prop.DisplayName = displayName;
            prop.DisplayGroup = groupName;
            prop.DefaultValue = defaultValue ? CustomProperty.BoolTrue.ToString() : CustomProperty.BoolFalse.ToString();
            prop.ValueKind = CustomPropertyValueKind.Bool;

            this.AddProperty(prop);
        }

        public void AddIntProperty(string key, string displayName, int defaultValue, string groupName)
        {
            var prop = new CustomProperty();            
            prop.InternalKey = key;
            prop.DisplayName = displayName;
            prop.DisplayGroup = groupName;
            prop.DefaultValue = defaultValue.ToString();
            prop.ValueKind = CustomPropertyValueKind.Int;

            this.AddProperty(prop);
        }

        public void AddRangeProperty(string key, string displayName, int defaultValue, int min, int max, string groupName)
        {
            var prop = new CustomProperty();            
            prop.InternalKey = key;
            prop.DisplayName = displayName;
            prop.DisplayGroup = groupName;
            prop.DefaultValue = defaultValue.ToString();

            prop.ValueRange = new RangeElement(min, max);
            prop.ValueKind = CustomPropertyValueKind.Range;

            this.AddProperty(prop);            
        }

        public bool SetValue(string propertyKey, string newValue)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return false;
            }

            return prop.SetValue(newValue);
        }

        public bool SetValue(string propertyKey, bool newValue)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return false;
            }

            if (prop.ValueKind != CustomPropertyValueKind.Bool)
            {
                return false;
            }

            return prop.SetValue(newValue ? CustomProperty.BoolTrue.ToString() : CustomProperty.BoolFalse.ToString());
        }

        public bool SetValue(string propertyKey, int newValue)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return false;
            }

            if (! (prop.ValueKind == CustomPropertyValueKind.Int || prop.ValueKind == CustomPropertyValueKind.Range))
            {
                return false;
            }

            return prop.SetValue(newValue.ToString());
        }

        public string GetValue(string propertyKey)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return null;
            }            

            return prop.GetValue();
        }

        public int GetIntValue(string propertyKey)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return 0;
            }            

            return prop.GetIntValue();
        }

        public bool GetBoolValue(string propertyKey)
        {
            var prop = this.GetProperty(propertyKey);

            if (prop == null)
            {
                return false;
            }            

            return prop.GetBoolValue();
        }

        private void AddProperty(CustomProperty prop)
        {
            prop.List = this;

            this.properties.Add(prop.InternalKey, prop);
        }

    }
}
