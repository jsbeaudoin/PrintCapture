namespace PrintsCapture.Device.CardscanAware.Plugin
{
    using PrintsCapture.Prints.Enum;

    public class AwareScanner
    {
        public string DisplayName { get;  set; }

        public string InternalKey { get;  set; }

        public string HardwareMake { get;  set; }

        public string ModelName { get;  set; }

        public string ImageUri { get; set; }

        public PrintResolution SupportedResolutions { get;  set; }
    }
}
