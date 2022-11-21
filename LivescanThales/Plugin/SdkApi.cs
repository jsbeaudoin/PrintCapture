using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using PrintsCapture.Device.Enum;
using PrintsCapture.Device.Interface;
using PrintsCapture.Device.LivescanThales.Sdk;
using PrintsCapture.Prints.Enum;

namespace PrintsCapture.Device.LivescanThales.Plugin
{
    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public CaptureKind DeviceKind { get; private set; }

        public SdkApi()
        {
            this.DeviceKind = CaptureKind.Livescan;
            this.IsProvidingSerial = true;
            this.InitializeDeviceList();
        }

        public bool Open()
        {
            if (this.IsOpened)
            {
                return true;
            }

            this.SetDeviceInfo();

            try
            {
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_EnableDrySkinImgEnhance(true);
            }
            catch (Exception ex)
            {
                this.LastException = new ApplicationException("Cannot enable Dry Skin enhancement.", ex);
                return false;
            }
            

            this.LastException = null;                                 

            this.IsOpened = true;
            return true;
        }

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public ICaptureDevice AutoDetectedDevice()
        {
            try
            {
                this.Open();
                                

                var list = this.GetPluggedList();
                if (list == null || list.Count == 0)
                {
                    return null;
                }

                var result = list.First();
                                
                this.Close();
                return result;
            }
            catch (Exception ex)
            {
                this.LastException = ex;
                return null;
            }
        }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            return this.GetPluggedList();
        }

        public void Close()
        {
            this.IsOpened = false;
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return false; } }

        public Exception LastException { get; private set; }

        internal List<DeviceApi> GetPluggedList()
        {            
            var results = SupportedDeviceList.Cast<DeviceApi>().Where(x => x.IsPlugged).ToList();
            
            return results;
        }

        private void SetDeviceInfo()
        {
            var attachedDeviceList = new GBMSAPI_NET_DeviceInfoStruct[
                    GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM];
            for (int i = 0; i < GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM; i++)
            {
                attachedDeviceList[i] = new GBMSAPI_NET_DeviceInfoStruct();
            }

            int attachedDeviceCount;
            uint usbErrorCode;

            int retVal = GBMSAPI_NET_DeviceSettingRoutines.GBMSAPI_NET_GetAttachedDeviceList(
                attachedDeviceList, out attachedDeviceCount, out usbErrorCode);

            if (retVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                this.LastException = new SdkException("NONE", "GetPluggedList", SdkErrorKind.DeviceNotFound, string.Format("Cannot list devices. Usb Error : {0}", usbErrorCode));
            }

            if (attachedDeviceCount <= 0)
            {
                return;
            }

            var referenceList = new List<GBMSAPI_NET_DeviceInfoStruct>();
            for (var index = 0; index < attachedDeviceCount; index++)
            {
                referenceList.Add(attachedDeviceList[index]);
            }

            var results = new List<DeviceApi>();
            foreach (var captureDevice in SupportedDeviceList)
            {
                var capt = captureDevice as DeviceApi;
                var pluggedDevice = referenceList.FirstOrDefault(x => x.DeviceID == capt.ThalesId);
                if (pluggedDevice != null)
                {
                    capt.ModelName = ThalesDeviceNames.GetName(capt.ThalesId);
                    capt.SerialNumber = pluggedDevice.DeviceSerialNumber;
                    capt.IsPlugged = true;
                    results.Add(capt);
                }
            }
        }

        private void InitializeDeviceList()
        {
            this.SupportedDeviceList = new List<ICaptureDevice>
                                       {
                                           /*new DeviceApi(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C, 
                                               "Thales DactyScan", 
                                               PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger, 
                                               this, 
                                               "pack://application:,,,/PrintsCapture.Device.LivescanThales;component/Images/Ds84c_Flat.png",
                                               false  ),
                                        new DeviceApi(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527, 
                                               "Thales Multiscan 527", 
                                               PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatPartialPalm, 
                                               this, 
                                               "pack://application:,,,/PrintsCapture.Device.LivescanThales;component/Images/Ms527.png",
                                               true  ),*/
                                        new DeviceApi(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_CS500Q,
                                               "Thales CS500Q",
                                               PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatPartialPalm,
                                               this,
                                               "pack://application:,,,/PrintsCapture.Device.LivescanThales;component/Images/CS500Q_v2.png",
                                               true  ),
                                        

                                       };

        }

        
    }
}
