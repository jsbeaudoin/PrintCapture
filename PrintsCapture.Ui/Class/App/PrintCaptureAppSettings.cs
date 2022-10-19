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
    using System.IO;
    using XL_ID.Utilities.Log;
    using XL_ID.Utilities.Setting;
    using XL_ID.Utilities.XML;

    public class PrintCaptureAppSettings
    {
        private static PrintCaptureAppSettings _default;
        private static string FilePath = "";

        public const string AppName = "PrintsCapture";
        public const string SaveFolderName = "Configuration";
        public const string SettingsExtension = ".config";

        static PrintCaptureAppSettings() {
            SetSaveFolder(); // TODO : Define.. working in legacy AND remoteModule
        }

        internal static void SetSaveFolder()
        {
            string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(exeFilePath);
            var folder = Path.Combine(path, SaveFolderName);
            FilePath = Path.Combine(folder, AppName + SettingsExtension);
            
            if (Directory.Exists(folder)) return;

            LogDispatcher.DoLog($"Directory '{SaveFolderName}' did not exist. It will be created.");
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Directory creation for '{SaveFolderName}' failed. {ex.Message}", ex);
            }
        }

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

            if (!File.Exists(FilePath))
            {
                Default = new PrintCaptureAppSettings();
            } else
            {
                try
                {
                    Default = ObjectSerializer.GetInstanceFromXml<PrintCaptureAppSettings>(FilePath);
                }
                catch (Exception ex)
                {
                    var errMsg =
                        $"AppSettings - Cannot deserialize settings file : {Path.GetFileNameWithoutExtension(FilePath)}";
                    LogDispatcher.DoLog(errMsg, LogEventLevel.Error, ex);
                    throw new ApplicationException(errMsg, ex);
                }
            }

            return Default;
        }

        public static void Save()
        {
            try
            {
                ObjectSerializer.SaveInstanceToXml(FilePath, Default);
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot serialize settings file : {Path.GetFileNameWithoutExtension(FilePath)}";
                LogDispatcher.DoLog(errMsg, LogEventLevel.Error, ex);
                throw new ApplicationException(errMsg, ex);
            }
        }       

        public PrintSettings LivescanSetting { get; set; }
        public PrintSettings CardscanSetting { get; set; }
    }
}
