/// @brief  Definitions and functions of the TOUCHLAB SDK's touch-display handling API.
/// @file   TouchDisplayApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "TouchLabApi.h"
#include "ImageApi.h"

#ifdef DLL_TOUCHLAB_EXPORTS
    #define TOUCH_DISPLAY_API __declspec(dllexport)       ///< exports DLL symbols
#else
#define TOUCH_DISPLAY_API __declspec(dllimport)       ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif 

/// A user-defined function to be called after a button area was pressed.
///
/// @param context  a user-defined context as given on callback registration
typedef void( *TL_CallbackButtonTouched )(void* context);

/// Sets the background image in the area of the touch display ( the upper part of the scanner's display ).
/// The image file's dimensions must be 480 x 190 pixels.
/// Supported image file formats are bmp, png, jpeg, jpg.
/// It also sets the bottom area to black. This behavior is subject to change and will be addressed in the future by a separate display API.
///
/// @param scanner  a valid handle to a scanner
/// @param path     file path to an image file
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b path was given
/// @return @ref TL_INVALID_ARGUMENT if the image file's dimensions differs from the touch-displays native size of 480 x 190.
/// @return @ref TL_LOGIC if TL_Initialize() not called
TOUCH_DISPLAY_API bool TL_TouchDisplay_SetBackgroundImage( const TL_ScannerHandle scanner, const char* path );

/// Registers a rectangular area witch reacts as a touch button. When touched it calls the given callback function.
/// This function can be called multiple times to register a couple of buttons. Currently the number of buttons is unlimited.
/// The rectangle must be within the display touch area (max: width <= 480, height <= 190)
///
/// @param scanner  a valid handle to a scanner
/// @param x, y, width, height defines the rectangle of the button
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  a user-defined context (will be provided to callback function)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if x < 0
/// @return @ref TL_INVALID_ARGUMENT if y < 0
/// @return @ref TL_INVALID_ARGUMENT if x + width > 480
/// @return @ref TL_INVALID_ARGUMENT if y + height > 190
/// @return @ref TL_INVALID_ARGUMENT if button's rect intersects with the rect of another button
/// @return @ref TL_INVALID_ARGUMENT if the button's rect intersects with any other button rect or signature rect
/// @return @ref TL_LOGIC if TL_Initialize() not called
TOUCH_DISPLAY_API bool TL_TouchDisplay_RegisterButton( const TL_ScannerHandle scanner, int x, int y, int width, int height, TL_CallbackButtonTouched callback, void * context );

/// Starts to asynchronously monitoring the buttons input
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC in case of button monitoring is in progress
/// @return @ref TL_LOGIC if TL_Initialize() not called
TOUCH_DISPLAY_API bool TL_TouchDisplay_StartButtonMonitoring( const TL_ScannerHandle scanner );

/// Stops monitoring the buttons input
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
TOUCH_DISPLAY_API bool TL_TouchDisplay_StopButtonMonitoring( const TL_ScannerHandle scanner );

/// Unregisters all buttons at once
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
TOUCH_DISPLAY_API bool TL_TouchDisplay_UnregisterButtons( const TL_ScannerHandle scanner );


#ifdef __cplusplus
}
#endif 
