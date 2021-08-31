namespace Livescan.Scanners.DriverEssential.Sdk
{
    using System;

    /// <summary>
    /// Generic callback function.
    /// It is used for notifications that do not provide additional information: \n
    /// LSCAN_Main_RegisterCallbackCommunicationBreak(), \n
    /// LSCAN_Capture_RegisterCallbackTakingResultImage() and \n
    /// LSCAN_Capture_RegisterCallbackAcquisitionComplete()
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for </param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_Callback)(const int deviceHandle, IntPtr pContext);</remarks>
    public delegate void LSCAN_Callback(int deviceHandle, IntPtr pContext);




    /// <summary>
    /// Preview image available notification.
    /// To register for the notification call LSCAN_Capture_RegisterCallbackPreviewImage().
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext"> caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="image">preview image (valid until callback function is exited)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackPreviewImage)(const int deviceHandle, IntPtr pContext, const ImageData image);</remarks>
    public delegate void LSCAN_CallbackPreviewImage(int deviceHandle, IntPtr pContext, ImageData image);

    /// <summary>
    /// Object/Finger count change notification.
    /// To register for the notification call LSCAN_Capture_RegisterCallbackObjectCount().
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="fingerCountState">finger count state (OK, too few, too many)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackObjectCount)(const int deviceHandle, IntPtr pContext, const LScanObjectCountState fingerCountState);</remarks>
    public delegate void LSCAN_CallbackObjectCount(int deviceHandle, IntPtr pContext, enumLScanObjectCountState fingerCountState);


    /// <summary>
    /// Finger tip quality change notification.
    /// To register for the notification call LSCAN_Capture_RegisterCallbackObjectQuality().
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="pQualityArray">pointer to an array of finger tip quality states (array size in quality count)</param>
    /// <param name="qualityCount">size of finger tip quality array</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackObjectQuality)(const int deviceHandle, IntPtr pContext, const LScanObjectQualityState *pQualityArray, const int qualityCount);</remarks>
    public delegate void LSCAN_CallbackObjectQuality(int deviceHandle, IntPtr pContext, IntPtr pQualityArray, int qualityCount);

    /// <summary>
    /// Result image availability notification function.
    /// To register for the notification call LSCAN_Capture_RegisterCallbackResultImage().
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="imageStatus">result image status value (value >= LSCAN_STATUS_OK indicates success)</param>
    /// <param name="image">result image (valid until callback function is exited)</param>
    /// <param name="imageType">if auto capture license available and imageStatus >= LSCAN_STATUS_OK: 
    /// detected result image type (e.g. LSCAN_ROLL_SINGLE_FINGER). else if imageStatus >= LSCAN_STATUS_OK: image type as selected by LSCAN_Capture_SetMode()
    /// else:  LSCAN_TYPE_NONE
    /// </param>
    /// <param name="detectedObjects">if imageStatus >= LSCAN_STATUS_OK and auto capture license available: 
    /// number of detected objects/fingers in result image. else: -1 
    /// </param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackResultImage)(const int deviceHandle, IntPtr pContext, const int imageStatus, const ImageData image, const LScanImageType imageType, const int detectedObjects);</remarks>
    public delegate void LSCAN_CallbackResultImage(int deviceHandle, IntPtr pContext, int imageStatus, ImageData image, enumLScanImageType imageType, int detectedObjects);


    /// <summary>
    /// This notification indicates that finger/hand needs to be removed from platen prior to new capture.
    /// To register for the notification call LSCAN_Capture_RegisterCallbackClearObjectsFromPlaten().
    /// Once the notification is sent with platen state @ref LSCAN_CLEAR_OBJECT_FROM_PLATEN auto capture 
    /// will not trigger until object is removed and a second notification with platen state 
    /// @ref LSCAN_PLATED_CLEARED is sent. \n
    /// The check can be disabled by deregistration of the callback. \n
    /// This callback is not depending on availability of the auto capture license.
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="state"> platen state information</param>
    /// <remarks> @note When the system is in state @ref LSCAN_CLEAR_OBJECT_FROM_PLATEN then automatic capture will
    /// not trigger result image acquisition but a call to LSCAN_Capture_TakeResultImage() will do so.
    /// ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackClearObjectsFromPlaten)(const int deviceHandle, IntPtr pContext, LScanPlatenState state);</remarks>
    public delegate void LSCAN_CallbackClearObjectsFromPlaten(int deviceHandle, IntPtr pContext, enumLScanPlatenState state);

    /// <summary>
    /// Key(s) pressed/released notification.
    /// The notification is fired when any key state has changed. \n
    /// To register for the notification call LSCAN_Controls_RegisterCallbackKeys(). \n
    /// In addition LSCAN_Controls_SetActiveKeys() must be called to define key(s) \n
    /// that trigger the notification callback.
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="pressedKeys">current key state (see @ref KeypadConstants)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackKeys)(const int deviceHandle, IntPtr pContext, const uint pressedKeys);</remarks>
    public delegate void LSCAN_CallbackKeys(int deviceHandle, IntPtr pContext, uint pressedKeys);

    /// <summary>
    /// Device operation progress notification.
    /// Currently progress callbacks are generated for initialization and infield test. \n
    /// To register for the notification call LSCAN_Main_RegisterCallbackProgress().
    /// </summary>
    /// <param name="deviceHandle">device handle to identify device the callback is called for</param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <param name="type">operation type (e.g initialization or infield test)</param>
    /// <param name="progressValue">current progress value (0.0..1.0)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackProgress)(const int deviceIndex, IntPtr pContext, const LScanOperationType type, const float progressValue);</remarks>
    public delegate void LSCAN_CallbackProgress(int deviceIndex, IntPtr pContext, enumLScanOperationType type, float progressValue);

    /// <summary>
    /// Device count change notification.
    /// Currently progress callbacks are generated for initialization and infield test. \n
    /// To register for the notification call LSCAN_Main_RegisterCallbackDeviceCount().
    /// </summary>
    /// <param name="detectedDevices">number of detected devices </param>
    /// <param name="pContext">caller defined context (e.g. handler object instance pointer)</param>
    /// <remarks>ORIGINAL LINE: typedef void (CALLBACK *LSCAN_CallbackDeviceCount)(const int detectedDevices, IntPtr pContext);</remarks>
    public delegate void LSCAN_CallbackDeviceCount(int detectedDevices, IntPtr pContext);
}
