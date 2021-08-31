namespace PrintsCapture.QuickCaptureControl.Class.App
{
    using System.Windows.Forms;

    using PrintsCapture.Prints;

    public class PrintCaptureAppSettings
    {
        private static PrintCaptureAppSettings _default;
        public const string AppName = "PrintsCapture";

        public static PrintCaptureAppSettings Default
        {
            get
            {
                if (_default == null)
                {
                    Load();
                }
                return _default;
            }
            set { _default = value; }
        }

        public static PrintCaptureAppSettings Load()
        {
            //Default =  AppSettings.GetSettings<PrintCaptureAppSettings>(AppName);
            return Default;
        }

        public static void Save()
        {
            //AppSettings.SaveSettings(Default, AppName);
        }       

        public PrintSettings LivescanSetting { get; set; }
        public PrintSettings CardscanSetting { get; set; }
    }
}
