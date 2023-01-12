namespace PrintsCapture.Prints
{
    public class PrintSettings
    {
        public PrintSettings()
        {
            this.Rules = new PrintRules();
        }

        public string SelectedDeviceKey { get; set; }

        public PrintRules Rules { get; set; }

    }
}
