namespace PrintsCapture.Prints.Sequence
{
    using PrintsCapture.Prints.Enum;

    public class CaptureDeviceInfo
    {
        private string make;
        private string modelName;
        private string serialNumber;

        public string Make
        {
            get { return make; }
            set
            {
                make = this.TrimText(value);
            }
        }

        public string ModelName
        {
            get { return modelName; }
            set
            {
                modelName = this.TrimText(value);
            }
        }

        public string SerialNumber
        {
            get { return serialNumber; }
            set { serialNumber = this.TrimText(value); }
        }

        public CaptureKind Kind { get; set; }

        private string TrimText(string text)
        {
            if (text == null)
            {
                return null;
            }

            var newText = text.ToUpperInvariant();

            newText = newText.Replace("+", "PLUS").Replace("@","AT").Replace("%","PER");

            return newText;
        }
    }
}
