namespace Livescan.Scanners.DriverEssential.Sdk
{
    static class LSE_ErrorCode
        {

            public const int LSCAN_STATUS_OK = 0;
            public const int LSCAN_ERR_INVALID_PARAM_VALUE = -1;
            public const int LSCAN_ERR_MEM_ALLOC = -2;
            public const int LSCAN_ERR_NOT_SUPPORTED = -3;
            public const int LSCAN_ERR_FILE_OPEN = -4;
            public const int LSCAN_ERR_FILE_READ = -5;
            public const int LSCAN_ERR_RESOURCE_LOCKED = -6;
            public const int LSCAN_ERR_MISSING_RESOURCE = -7;
            public const int LSCAN_ERR_VISUALIZATION = -400;
            public const int LSCAN_ERR_DEVICE_IO = -600;
            public const int LSCAN_ERR_COMMAND_FAILED = -601;
            public const int LSCAN_ERR_COMMAND_TIMEOUT = -602;
            public const int LSCAN_ERR_NO_DEVICE = -603;
            public const int LSCAN_ERR_NO_MATCHING_DEVICE = -604;
            public const int LSCAN_ERR_DEVICE_ACTIVE = -605;
            public const int LSCAN_ERR_NOT_INITIALIZED = -606;
            public const int LSCAN_ERR_IS_INITIALIZED = -607;
            public const int LSCAN_ERR_DEVICE_INVALID_STATE = -608;
            public const int LSCAN_ERR_DEVICE_BUSY = -609;
            public const int LSCAN_ERR_NO_HARDWARE_SUPPORT = -610;
            public const int LSCAN_ERR_DEVICE_READ_PARAM = -611;
            public const int LSCAN_ERR_DEVICE_WRITE_PARAM = -612;
            public const int LSCAN_ERR_DEVICE_INVALID_PARAM = -613;
            public const int LSCAN_ERR_DEVICE_INSUFFICIENT_MEMORY = -614;
            public const int LSCAN_ERR_DEVICE_WRONG_OPERATION_MODE = -615;
            public const int LSCAN_ERR_MULTIPLE_DEVICES_NOT_SUPPORTED = -616;
            public const int LSCAN_ERR_INVALID_LICENSING_FILE = -617;
            public const int LSCAN_ERR_USB20_REQUIRED = -618;
            public const int LSCAN_ERR_NO_CHANNEL = -620;
            public const int LSCAN_ERR_NO_MATCHING_CHANNEL = -621;
            public const int LSCAN_ERR_CHANNEL_NOT_INITIALIZED = -622;
            public const int LSCAN_ERR_CHANNEL_NOT_ACTIVE = -623;
            public const int LSCAN_ERR_CHANNEL_BUSY = -624;
            public const int LSCAN_ERR_CAPTURE_IN_PROGRESS = -625;
            public const int LSCAN_ERR_NOT_CAPTURING = -626;
            public const int LSCAN_ERR_CAPTURE = -627;
            public const int LSCAN_ERR_CAPTURE_TIMEOUT = -628;
            public const int LSCAN_ERR_STOP_CAPTURE = -629;
            public const int LSCAN_ERR_CAPTURE_BUFFER_FULL = -630;
            public const int LSCAN_ERR_CHANNEL_ALREADY_INITIALIZED = -631;
            public const int LSCAN_ERR_CHANNEL_ALREADY_ACTIVE = -632;
            public const int LSCAN_ERR_CHANNEL_INVALID_CAPTURE_MODE = -633;
            public const int LSCAN_ERR_CHANNEL_NO_SHUTTER = -634;
            public const int LSCAN_ERR_CHANNEL_NO_LUT = -635;
            public const int LSCAN_ERR_CHANNEL_NO_CAMERA_SETTINGS = -636;
            public const int LSCAN_ERR_CHANNEL_NO_ILLUMINATION = -637;
            public const int LSCAN_ERR_CHANNEL_NO_HEATER = -638;
            public const int LSCAN_ERR_CHANNEL_INVALID_IMAGE_DATA = -639;
            public const int LSCAN_ERR_CHANNEL_NO_SCAN_TABLE = -640;
            public const int LSCAN_ERR_CHANNEL_INVALID_SCAN_TABLE = -641;
            public const int LSCAN_ERR_CHANNEL_SCANNER_NOT_ADJUSTED = -642;
            public const int LSCAN_ERR_CHANNEL_NOT_SCANNING = -643;
            public const int LSCAN_ERR_CHANNEL_NO_CALIBRATION_RECTS = -644;
            public const int LSCAN_ERR_IMG_PROCESSING = -660;
            public const int LSCAN_ERR_NO_HAND_FINGER = -661;
            public const int LSCAN_ERR_ROLL_PROC = -662;
            public const int LSCAN_ERR_ROLL_TIMEOUT = -663;
            public const int ERR_INFIELDTEST_TEST_FAILED = -700;
            public const int ERR_INFIELDTEST_LOAD_PARAMETERS_FAILED = -701;
            public const int WRN_INFIELDTEST_SUITE_WRONG_VERSION = 700;
            public const int WRN_INFIELDTEST_SUITE_WRONG_DEVICETYPE = 701;
            public const int LSCAN_ERR_AUTOCAPTURE_NOT_INITIALIZED = -800;
            public const int LSCAN_ERR_AUTOCAPTURE_IS_INITIALIZED = -801;
            public const int LSCAN_ERR_AUTOCAPTURE_IMG_NOT_ALLOCATED = -802;
            public const int LSCAN_ERR_AUTOCAPTURE_SEQ_NOT_ALLOCATED = -803;
            public const int LSCAN_ERR_AUTOCAPTURE_UNKNOWN_IMAGE_ID = -804;
            public const int LSCAN_ERR_AUTOCAPTURE_IMAGE_ID_NOT_IN_RANGE = -805;
            public const int LSCAN_ERR_AUTOCAPTURE_UNKNOWN_FINGER_ID = -806;
            public const int LSCAN_ERR_AUTOCAPTURE_MAX_IMAGES_IN_QUEUE_IS_UNKNOWN = -807;
            public const int LSCAN_ERR_AUTOCAPTURE_HANDLE_NOT_AVAILABLE = -808;
            public const int LSCAN_ERR_AUTOCAPTURE_DLL_PATH_IS_MISSING = -809;

