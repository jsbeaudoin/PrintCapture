using System;
using System.Windows;
using PrintsCapture.Device.DataLayer;
using XL_ID.Utilities.Log;


namespace PrintsCapture.Livescan
{
    using XL_ID.Utilities.Setting;

    public class LivescanSettings
    {
        private static LivescanSettings _default;
        public const string AppName = "PrintsCapture";

        public static LivescanSettings Default
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

        public static LivescanSettings Load()
        {
            Default = AppSettings.GetSettings<LivescanSettings>(AppName);
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
                LogDispatcher.DoLog("Livescan : Error in SaveSettings", LogEventLevel.Error, ex);
                MessageBox.Show(Prints.Language.CommonText.UnexpectedErrorSeeLogs);
            }
            
        }       

        public int ScannerFingerRes { get; set; }

        public int ScannerPalmRes { get; set; }

        public DeviceSettingList DeviceConfiguration { get; set; }
    }
}
