using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintsCapture.Livescan;


namespace PrintsCapture.Ui.Class
{
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.Setting;

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
            Default = AppSettings.GetSettings<PrintCaptureAppSettings>(AppName);
            return Default;
        }

        public static void Save()
        {
            AppSettings.SaveSettings(Default, AppName);
        }       

        public PrintSettings LivescanSetting { get; set; }
        public PrintSettings CardscanSetting { get; set; }
    }
}