            /// <summary>
            /// Refer to the @ref TTRACE_DESCRIPTION "error logs" for more detailed LayoutRenderer error messages.
            /// </summary>
            public const int LSCAN_ERR_LAYOUTRENDERER_FAILED =-900;

        /// <summary>
        /// LayoutRenderer external params with empty key
        /// </summary>
        public const int LSCAN_ERR_LAYOUTRENDERER_PARAMS_EMPTY_KEY = -901;

        /// <summary>
        /// LayoutRenderer external params with duplicate key
        /// </summary>
        public const int LSCAN_ERR_LAYOUTRENDERER_PARAMS_DUPLICATE_KEY = -902;

        /// <summary>
        /// LayoutRenderer external params use internal key
        /// </summary>
        public const int LSCAN_ERR_LAYOUTRENDERER_PARAMS_INTERNAL_KEY_USED = -903;


        public const int LSCAN_WRN_OUTDATED_FIRMWARE = 600;
            public const int LSCAN_WRN_OPTICS_SURFACE_DIRTY = 601;
            public const int LSCAN_WRN_ALREADY_INITIALIZED = 603;
            public const int LSCAN_WRN_NO_OBJECT = 604;
            public const int LSCAN_WRN_BAD_SCAN = 605;
            public const int LSCAN_WRN_ROLL_BASE = 608;
            public const int LSCAN_WRN_ROLL_WRONG_ORIENT = 664;
            public const int LSCAN_WRN_ROLL_WRONG_BOUNDS = 665;
            public const int LSCAN_WRN_ROLL_WRONG_INTERSECT = 666;
            public const int LSCAN_WRN_ROLL_MERGE_PATH_OUTSIDE = 667;
            public const int LSCAN_WRN_SPOOF_DETECTED = 704;
            public const int LSCAN_WRN_AUTOCAPTURE_SEGMENTATION = 801;




