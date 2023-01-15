// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SdkApi.cs" company="Solutions XL-ID inc">
//   
// </copyright>
// <summary>
//   Defines the SdkApi type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Livescan.Scanners.DriverEssential;

namespace PrintsCapture.Device.LivescanCrossmatchEssential.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;

    using Livescan.Scanners.DriverEssential.Plugin;
    using Livescan.Scanners.DriverEssential.Sdk;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;
    using XL_ID.Utilities.Log;

    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        private List<DeviceApi> supportedDevices = null;

        private bool mustInitialize;

        //private ILivescanDevice connectedDevice;                

        internal static List<DeviceInfo> GetConnectedDevices()
        {
            var result = new List<DeviceInfo>();            
            
            try
            {
                int count = 0;                

                int rslt = LSE_SDK.LSCAN_Main_GetDeviceCount(ref count);
                var essentialDevice = new LScanDeviceInfo() { interfaceType = "", serialNumber = "", typeName = "" };
                
                for (int i = 0; i < count; i++)
                {
                    LSE_SDK.LSCAN_Main_GetDeviceInfo(i, out essentialDevice);

                    result.Add(new DeviceInfo(i, essentialDevice));
                }
                
            }
            catch (Exception)
            {
                // nothing !
            }

            return result;
        }

        public SdkApi()
        {
            this.CheckApiVersion();
            this.InitSupportedList();
            this.IsProvidingSerial = true;
        }

        public bool Open()
        {
            try
            {
                this.LastException = null;

                if (this.mustInitialize)
                {
                    LSE_SDK.LSCAN_InitAPI();
                }

                this.IsOpened = true;
                return true;
            }
            catch (Exception ex)
            {
                this.LastException = ex;
                LogDispatcher.DoLog("CrossmatchApi Open failed", LogEventLevel.Error, ex);
                return false;
            }
        }

        public CaptureKind DeviceKind { get; private set; }
        

        public bool IsOpened { get; private set; }        

        public IEnumerable<ICaptureDevice> SupportedDeviceList
        {
            get { return this.supportedDevices; }
        }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            if (!this.IsOpened)
            {
                return null;
            }

            var devices = SdkApi.GetConnectedDevices();

            if (devices == null || devices.Count == 0)
            {
                return null;
            }

            var result = new List<ICaptureDevice>();

            foreach (var device in devices)
            {
                var livescan = this.supportedDevices.FirstOrDefault(x => x.InternalKey == device.TypeName);
                if (livescan != null)
                {
                    result.Add(livescan);
                }
            }

            return result;            
        }

        public void Close()
        {
            if (this.supportedDevices == null)
            {
                return;
            }

            try
            {
                if (this.mustInitialize)
                {
                    LSE_SDK.LSCAN_ExitAPI();
                }
                
            }
            catch (Exception ex)
            {
                // Nothing !
                this.LastException = ex;
                LogDispatcher.DoLog("CrossmatchApi Close failed", LogEventLevel.Error, ex);
            }

            this.IsOpened = false;            
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return false; } }

        public Exception LastException { get; private set; }

        private void InitSupportedList()
        {
            LogDispatcher.DoLog("Init Supported List CrossmatchEssential", LogEventLevel.Info);
            this.supportedDevices = new List<DeviceApi>();

            this.supportedDevices.Add(new DeviceApi(
                "L SCAN GUARDIAN USB", 
                "Crossmatch Guardian", 
                PrintResolution.Dpi500, 
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/LivescanGuardian.png",
                this, true) );

            this.supportedDevices.Add(new DeviceApi(
                "PATROL", 
                "Crossmatch Patrol",
                PrintResolution.Dpi500, 
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/LivescanPatrol.png",
                this, true));

            this.supportedDevices.Add(new DeviceApi(
                "PATROL ID",
                "Crossmatch Patrol ID",
                PrintResolution.Dpi500,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/LivescanPatrol.png",
                this, true));

            this.supportedDevices.Add(new DeviceApi(
                "L SCAN 500P",
                "Crossmatch 500P",
                PrintResolution.Dpi500,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger |
                    DeviceScanKind.FlatPartialPalm,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/Livescan1000Px.png",
                this));

            this.supportedDevices.Add(new DeviceApi(
                "L SCAN 1000P", 
                "Crossmatch 1000P",
                PrintResolution.Dpi500 | PrintResolution.Dpi1000,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger |
                    DeviceScanKind.FlatPartialPalm,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/Livescan1000Px.png",
                this));

            this.supportedDevices.Add(new DeviceApi("L SCAN 1000PX", "Crossmatch 1000 px",
                PrintResolution.Dpi500 | PrintResolution.Dpi1000, 
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger |
                    DeviceScanKind.FlatPartialPalm,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/Livescan1000Px.png",
                this ));

           
            this.supportedDevices.Add(new DeviceApi(
                "GUARDIAN 200",
                "Crossmatch Guardian 200",
                PrintResolution.Dpi500,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/LivescanGuardian.png",
                this));

            this.supportedDevices.Add(new DeviceApi("L SCAN 1000", "Crossmatch LScan 1000",
                PrintResolution.Dpi500 | PrintResolution.Dpi1000,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger |
                    DeviceScanKind.FlatPartialPalm,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/Livescan1000Px.png",
                this));

            this.supportedDevices.Add(new DeviceApi("L SCAN 500", "Crossmatch LScan 500",
                PrintResolution.Dpi500 ,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger |
                    DeviceScanKind.FlatPartialPalm,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/Livescan1000Px.png",
                this));

            this.supportedDevices.Add(new DeviceApi(
                "*",
                "Crossmatch ANY DEVICE",
                PrintResolution.Dpi500,
                DeviceScanKind.FlatSingleFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanCrossmatchEssential;component/Images/LivescanPatrol.png",
                this, true));

        }        

        private void CheckApiVersion()
        {
            try
            {
                var appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty) + "\\";
                var dllPath = appPath + "LScanEssentials.dll";
                var versionApi = FileVersionInfo.GetVersionInfo(dllPath);
                var productVersion = versionApi.ProductVersion.Replace(',','.');
                Version version;

                LogDispatcher.DoLog($"Checking API Version. Dll path : {dllPath} Version API: {versionApi}", LogEventLevel.Info);

                if (!Version.TryParse(productVersion, out version))
                {
                    mustInitialize = false;
                }
                else
                {
                    mustInitialize = (version.Major > 6);            
                }
            }
            catch (Exception e)
            {
                LogDispatcher.DoLog("LivescanCrossmatchEssential - CheckApiVersion", LogEventLevel.Error, e);
            }
            
        }
    }
}
