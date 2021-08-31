/// @brief  Definitions and functions of the TOUCHLAB SDK's scanner handling API.
/// @file   ScannerApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "TouchLabApi.h"
#include "ImageApi.h"

#ifdef DLL_TOUCHLAB_EXPORTS
    #define SCANNER_API __declspec(dllexport)       ///< exports DLL symbols
#else
    #define SCANNER_API __declspec(dllimport)       ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif 
    
/// Supported scan modes.
typedef enum
{
    TL_PLAIN_4_FINGERS_RIGHT,               ///< Plain right fingers. Typical number of fingers in result image: 4
    TL_PLAIN_4_FINGERS_LEFT,                ///< Plain left fingers. Typical number of fingers in result image: 4

    TL_PLAIN_2_THUMBS,                      ///< Both plain thumbs. Typical number of fingers in result image: 2
    TL_PLAIN_1_THUMB_RIGHT,                 ///< Plain right thumb. Typical number of fingers in result image: 1
    TL_PLAIN_1_THUMB_LEFT,                  ///< Plain left thumb. Typical number of fingers in result image: 1

    TL_PLAIN_2_FINGERS_INDEX,               ///< Both plain index fingers. Typical number of fingers in result image: 2

    TL_PLAIN_1_INDEX_FINGER_RIGHT,          ///< Plain right index finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_MIDDLE_FINGER_RIGHT,         ///< Plain right middle finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_RING_FINGER_RIGHT,           ///< Plain right ring finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_LITTLE_FINGER_RIGHT,         ///< Plain right little finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_INDEX_FINGER_LEFT,           ///< Plain left index finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_MIDDLE_FINGER_LEFT,          ///< Plain left middle finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_RING_FINGER_LEFT,            ///< Plain left ring finger. Typical number of fingers in result image: 1
    TL_PLAIN_1_LITTLE_FINGER_LEFT,          ///< Plain left little finger. Typical number of fingers in result image: 1
    

    TL_PLAIN_2_FINGERS_RIGHT_INDEX_MIDDLE,  ///< Plain right index and middle fingers. Typical number of fingers in result image: 2
    TL_PLAIN_2_FINGERS_RIGHT_RING_LITTLE,   ///< Plain right ring and little fingers. Typical number of fingers in result image: 2

    TL_PLAIN_2_FINGERS_LEFT_INDEX_MIDDLE,   ///< Plain left index and middle fingers. Typical number of fingers in result image: 2
    TL_PLAIN_2_FINGERS_LEFT_RING_LITTLE,    ///< Plain left ring and little fingers. Typical number of fingers in result image: 2

    TL_ROLLED_1_THUMB_RIGHT,                ///< Rolled right thumb. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_INDEX_FINGER_RIGHT,         ///< Rolled right index finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_MIDDLE_FINGER_RIGHT,        ///< Rolled right middle finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_RING_FINGER_RIGHT,          ///< Rolled right ring finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_LITTLE_FINGER_RIGHT,        ///< Rolled right little finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_THUMB_LEFT,                 ///< Rolled left thumb. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_INDEX_FINGER_LEFT,          ///< Rolled left index finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_MIDDLE_FINGER_LEFT,         ///< Rolled left middle finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_RING_FINGER_LEFT,           ///< Rolled left ring finger. Mandatory number of fingers in result image: 1
    TL_ROLLED_1_LITTLE_FINGER_LEFT,         ///< Rolled left little finger. Mandatory number of fingers in result image: 1
} TL_ScanMode;


