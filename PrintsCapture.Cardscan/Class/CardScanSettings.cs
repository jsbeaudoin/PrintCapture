using System;
using System.ComponentModel.Design.Serialization;
using System.Xml.Serialization;
using PrintsCapture.Cardscan.ViewModel;
using PrintsCapture.Cardscan.Window;
using PrintsCapture.Device.DataLayer;
using PrintsCapture.Prints.Language;

namespace PrintsCapture.Cardscan
{
    using System.Collections.Generic;

    using XL_ID.Utilities.Setting;

    public class CardScanSettings
    {

        private static CardScanSettings _default;
        public const string AppName = "PrintsCapture";
        public const string ModelFolder = "CardScanModels";

        public static CardScanSettings Default
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

        public static CardScanSettings Load()
        {
            //AppSettings.DeleteSettings<CardScanSettings>(AppName);
            Default = AppSettings.GetSettings<CardScanSettings>(AppName);

            if (Default.Presets == null)
            {
                Default.Presets = ScanPreset.GetBaseList();
            }

            if (Default.ScanModels == null)
            {
                Default.ScanModels = new ScanModelList(AppSettings.GetSettingsList<ScanModel>(AppName, ModelFolder));
            }

            return Default;
        }

        public static void Save()
        {
            AppSettings.SaveSettings(Default, AppName);
            AppSettings.SaveSettingsList(AppName, ModelFolder, Default.ScanModels.GetSaveList());
        }        


        public CardScanSettings()
        {
            this.PresetCode = 1;
        }

        public List<ScanPreset> Presets { get; set; }

        [XmlIgnore]
        public ScanModelList ScanModels { get; set; }

        public int PresetCode { get; set; }

        public float PresetManualOffset { get; set; }

        public float PresetManualSize { get; set; }

        public int ZoomLevel { get; set; }

        public DeviceSettingList DeviceConfig { get; set; }
       
    }
}
