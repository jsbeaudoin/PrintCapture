namespace Livescan.Scanners.DriverEssential.Sdk
{
    using System;
    using System.Runtime.InteropServices;

    /* ****************************************************************
    Changes From version 6 to 7
       Added LSCAN_InitAPI, LSCAN_ExitAPI
    ******************************************************************/

        static class LSE_SDK
        {
            #region Main Interface Functions
            /*
         * ****************************************************************************************************************
         * Main Interface Functions
         * ****************************************************************************************************************
        */

            /// <summary>
            ///	Initializes the LScanEssentials API.
            ///	</summary>
            ///	<remarks>
            ///	New From version 7
            ///	Needs to be called before using any other API function!
            ///	Warnings :  Do not call within the (de-)initialization of static objects (involves thread initialization)!
            ///             Do not call outside the main thread!
            ///             Qt users: Do not call before creating your @c QApplication instance!
            /// </remarks>
            /// <returns>
            /// status code as defined in Constants.LSE_ErrorCode
            /// </returns>            
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_InitAPI();

            ///<summary>
            /// New From version 7
            /// De-initializes the LScanEssentials API.
            ///</summary>
            ///	<remarks>
            ///	Do not call within the (de-)initialization of static objects (involves thread initialization)!
            /// </remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern void LSCAN_ExitAPI();

            /// <summary>
            /// Get DLL version information.        
            /// </summary>
            /// <param name="version"></param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_GetAPIVersion(ref LScanApiVersion version);

            /// <summary>
            /// Retrieve count of connected L SCAN live scanner devices.
            /// </summary>
            /// <param name="deviceCount">Number of connected devices. Memory must be provided by caller</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_GetDeviceCount(ref int deviceCount);


            /// <summary>
            /// Retrieve detailed device information about particular scanner given by logical index.
            /// </summary>
            /// <param name="deviceIndex">Zero-based device index for device to lookup</param>
            /// <param name="deviceInfo">Basic device information</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_GetDeviceInfo(int deviceIndex, out LScanDeviceInfo deviceInfo);


            /// <summary>
            /// Callback to signal operation progress.
            /// This registers a global callback that notifies progress for any connected device. 
            /// Currently the callback is fired for initialization and infield test progress changes.
            /// </summary>
            /// <param name="callback">Pointer to the notification function </param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_RegisterCallbackProgress(LSCAN_CallbackProgress callback, IntPtr context);

            /// <summary>
            /// Callback to signal device count changes.
            /// This registers a global callback that notifies changes in number of connected device.
            /// </summary>
            /// <param name="callback">Pointer to the notification function </param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_RegisterCallbackDeviceCount(LSCAN_CallbackDeviceCount callback, IntPtr context);


            /// <summary>
            /// Check if device is adjusted to meet image quality requirements.
            /// </summary>
            /// <param name="deviceIndex">Zero-based device index for device to lookup</param>
            /// <param name="logFilePath">Complete path to log file generated in case of test not passed</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_ImageQualityInfieldTest(int deviceIndex, string logFilePath);

            /// <summary>
            /// Install licenses from license file to device.
            /// LSCAN_Main_GetDeviceCount() together with LSCAN_Main_GetDeviceInfo() can be used 
            /// to determine the device to load with the given license file.
            /// </summary>
            /// <param name="deviceIndex">Zero based device index (range: 0 &lt;= Index &lt; LS_Device_GetDeviceCount() )</param>
            /// <param name="licenseFileName">File name of license file to load</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            /// <remarks>The device is not allowed to be initialized before.</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_InstallLicenseFile(int deviceIndex, string licenseFileName);


            /// <summary>
            /// Initialize device given by device index.
            /// An automatic cleanliness check is performed if the device is used the first time after power up
            /// or if parameter @e reset is set to TRUE. 
            /// </summary>
            /// <param name="deviceIndex">Zero based device index (range: 0 &lt;= Index &lt; LS_Device_GetDeviceCount() )</param>
            /// <param name="reset">Bool : Perform a device reset prior to initialization.</param>
            /// <param name="handle">Function returns device handle to be used for subsequent function calls</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_STATUS_OK -> device is ready to be used
            /// LSCAN_WRN_ALREADY_INITIALIZED -> indicates that the device was already initialized and can be used
            ///   (note: device state now as if newly initialized)
            /// LSCAN_WRN_OPTICS_SURFACE_DIRTY -> capture platen needs to be cleaned; in this case device should be released, 
            ///   platen cleaned and then initialized again with parameter @e reset= true</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_Initialize(int deviceIndex, int reset, out int handle);


            /// <summary>
            /// <see cref="LSCAN_Main_Initialize"/>
            /// Unlike LSCAN_Main_Initialize(), all subsequent visualization requests will be
            /// redirected to an external process through a named pipe.
            /// </summary>
            ///<param name="deviceIndex">Zero based device index (range: 0 &lt;= Index &lt; LS_Device_GetDeviceCount() )</param>
            /// <param name="reset">Bool : Perform a device reset prior to initialization.</param>
            /// <param name="handle">Function returns device handle to be used for subsequent function calls</param>
            /// <param name="pipeName">name of the visualization pipe received by Visualization_Create()</param>
            /// <returns> Always @ref LSCAN_ERR_NOT_SUPPORTED for the current implementation.</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_Initialize_ExternalVisualization(int deviceIndex, int reset, out int handle, string pipeName);


            /// <summary>
            /// Release device.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="sendToStandby">Set device in standby mode (if set to TRUE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_RESOURCE_LOCKED -> a callback is still active
            /// LSCAN_ERR_NOT_INITIALIZED -> device(s) in use are identified by index;
            ///   so either device has aready been released or is unknown</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_Release(int handle, int sendToStandby);

            /// <summary>
            /// Release all currently initialized devices.
            /// </summary>
            /// <param name="sendToStandby">Set device in standby mode (if set to TRUE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_RESOURCE_LOCKED -> a callback is still active</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_ReleaseAll(int sendToStandby);


            /// <summary>
            /// Check if device is initialized.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_STATUS_OK -> device is ready to be used
            /// LSCAN_ERR_INVALID_PARAM_VALUE -> if handle value is out of valid range
            /// LSCAN_ERR_NOT_INITIALIZED -> device is not initialized
            /// LSCAN_ERR_DEVICE_IO -> device is initialized but there was a communication problem 
            ///   (e.g. disconnection; LSCAN_Main_Release() must be called in order to free allocated resources)</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_IsInitialized(int handle);

            /// <summary>
            /// Retrieve device property value
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="propertyId">Property identifier to get value for</param>
            /// <param name="stringpropertyValue">String returning property value</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_GetProperty(int handle, enumLScanPropertyId propertyId, out LScanResultString stringpropertyValue);

            /// <summary>
            /// Set device property value.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="propertyId">Property identifier to get value for</param>
            /// <param name="propertyValue">String containing property value</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_SetProperty(int handle, enumLScanPropertyId propertyId, string propertyValue);


            /// <summary>
            /// Check if scanner surface(s) is/are free of dirt.
            /// If a capture is pending then the check is performed only on the active channel.
            /// Otherwise the check is performed on all channels.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_STATUS_OK -> if platen surface(s) is/are clean
            /// LSCAN_WRN_OPTICS_SURFACE_DIRTY -> if platen surface(s) need(s) to be cleaned</returns>
            /// <remarks>This check only recognizes dirt added after the last full device initialization.</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_CheckCleanliness(int handle);


            /// <summary>
            /// Perform an adjustment of the device. 
            /// Calls to this function do not affect the automatic adjustment set by property @ref LSCAN_PROPERTY_AUTOMATIC_ADJUSTMENT.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CAPTURE_IN_PROGRESS if a capture is active
            /// LSCAN_WRN_OPTICS_SURFACE_DIRTY if adjustment could not be performed because the platen was dirty</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_ForceReadjustment(int handle);


            /// <summary>
            /// Callback to signal device communication interruption.
            /// This function registers a device related callback that notifies device communication breakage.
            /// Once the callback is fired all subsequent API functions (except LSCAN_Main_Release())
            /// return error code @ref LSCAN_ERR_DEVICE_IO.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function </param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Main_RegisterCallbackCommunicationBreak(int handle, LSCAN_Callback callback, IntPtr context);


            #endregion

            #region Image Acquisition Related Interface Functions

            /*
         * ****************************************************************************************************************
         * Image Acquisition Related Interface Functions
         * ****************************************************************************************************************
        */

            /// <summary>
            /// Check if requested capture mode is supported by the device.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="imageType">Image type to verify</param>
            /// <param name="imageResolution">Requested capture resolution</param>
            /// <param name="isAvailable">Returns TRUE if mode is available</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_IsModeAvailable(int handle, enumLScanImageType imageType,
                    enumLScanImageResolution imageResolution, ref int isAvailable);


            /// <summary>
            /// Set capture mode.
            /// Capture mode setting is a prerequisite for LSCAN_CAPTURE_start().
            /// </summary>
            // <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="imageType">Image type to verify</param>
            /// <param name="imageResolution">Requested capture resolution</param>
            /// <param name="lineOrder">Required result image line order</param>
            /// <param name="captureOptions">Bit coded capture options to use (see @ref CaptureOptions)</param>
            /// <param name="resultWidth">Returns width of captured result image in pixels</param>
            /// <param name="resultHeight">Returns height of captured result image in pixels</param>
            /// <param name="baseResolutionX">Returns image horizontal base resolution before processing</param>
            /// <param name="baseResolutionY">Returns image vertical base resolution before processing</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_SetMode(int handle, enumLScanImageType imageType,
                    enumLScanImageResolution imageResolution, enumLScanImageOrientation lineOrder, uint captureOptions,
                    out int resultWidth, out int resultHeight, out int baseResolutionX, out int baseResolutionY);

            /// <summary>
            /// Start image acquisition.
            /// The acquisition is done asynchronously.
            /// LSCAN_Capture_Abort() can be called to abort acquisition.
            /// Result image acquisition is done by auto capture functionality (if enabled)
            /// or by call to LSCAN_Capture_TakeResultImage().  
            /// Result image acquisition completion is signaled by callback LSCAN_CallbackResultImageAvailable(). 
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="numberOfObjects">Number of expected objects (fingertips + palm areas) in the image.
            /// It is used if option @ref LSCAN_OPTION_AUTO_CAPTURE is set 
            ///      at @ref LSCAN_Capture_SetMode(). Please refer to the image types 
            ///      @ref enumLScanImageType for details about valid values. </param>
            /// <returns> status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CAPTURE_IN_PROGRESS -> an aquisition is currently pending and needs to be completed first
            /// LSCAN_ERR_INVALID_PARAM_VALUE -> parameter @e numberOfObjects needs to be in range 1..4
            /// LSCAN_ERR_CHANNEL_INVALID_CAPTURE_MODE -> acquisition mode needs to be set as a prerequisite 
            ///   -> call LSCAN_Capture_SetMode() first</returns>
            /// à
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_Start(int handle, int numberOfObjects);


            /// <summary>
            /// Abort running image acquisition.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            ///  LSCAN_ERR_NOT_CAPTURING -> no active acquisition to be aborted </returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_Abort(int handle);




            /// <summary>
            /// Check if image acquisition is in progress.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="isActive">Returns TRUE if acquisition is in progress
            ///       (preview or result image acquisition)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_IsActive(int handle, out int isActive);


            /// <summary>
            /// Initiate result image acquisition.
            /// For flat finger or palm image acquisition this function
            /// triggers result image acquisition (regardless to auto capture option) \n
            /// It overwrites @ref LSCAN_CLEAR_OBJECT_FROM_PLATEN state - a subsequent LSCAN_CallbackClearObjectsFromPlaten()
            /// with state @ref LSCAN_PLATED_CLEARED is not sent. \n\n
            /// For rolled finger print acquisition it: \n
            /// 1. triggers roll start (only for captures without auto capture option) \n
            /// 2. accepts result image (if device option @ref LSCAN_PROPERTY_ROLL_ALLOW_RESTART is set to TRUE) \n\n
            /// Result image acquisition completion is signaled by AcquisitionComplete() callback, 
            /// availability of the result image by ResultImage() callback.
            /// </summary>
            /// <param name="handle"> Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NOT_CAPTURING -> no active acquisition to trigger result image for </returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_TakeResultImage(int handle);


            /// <summary>
            /// Perform a contrast optimization.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CHANNEL_NOT_ACTIVE -> acquisition channel needs to be selected as a prerequisite 
            ///   -> call LSCAN_Capture_SetMode() first
            /// LSCAN_ERR_NO_HAND_FINGER -> finger/hand requires to be placed on platen during contrast optimization</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_OptimizeContrast(int handle);


            /// <summary>
            /// Get contrast value for selected capture mode selected by LSCAN_Capture_SetMode().
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="contrastValue">Contrast value (range: 0 &lt;= value &lt;= @ref LSCAN_MAX_CONTRAST_VALUE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CHANNEL_NOT_ACTIVE -> acquisition channel needs to be selected as a prerequisite 
            ///   -> call LSCAN_Capture_SetMode() first</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_GetContrast(int handle, out int contrastValue);


            /// <summary>
            /// Set contrast value for selected capture mode selected by LSCAN_Capture_SetMode().
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="contrastValue">Contrast value (range: 0 &lt;= value &lt;= @ref LSCAN_MAX_CONTRAST_VALUE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CHANNEL_NOT_ACTIVE -> acquisition channel needs to be selected as a prerequisite 
            ///   -> call LSCAN_Capture_SetMode() first</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_SetContrast(int handle, int contrastValue);


            /// <summary>
            /// Set result image clipping area.
            /// This function defines the area of result image that is returned with LSCAN_CallbackResultImage(). \n
            /// Point of origin is the upper left corner of the complete image. \n\n
            /// Depending on given parameters several modes for image clip are supported: \n
            /// x>=0; y>=0; width>0; height>0 : fixed clip size and position \n
            /// any other invalid parameters  : disable clipping, return full result image. \n\n
            /// L SCAN 1000PX and L SCAN 500P support additional mode for capture of flats: \n
            /// x=-1; y=-1; width>0; height>0 : fixed clip size, automatic clip position detection; for flats modes only \n
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="x">Clipping horizontal start position</param>
            /// <param name="y">Clipping vertical start position</param>
            /// <param name="width">Clipping width</param>
            /// <param name="height"> Clipping height</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_CAPTURE_IN_PROGRESS -> active area cannot be set while acquisition is pending
            /// LSCAN_ERR_CHANNEL_INVALID_CAPTURE_MODE -> acquisition mode needs to be set as a prerequisite 
            ///   -> call LSCAN_Capture_SetMode() first
            /// LSCAN_ERR_INVALID_PARAM_VALUE -> active area needs to be within result image dimensions</returns>
            /// <remarks>The clipping does not affect the size of the displayed image.\n
            /// Active area is automatically reset when calling any of the following functions: 
            /// LSCAN_Main_Release(), LSCAN_Capture_SetMode() \n
            /// or if called with invalid parameter value(s). \n
            /// Width and height have to be dividable by 4 when using this function on L SCAN 1000PX device! \n</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_SetActiveArea(int handle, int x, int y, int width, int height);


            /// <summary>
            ///  Register callback function for preview image availability.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NOT_SUPPORTED if auto capture license is not available</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackPreviewImage(int handle, LSCAN_CallbackPreviewImage callback, IntPtr context);


            /// <summary>
            /// Register callback function for object (finger)finger count change during capture operation.
            /// Assumes empty scanner surface at the beginning.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NOT_SUPPORTED if auto capture license is not available</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackObjectCount(int handle, LSCAN_CallbackObjectCount callback, IntPtr context);



            /// <summary>
            /// Register callback function for fingertip qualities change during capture operation.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackObjectQuality(int handle, LSCAN_CallbackObjectQuality callback, IntPtr context);


            /// <summary>
            /// Register result image start callback function.
            /// The registered function is then called by the SDK to signal begin of result image acquisition.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackTakingResultImage(int handle, LSCAN_Callback callback, IntPtr context);


            /// <summary> Register result image acquisition completion callback function.
            /// The registered function is then called by the SDK to signal the finish of result image acquisition.
            /// Depending on device and image type the notification might be called 
            /// immediately after LSCAN_CallbackTakingResultImage(). 
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            /// <remarks>The result image availability is notified after post process completion 
            /// via callback registered by LSCAN_Capture_RegisterCallbackResultImage().</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackAcquisitionComplete(int handle, LSCAN_Callback callback, IntPtr context);


            /// <summary>
            /// Register callback function for result image availability.
            /// Warning : The image buffer returned is set free as soon as callback is done. 
            /// Copy buffer before starting a new thread
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackResultImage(int handle, LSCAN_CallbackResultImage callback, IntPtr context);


            /// <summary>
            /// Register callback function for notification to remove finger(s)/hand from scanner surface prior 
            /// to beginning of new capture. \n
            /// The registration for this callback is not depending on availability of the auto capture license.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Capture_RegisterCallbackClearObjectsFromPlaten(int handle, LSCAN_CallbackClearObjectsFromPlaten callback, IntPtr context);

            #endregion

            #region User Controls Related Interface Functions
            /*
         * ****************************************************************************************************************
         * User Controls Related Interface Functions
         * ****************************************************************************************************************
        */


            /// <summary>
            /// Get available beeper type.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="beeperType">Type of beeper</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_GetAvailableBeeper(int handle, out enumLScanBeeperType beeperType);


            /// <summary>
            /// Let device beep following a predefined pattern.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="pattern">Beeper pattern (0..7)</param>
            /// <param name="volume">Beeper volume in percent (value >= 0), 0 means off;
            ///      supported for L SCAN 500P only, otherwise ignored</param>
            /// <returns> status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NO_HARDWARE_SUPPORT -> device is not equipped with a beeper</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_Beeper(int handle, int pattern, int volume);


            /// <summary>
            /// Get available user input keys (including foot switch).
            /// </summary>
            /// <param name="handle"> Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="keypadType"> Type of keypad </param>
            /// <param name="keyCount">Number of keys on keypad (without foot switch)</param>
            /// <param name="availableKeys">Bit pattern of available keys including foot switch</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            /// <remarks>The optional USB connected foot switch is also supported by this API.</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_GetAvailableKeys(int handle, out enumLScanKeypadType keypadType,
                out int keyCount, out uint availableKeys);



            /// <summary>
            /// Set active user input keys (including foot switch).
            /// State changes for the keys that are enabled by this function are signaled
            /// via callback registered with LSCAN_Controls_RegisterCallbackKeys().
            /// Please call LSCAN_Controls_GetAvailableKeys() to obtain type of 
            /// built in keypad and supported keys.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="activeKeys">Bit pattern of active keys including foot switch</param>
            /// <returns> status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NO_HARDWARE_SUPPORT -> attempted to activate keys that
            /// the device is not equipped with</returns>
            /// <remarks>Keys must be released while or after call to the function before they get
            /// signaled by the callback. This is necessary to avoid keybounce.
            /// The optional USB connected foot switch is also supported by this API.</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_SetActiveKeys(int handle, uint activeKeys);


            /// <summary>
            /// Get available status LED's.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="ledType">Type of LED's</param>
            /// <param name="ledCount">Number of LED's</param>
            /// <param name="availableLEDs">Bit pattern of available LED's</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_GetAvailableLEDs(int handle, out enumLScanLedType ledType,
                out int ledCount, out uint availableLEDs);

            /// <summary>
            /// Set active status LED's on device.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="activeLEDs">Bit pattern of active LED's</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode
            /// LSCAN_ERR_NO_HARDWARE_SUPPORT -> device is not equipped with status LEDs</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_SetActiveLEDs(int handle, uint activeLEDs);


            /// <summary>
            /// Get active status LED's for device.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="activeLEDs">Bit pattern of active LED's</param>
            /// <returns> status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_GetActiveLEDs(int handle, out uint activeLEDs);


            /// <summary>
            /// Register callback for keypad/foot switch events.
            /// This function registers/unregisters a callback function for receiving key state change notifications.        
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="callback">Pointer to the notification function</param>
            /// <param name="context">Pointer to user context; this value is used as parameter for callback</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            /// <remarks>LSCAN_Controls_SetActiveKeys() must also be called to specify active keys. 
            /// Per default no keys are active. The calling sequence is not important.
            /// </remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Controls_RegisterCallbackKeys(int handle, LSCAN_CallbackKeys callback, IntPtr context);

            #endregion

            #region DEVICE DISPLAY

          /*     
         * *********************************************************************************************************
         * DEVICE DISPLAY FUNCTIONS : For Devices with Display (1000 px and some guardians)
         * *********************************************************************************************************
            */

                /// <summary>
                /// Show company logo screen on device display.
                /// </summary>
                /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
                /// <param name="logoOption">option for screen display (see @ref DisplayLogoOptions)</param>
                /// <param name="progressBarPercent">progress bar value in 0%...100%</param>
                /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
                [DllImport("LScanEssentials.dll")]
                internal  static extern int LSCAN_Controls_DisplayShowLogoScreen(int handle, enumLScanDisplayLogoOption logoOption, int progressBarPercent);


        
        
        
        /// <summary>
        /// Show operation mode selection screen on device display.
        /// </summary>
        /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        [DllImport("LScanEssentials.dll")]
        internal  static extern int  LSCAN_Controls_DisplayShowModeSelectScreen(int handle);        
        
        /// <summary>
        /// Show resolution selection screen on device display.
        /// </summary>
        /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        /// <remarks>Function can be used for devices which support more than one native capture resolution (e.g. 500 and 1000ppi)</remarks>
        [DllImport("LScanEssentials.dll")]
        internal  static extern int LSCAN_Controls_DisplayShowResolutionSelectScreen(int handle);

        /// <summary>
        /// Show finger selection screen on device display.
        /// </summary>
        /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
        /// <param name="ctrlLeft">icon for left button: finger selection type (e.g. bandaged), (see @ref enumLScanDisplaySelectionCtrl)</param>
        /// <param name="ctrlRight">icon for right button (see @ref enumLScanDisplayCommonCtrl)</param>
        /// <param name="colorLeftPalm">color state for left palm (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftThenar">color state for left thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftLowerThenar">color state for left lower thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftInterDigital">color state for left interdigital (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftThumb">color state for left thumb (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftIndex">color state for left index finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftMiddle">color state for left middle finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftRing">color state for left ring finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftSmall">color state for left small finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightPalm">color state for right palm (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightThenar">color state for right thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightLowerThenar">color state for right lower thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightInterDigital">color state for right interdigital (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightThumb">color state for right thumb (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightIndex">color state for right index finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightMiddle">color state for right middle finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightRing">color state for right ring finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightSmall">color state for right small finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        [DllImport("LScanEssentials.dll")]
        internal  static extern int LSCAN_Controls_DisplayShowFingerSelectionScreen(
               int                       handle,                 
               enumLScanDisplaySelectionCtrl ctrlLeft,                
               enumLScanDisplayCommonCtrl    ctrlRight,               
               enumLScanDisplayObjectColor   colorLeftPalm,           
               enumLScanDisplayObjectColor   colorLeftThenar,         
               enumLScanDisplayObjectColor   colorLeftLowerThenar,    
               enumLScanDisplayObjectColor   colorLeftInterDigital,   
               enumLScanDisplayObjectColor   colorLeftThumb,          
               enumLScanDisplayObjectColor   colorLeftIndex,          
               enumLScanDisplayObjectColor   colorLeftMiddle,         
               enumLScanDisplayObjectColor   colorLeftRing,           
               enumLScanDisplayObjectColor   colorLeftSmall,          
               enumLScanDisplayObjectColor   colorRightPalm,          
               enumLScanDisplayObjectColor   colorRightThenar,        
               enumLScanDisplayObjectColor   colorRightLowerThenar,   
               enumLScanDisplayObjectColor   colorRightInterDigital,  
               enumLScanDisplayObjectColor   colorRightThumb,         
               enumLScanDisplayObjectColor   colorRightIndex,         
               enumLScanDisplayObjectColor   colorRightMiddle,        
               enumLScanDisplayObjectColor   colorRightRing,          
               enumLScanDisplayObjectColor   colorRightSmall          
            );

        /// <summary>
        /// Display next appropriate finger selection type on finger selection screen.
        /// This cycles through bandaged, missing, restricted and unrestricted.
        /// Note: finger selection screen must be active (call LSCAN_Controls_DisplayShowFingerSelectionScreen once before!).
        /// </summary>
        /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
        /// <param name="pNextCtrlLeft">Icon for left button: current selection type which has been activated,(see @ref DisplaySelectionCtrl). Memory must be provided by caller</param>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        [DllImport("LScanEssentials.dll")]
        internal  static extern int LSCAN_Controls_DisplayShowNextFingerSelection(int handle, out enumLScanDisplaySelectionCtrl pNextCtrlLeft);

        /// <summary>
        /// Show capture progress screen on device display.
        /// </summary>
        /// <param name="handle">Device handle obtained by LS_Device_InitializeDevice()</param>
        /// <param name="ctrlLeft">icon for left button (see @ref enumLScanDisplayCommonCtrl)</param>
        /// <param name="ctrlRight">icon for right button (see @ref enumLScanDisplayCommonCtrl)</param>
        /// <param name="scanStatTop">scan state icon (e.g. roll direction) (see @ref enumLScanDisplayStatTop)</param>
        /// <param name="scanStatBottom">icon for scan result (e.g. roll error) (see @ref enumLScanDisplayStatBottom)</param>
        /// <param name="colorLeftPalm">color state for left palm (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftThenar">color state for left thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftLowerThenar">color state for left lower thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftInterDigital">color state for left interdigital (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftThumb">color state for left thumb (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftIndex">color state for left index finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftMiddle">color state for left middle finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftRing">color state for left ring finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorLeftSmall">color state for left small finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightPalm">color state for right palm (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightThenar">color state for right thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightLowerThenar">color state for right lower thenar (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightInterDigital">color state for right interdigital (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightThumb">color state for right thumb (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightIndex">color state for right index finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightMiddle">color state for right middle finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightRing">color state for right ring finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <param name="colorRightSmall">color state for right small finger (see @ref enumLScanDisplayObjectColor)</param>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        [DllImport("LScanEssentials.dll")]
        internal  static extern int LSCAN_Controls_DisplayShowCaptureProgressScreen(
               int                     handle,                   
               enumLScanDisplayCommonCtrl  ctrlLeft,                  
               enumLScanDisplayCommonCtrl  ctrlRight,                 
               enumLScanDisplayStatTop     scanStatTop,               
               enumLScanDisplayStatBottom  scanStatBottom,            
               enumLScanDisplayObjectColor colorLeftPalm,             
               enumLScanDisplayObjectColor colorLeftThenar,           
               enumLScanDisplayObjectColor colorLeftLowerThenar,      
               enumLScanDisplayObjectColor colorLeftInterDigital,     
               enumLScanDisplayObjectColor colorLeftThumb,            
               enumLScanDisplayObjectColor colorLeftIndex,            
               enumLScanDisplayObjectColor colorLeftMiddle,           
               enumLScanDisplayObjectColor colorLeftRing,             
               enumLScanDisplayObjectColor colorLeftSmall,            
               enumLScanDisplayObjectColor colorRightPalm,            
               enumLScanDisplayObjectColor colorRightThenar,          
               enumLScanDisplayObjectColor colorRightLowerThenar,     
               enumLScanDisplayObjectColor colorRightInterDigital,    
               enumLScanDisplayObjectColor colorRightThumb,           
               enumLScanDisplayObjectColor colorRightIndex,           
               enumLScanDisplayObjectColor colorRightMiddle,          
               enumLScanDisplayObjectColor colorRightRing,            
               enumLScanDisplayObjectColor colorRightSmall            
            );



        /// <summary>
        /// Set the template URL used to render to Display.
        /// </summary>
        /// <param name="handle">Device handle obtained by LS_Device_InitializeDevice()</param>
        /// <param name="templateUrl">The URL to the template</param>
        /// <remarks>This is only for scanners equipped with a layout rendering display</remarks>
        /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
        [DllImport("LScanEssentials.dll")]
        internal static extern int LSCAN_Controls_TouchDisplaySetTemplate(
            int handle,
            string templateUrl
        );

        /// <summary>
        /// Set the template URL used to render to Display and the external parameters used.        
        /// </summary>
        /// <param name="handle">Device handle obtained by LS_Device_InitializeDevice()</param>
        /// <param name="templateUrl">The URL to the template</param>
        /// <param name="keyValues">The array with key/value pairs</param>
        /// <param name="parameterCount">count of key/value pairs in the array</param>
        /// <remarks>This is only for scanners equipped with a layout rendering display</remarks>
        /// <returns>
        /// status code as defined in Constants.LSE_ErrorCode
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_EMPTY_KEY -> parameters have empty key(s)
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_DUPLICATE_KEY -> parameters have duplicate keys
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_INTERNAL_KEY_USED -> parameters use internal keys
        /// </returns>
        [DllImport("LScanEssentials.dll")]
        internal static extern int LSCAN_Controls_TouchDisplaySetTemplate_and_ExternalParameters(
            int handle,
            string templateUrl,
            IntPtr keyValues, //LSCAN_Controls_KeyValue[] keyValues, //IntPtr keyValues ,
            int parameterCount
        );

        /// <summary>
        /// Set the external parameters for template URL used to render to Display.               
        /// </summary>
        /// <param name="handle">Device handle obtained by LS_Device_InitializeDevice()</param>        
        /// <param name="keyValues">The array with key/value pairs</param>
        /// <param name="parameterCount">count of key/value pairs in the array</param>
        /// <remarks>
        ///  This is only for scanners equipped with a layout rendering display
        ///  Every call of this function replaces existing parameters, if parameterCount is zero all parameters will be deleted
        ///  The key and value strings are copied to internal data in the function 
        /// </remarks>
        /// <returns>
        /// status code as defined in LScanEssentialsApi_err.h
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_EMPTY_KEY -> parameters have empty key(s)
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_DUPLICATE_KEY -> parameters have duplicate keys
        ///  LSCAN_ERR_LAYOUTRENDERER_PARAMS_INTERNAL_KEY_USED -> parameters use internal keys
        /// </returns>
        [DllImport("LScanEssentials.dll")]
        internal static extern int LSCAN_Controls_TouchDisplaySetExternalParameters(
            int handle,                          
            LSCAN_Controls_KeyValue[] keyValues,
            int parameterCount                   
        );

        /// <summary>
        /// Simulate a touch event at a given position on a touch display of a device.              
        /// </summary>    
        /// <param name="handle">Device handle obtained by LS_Device_InitializeDevice()</param>
        /// <param name="x">x position of touch event (in display coordinates)</param>    
        /// <param name="y">y position of touch event (in display coordinates)</param>
        /// <remarks>This is only for scanners equipped with a layout rendering display</remarks>
        /// <returns>status code as defined in Constants.LSE_ErrorCode
        /// LSCAN_ERR_NOT_SUPPORTED if layout rendering not supported by device, 
        /// LSCAN_ERR_INVALID_PARAM_VALUE if coordinates are not within render size,
        /// </returns>
        [DllImport("LScanEssentials.dll")]
        internal static extern int LSCAN_Controls_TouchDisplayTouchPosition(
            int handle,
            int x,
            int y
        );

        #endregion

        #region Image Visualisation Related Interface Functions
        /*
     * *********************************************************************************************************
     * Image Visualization Related Interface Functions
     * *********************************************************************************************************
     */

        /// <summary>
        /// Create a Visualization Objects used for the current process. 
        /// This function returns the name of a pipe to be used for communication 
        /// between device ans visualization, which may work in different processes.
        /// The handle obtained has to be passed to the device on Main_Initialize.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This function must be called from the process, which owns the GUI.</remarks>
        [DllImport("LScanEssentials.dll")]
            internal static extern string LSCAN_Visualization_Create();


            /// <summary>
            /// Destroy a Visualization Object
            /// </summary>
            /// <param name="visPipeName">pipeName, obtained from LSCAN_Visualization_Create()</param>
            [DllImport("LScanEssentials.dll")]
            internal static extern void LSCAN_Visualization_Destroy(string visPipeName);



            /// <summary>
            /// Set active visualization mode.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="mode">Visualization mode to use</param>
            /// <param name="options">Bit coded visualization options to use (see @ref VisualizationOptions)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            /// <remarks> LSCAN_Visualization_SetWindow() must be called also to set area to display image in</remarks>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_SetMode(int handle, enumLScanVisMode mode, uint options);


            /// <summary>
            /// Set Window and window area for display operations.
            /// Preview and result images are automatically scaled to fit into this area. 
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="hWnd">window handle of window to draw to</param>
            /// <param name="drawRect">client rectangle to draw to\n
            /// Valid values: coordinates >= 0 and right >= left and bottom >= top\n
            /// If invalid then whole client area is used</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_SetWindow(int handle, uint hWnd, System.Drawing.Rectangle drawRect);



            /// <summary>
            /// Get the scale factor of the displayed image in relation to the result image size.
            /// The scale factor depends on:\n
            /// - The active visualization area (see LSCAN_Visualization_SetWindow())\n
            /// - The active capture area (if activated by not setting option @ref LSCAN_OPTION_VIS_FULL_IMAGE with
            ///   LSCAN_Visualization_SetMode())\n
            /// - The active capture mode (see LSCAN_Capture_SetMode())
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="scaleFactor">The currently active scale factor</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_GetScaleFactor(int handle, out double scaleFactor);


            /// <summary>
            /// Set background color of visualization area.
            /// The default color after initialization is 0xe00e0e0e0.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="color">Color to use (format: 0x00bbggrr)</param>
            /// <returns></returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_SetBackgroundColor(int handle, COLORREF color);

            #endregion

            #region OVERLAY APIs

            /// <summary>
            /// Remove an overlay from visualization
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="overlayHandle">Handle to overlay to be removed from visualization</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_RemoveOverlay(int handle, uint overlayHandle);

            /// <summary>
            /// Remove all overlays from visualization.
            /// note The internally generated overlay to mark active user area is not removed.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_RemoveAllOverlays(int handle);



            /// <summary>
            /// Show/hide an overlay.
            /// </summary>
            /// <param name="handle">Handle to overlay to show/hide</param>
            /// <param name="overlayHandle"></param>
            /// <param name="show">Show overlay if TRUE (hide if FALSE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_ShowOverlay(int handle, uint overlayHandle, int show);


            /// <summary>
            /// Show/hide all existing overlays.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="show">Show overlays if TRUE (hide if FALSE)</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_ShowAllOverlays(int handle, int show);


            /// <summary>
            /// Add a text overlay to visualization set by LSCAN_Visualization_SetWindow()
            /// (exception: belongsToImage is set to TRUE)
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="text">Text to display</param>
            /// <param name="posX">Horizontal text position</param>
            /// <param name="posY">Vertical text position</param>
            /// <param name="color">Text color (format: 0x00bbggrr)</param>
            /// <param name="fontName">Text font</param>
            /// <param name="fontSize">Text font size </param>
            /// <param name="belongsToImage">Specify if overlay is related to image
            /// TRUE: position is set relative to image top left corner; overlay is visible while image available
            /// FALSE: position is set relative to the top left corner of drawing area; overlay is visible while visualization is active
            /// </param>
            /// <param name="overlayHandle">Handle to the generated overlay</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_AddOverlayText(int handle, string text, int posX, int posY, COLORREF color, string fontName, int fontSize,
                            int belongsToImage, out uint overlayHandle);


            /// <summary>
            /// Modify an existing text overlay.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="overlayHandle">Handle to text overlay to be modified </param>
            /// <param name="text">Text to display</param>
            /// <param name="posX">Horizontal text position</param>
            /// <param name="posY">Vertical text position</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_ModifyOverlayText(int handle, uint overlayHandle, string text, int posX, int posY);



            /// <summary>
            /// Add a quadrangle overlay to visualization.set by LSCAN_Visualization_SetWindow() (exception: belongsToImage is set to TRUE)
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="x1">X coordinate of starting point</param>
            /// <param name="y1">Y coordinate of starting point</param>
            /// <param name="x2">X coordinate of first corner</param>
            /// <param name="y2">Y coordinate of first corner</param>
            /// <param name="x3">X coordinate of second corner</param>
            /// <param name="y3">Y coordinate of second corner</param>
            /// <param name="x4">X coordinate of third corner</param>
            /// <param name="y4">Y coordinate of third corner</param>
            /// <param name="color">Text color (format: 0x00bbggrr)</param>
            /// <param name="lineWidth">Quadrangle line width</param>
            /// <param name="belongsToImage">pecify if overlay is related to image
            /// TRUE: position is set relative to image top left corner;overlay is visible while image available
            /// FALSE: position is set relative to the top left corner of drawing area;overlay is visible while visualization is active
            /// </param>
            /// <param name="overlayHandle">Handle to the generated overlay</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_AddOverlayQuadrangle(int handle, int x1, int y1, int x2, int y2,
                            int x3, int y3, int x4, int y4, COLORREF color, int lineWidth, int belongsToImage, out uint overlayHandle);



            /// <summary>
            /// Modify an existing quadrangle overlay.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="overlayHandle">Handle to quadrangle overlay to be modified </param>
            /// <param name="x1">X coordinate of starting point</param>
            /// <param name="y1">Y coordinate of starting point</param>
            /// <param name="x2">X coordinate of first corner</param>
            /// <param name="y2">Y coordinate of first corner</param>
            /// <param name="x3">X coordinate of second corner</param>
            /// <param name="y3">Y coordinate of second corner</param>
            /// <param name="x4">X coordinate of third corner</param>
            /// <param name="y4">Y coordinate of third corner</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_ModifyOverlayQuadrangle(int handle, uint overlayHandle, int x1, int y1,
                                int x2, int y2, int x3, int y3, int x4, int y4);


            /// <summary>
            /// Add a line overlay to visualization. set by LSCAN_Visualization_SetWindow() (exception: belongsToImage is set to TRUE)
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="x1">X coordinate of starting point</param>
            /// <param name="y1">Y coordinate of starting point</param>
            /// <param name="x2">X coordinate of ending point</param>
            /// <param name="y2">Y coordinate of ending point</param>
            /// <param name="color">Text color (format: 0x00bbggrr)</param>
            /// <param name="lineWidth">Quadrangle line width</param>
            /// <param name="belongsToImage">pecify if overlay is related to image
            /// TRUE: position is set relative to image top left corner;overlay is visible while image available
            /// FALSE: position is set relative to the top left corner of drawing area;overlay is visible while visualization is active
            /// </param>
            /// <param name="overlayHandle">Handle to the generated overlay</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_AddOverlayLine(int handle, int x1, int y1, int x2, int y2, COLORREF color,
                                    int lineWidth, int belongsToImage, out uint overlayHandle);

            /// <summary>
            /// Modify an existing line overlay.
            /// </summary>
            /// <param name="handle">Device handle obtained by LSCAN_Main_Initialize()</param>
            /// <param name="overlayHandle">Handle to quadrangle overlay to be modified </param>
            /// <param name="x1">X coordinate of starting point</param>
            /// <param name="y1">Y coordinate of starting point</param>
            /// <param name="x2">X coordinate of ending point</param>
            /// <param name="y2">Y coordinate of ending point</param>
            /// <returns>status code as defined in Constants.LSE_ErrorCode</returns>
            [DllImport("LScanEssentials.dll")]
            internal static extern int LSCAN_Visualization_ModifyOverlayLine(int handle, uint overlayHandle, int x1, int y1, int x2, int y2);

            #endregion

        }
}
