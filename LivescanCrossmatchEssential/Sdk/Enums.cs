namespace Livescan.Scanners.DriverEssential.Sdk
{
    /// Supported image types.
    /// This is an enumeration of the image types supported by the SDK.
    /// It is used as parameter for LSCAN_Capture_SetMode(), LSCAN_Capture_IsModeAvailable()
    /// and LSCAN_CallbackResultImage().
    public enum enumLScanImageType
    {
        /// <summary>
        /// Image type not set or unknown
        /// This type can be set via LSCAN_Capture_SetMode()
        /// to ensure background image streaming is stopped
        /// </summary>
        LSCAN_TYPE_NONE,
        /// <summary>
        /// Rolled finger print image.
        /// Availability of this image type is linked to the finger print rolling license stored for the device.
        /// Options to allow automatic recapture are configurable via INI file
        /// and/or device properties. If capture option LSCAN_OPTION_AUTO_CAPTURE is used then in addition:
        /// - minimum start distance and roll start zones are configurable via INI file and/or device properties \n        
        ///  - capture area is divided into 3 segments; from the left and right third only rolling to the opposite direction is allowed;        
        ///  from the middle third a change of rolling direction is required in order to trigger roll start        
        /// </summary>												      
        LSCAN_ROLL_SINGLE_FINGER,
        /// <summary>
        /// Flat single finger
        /// Typical number of objects in result image: 1
        /// </summary>
        LSCAN_FLAT_SINGLE_FINGER,
        /// <summary>
        /// Right flat fingers
        /// Typical number of objects in result image: 4
        /// </summary>
        LSCAN_FLAT_RIGHT_FINGERS,
        /// <summary>
        /// Left flat fingers
        /// Typical number of objects in result image: 4
        /// </summary>
        LSCAN_FLAT_LEFT_FINGERS,
        /// <summary>
        /// Two flat thumbs
        ///  The acquisition is done at the capture area used also for 4 flats      
        /// Typical number of objects in result image: 2
        /// </summary>
        LSCAN_FLAT_TWO_THUMBS,

        /// <summary>
        /// Full right palm
        /// Typical number of objects in result image: 6 (5 fingers + palm area)
        /// </summary>
        LSCAN_PALM_RIGHT_FULL,
        /// <summary>
        /// Right writers palm
        /// Typical number of objects in result image: 1
        /// </summary>
        LSCAN_PALM_RIGHT_WRITERS,
        /// <summary>
        /// Right lower palm
        /// Typical number of objects in result image: 1
        /// </summary>
        LSCAN_PALM_RIGHT_LOWER,
        /// <summary>
        /// Right upper palm
        /// Typical number of objects in result image: 5 (4 fingers + palm area)
        /// </summary>
        LSCAN_PALM_RIGHT_UPPER,
        /// <summary>
        /// Full left palm
        /// Typical number of objects in result image: 6 (5 fingers + palm area)
        /// </summary>
        LSCAN_PALM_LEFT_FULL,
        /// <summary>
        /// Left writers palm
        /// Typical number of objects in result image: 1
        /// </summary>
        LSCAN_PALM_LEFT_WRITERS,
        /// <summary>
        /// Left lower palm
        /// Typical number of objects in result image: 1
        /// </summary>
        LSCAN_PALM_LEFT_LOWER,
        /// <summary>
        /// Left upper palm
        /// Typical number of objects in result image: 5 (4 fingers + palm area)
        /// </summary>
        LSCAN_PALM_LEFT_UPPER,
        /// <summary>
        /// Two flat fingers
        /// The acquisition is done at the capture area used also for 1 single finger
        /// Typical number of objects in result image: 2
        /// </summary>
        LSCAN_FLAT_TWO_FINGERS
    }


    public enum enumLScanImageResolution
    {
        /// <summary>
        /// 500 ppi
        /// </summary>
        LSCAN_IMAGE_RESOLUTION_500 = 500,
        /// <summary>
        /// 1000 ppi
        /// </summary>
        LSCAN_IMAGE_RESOLUTION_1000 = 1000
    }

    /// Image line order types.
    /// It defines how the image is aligned in memory.
    /// They are used as parameter for LSCAN_Capture_SetMode().
    public enum enumLScanImageOrientation
    {
        /// <summary>
        /// Use line order as delivered from device
        /// </summary>
        LSCAN_IMAGE_INHERIT_LINE_ORDER,
        /// <summary>
        /// Force result image line order to be "bottom -> up"
        /// </summary>
        LSCAN_IMAGE_BOTTOM_UP,
        /// <summary>
        /// Force result image line order to be "top -> down"
        /// </summary>
        LSCAN_IMAGE_TOP_DOWN
    }

    /// Keypad types.
    /// Depending on device type and hardware configuration LSCAN_Controls_GetAvailableKeys() returns the 
    /// keypad type.  
    public enum enumLScanKeypadType
    {
        /// <summary>
        /// No keypad available
        /// </summary>
        LSCAN_KEYPAD_NONE,
        /// <summary>
        /// 5 button keypad
        /// </summary>
        LSCAN_KEYPAD_5_KEYS,
        /// <summary>
        /// 2 button keypad
        /// </summary>
        LSCAN_KEYPAD_2_KEYS
    }

    /// Beeper types.
    /// Depending on device type and hardware configuration LSCAN_Controls_GetAvailableBeeper() returns the 
    /// beeper type.
    public enum enumLScanBeeperType
    {
        /// <summary>
        /// No beeper available
        /// </summary>
        LSCAN_BEEPER_NONE,
        /// <summary>
        /// Generic beeper (8 patterns)
        /// </summary>
        LSCAN_BEEPER_GENERIC,
        /// <summary>
        /// generic beeper (8 patterns, volume setting)
        /// </summary>
        LSCAN_BEEPER_GENERIC_VOLUME_4,
        /// <summary>
        /// generic beeper (8 patterns, volume setting)
        /// </summary>
        LSCAN_BEEPER_GENERIC_VOLUME_64
    }

    /// LED field types.
    /// This is a list of different LED fields.
    /// LSCAN_Controls_GetAvailableLEDs() returns the component type for the selected device.
    public enum enumLScanLedType
    {
        /// <summary>
        /// no LED field available
        /// </summary>
        LSCAN_LED_TYPE_NONE,

        /// <summary>
        /// LSCAN LED field (LEDs in 'OK' and 'Cancel' button) as used in L SCAN 1000P and L SCAN 1000T
        /// </summary>
        LSCAN_LED_TYPE_LSCAN, 

        /// <summary>
        /// LEDs to indicate finger status + symbol LEDs as used for L SCAN GUARDIAN and L SCAN GUARDIAN USB
        /// </summary>
        LSCAN_LED_TYPE_STATUS,

        /// <summary>
        /// LSCAN LED field (LEDs in 'OK' and 'Cancel' button) emulation on Display as used in L SCAN 500P
        /// </summary>
        LSCAN_LED_TYPE_LSCAN_DISPLAY_EMULATION,
        
        /// <summary>
        /// LSCAN LED field (LEDs in 'OK' and 'Cancel' button) emulation on Display as used for 1000ppi devices
        /// </summary>
        LSCAN_LED_TYPE_LSCAN_DISPLAY_EMULATION_1000PPI 
    }



    /// <summary>
    /// Device property definitions.
    /// Certain device details and settings can be requested by LSCAN_Main_GetProperty(). Also specific
    /// parameters are adjustable via LSCAN_Main_SetProperty(). 
    /// </summary>
    public enum enumLScanPropertyId
    {
        /// <summary>
        /// [get] Product name string (e.g. "L SCAN 1000P")
        /// </summary>
        LSCAN_PROPERTY_PRODUCT_ID,


        /// <summary>
        /// [get] Serial number string  
        /// </summary>
        LSCAN_PROPERTY_SERIAL_NUMBER,

        /// <summary>
        /// [get] Device manufacturer identifier
        /// </summary>
        LSCAN_PROPERTY_VENDOR_ID,

        /// <summary>
        /// [get] IBIA vendor ID
        /// </summary>
        LSCAN_PROPERTY_IBIA_VENDOR_ID,

        /// <summary>
        /// [get] IBIA version information
        /// </summary>
        LSCAN_PROPERTY_IBIA_VERSION,

        /// <summary>
        /// [get] IBIA device ID
        /// </summary>
        LSCAN_PROPERTY_IBIA_DEVICE_ID,

        /// <summary>
        /// [get] Firmware version string
        /// </summary>
        LSCAN_PROPERTY_FIRMWARE,

        /// <summary>
        /// [get] Device revision string
        /// </summary>
        LSCAN_PROPERTY_REVISION,

        /// <summary>
        /// [get] Production date string
        /// </summary>
        LSCAN_PROPERTY_PRODUCTION_DATE,

        /// <summary>
        /// [get] Last service date string
        /// </summary>
        LSCAN_PROPERTY_SERVICE_DATE,

        /// <summary>
        /// [get] Number of acquired images
        /// </summary>
        LSCAN_PROPERTY_ACQUISITIONS,

        /// <summary>
        /// [get] Number of device resets
        /// </summary>
        LSCAN_PROPERTY_RESETS,

        /// <summary>
        /// [get] Power on time (in seconds)
        /// </summary>
        LSCAN_PROPERTY_POWER_ON_TIME,

        /// <summary>
        /// [get] Standby time (in seconds)
        /// </summary>
        LSCAN_PROPERTY_STANDBY_TIME,

        /// <summary>
        /// [get] Certification level
        /// </summary>
        LSCAN_PROPERTY_CERTIFICATION_LEVEL,

        /// <summary>
        /// [get] License for automatic acquisition trigger
        /// </summary>
        LSCAN_PROPERTY_AUTO_CAPTURE,

        /// <summary>
        /// [get] License to access preview images
        /// </summary>
        LSCAN_PROPERTY_PREVIEW_IMAGES,

        /// <summary>
        /// [get] Device is equipped with a silicone pad
        /// </summary>
        LSCAN_PROPERTY_SILICONE_PAD,

        /// <summary>
        /// [get] Device is capable/licensed for fingerprint rolling
        /// </summary>
        LSCAN_PROPERTY_FINGERPRINT_ROLLING,

        /// <summary>
        /// [get] Device is capable/licensed for image quality infield test
        /// </summary>
        LSCAN_PROPERTY_INFIELD_TEST,

        /// <summary>
        /// [get] Heater boost mode enabled
        /// </summary>
        LSCAN_PROPERTY_HEATER_BOOST_MODE,

        /// <summary>
        /// [get/set] Index of predefined roll profile to use for fingerprint rolling
        /// Please refer to LScanEssentials.ini for details!
        /// </summary>
        LSCAN_PROPERTY_ROLL_PROFILE,

        /// <summary>
        /// [get/set] Minimum roll distance \n
        /// It defines the threshold for automatic recapture if option LSCAN_OPTION_AUTO_CAPTURE is used \n
        ///  The roll distance is calculated as movement of center mass point \n
        ///  Unit: mm \n
        ///  Range: 0 &lt;= value &lt;= 30 \n
        ///  Default value: 10
        /// </summary>
        LSCAN_PROPERTY_ROLL_MIN_DISTANCE,

        /// <summary>
        /// [get/set] Index of predefined auto capture profile to use \n
        ///  Please refer to LScanEssentials.ini for details!
        /// </summary>
        LSCAN_PROPERTY_AUTO_CAPTURE_PROFILE,

        /// <summary>
        /// [get/set] Time an object needs to be kept still before acquisition is triggered \n
        ///  Unit: ms \n
        ///  Range: 0 &lt; value &lt; 10000 \n
        ///  Default value: 750
        /// </summary>
        LSCAN_PROPERTY_AUTO_CAPTURE_TRACKING_TIME,

        /// <summary>
        /// [get/set] Time an object needs to be kept still before
        ///  <em> Auto Capture </em> triggers result image acquisition 
        ///  when conditions <em>object count </em> and/or \e contrast do not meet \n
        ///  Unit: ms \n
        ///  Range:  @ref LSCAN_PROPERTY_AUTO_CAPTURE_TRACKING_TIME &lt;= value &lt; 10000 \n
        ///  Default value: 4000
        /// </summary>
        LSCAN_PROPERTY_AUTO_CAPTURE_OVERRIDE_TIME,

        /// <summary>
        /// [get/set] Flag to allow object to overlap platen or active area \n
        ///  If disabled and hand/finger placed on capture area border then: \n
        ///  > Overlay arrows in visualization show the directions to shift hand/finger \n
        ///  > Auto capture won't trigger  \n
        ///  Range: TRUE, FALSE \n
        ///  Default value: FALSE
        /// </summary>
        LSCAN_PROPERTY_ALLOW_OBJECT_ON_BORDER,

        /// <summary>
        /// [get/set] Automatic device readjustment \n
        ///  If enabled then the device is automatically adjusted in intervals 
        ///  (starting with 10 minutes and growing to 1 hour) \n
        ///  Range: TRUE, FALSE \n
        ///  Default value: TRUE \n
        ///  @b Note: A call to LSCAN_Capture_Start() internally blocks until a possible pending adjustment is finished
        /// </summary>
        LSCAN_PROPERTY_AUTOMATIC_ADJUSTMENT,

        /// <summary>
        /// [get/set] Roll distance to trigger roll start detection if option LSCAN_OPTION_AUTO_CAPTURE is used for
        ///  finger print rolling; Exceeding of this distance triggers TakingResultImage callback \n
        ///  The roll distance is calculated as movement of center mass point \n
        ///  A value of 0 disables the check \n
        ///  Unit: mm \n
        ///  Range: 0 &lt;= value &lt;= 30 \n
        ///  Default value: 3
        /// </summary>
        LSCAN_PROPERTY_ROLL_START_DISTANCE,

        /// <summary>
        /// [get/set] Allow automatic recapture of rolled fingerprint until finger is lifted \n
        ///  Range: TRUE, FALSE \n
        ///  Default value:  FALSE
        /// </summary>
        LSCAN_PROPERTY_ROLL_ALLOW_RESTART,

        /// <summary>
        /// [get/set] Time to wait for finger re-placement to recapture a rolled fingerprint
        ///  after finger is lifted \n                                                      
        ///  The result image is accepted when the timeout period is exceeded or if LSCAN_Capture_TakeResultImage() is called \n
        ///  Values: 0 re-placement deactivated, -1 infinite timeout, > 0 timeout value \n
        ///  Unit: ms \n
        ///  Range: -1 &lt;= value &lt;= 5000 \n
        ///  Default value: 0
        /// </summary>
        LSCAN_PROPERTY_ROLL_TIMEOUT_LIFT,

        /// <summary>
        /// [get/set] Minimum roll distance \n
        /// It defines the threshold for automatic recapture if option LSCAN_OPTION_AUTO_CAPTURE is used \n
        ///  The roll width is defined as the result image width \n
        ///  A value of 0 disables the check \n
        ///  Unit: mm \n
        ///  Range: 0 &lt;= value &lt;= 50 \n
        ///  Default value: 10
        /// </summary>
        LSCAN_PROPERTY_ROLL_MIN_WIDTH,

        /// <summary>
        /// [get/set] Enable 'Perfect Image' enhancement \n
        ///  If enabled then image is automatically corrected for effects resulting from condensate and dirt
        ///  on the platen \n
        ///  Range: TRUE, FALSE \n
        ///  Default value: TRUE
        /// </summary>
        LSCAN_PROPERTY_PERFECT_IMAGE,

        /// <summary>
        /// [get/set] Roll start area in percent \n
        /// It defines the width of left and right roll start areas if option LSCAN_OPTION_AUTO_CAPTURE is used \n
        ///  Unit: percent \n
        ///  Range: 0 &lt;= value &lt;= 50 \n
        ///  Default value: 33
        /// </summary>
        LSCAN_PROPERTY_ROLL_START_AREA,

        /// <summary>
        /// [get] License for Spoof Detection
        /// </summary>
        LSCAN_PROPERTY_SPOOF_DETECTION,

        /// <summary>
        /// [get/set] Confidence level for Spoof Detection \n
        ///  It defines the confidence level for the Spoof Detection which is active if option  LSCAN_OPTION_SPOOF_DETECTION is used \n
        ///  Unit: percent \n
        ///  Range: 0 &lt;= value &lt;= 100 \n
        ///  -1 restores internal default value \n
        ///  Default value: -1
        /// </summary>
        LSCAN_PROPERTY_SPOOF_DETECTION_CONFIDENCE_LEVEL,

        /// <summary>
        /// [get/set] Show overlay for rolling zones before rolling has started \n
        ///  Range: TRUE, FALSE \n
        ///  Default value: FALSE
        /// </summary>
        LSCAN_PROPERTY_ROLL_START_SHOW_OVERLAY,

        /// <summary>
        /// [get/set] Show overlay for finger position before rolling has started \n
        ///  Overlay is only displayed if LSCAN_PROPERTY_ROLL_START_SHOW_OVERLAY is activated too. \n
        ///  Range: TRUE, FALSE \n
        ///  Default value: FALSE
        /// </summary>
        LSCAN_PROPERTY_ROLL_START_SHOW_OVERLAY_FINGER,

        /// <summary>
        /// [get] Last result of automatic adjustment \n
        ///  Returns the last result value of automatic adjustment. For details please see @ref RETURN_CODES. \n
        ///  Range: text string containing return code value.
        /// </summary>
        LSCAN_PROPERTY_LAST_AUTOMATIC_ADJUSTMENT_RESULT,

        /// <summary>
        /// [get/set] Time for automatic contrast optimization to start after finger placement \n
        ///  Unit: ms \n
        ///  Range: 0 &lt; value &lt; 10000 \n
        ///  Default value: 500,
        /// </summary>
        LSCAN_PROPERTY_AUTO_CONTRAST_WAIT_TIME,

        /// <summary>
        /// [get/set] AutoCapture mode for alternative trigger \n
        ///  Requires @ref LSCAN_OPTION_AUTO_OVERRIDE = TRUE to take effect.
        ///  Valid values: @ref SEC_AUTOCAPTUREOVERRIDE_ONINSUFFICIENTCOUNT "OnInsufficientCount", 
        ///                @ref SEC_AUTOCAPTUREOVERRIDE_ONINSUFFICIENTQUALITY "OnInsufficientQuality" \n
        ///  Default value: @ref SEC_AUTOCAPTUREOVERRIDE_ONINSUFFICIENTCOUNT "OnInsufficientCount"
        /// </summary>
        LSCAN_PROPERTY_AUTO_CAPTURE_OVERRIDE_MODE,

        /// <summary>
        /// [get/set] Optical resolution of preview images in ppi.
        /// Valid values: 125, 250 (127, 254 for L SCAN 500PJ)\n
        /// Default value: 250 (254 for L SCAN 500PJ)
        /// </summary>
        LSCAN_PROPERTY_PREVIEW_RESOLUTION,

        /// <summary>
        /// [get/set] Enable 'Flexible Rolling Mode' enhancement \n
        ///  If TRUE then the flexible rolling capture mode is used.
        ///  Range: TRUE, FALSE \n
        ///  Default value: FALSE
        /// </summary>
        LSCAN_PROPERTY_ROLL_FLEXIBLE
    }

    /// Progress type definitions.
    /// During device initialization and infield test the SDK provides progress feedback via
    /// callback LSCAN_CallbackProgress().
    public enum enumLScanOperationType
    {
        /// <summary>
        /// Progress of LSCAN_Main_Initialize()
        /// </summary>
        LSCAN_OPERATION_INITIALIZATION = 0,

        /// <summary>
        /// Progress of LSCAN_Main_ImageQualityInfieldTest()
        /// </summary>
        LSCAN_OPERATION_INFIELD_TEST = 1
    }

    /// Visualization mode definitions.
    /// Used to control visualization behavior
    public enum enumLScanVisMode
    {

        /// <summary>
        /// Image display only during image acquisition
        /// This is the default mode after initialization.
        /// </summary>
        LSCAN_VIS_PREVIEW_ONLY,
        /// <summary>
        /// Result image is displayed when available
        /// </summary>
        LSCAN_VIS_RESULT,

        /// <summary>
        /// Visualization always active (even if no acquisition active)
        /// </summary>
        LSCAN_VIS_ALWAYS
    }

    /// Object/Finger count state definitions.
    public enum enumLScanObjectCountState
    {
        LSCAN_OBJECT_COUNT_OK,
        LSCAN_TOO_MANY_OBJECTS,
        LSCAN_TOO_FEW_OBJECTS
    }

    /// Quality state definitions for detected finger/palm object.
    public enum enumLScanObjectQualityState : int
    {
        LSCAN_OBJECT_NOT_PRESENT, ///< Object not detected
        ///  (only sent if capture license available)
        LSCAN_OBJECT_GOOD, ///< Object tracking OK;
        ///  Sent if (1) Auto capture license available 
        ///  and object good for flat/palm images to 
        ///  trigger result image or if 
        ///  (2) Minimum roll distance/width has been
        ///  reached for fingerprint rolling with auto 
        ///  capture option set
        LSCAN_OBJECT_TOO_LIGHT, ///< Object contrast too low
        ///  (only sent if capture license available)
        LSCAN_OBJECT_TOO_DARK, ///< Object contrast too dark
        ///  (only sent if capture license available)
        LSCAN_OBJECT_BAD_SHAPE, ///< Object shape not OK
        ///  (only sent if capture license available)
        LSCAN_OBJECT_POSITION_NOT_OK, ///< Object position not within tracking area
        ///  (only sent if capture license available)
        LSCAN_OBJECT_CORE_NOT_PRESENT, ///< Object core not found
        ///  (only sent if capture license available)
        LSCAN_OBJECT_TRACKING_NOT_OK, ///< Sent if (1) Required tracking time has not
        ///  been reached yet for flat/palm images
        ///  (keep object calm to trigger result image) 
        ///  or if (2) minimum roll distance/width
        ///  has not been reached yet for fingerprint
        ///  rolling with auto capture option set;
        ///  (auto capture license must be available for both cases)
        LSCAN_OBJECT_POSITION_TOO_HIGH, ///< Object position not within tracking area
        ///  (only sent if capture license available)
        LSCAN_OBJECT_POSITION_TOO_LEFT, ///< Object position not within tracking area
        ///  (only sent if capture license available)
        LSCAN_OBJECT_POSITION_TOO_RIGHT, ///< Object position not within tracking area
        ///  (only sent if capture license available)
        LSCAN_OBJECT_FLEX_POSITION_TOO_HIGH, /// Flex Flats Mode only: Object position not within active area
        LSCAN_OBJECT_FLEX_POSITION_TOO_LEFT, ///< Flex Flats Mode only: Object position not within active area
        LSCAN_OBJECT_FLEX_POSITION_TOO_RIGHT, ///< Flex Flats Mode only: Object position not within active area
        LSCAN_OBJECT_FLEX_POSITION_TOO_LOW ///< Flex Flats Mode only: Object position not within active area
    }

    /// Platen surface state definitions.
    public enum enumLScanPlatenState
    {
        LSCAN_CLEAR_OBJECT_FROM_PLATEN, ///< fingers are detected at the beginning of a capture sequence
        ///< but platen should be cleared to ensure different hand/fingers placed 
        LSCAN_PLATED_CLEARED ///< platen has been cleared from initially placed hand/finger
    }

    /// Display logo screen option constants
    /// @anchor DisplayLogoOptions
    public enum enumLScanDisplayLogoOption
    {
        LSCAN_DISPLAY_LOGO_OPTION_ERASE, ///< erase option area
        LSCAN_DISPLAY_LOGO_OPTION_SHOW_FW_VERSION, ///< show firmware version on bottom
        LSCAN_DISPLAY_LOGO_OPTION_LEAVE_UNCHANGED ///< do not modify item
    }


    /// Display control codes for selection screen (left button).
    /// @anchor DisplaySelectionCtrl
    public enum enumLScanDisplaySelectionCtrl
    {
        LSCAN_DISPLAY_SELECTION_CTRL_ERASE, ///< erase selection control area
        LSCAN_DISPLAY_SELECTION_CTRL_OBJECT_OK, ///< object is OK, unrestricted
        LSCAN_DISPLAY_SELECTION_CTRL_OBJECT_BANDAGED, ///< object is bandaged
        LSCAN_DISPLAY_SELECTION_CTRL_OBJECT_MISSING, ///< object is missing
        LSCAN_DISPLAY_SELECTION_CTRL_OBJECT_RESTRICTED, ///< object is restricted
        LSCAN_DISPLAY_SELECTION_CTRL_REPEAT_YELLOW, ///< repeat selection
        LSCAN_DISPLAY_SELECTION_CTRL_LEAVE_UNCHANGED ///< do not modify item
    }

    /// Display control codes for selection screen (right button) & scan screen (left + right buttons).
    /// @anchor DisplayCommonCtrl
    public enum enumLScanDisplayCommonCtrl
    {
        LSCAN_DISPLAY_COMMON_CTRL_ERASE, /// erase control area
        LSCAN_DISPLAY_COMMON_CTRL_YELLOW_OK, /// yellow OK button
        LSCAN_DISPLAY_COMMON_CTRL_YELLOW_OVERRIDE, /// yellow override button
        LSCAN_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST, /// yellow contrast optimization button
        LSCAN_DISPLAY_COMMON_CTRL_YELLOW_REPEAT, /// yellow repeat button
        LSCAN_DISPLAY_COMMON_CTRL_GREEN_OK, /// green OK button
        LSCAN_DISPLAY_COMMON_CTRL_GREEN_OVERRIDE, /// green override button
        LSCAN_DISPLAY_COMMON_CTRL_GREEN_CONTRAST, /// green contrast optimization button
        LSCAN_DISPLAY_COMMON_CTRL_GREEN_REPEAT, /// green repeat button
        LSCAN_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED /// do not modify item
    }

    /// Display colors for finger / hand objects.
    /// @anchor DisplayObjectColor
    public enum enumLScanDisplayObjectColor
    {
        LSCAN_DISPLAY_OBJECT_MISSING, ///< object missing (background color)
        LSCAN_DISPLAY_OBJECT_CURRENT_SELECTION, ///< current selection (cursor, yellow)
        LSCAN_DISPLAY_OBJECT_ACTIVE, ///< object is active (light blue)
        LSCAN_DISPLAY_OBJECT_RESTRICTED, ///< object has restrictions (magenta)
        LSCAN_DISPLAY_OBJECT_INACTIVE, ///< object is inactive (dark blue)
        LSCAN_DISPLAY_OBJECT_AUTOCAPTURE_OK, ///< object is OK (AutoCapture, green)
        LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED ///< do not modify item
    }

    /// Display state codes for top state display.
    /// @anchor DisplayStatTop
    public enum enumLScanDisplayStatTop
    {
        LSCAN_DISPLAY_STAT_TOP_ERASE, ///< erase top state area
        LSCAN_DISPLAY_STAT_TOP_ROLL_HORIZONTAL, ///< horizontal finger rolling state
        LSCAN_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_LEFT, ///< horizontal finger rolling state (right to left)
        LSCAN_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_RIGHT, ///< horizontal finger rolling state (left to right)
        LSCAN_DISPLAY_STAT_TOP_CAPTURE_FLAT, ///< flat capture state (palm, tenprint, flat fingers)
        LSCAN_DISPLAY_STAT_TOP_LEAVE_UNCHANGED ///< do not modify item
    }

    /// Display state codes for bottom state display.
    /// @anchor DisplayStatBottom
    public enum enumLScanDisplayStatBottom
    {
        LSCAN_DISPLAY_STAT_BOTTOM_ERASE, ///< erase top state area
        LSCAN_DISPLAY_STAT_BOTTOM_OK, ///< OK, no capture error (green)
        LSCAN_DISPLAY_STAT_BOTTOM_OK_ALT_1, ///< OK, no capture error (alternate symbol #1, blue)
        LSCAN_DISPLAY_STAT_BOTTOM_CLEAN_SURFACE, ///< clean scanner surface warning (red)
        LSCAN_DISPLAY_STAT_BOTTOM_CLEAN_SURFACE_ALT_1, ///< clean scanner surface warning (alternate symbol #1, blue)
        LSCAN_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY, ///< scanner surface is dirty
        LSCAN_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY_ALT_1, ///< scanner surface is dirty (alternate symbol #1, blue)
        LSCAN_DISPLAY_STAT_BOTTOM_COMMON_ERROR, ///< common error
        LSCAN_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR, ///< capture error
        LSCAN_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR_ALT_1, ///< capture error (alternate symbol #1)
        LSCAN_DISPLAY_STAT_BOTTOM_QUALITY_CHECK_ERROR, ///< quality check error
        LSCAN_DISPLAY_STAT_BOTTOM_ROLL_ERROR, ///< rolling error
        LSCAN_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_1, ///< rolling error (alternate symbol #1)
        LSCAN_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_2, ///< rolling error (alternate symbol #2)
        LSCAN_DISPLAY_STAT_BOTTOM_ROLL_ERROR_ALT_3, ///< rolling error (alternate symbol #3)
        LSCAN_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR, ///< sequence check error
        LSCAN_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR_ALT_1, ///< sequence check error (alternate symbol #1)
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_UP, ///< object placed too low, move up
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_UP_RIGHT, ///< object placed too low + left, move up + right
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_RIGHT, ///< object placed too left, move right
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_DOWN_RIGHT, ///< object placed too high + left, move down + right
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_DOWN, ///< object placed too high, move down
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT, ///< object placed too high + right, move down + left
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_LEFT, ///< object placed too right, move left
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_UP_LEFT, ///< object placed too low + right, move up + left
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT_RIGHT_UP, ///< object placed too high + left + right, move down + center it
        LSCAN_DISPLAY_STAT_BOTTOM_POSITION_LEFT_RIGHT, ///< object placed too left + right, center it
        LSCAN_DISPLAY_STAT_BOTTOM_HOURGLASS_STATIC, ///< static hourglass
        LSCAN_DISPLAY_STAT_BOTTOM_HOURGLASS_ANIMATED, ///< animated hourglass
        LSCAN_DISPLAY_STAT_BOTTOM_CAPTURING_ANIMATED, ///< animated capturing
        LSCAN_DISPLAY_STAT_BOTTOM_ROLLING_LEFT_ANIMATED, ///< animated rolling into left direction
        LSCAN_DISPLAY_STAT_BOTTOM_ROLLING_RIGHT_ANIMATED, ///< animated rolling into right direction
        LSCAN_DISPLAY_STAT_BOTTOM_LEAVE_UNCHANGED, ///< do not modify item
        LSCAN_DISPLAY_STAT_BOTTOM_COMPRESSION_ERROR, ///< compression error
        LSCAN_DISPLAY_STAT_BOTTOM_SEGMENTATION_ERROR ///< segmentation error

    }
}