        /// <summary>
        /// Manage the return value into a string with the constant name
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string returnErrorText(int error)
            {
                if (msgs == null)
                {
                    initMsgs();
                }
                string errorMsg = string.Empty;
                if (!msgs.TryGetValue(error, out errorMsg))
                {
                    errorMsg = "Unknown error : " + error.ToString();
                }

                return errorMsg;
            }

            private static System.Collections.Generic.Dictionary<int, string> msgs = null;

            private static void initMsgs()
            {
                msgs = new System.Collections.Generic.Dictionary<int, string>();
                msgs.Add(LSCAN_STATUS_OK, "LSCAN_STATUS_OK");                
                msgs.Add(LSCAN_ERR_INVALID_PARAM_VALUE, "LSCAN_ERR_INVALID_PARAM_VALUE");
                msgs.Add(LSCAN_ERR_MEM_ALLOC, "LSCAN_ERR_MEM_ALLOC");
                msgs.Add(LSCAN_ERR_NOT_SUPPORTED, "LSCAN_ERR_NOT_SUPPORTED");
                msgs.Add(LSCAN_ERR_FILE_OPEN, "LSCAN_ERR_FILE_OPEN");
                msgs.Add(LSCAN_ERR_FILE_READ, "LSCAN_ERR_FILE_READ");
                msgs.Add(LSCAN_ERR_RESOURCE_LOCKED, "LSCAN_ERR_RESOURCE_LOCKED");
                msgs.Add(LSCAN_ERR_MISSING_RESOURCE, "LSCAN_ERR_MISSING_RESOURCE");
                msgs.Add(LSCAN_ERR_VISUALIZATION, "LSCAN_ERR_VISUALIZATION");
                msgs.Add(LSCAN_ERR_DEVICE_IO, "LSCAN_ERR_DEVICE_IO");
                msgs.Add(LSCAN_ERR_COMMAND_FAILED, "LSCAN_ERR_COMMAND_FAILED");
                msgs.Add(LSCAN_ERR_COMMAND_TIMEOUT, "LSCAN_ERR_COMMAND_TIMEOUT");
                msgs.Add(LSCAN_ERR_NO_DEVICE, "LSCAN_ERR_NO_DEVICE");
                msgs.Add(LSCAN_ERR_NO_MATCHING_DEVICE, "LSCAN_ERR_NO_MATCHING_DEVICE");
                msgs.Add(LSCAN_ERR_DEVICE_ACTIVE, "LSCAN_ERR_DEVICE_ACTIVE");
                msgs.Add(LSCAN_ERR_NOT_INITIALIZED, "LSCAN_ERR_NOT_INITIALIZED");
                msgs.Add(LSCAN_ERR_IS_INITIALIZED, "LSCAN_ERR_IS_INITIALIZED");
                msgs.Add(LSCAN_ERR_DEVICE_INVALID_STATE, "LSCAN_ERR_DEVICE_INVALID_STATE");
                msgs.Add(LSCAN_ERR_DEVICE_BUSY, "LSCAN_ERR_DEVICE_BUSY");
                msgs.Add(LSCAN_ERR_NO_HARDWARE_SUPPORT, "LSCAN_ERR_NO_HARDWARE_SUPPORT");
                msgs.Add(LSCAN_ERR_DEVICE_READ_PARAM, "LSCAN_ERR_DEVICE_READ_PARAM");
                msgs.Add(LSCAN_ERR_DEVICE_WRITE_PARAM, "LSCAN_ERR_DEVICE_WRITE_PARAM");
                msgs.Add(LSCAN_ERR_DEVICE_INVALID_PARAM, "LSCAN_ERR_DEVICE_INVALID_PARAM");
                msgs.Add(LSCAN_ERR_DEVICE_INSUFFICIENT_MEMORY, "LSCAN_ERR_DEVICE_INSUFFICIENT_MEMORY");
                msgs.Add(LSCAN_ERR_DEVICE_WRONG_OPERATION_MODE, "LSCAN_ERR_DEVICE_WRONG_OPERATION_MODE");
                msgs.Add(LSCAN_ERR_MULTIPLE_DEVICES_NOT_SUPPORTED, "LSCAN_ERR_MULTIPLE_DEVICES_NOT_SUPPORTED");
                msgs.Add(LSCAN_ERR_INVALID_LICENSING_FILE, "LSCAN_ERR_INVALID_LICENSING_FILE");
                msgs.Add(LSCAN_ERR_USB20_REQUIRED, "LSCAN_ERR_USB20_REQUIRED");
                msgs.Add(LSCAN_ERR_NO_CHANNEL, "LSCAN_ERR_NO_CHANNEL");
                msgs.Add(LSCAN_ERR_NO_MATCHING_CHANNEL, "LSCAN_ERR_NO_MATCHING_CHANNEL");
                msgs.Add(LSCAN_ERR_CHANNEL_NOT_INITIALIZED, "LSCAN_ERR_CHANNEL_NOT_INITIALIZED");
                msgs.Add(LSCAN_ERR_CHANNEL_NOT_ACTIVE, "LSCAN_ERR_CHANNEL_NOT_ACTIVE");
                msgs.Add(LSCAN_ERR_CHANNEL_BUSY, "LSCAN_ERR_CHANNEL_BUSY");
                msgs.Add(LSCAN_ERR_CAPTURE_IN_PROGRESS, "LSCAN_ERR_CAPTURE_IN_PROGRESS");
                msgs.Add(LSCAN_ERR_NOT_CAPTURING, "LSCAN_ERR_NOT_CAPTURING");
                msgs.Add(LSCAN_ERR_CAPTURE, "LSCAN_ERR_CAPTURE");
                msgs.Add(LSCAN_ERR_CAPTURE_TIMEOUT, "LSCAN_ERR_CAPTURE_TIMEOUT");
                msgs.Add(LSCAN_ERR_STOP_CAPTURE, "LSCAN_ERR_STOP_CAPTURE");
                msgs.Add(LSCAN_ERR_CAPTURE_BUFFER_FULL, "LSCAN_ERR_CAPTURE_BUFFER_FULL");
                msgs.Add(LSCAN_ERR_CHANNEL_ALREADY_INITIALIZED, "LSCAN_ERR_CHANNEL_ALREADY_INITIALIZED");
                msgs.Add(LSCAN_ERR_CHANNEL_ALREADY_ACTIVE, "LSCAN_ERR_CHANNEL_ALREADY_ACTIVE");
                msgs.Add(LSCAN_ERR_CHANNEL_INVALID_CAPTURE_MODE, "LSCAN_ERR_CHANNEL_INVALID_CAPTURE_MODE");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_SHUTTER, "LSCAN_ERR_CHANNEL_NO_SHUTTER");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_LUT, "LSCAN_ERR_CHANNEL_NO_LUT");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_CAMERA_SETTINGS, "LSCAN_ERR_CHANNEL_NO_CAMERA_SETTINGS");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_ILLUMINATION, "LSCAN_ERR_CHANNEL_NO_ILLUMINATION");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_HEATER, "LSCAN_ERR_CHANNEL_NO_HEATER");
                msgs.Add(LSCAN_ERR_CHANNEL_INVALID_IMAGE_DATA, "LSCAN_ERR_CHANNEL_INVALID_IMAGE_DATA");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_SCAN_TABLE, "LSCAN_ERR_CHANNEL_NO_SCAN_TABLE");
                msgs.Add(LSCAN_ERR_CHANNEL_INVALID_SCAN_TABLE, "LSCAN_ERR_CHANNEL_INVALID_SCAN_TABLE");
                msgs.Add(LSCAN_ERR_CHANNEL_SCANNER_NOT_ADJUSTED, "LSCAN_ERR_CHANNEL_SCANNER_NOT_ADJUSTED");
                msgs.Add(LSCAN_ERR_CHANNEL_NOT_SCANNING, "LSCAN_ERR_CHANNEL_NOT_SCANNING");
                msgs.Add(LSCAN_ERR_CHANNEL_NO_CALIBRATION_RECTS, "LSCAN_ERR_CHANNEL_NO_CALIBRATION_RECTS");
                msgs.Add(LSCAN_ERR_IMG_PROCESSING, "LSCAN_ERR_IMG_PROCESSING");
                msgs.Add(LSCAN_ERR_NO_HAND_FINGER, "LSCAN_ERR_NO_HAND_FINGER");
                msgs.Add(LSCAN_ERR_ROLL_PROC, "LSCAN_ERR_ROLL_PROC");
                msgs.Add(LSCAN_ERR_ROLL_TIMEOUT, "LSCAN_ERR_ROLL_TIMEOUT");
                msgs.Add(ERR_INFIELDTEST_TEST_FAILED, "ERR_INFIELDTEST_TEST_FAILED");
                msgs.Add(ERR_INFIELDTEST_LOAD_PARAMETERS_FAILED, "ERR_INFIELDTEST_LOAD_PARAMETERS_FAILED");
                msgs.Add(WRN_INFIELDTEST_SUITE_WRONG_VERSION, "WRN_INFIELDTEST_SUITE_WRONG_VERSION");
                msgs.Add(WRN_INFIELDTEST_SUITE_WRONG_DEVICETYPE, "WRN_INFIELDTEST_SUITE_WRONG_DEVICETYPE");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_NOT_INITIALIZED, "LSCAN_ERR_AUTOCAPTURE_NOT_INITIALIZED");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_IS_INITIALIZED, "LSCAN_ERR_AUTOCAPTURE_IS_INITIALIZED");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_IMG_NOT_ALLOCATED, "LSCAN_ERR_AUTOCAPTURE_IMG_NOT_ALLOCATED");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_SEQ_NOT_ALLOCATED, "LSCAN_ERR_AUTOCAPTURE_SEQ_NOT_ALLOCATED");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_UNKNOWN_IMAGE_ID, "LSCAN_ERR_AUTOCAPTURE_UNKNOWN_IMAGE_ID");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_IMAGE_ID_NOT_IN_RANGE, "LSCAN_ERR_AUTOCAPTURE_IMAGE_ID_NOT_IN_RANGE");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_UNKNOWN_FINGER_ID, "LSCAN_ERR_AUTOCAPTURE_UNKNOWN_FINGER_ID");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_MAX_IMAGES_IN_QUEUE_IS_UNKNOWN, "LSCAN_ERR_AUTOCAPTURE_MAX_IMAGES_IN_QUEUE_IS_UNKNOWN");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_HANDLE_NOT_AVAILABLE, "LSCAN_ERR_AUTOCAPTURE_HANDLE_NOT_AVAILABLE");
                msgs.Add(LSCAN_ERR_AUTOCAPTURE_DLL_PATH_IS_MISSING, "LSCAN_ERR_AUTOCAPTURE_DLL_PATH_IS_MISSING");

                msgs.Add(LSCAN_ERR_LAYOUTRENDERER_FAILED, "LSCAN_ERR_LAYOUTRENDERER_FAILED");
                msgs.Add(LSCAN_ERR_LAYOUTRENDERER_PARAMS_EMPTY_KEY, "LSCAN_ERR_LAYOUTRENDERER_PARAMS_EMPTY_KEY");
                msgs.Add(LSCAN_ERR_LAYOUTRENDERER_PARAMS_DUPLICATE_KEY, "LSCAN_ERR_LAYOUTRENDERER_PARAMS_DUPLICATE_KEY");
                msgs.Add(LSCAN_ERR_LAYOUTRENDERER_PARAMS_INTERNAL_KEY_USED, "LSCAN_ERR_LAYOUTRENDERER_PARAMS_INTERNAL_KEY_USED");

                msgs.Add(LSCAN_WRN_OUTDATED_FIRMWARE, "LSCAN_WRN_OUTDATED_FIRMWARE");
                msgs.Add(LSCAN_WRN_OPTICS_SURFACE_DIRTY, "LSCAN_WRN_OPTICS_SURFACE_DIRTY");
                msgs.Add(LSCAN_WRN_ALREADY_INITIALIZED, "LSCAN_WRN_ALREADY_INITIALIZED");
                msgs.Add(LSCAN_WRN_NO_OBJECT, "LSCAN_WRN_NO_OBJECT");
                msgs.Add(LSCAN_WRN_BAD_SCAN, "LSCAN_WRN_BAD_SCAN");
                msgs.Add(LSCAN_WRN_ROLL_BASE, "LSCAN_WRN_ROLL_BASE");
                msgs.Add(LSCAN_WRN_ROLL_WRONG_ORIENT, "LSCAN_WRN_ROLL_WRONG_ORIENT");
                msgs.Add(LSCAN_WRN_ROLL_WRONG_BOUNDS, "LSCAN_WRN_ROLL_WRONG_BOUNDS");
                msgs.Add(LSCAN_WRN_ROLL_WRONG_INTERSECT, "LSCAN_WRN_ROLL_WRONG_INTERSECT");
                msgs.Add(LSCAN_WRN_ROLL_MERGE_PATH_OUTSIDE, "LSCAN_WRN_ROLL_MERGE_PATH_OUTSIDE");
                msgs.Add(LSCAN_WRN_SPOOF_DETECTED, "LSCAN_WRN_SPOOF_DETECTED");
                msgs.Add(LSCAN_WRN_AUTOCAPTURE_SEGMENTATION, "LSCAN_WRN_AUTOCAPTURE_SEGMENTATION");

            }
        }

        static class LSE_Constants // L Scan Essentials Constants
        {
            public const int BOOL_FALSE = 0;
            public const int BOOL_TRUE = 1;
            public const int LSCAN_MAX_STR_LEN = 128;
            public const int LSCAN_MAX_CONTRAST_VALUE = 48;

        }

        static class LSE_Key_Constants
        {
            public const int LSCAN_KEY_NONE = 0x00;
            public const int LSCAN_KEY_FOOTSWITCH = 0x80;
            public const int LSCAN_KEY_2_OK = 0x01;
            public const int LSCAN_KEY_2_CANCEL = 0x02;
            public const int LSCAN_KEY_2_SAVE = 0x01;
            public const int LSCAN_KEY_2_SCAN = 0x02;
            public const int LSCAN_KEY_5_UP = 0x01;
            public const int LSCAN_KEY_5_RIGHT = 0x02;
            public const int LSCAN_KEY_5_OK = 0x04;
            public const int LSCAN_KEY_5_DOWN = 0x08;
            public const int LSCAN_KEY_5_LEFT = 0x10;
        }

        static class LSE_LED_Constants
        {
            public const uint LSCAN_LED_NONE = 0x0;
            public const uint LSCAN_LED_ALL = 0xffffffff;
            public const uint LSCAN_LED_OK_GREEN_B1 = 0x00000010;
            public const uint LSCAN_LED_OK_GREEN_B2 = 0x00000020;
            public const uint LSCAN_LED_OK_GREEN = 0x00000030;
            public const uint LSCAN_LED_OK_YELLOW_B1 = 0x00000040;
            public const uint LSCAN_LED_OK_YELLOW_B2 = 0x00000080;
            public const uint LSCAN_LED_OK_YELLOW = 0x000000C0;
            public const uint LSCAN_LED_CANCEL_B1 = 0x00000100;
            public const uint LSCAN_LED_CANCEL_B2 = 0x00000200;
            public const uint LSCAN_LED_CANCEL = 0x00000300;
            public const uint LSCAN_LED_I1_RED_B1 = 0x00001000;
            public const uint LSCAN_LED_I1_RED_B2 = 0x00002000;
            public const uint LSCAN_LED_I2_RED_B1 = 0x00004000;
            public const uint LSCAN_LED_I2_RED_B2 = 0x00008000;
            public const uint LSCAN_LED_I3_RED_B1 = 0x00000400;
            public const uint LSCAN_LED_I3_RED_B2 = 0x00000800;
            public const uint LSCAN_LED_I4_RED_B1 = 0x00000100;
            public const uint LSCAN_LED_I4_RED_B2 = 0x00000200;
            public const uint LSCAN_LED_I1_GREEN_B1 = 0x00000010;
            public const uint LSCAN_LED_I1_GREEN_B2 = 0x00000020;
            public const uint LSCAN_LED_I2_GREEN_B1 = 0x00000040;
            public const uint LSCAN_LED_I2_GREEN_B2 = 0x00000080;
            public const uint LSCAN_LED_I3_GREEN_B1 = 0x00000004;
            public const uint LSCAN_LED_I3_GREEN_B2 = 0x00000008;
            public const uint LSCAN_LED_I4_GREEN_B1 = 0x00000001;
            public const uint LSCAN_LED_I4_GREEN_B2 = 0x00000002;
            public const uint LSCAN_LED_S1_RED_B1 = 0x40000000;
            public const uint LSCAN_LED_S1_RED_B2 = 0x80000000;
            public const uint LSCAN_LED_S2_RED_B1 = 0x10000000;
            public const uint LSCAN_LED_S2_RED_B2 = 0x20000000;
            public const uint LSCAN_LED_S3_RED_B1 = 0x04000000;
            public const uint LSCAN_LED_S3_RED_B2 = 0x08000000;
            public const uint LSCAN_LED_S4_RED_B1 = 0x01000000;
            public const uint LSCAN_LED_S4_RED_B2 = 0x02000000;
            public const uint LSCAN_LED_S1_GREEN_B1 = 0x00400000;
            public const uint LSCAN_LED_S1_GREEN_B2 = 0x00800000;
            public const uint LSCAN_LED_S2_GREEN_B1 = 0x00100000;
            public const uint LSCAN_LED_S2_GREEN_B2 = 0x00200000;
            public const uint LSCAN_LED_S3_GREEN_B1 = 0x00040000;
            public const uint LSCAN_LED_S3_GREEN_B2 = 0x00080000;
            public const uint LSCAN_LED_S4_GREEN_B1 = 0x00010000;
            public const uint LSCAN_LED_S4_GREEN_B2 = 0x00020000;
            public const uint LSCAN_LED_BITMASK_ICONS = 0x0000ffff;
            public const uint LSCAN_LED_BITMASK_STATUS = 0xffff0000;

        }

        static class LSE_ROLL_Error
        {
            /// <summary>
            /// Finger was shifted horizontally
            /// </summary>
            public const int LSCAN_WRN_ROLL_SHIFTED_HORIZONTALLY = 1;
            /// <summary>
            /// Finger was shifted vertically
            /// </summary>
            public const int LSCAN_WRN_ROLL_SHIFTED_VERTICALLY = 2;
            /// <summary>
            /// Finger tip was lifted
            /// </summary>
            public const int LSCAN_WRN_ROLL_LIFTED_TIP = 4;
            /// <summary>
            /// Finger was rolled to border
            /// </summary>
            public const int LSCAN_WRN_ROLL_ON_BORDER = 8;
            /// <summary>
            /// Finger was kept still during roll
            /// </summary>
            public const int LSCAN_WRN_ROLL_PAUSED = 16;

        }

        static class LSE_CaptureOptions
        {
            public const int LSCAN_OPTION_AUTO_CAPTURE = 1;
            public const int LSCAN_OPTION_AUTO_CONTRAST = 2;
            public const int LSCAN_OPTION_AUTO_OVERRIDE = 4;
            public const int LSCAN_OPTION_SPOOF_DETECTION = 8;
        }

        static class LSE_VisualisationOptions
        {
            public const int LSCAN_OPTION_VIS_FULL_IMAGE = 1;
        }





}