/// Finger positions.
typedef enum
{
    TL_LEFT_UNKNOWN_FINGER  = 0x001,  ///< finger position of an unknown finger at the left hand
    TL_LEFT_THUMB           = 0x002,  ///< finger position of the left thumb
    TL_LEFT_INDEX_FINGER    = 0x004,  ///< finger position of the left index finger
    TL_LEFT_MIDDLE_FINGER   = 0x008,  ///< finger position of the left middle finger
    TL_LEFT_RING_FINGER     = 0x010,  ///< finger position of the left ring finger
    TL_LEFT_LITTLE_FINGER   = 0x020,  ///< finger position of the left little finger

    TL_RIGHT_UNKNOWN_FINGER = 0x040,  ///< finger position of an unknown finger at the right hand
    TL_RIGHT_THUMB          = 0x080,  ///< finger position of the right thumb
    TL_RIGHT_INDEX_FINGER   = 0x100,  ///< finger position of the right index finger
    TL_RIGHT_MIDDLE_FINGER  = 0x200,  ///< finger position of the right middle finger
    TL_RIGHT_RING_FINGER    = 0x400,  ///< finger position of the right ring finger
    TL_RIGHT_LITTLE_FINGER  = 0x800,  ///< finger position of the right little finger
} TL_FingerPositions;


/// Possible image content analyses (e.g. can be used to disable certain image content analyses @ref TL_Scanner_StartAutoScan).
typedef enum 
{
    TL_ANALYSIS_NONE                = 0x000,     ///< no image content analyses
    TL_ANALYSIS_ALL                 = 0x001,     ///< all image content analyses

    TL_ANALYSIS_TIP                 = 0x002,     ///< fingertip analysis
    TL_ANALYSIS_HAND_SIDE           = 0x004,     ///< hand side analysis
    TL_ANALYSIS_POSITION            = 0x008,     ///< position analysis
    TL_ANALYSIS_ROTATION            = 0x010,     ///< rotation analysis
    TL_ANALYSIS_SPREADING           = 0x020,     ///< finger spreading analysis
    TL_ANALYSIS_ROLL_DURATION       = 0x040,     ///< roll duration analysis
    TL_ANALYSIS_PRESENTATION_ATTACK = 0x080,     ///< presentation attack analysis
    TL_ANALYSIS_ROLL_DISTANCE       = 0x100,     ///< roll distance analysis
} TL_ImageContentAnalysis;
 
/// Set of fingers.
typedef struct
{
    bool TL_LEFT_UNKNOWN_FINGER;    ///< finger position of an unknown finger at the left hand in set of fingers or not
    bool TL_LEFT_THUMB;             ///< finger position of the left thumb in set of fingers or not
    bool TL_LEFT_INDEX_FINGER;      ///< finger position of the left index finger in set of fingers or not
    bool TL_LEFT_MIDDLE_FINGER;     ///< finger position of the left middle finger in set of fingers or not
    bool TL_LEFT_RING_FINGER;       ///< finger position of the left ring finger in set of fingers or not
    bool TL_LEFT_LITTLE_FINGER;     ///< finger position of the left little finger in set of fingers or not

    bool TL_RIGHT_UNKNOWN_FINGER;   ///< finger position of an unknown finger at the right hand in set of fingers or not
    bool TL_RIGHT_THUMB;            ///< finger position of the right thumb in set of fingers or not
    bool TL_RIGHT_INDEX_FINGER;     ///< finger position of the right index finger in set of fingers or not
    bool TL_RIGHT_MIDDLE_FINGER;    ///< finger position of the right middle finger in set of fingers or not
    bool TL_RIGHT_RING_FINGER;      ///< finger position of the right ring finger in set of fingers or not
    bool TL_RIGHT_LITTLE_FINGER;    ///< finger position of the right little finger in set of fingers or not
} TL_FingersSet;

/// Set of capabilities for one scanner.
typedef struct
{
    bool TL_SUPPORTS_DISPLAY;               ///< if scanner supports display output, then true, otherwise false
    bool TL_SUPPORTS_TOUCHSCREEN;           ///< if scanner supports touch screen functionality, then true, otherwise false
    bool TL_SUPPORTS_BEEP;                  ///< if scanner supports to do beep, then true, otherwise false
    bool TL_SUPPORTS_ROLLING;               ///< if scanner physically supports rolling scan modes, then true, otherwise false
} TL_Scanner_Capabilities;

