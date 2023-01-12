using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintsCapture.Livescan;


namespace PrintsCapture.Ui.Class
{
    using PrintsCapture.Prints;
    using System.IO;
    using XL_ID.Utilities.Log;
    using XL_ID.Utilities.Setting;
    using XL_ID.Utilities.XML;

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
            try
            {
                Default = AppSettings.GetSettings<PrintCaptureAppSettings>(AppName);
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"AppSettings - Cannot deserialize settings file";
                LogDispatcher.DoLog(errMsg, LogEventLevel.Error, ex);
                throw new ApplicationException(errMsg, ex);
            }   

            return Default;
        }

        public static void Save()
        {
            try
            {
                AppSettings.SaveSettings(Default, AppName);
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot serialize settings file";
                LogDispatcher.DoLog(errMsg, LogEventLevel.Error, ex);
                throw new ApplicationException(errMsg, ex);
            }
        }

        public PrintSettings LivescanSetting { get; set; }
        public PrintSettings CardscanSetting { get; set; }
    }
}
