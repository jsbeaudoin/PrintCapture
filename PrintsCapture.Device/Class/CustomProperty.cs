// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomProperty.cs" company="Solutions XL-ID inc.">
//   Update text
// </copyright>
// <summary>
//   Defines the CustomProperty type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PrintsCapture.Device
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using PrintsCapture.Device.Enum;

    public class CustomProperty
    {
        public const int BoolTrue = 1;
        public const int BoolFalse = 0;

        internal CustomPropertyList List;

        private List<ValueElement> valueList;

        internal CustomProperty()
        {
            this.valueList = new List<ValueElement>();
        }

        public string InternalKey { get; internal set; }

        public string DisplayName { get; internal set; }

        public string DisplayGroup { get; internal set; }

        public CustomPropertyValueKind ValueKind { get; internal set; }

        public ReadOnlyCollection<ValueElement> ValueList { get { return this.valueList.AsReadOnly(); } }

        public RangeElement ValueRange { get; internal set; }

        public string HelpText { get; set; }

        public string DefaultValue { get; internal set; }

        public void DefineValueList(IEnumerable<ValueElement> values)
        {
            this.valueList.Clear();
            this.valueList.AddRange(values);
        }

        public bool ValidateValue(string newValue)
        {
            switch (this.ValueKind)
            {
                case CustomPropertyValueKind.Text:
                    return true;                    

                case CustomPropertyValueKind.Int:
                    int test;
                    return int.TryParse(newValue, out test);                    

                case CustomPropertyValueKind.Bool:
                    int intTest;
                    if (!int.TryParse(newValue, out intTest))
                    {
                        return false;
                    }

                    return intTest == BoolFalse || intTest == BoolTrue;

                case CustomPropertyValueKind.Range:
                    int rangeTest;
                    if (this.ValueRange == null) {
                        return false;
                    }

                    if (!int.TryParse(newValue, out rangeTest))
                    {
                        return false;
                    }

                    return rangeTest >= this.ValueRange.Min && rangeTest <= this.ValueRange.Max;

                case CustomPropertyValueKind.List:
                    if (this.ValueList == null)
                    {
                        return false;
                    }

                    var val = this.ValueList.SingleOrDefault(x => x.Value == newValue);

                    return val != null;
            }

            return false;

        }

        public bool SetValue(string newValue)
        {
            // 1st --> IS value valid ?
            if (!this.ValidateValue(newValue))
            {
                return false;
            }

            var val = this.List.PropertyValues.SingleOrDefault(x => x.Key == this.InternalKey);

            if (val == null)
            {
                val = new ValueElement(this.InternalKey, string.Empty);
                this.List.PropertyValues.Add(val);
            }

            val.Value = newValue;           

            return true;
        }

        public string GetValue()
        {
            // 1st --> Is value defined ?
            var val = this.List.PropertyValues.SingleOrDefault(x => x.Key == this.InternalKey);

            string result = val == null ? this.DefaultValue : val.Value;

            return result;
        }

        public int GetIntValue()
        {
            int result;
            var text = this.GetValue();            

            int.TryParse(text, out result);

            return result;
        }

        public bool GetBoolValue()
        {
            int result;
            var text = this.GetValue();

            int.TryParse(text, out result);

            return result == BoolTrue;
        }
    }
}