/// Set of capabilities for a scan mode of a given scanner (as returned by TL_Scanner_GetModeCapabilities()).
typedef struct
{
    bool TL_IS_MODE_SUPPORTED;           ///< @b true if scanner supports the given scan mode, otherwise @b false
    bool TL_SUPPORTS_AUTO_SCAN;          ///< @b true if given scan mode of scanner does support the automatic triggering of scan functionality, otherwise @b false
    bool TL_SUPPORTS_MANUAL_SCAN;        ///< @b true if given scan mode of scanner does support the manual triggering of scan functionality, otherwise @b false
} TL_Scanner_ModeCapabilities;


/// Supported workflow types.
typedef enum
{
    TL_WORKFLOW_442,        ///< 3 images scanned in following order: @ref TL_PLAIN_4_FINGERS_RIGHT, @ref TL_PLAIN_4_FINGERS_LEFT, @ref TL_PLAIN_2_THUMBS
    TL_WORKFLOW_4141,       ///< 4 images scanned in following order: @ref TL_PLAIN_4_FINGERS_RIGHT, @ref TL_PLAIN_1_THUMB_RIGHT, @ref TL_PLAIN_4_FINGERS_LEFT, @ref TL_PLAIN_1_THUMB_LEFT
    TL_WORKFLOW_SIGNATURE,  ///< 1 image of a signature
} TL_WorkflowTypes;

/// A user-defined function to be called after a display animation is finished.
///
/// @param context  a user-defined context as given on callback registration
/// @param wasAborted  informs if the animation was aborted by starting another animation or a capture call
typedef void( *TL_CallbackAnimationFinished )( void *context, bool wasAborted );

/// Validates a given scanner handle.
///
/// A scanner handle is valid only when successfully returned from TL_OpenScanner() and before TL_CloseScanner() was called.
///
/// @param scanner      a scanner handle to be validated
/// @param[out] valid   a pointer receiving the validation result (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b valid parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_IsValid( const TL_ScannerHandle scanner, bool* valid );

/// Gets the name (type) of a given scanner.
///
/// @param scanner      a valid handle to a scanner
/// @param[out] name    a buffer receiving the name (must not be null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b name parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_GetName( const TL_ScannerHandle scanner, char* name );

/// Gets the serial number of a given scanner.
///
/// @param scanner      a valid handle to a scanner
/// @param[out] serial  a buffer receiving the serial number (must not be null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b serial parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_GetSerialNumber( const TL_ScannerHandle scanner, char* serial );

/// Gets the firmware version of a given scanner.
///
/// @param scanner      a valid handle to a scanner
/// @param[out] version a buffer receiving the firmware version (must not be null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b version parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_GetFirmwareVersion( const TL_ScannerHandle scanner, char* version );

/// Gets the current contrast control value of a given scanner. 
/// TL_Scanner_StartPreview() must called before, because each ScanMode has its own contrast control value.
///
/// @param scanner            a valid handle to a scanner
/// @param[out] value         a pointer receiving the current value (must not be null)
/// @param[out] minimum       a pointer receiving the minimum value (ignored if null)
/// @param[out] maximum       a pointer receiving the maximum value (ignored if null)
/// @param[out] defaultValue  a pointer receiving the default value (ignored if null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b value parameter
/// @return @ref TL_LOGIC if TL_Initialize(), TL_Scanner_StartPreview() not called
SCANNER_API bool TL_Scanner_GetContrastControl( const TL_ScannerHandle scanner, int* value, int* minimum, int* maximum, int* defaultValue);

/// Sets the contrast control value of a given scanner.
/// Change this value to enhance the contrast in a captured image. TL_Scanner_StartPreview() must called before, because each ScanMode has its own contrast control value.
///
/// @param scanner      a valid handle to a scanner
/// @param[in]          value
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if given @b value parameter is not in the range returned by TL_Scanner_GetContrastControl()
/// @return @ref TL_LOGIC if TL_Initialize(), TL_Scanner_StartPreview() not called
SCANNER_API bool TL_Scanner_SetContrastControl( const TL_ScannerHandle scanner, const int value );

/// Registers a user function to be called when a preview image was captured. 
/// For each scanner, only one function can be registered at a given moment in time.
/// @note Do not call while capturing preview images!
/// @note Also, the callback will be called from a different thread, so only perform thread-safe operations in the callback
///
/// @param scanner  a valid handle to a scanner
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  a user-defined context (will be provided to callback function)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC while capturing preview images
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_RegisterCallbackPreviewImage(const TL_ScannerHandle scanner, TL_CallbackImageAvailable callback, void* context);

