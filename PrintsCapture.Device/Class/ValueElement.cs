namespace PrintsCapture.Device
{
    public class ValueElement
    {
        public ValueElement(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; private set; }

        public string Value { get; internal set; }
    }
}
