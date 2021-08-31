/// @brief  Definitions and functions of the TOUCHLAB SDK's image handling API.
/// @file   ImageApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "EvaluationApi.h"


#ifdef DLL_TOUCHLAB_EXPORTS
    #define IMAGE_API __declspec(dllexport)         ///< exports DLL symbols
#else
    #define IMAGE_API __declspec(dllimport)         ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif

/// Handle to a gray-scale image which allocates 1 byte per pixel. 
typedef int TL_GrayscaleImageHandle;

/// Handle to a colored image which allocates 4 byte per pixel. 
typedef int TL_ImageHandle;

/// Feedback values describing the content of a captured image
enum TL_ImageContentFeedback
{
    TL_CONTENT_VALID                    =  0,   ///< expected count of fingers/objects was detected

    TL_ROTATED_HAND_COUNTERCLOCKWISE    =  1,   ///< hand or finger or single thumb was counterclockwise rotated and needs to be rotated clockwise
    TL_ROTATED_HAND_CLOCKWISE           =  2,   ///< hand or finger or single thumb was clockwise rotated and needs to be rotated counterclockwise
    TL_ROTATED_THUMBS                   =  3,   ///< any of the 2 thumbs were rotated and need correction
    TL_ROTATED_TWO_INDEX_FINGERS        = 22,   ///< any of the 2 index fingers were rotated and need correction

    TL_HAND_ON_BORDER_BOTTOM            =  4,   ///< hand was detected on bottom border, hand has to be moved upwards
    TL_HAND_ON_BORDER_TOP               =  5,   ///< hand was detected on top border, hand has to be moved downwards
    TL_HAND_ON_BORDER_RIGHT             =  6,   ///< hand was detected on right border, hand has to be moved leftwards
    TL_HAND_ON_BORDER_LEFT              =  7,   ///< hand was detected on left border, hand has to be moved rightwards

    TL_SPREAD_FINGERS                   =  8,   ///< spread fingers detected
    TL_FINGERTIPS                       =  9,   ///< only fingertips detected
    TL_EXPECTED_HANDSIDE_NOT_FOUND      = 10,   ///< expected hand side was not found

    TL_TOO_MANY_FINGERS                 = 11,   ///< more fingers detected than expected in current scenario
    TL_TOO_MANY_VALID_FINGERS           = 12,   ///< @deprecated No longer analyzed or reported. Typically, @ref TL_TOO_MANY_FINGERS might be reported instead.
    TL_NOT_ENOUGH_FINGERS               = 13,   ///< less valid fingers detected than expected in current scenario 
    TL_NOT_ENOUGH_VALID_FINGERS         = 14,   ///< @deprecated No longer analyzed or reported. Typically, @ref TL_NOT_ENOUGH_FINGERS might be reported instead.
    TL_NO_FINGERS_DETECTED              = 15,   ///< no fingers detected

    TL_UNEXPECTED_CONTENT               = 16,   ///< image content could not be identified correctly - this should not happen

    TL_READY_TO_ROLL                    = 17,   ///< finger detected, but rolling not started
    TL_ROLLING_STARTED                  = 18,   ///< finger has started to roll
    TL_ROLL_DURATION_TOO_SHORT          = 19,   ///< roll duration too short (roll speed too fast?!)
    TL_ROLL_DISTANCE_TOO_SHORT          = 21,   ///< roll distance too short

    TL_PRESENTATION_ATTACK_DETECTED     = 20,   ///< @deprecated No longer reported. Superseded by TL_Evaluation_WasAttackDetected()
};

/// Image format definition, an image can be of one of that format values
typedef enum TL_ImageFormat
{
    TL_IMAGEFORMAT_GRAY8  = 0x001,        ///< defines a gray scale image, one byte per pixel   
    TL_IMAGEFORMAT_ARGB32 = 0x002,        ///< defines a color image with 4 byte per pixel "alpha channel, red, green, blue"  
};