/// Registers a user function to be called when the final scan image was captured. 
/// For each each scanner, only 1 function can be registered at a given moment in time.
/// @note Do not call while capturing the scan image!
/// @note Also, the callback will be called from a different thread, so only perform thread-safe operations in the callback
///
/// @param scanner  a valid handle to a scanner
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  a user-defined context (will be provided to callback function)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC while capturing the scan image
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// void tClient::onImageScanned(void* context, TL_GrayscaleImageHandle image, TL_ImageContentFeedback feedback, TL_EvaluationHandle eval)
/// {
///     tClient * that = static_cast<tClient*>(context);
///
///     // Careful .. we're not in main thread now so don't do anything that should only be done in main thread, eg gui manipulation
///     // Copy the image data and schedule the actual display for when the main (gui) thread resumes
///     QImage Img = makeQImage(image);
///     that->emitSigImgReady(Img);  
///
///     qDebug() << " onImageScanned ... " << imgCntFbkStr(feedback);
/// }
/// //----------------------------------------------
/// bool tClient::regOnImageScanned(TL_ScannerHandle h)
/// {
///     void * context = this;
///     bool ret = TL_Scanner_RegisterCallbackScanAvailableImage(h, tClient::onImageScanned, context);
///     if(!ret)
///         checkError("regOnImageScanned()");
///     return ret;
///  }
/// @endcode
SCANNER_API bool TL_Scanner_RegisterCallbackScanImageAvailable(const TL_ScannerHandle scanner, TL_CallbackScanImageAvailable callback, void* context);

/// Starts to asynchronously capture images which may help to preview before scanning the final image.
/// Each preview image is delivered to a user-defined callback (if earlier registered with TL_Scanner_RegisterCallbackPreviewImage()).
/// Call TL_Scanner_AbortPreview() to stop.
/// @note Requires both TL_Scanner_ModeCapabilities::TL_IS_MODE_SUPPORTED and TL_Scanner_ModeCapabilities::TL_SUPPORTS_MANUAL_SCAN to be @b true!
///
/// @param scanner  a valid handle to a scanner
/// @param mode     scan mode
/// @param requiredFingers  a bit field of required fingers. Finger positions are defined in @ref TL_FingerPositions.
/// @param contentAnalysesToDisable specifies the image content analyses to disable. See  @ref TL_ImageContentAnalysis. Don't combine  @ref TL_ANALYSIS_NONE or @ref TL_ANALYSIS_ALL with any others.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_RUNTIME if unsupported @b mode given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if scanner is already capturing preview images
/// @return @ref TL_LOGIC if manual triggering not supported by given @b mode
/// @return @ref TL_LOGIC if TL_Initialize() not called
/// @note Although the function returns immediately, the first preview-callback will be signaled after the display-animation is finished
///
/// @code 
/// TL_ScannerHandle scanner;
/// ...
/// int requiredLeftFingers = TL_LEFT_INDEX_FINGER | TL_LEFT_MIDDLE_FINGER | TL_LEFT_RING_FINGER | TL_LEFT_LITTLE_FINGER;
/// TL_Scanner_StartPreview( scanner, TL_PLAIN_4_FINGERS_LEFT, requiredLeftFingers );
/// @endcode
SCANNER_API bool TL_Scanner_StartPreview(const TL_ScannerHandle scanner, const TL_ScanMode mode, const int requiredFingers, const int contentAnalysesToDisable);

/// Stops capturing preview images (if started with TL_Scanner_StartPreview()), and autoScan (if started with TL_Scanner_StartAutoScan()).
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_AbortPreview( const TL_ScannerHandle scanner );

