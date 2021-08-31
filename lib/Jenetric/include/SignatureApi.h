/// @brief  Definitions and functions of the TOUCHLAB SDK's signature handling API. 
/// **The signature API requires a license bound on a scanner's serial number.** The license will be checked with each call of TL_Signature_StartRecord().
/// Licensing is explained here: @ref main_licensing "Licensing".
/// @file   SignatureApi.h
///
/// @copyright  (C) JENETRIC GmbH


#pragma once

#include "TouchLabApi.h"
#include "ImageApi.h"

#ifdef DLL_TOUCHLAB_EXPORTS
    #define SIGNATURE_API __declspec(dllexport)       ///< exports DLL symbols
#else
    #define SIGNATURE_API __declspec(dllimport)       ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif 

/// Represents the type of a touch action.
typedef int TL_TouchAction;
/// Specifies the identifiers of @ref TL_TouchAction.
enum
{
    TL_TOUCH_ACTION_NONE = 0,   ///< Represents no action.
    TL_TOUCH_ACTION_DOWN = 1,   ///< Represents the touch down action.
    TL_TOUCH_ACTION_UP   = 2    ///< Represents the touch up action.
};

/// A user-defined function to be called after capturing a preview signature scan image.
///
/// @param context    a user-defined context as given on callback registration
/// @param image      a valid handle to current preview image, gets invalid when leaving the callback function
/// @param action     the touch action.
/// @param x          the touch x coordinate relative to the signature rectangle.
/// @param y          the touch y coordinate relative to the signature rectangle.
/// @param time       the touch time [ms].
typedef void(*TL_CallbackSignaturePreviewAvailable)(void* context, TL_ImageHandle image, TL_TouchAction action, float x, float y, unsigned int time);

/// A user-defined function to be called after capturing a final signature scan image.
///
/// @param context    a user-defined context as given on callback registration
/// @param image      a valid handle to current preview/scan image, gets invalid when leaving the callback function
typedef void(*TL_CallbackSignatureScanAvailable)(void* context, TL_ImageHandle image);

/// Registers a user function to be called when a signature preview image was captured. 
/// For each scanner, only one function can be registered at a given moment in time.
///
/// @param scanner  a valid handle to a scanner
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  a user-defined context (will be provided to callback function)
/// @param imageWidth, imageHeight defines the size of the preview image
/// @param penWidth defines the size of the preview image
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if imageWidth <= 0
/// @return @ref TL_INVALID_ARGUMENT if imageHeight <= 0
/// @return @ref TL_INVALID_ARGUMENT if penWidth <= 0
/// @return @ref TL_LOGIC while capturing preview images
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_RegisterCallbackPreviewImage(const TL_ScannerHandle scanner, TL_CallbackSignaturePreviewAvailable callback, void* context, const int imageWidth, const int imageHeight, const int penWidth);

/// Registers a user function to be called when after the scan stopped. 
/// For each scanner, only one function can be registered at a given moment in time.
///
/// @param scanner  a valid handle to a scanner
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  a user-defined context (will be provided to callback function)
/// @param imageWidth, imageHeight defines the size of the preview image
/// @param penWidth defines the size of the preview image
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if imageWidth <= 0
/// @return @ref TL_INVALID_ARGUMENT if imageHeight <= 0
/// @return @ref TL_INVALID_ARGUMENT if penWidth <= 0
/// @return @ref TL_LOGIC while capturing preview images
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_RegisterCallbackScanImage(const TL_ScannerHandle scanner, TL_CallbackSignatureScanAvailable callback, void* context, const int imageWidth, const int imageHeight, const int penWidth);

/// Starts to asynchronously record signature input
/// preview images are delivered to a user-defined callback (if earlier registered with TL_Signature_RegisterCallbackPreviewImage()).
/// Call TL_Signature_StopRecord() to stop.
///
/// This function checks for a valid license. Licensing is explained here: @ref main_licensing "Licensing".
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if there is no valid license for the signature feature (matching with the connected scanner) available
/// @return @ref TL_LOGIC if scanner is already signature scanning
/// @return @ref TL_LOGIC in case of button monitoring is in progress and the signature rect intersects with a button
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_StartRecord(const TL_ScannerHandle scanner);

/// Stops recording signature input and calls the user-defined scan callback once (if earlier registered with TL_Signature_RegisterCallbackScanImage()).
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_StopRecord(const TL_ScannerHandle scanner);

/// Sets a rectangular area in display coordinates where signatures will be captured
///
/// @param scanner  a valid handle to a scanner
/// @param x        x start position of rectangle
/// @param y        y start position of rectangle
/// @param width    width of rectangle
/// @param height   height of rectangle
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_SYSTEM on low-level device communication errors
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if x < 0
/// @return @ref TL_INVALID_ARGUMENT if y < 0
/// @return @ref TL_INVALID_ARGUMENT if width <= 0
/// @return @ref TL_INVALID_ARGUMENT if height <= 0
/// @return @ref TL_INVALID_ARGUMENT if x + width > 480
/// @return @ref TL_INVALID_ARGUMENT if y + height > 190
/// @return @ref TL_LOGIC in case of button monitoring is in progress and the signature rect intersects with a button
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_SetSignRect(const TL_ScannerHandle scanner, int x, int y, int width, int height);

/// Clears the recorded signature data and clears the preview image
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
SIGNATURE_API bool TL_Signature_ResetRecord(const TL_ScannerHandle scanner);


#ifdef __cplusplus
}
#endif 