/// A user-defined function to be called after capturing a preview or final scan image.
///
/// @param context    a user-defined context as given on callback registration
/// @param image      a valid handle to current preview/scan image, gets invalid when leaving the callback function
/// @param feedback   feedback about the image content
typedef void(*TL_CallbackImageAvailable)(void *context, TL_GrayscaleImageHandle image, enum TL_ImageContentFeedback feedback);

/// A user-defined function to be called after capturing a final scan image.
///
/// @param context    a user-defined context as given on callback registration
/// @param image      a valid handle to current preview/scan image, gets invalid when leaving the callback function
/// @param feedback   feedback about the image content
/// @param eval       a valid handle to evaluation data of the image
typedef void(*TL_CallbackScanImageAvailable)(void *context, TL_GrayscaleImageHandle image, enum TL_ImageContentFeedback feedback, TL_EvaluationHandle eval);

/// Validates a given image handle.
///
/// @param      image   an image handle to be validated
/// @param[out] valid   a pointer receiving the validation result (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b valid parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_GrayscaleImage_IsValid( const TL_GrayscaleImageHandle image, bool* valid );

/// Gets the width of a given image.
///
/// @param      image   a valid handle to an image
/// @param[out] width   a pointer receiving the image width (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b width parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_GrayscaleImage_GetWidth( const TL_GrayscaleImageHandle image, int* width );

/// Gets the height of a given image.
///
/// @param      image   a valid handle to an image
/// @param[out] height  a pointer receiving the image height (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b height parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_GrayscaleImage_GetHeight( const TL_GrayscaleImageHandle image, int* height );

/// Gets access to the pixel data of a given image.
///
/// Each gray-scale image allocates width*height bytes (1 byte per pixel). 
/// The pixel data is continuously stored from top to bottom row (no fill bytes between successive rows).
/// @note The pixel data can be accessed only as long as its associated image handle is valid.
///
/// @param      image   a valid handle to an image
/// @param[out] data    a pointer receiving a temp. pointer to the image's pixel data (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b data parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_GrayscaleImage_GetData( const TL_GrayscaleImageHandle image, unsigned char** data );

/// Releases the memory of an image with a given image handle.
///
/// @param      image   a valid handle to an image
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_GrayscaleImage_Release( const TL_GrayscaleImageHandle image );

/// Validates a given image handle.
///
/// @param      image   an image handle to be validated
/// @param[out] valid   a pointer receiving the validation result (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b valid parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_IsValid(const TL_ImageHandle image, bool* valid);

/// Gets the width of a given image.
///
/// @param      image   a valid handle to an image
/// @param[out] width   a pointer receiving the image width (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b width parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_GetWidth(const TL_ImageHandle image, int* width);

/// Gets the height of a given image.
///
/// @param      image   a valid handle to an image
/// @param[out] height  a pointer receiving the image height (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b height parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_GetHeight(const TL_ImageHandle image, int* height);

/// Gets access to the pixel data of a given image.
///
/// Each gray-scale image allocates width*height bytes (1 byte per pixel). 
/// The pixel data is continuously stored from top to bottom row (no fill bytes between successive rows).
/// @note The pixel data can be accessed only as long as its associated image handle is valid.
///
/// @param      image   a valid handle to an image
/// @param[out] data    a pointer receiving a temp. pointer to the image's pixel data (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b data parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_GetData(const TL_ImageHandle image, unsigned char** data);

/// Gets the format of a given image.
///
/// @param      image   a valid handle to an image
/// @param[out] imageFormat value as a pointer receiving the image format (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b height parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_GetFormat(const TL_ImageHandle image, enum TL_ImageFormat* imageFormat);

/// Releases the memory of an image with a given image handle.
///
/// @param      image   a valid handle to an image
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b image handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
IMAGE_API bool TL_Image_Release(const TL_ImageHandle image);


#ifdef __cplusplus
}
#endif
