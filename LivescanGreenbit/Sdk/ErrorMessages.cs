using System;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;

namespace PrintsCapture.Device.LivescanGreenbit.Sdk
{
    public class ErrorMessages
    {
        public static String GBMSAPI_Example_GetErrorStringFromCode(Int32 ErrorCode)
        {
            switch (ErrorCode)
            {
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR):
                    {
                        return "NO_ERROR";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_DRIVER):
                    {
                        return "USB_DRIVER";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_FOUND):
                    {
                        return "DEVICE_NOT_FOUND";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_THREAD):
                    {
                        return "USB_THREAD";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_PARAMETER):
                    {
                        return "PARAMETER";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCANNER_NOT_CONFIGURED):
                    {
                        return "SCANNER_NOT_CONFIGURED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_RESPONDING):
                    {
                        return "DEVICE_NOT_RESPONDING";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCANNER_COMMUNICATION):
                    {
                        return "SCANNER_COMMUNICATION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_UNAVAILABLE_OPTION):
                    {
                        return "UNAVAILABLE_OPTION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_INTERNAL):
                    {
                        return "INTERNAL";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SPECIFIC_DLL_NOT_LOADED):
                    {
                        return "SPECIFIC_DLL_NOT_LOADED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_METHOD_NOT_SUPPORTED):
                    {
                        return "METHOD_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_OBJECT_TYPE_NOT_SUPPORTED):
                    {
                        return "OBJECT_TYPE_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCAN_AREA_NOT_SUPPORTED):
                    {
                        return "SCAN_AREA_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_ACQUISITION_THREAD):
                    {
                        return "ACQUISITION_THREAD";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_ACQUISITION_ALREADY_STARTED):
                    {
                        return "ACQUISITION_ALREADY_STARTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_FEATURE_NOT_SUPPORTED):
                    {
                        return "FEATURE_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_CURRENT_DEV_NOT_SET):
                    {
                        return "CURRENT_DEV_NOT_SET";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_MEMORY_ALLOCATION):
                    {
                        return "MEMORY_ALLOCATION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_GENERIC):
                    {
                        return "GENERIC";
                    }
            }
            return "";
        }
    }
}
