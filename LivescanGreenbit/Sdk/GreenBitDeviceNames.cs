using System;
using System.Collections.Generic;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;

namespace PrintsCapture.Device.LivescanGreenbit.Sdk
{
    internal static class GreenBitDeviceNames
    {
        private static Dictionary<byte, string> devicesDictionary = new Dictionary<byte, string>();

        private static void Load()
        {
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26, "DactyScan26");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32, "");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40, "DactyScan40");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I, "DactyScan40I");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84, "DactyScan84");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C, "DactyScan84C");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC500, "MC500");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517, "MC517");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS1000, "MultiScan1000");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS500, "MultiScan500");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC500, "MSC500");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517, "MSC517");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_PS2, "Poliscan2");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_VS3, "Visascan3");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527, "MS527");
        }


        static GreenBitDeviceNames()
        {
            Load();
        }

        public static string GetName(byte id)
        {
            if (devicesDictionary.ContainsKey(id))
            {
                return devicesDictionary[id];
            }

            throw new ApplicationException("Id is not in Sdk supported list");
        }

    }
}