/// Scans the final image and blocks the calling thread until finished.
/// The scanned image is delivered to a user-defined callback (if earlier registered with TL_Scanner_RegisterCallbackScanImageAvailable()).
/// @note Do not call while capturing preview images!
/// @note Requires both TL_Scanner_ModeCapabilities::TL_IS_MODE_SUPPORTED and TL_Scanner_ModeCapabilities::TL_SUPPORTS_MANUAL_SCAN to be @b true!
///
/// @param scanner  a valid handle to a scanner
/// @param mode     scan mode
/// @param requiredFingers  a bit field of required fingers. Finger positions are defined in @ref TL_FingerPositions.
/// @param contentAnalysesToDisable specifies the image content analyses to disable. See  @ref TL_ImageContentAnalysis. Don't combine  @ref TL_ANALYSIS_NONE or @ref TL_ANALYSIS_ALL with any others.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_RUNTIME if unsupported @b mode given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if a required license (e.g. for presentation attack detection, ...) is missing
/// @return @ref TL_LOGIC while capturing preview images
/// @return @ref TL_LOGIC if manual triggering not supported by given @b mode
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// bool tClient::doScan()
/// {
///     TL_ScannerHandle Dev = myDev();
///     if (!devOk(Dev))
///         return false;
///
///     int requiredFingers = TL_LEFT_INDEX_FINGER | TL_LEFT_MIDDLE_FINGER | TL_LEFT_RING_FINGER | TL_LEFT_LITTLE_FINGER;
///     TL_ScanMode mode = TL_PLAIN_4_FINGERS_LEFT;
///     bool ret = TL_Scanner_StartScan(Dev, mode, requiredFingers);
///     if (!ret)
///         checkError("TL_Scanner_StartScan()");
///     return ret;
/// }
/// @endcode
SCANNER_API bool TL_Scanner_StartScan(const TL_ScannerHandle scanner, const TL_ScanMode mode, const int requiredFingers, const int contentAnalysesToDisable);

/// Starts to asynchronously capture images with preview. Delivers the final image when image quality and requirements of the given mode are sufficient.
/// Each preview image is delivered to a user-defined callback (if earlier registered with TL_Scanner_RegisterCallbackPreviewImage()).
/// The final scan image is delivered to a user-defined callback (if earlier registered with TL_Scanner_RegisterCallbackScanImageAvailable()).
/// @note These callbacks will be called from a different thread, so only perform thread-safe operations in the callbacks
/// @note Requires both TL_Scanner_ModeCapabilities::TL_IS_MODE_SUPPORTED and TL_Scanner_ModeCapabilities::TL_SUPPORTS_AUTO_SCAN to be @b true!
/// @note Do not call while capturing preview images!
///
/// @param scanner  a valid handle to a scanner
/// @param mode     scan mode
/// @param requiredFingers  a bit field of required fingers. Finger positions are defined in @ref TL_FingerPositions.
/// @param contentAnalysesToDisable specifies the image content analyses to disable. See  @ref TL_ImageContentAnalysis. Don't combine  @ref TL_ANALYSIS_NONE or @ref TL_ANALYSIS_ALL with any others.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_RUNTIME if unsupported @b mode given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if a required license (e.g. for presentation attack detection, rolled prints, ...) is missing
/// @return @ref TL_LOGIC while capturing preview images
/// @return @ref TL_LOGIC if auto triggering not supported by given @b mode
/// @return @ref TL_LOGIC if TL_Initialize() not called 
/// @note Although the function returns immediately, the first preview-callback will be signaled after the display-animation is finished
/// 
/// @code
/// TL_ScannerHandle scanner;
/// ...
/// int requiredLeftFingers = TL_LEFT_INDEX_FINGER | TL_LEFT_MIDDLE_FINGER | TL_LEFT_RING_FINGER | TL_LEFT_LITTLE_FINGER;
/// int contentAnalysesToDisable = TL_ANALYSIS_NONE;
/// TL_Scanner_StartAutoScan( scanner, TL_PLAIN_4_FINGERS_LEFT, requiredLeftFingers, contentAnalysesToDisable );
/// @endcode
SCANNER_API bool TL_Scanner_StartAutoScan( const TL_ScannerHandle scanner, const TL_ScanMode mode, const int requiredFingers, const int contentAnalysesToDisable );

