using System;
using System.Collections.Generic;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;

namespace PrintsCapture.Device.LivescanThales.Sdk
{
    internal static class ThalesDeviceNames
    {
        private static Dictionary<byte, string> devicesDictionary = new Dictionary<byte, string>();

        private static void Load()
        {
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_CS500Q, "CS500Q");
            devicesDictionary.Add(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_CS500F, "CS500F");
        }


        static ThalesDeviceNames()
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