/// Shows an initial animation / image on the scanner's display corresponding to a given workflow type
///
/// @param scanner  a valid handle to a scanner
/// @param type     workflow type
/// @param callback a callback to get notified about the finished animation
/// @param context  a user-defined context (will be provided to callback function)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_RUNTIME if unsupported @b mode given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_ShowStartScreen( const TL_ScannerHandle scanner, const TL_WorkflowTypes type, TL_CallbackAnimationFinished callback, void* context );

/// Shows an end-animation / image on the scanner's display corresponding to a given workflow type
///
/// @param scanner  a valid handle to a scanner
/// @param type     workflow type
/// @param callback a callback to get notified about the finished animation
/// @param context  a user-defined context (will be provided to callback function)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_RUNTIME if unsupported @b mode given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_ShowEndScreen( const TL_ScannerHandle scanner, const TL_WorkflowTypes type, TL_CallbackAnimationFinished callback, void* context );

/// Let the scanner beep
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_Beep( const TL_ScannerHandle scanner );

/// Retrieves the finger sets for required and allowed fingers according to a given scan mode.
///
/// @param scanMode specifies the scan mode for which the finger sets should be retrieved.
/// @param requiredFingers  contains result information about the required fingers for the given scan mode.
/// @param requiredFingersCountMin  contains result information about the minimum count of fingers required for the given scan mode.
/// @param requiredFingersCountMax  contains result information about the maximum count of fingers required for the given scan mode.
/// @param allowedFingers   contains result information about the allowed fingers for the given scan mode. Required fingers is always a subset of the allowed ones.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b requiredFingers parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b requiredFingersCountMin parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b requiredFingersCountMax parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b allowedFingers parameter
/// @return @ref TL_INVALID_ARGUMENT if an unsupported value was given as @b scanMode parameter
SCANNER_API bool TL_Scanner_GetFingerSetsOfScanMode(const TL_ScanMode scanMode, TL_FingersSet* requiredFingers, int* requiredFingersCountMin, int* requiredFingersCountMax, TL_FingersSet* allowedFingers);

/// Retrieves a set of capabilities for the given scanner. Use this to get information about the scanner capabilities like display or touch support.
///
/// @param scanner  a valid handle to a scanner
/// @param capabilities  contains result information about the capabilities. See struct @ref TL_Scanner_Capabilities for details.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b capabilities parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_GetCapabilities(const TL_ScannerHandle scanner, TL_Scanner_Capabilities* capabilities);

/// Retrieves a set of capabilities for a scan mode of a given scanner.
/// Use this to get information whether to call TL_Scanner_StartPreview(), TL_Scanner_StartScan() or TL_Scanner_StartAutoScan() for scanning.
///
/// @param scanner  a valid handle to a scanner
/// @param mode     scan mode
/// @param[out] capabilities  contains result information about the capabilities
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_RUNTIME if unknown @b mode value was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b capabilities parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
SCANNER_API bool TL_Scanner_GetModeCapabilities( const TL_ScannerHandle scanner, const TL_ScanMode mode, TL_Scanner_ModeCapabilities* capabilities );

/// Sets the brightness of the scan area.
///
/// @param scanner a valid handle to a scanner.
/// @param value the brightness value {0, 100}.
/// 
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT @b scanner handle is invalid
/// @return @ref TL_INVALID_ARGUMENT @b value is not in {0, 100}
/// @return @ref TL_LOGIC TL_Initialize() is not called
/// @return @ref TL_LOGIC the operation is not supported
/// @return @ref TL_LOGIC the scanner is capturing
SCANNER_API bool TL_Scanner_SetScanAreaBrightness(const TL_ScannerHandle scanner, int value);

/// Gets the brightness of the scan area.
///
/// @param scanner a valid handle to a scanner.
/// @param[out] value the brightness value {0, 100}.
/// 
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT @b scanner handle is invalid
/// @return @ref TL_INVALID_ARGUMENT @b value is null
/// @return @ref TL_LOGIC TL_Initialize() is not called
/// @return @ref TL_LOGIC the operation is not supported
SCANNER_API bool TL_Scanner_GetScanAreaBrightness(const TL_ScannerHandle scanner, int* value);

#ifdef __cplusplus
}
#endif 
