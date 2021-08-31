/// @brief  Definitions and functions of the TOUCHLAB SDK's guidance handling API.
/// @file   GuidanceApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "TouchLabApi.h"
#include "ImageApi.h"
#include "ScannerApi.h"

#ifdef DLL_TOUCHLAB_EXPORTS
    #define GUIDANCE_API __declspec(dllexport)       ///< exports DLL symbols
#else
    #define GUIDANCE_API __declspec(dllimport)       ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif 

    
/// Handle to a guidance listener for new available images. That handles can be used to identify one among many listeners.
typedef int TL_GuidanceListenerImageAvailableHandle;

/// Handle to a display part supported by the user guidance. That handles can be used to identify and filter display parts.
typedef int TL_GuidanceDisplayPart;

/// this defines one special display part, the "whole screen". Using this, all display parts are drawn together.
const TL_GuidanceDisplayPart TL_DISPLAYPART_ALL = 0;

/// A user-defined function to be called after a new guidance image is available. Every new listener needs one of these callback functions.
///
/// @param context      a user-defined context as given on callback registration
/// @param image        a valid handle to current guidance image, gets invalid when leaving the callback function
/// @param displayPart  is the display part for which the new image was rendered. That can also be @ref TL_DISPLAYPART_ALL, then the whole display (all parts together) is meant.
/// @param time         is current animation time for the available image. While user is being guided, every available image is part of an animation at a time.
typedef void(*TL_CallbackGuidanceImageAvailable)(void *context, TL_ImageHandle image, TL_GuidanceDisplayPart displayPart, float time);

/// Defines the type of a display part. Every display part has an intended meaning, this defines that meaning.
typedef enum 
{
    TL_DISPLAYPARTTYPE_SYSTEMSTATUS = 1,   ///< This display part type is intended to give the user status information about the system itself (not for capture state).
    TL_DISPLAYPARTTYPE_STATUS = 2,         ///< This display part type is intended to give the user status information about the current capture process.
    TL_DISPLAYPARTTYPE_GUIDANCE = 3,       ///< This display part type is intended to guide the user with information about the current capture process. For LIVETOUCH quattro this is the top area.
    TL_DISPLAYPARTTYPE_SENSOR = 4          ///< This display part type is intended to give the user information about the positions to put the finger(s) on the sensor. For LIVETOUCH quattro this is the bottom area.
} TL_GuidanceDisplayPartType;

/// Contains detailed information about a single display part. 
typedef struct {
    TL_GuidanceDisplayPart ident;           ///< Is a unique identification for one display part, this is used in the SDK functions for user guidance to identify the display part to use.
    TL_GuidanceDisplayPartType type;        ///< This is a additional type for the display part, a part can aim to be intended for special user output. 
    int displayPartRectLeft;                ///< The left (x) coordinate of display rectangle of the given display part. This is relative to the physical display of the scanner device.
    int displayPartRectTop;                 ///< The top (y) coordinate of display rectangle of the given display part. This is relative to the physical display of the scanner device.
    int displayPartRectWidth;               ///< The width of display rectangle of the given display part. This is relative to the physical display of the scanner device.
    int displayPartRectHeight;              ///< The height of display rectangle of the given display part. This is relative to the physical display of the scanner device.
} TL_GuidanceDisplayPartInfo;

/// Contains a list of detailed information about a display part, list of TL_GuidanceDisplayPartInfo elements
typedef struct {
    int count;                                  ///< count of contained display part info entries
    TL_GuidanceDisplayPartInfo** displayParts;  ///< points to an array of TL_GuidanceDisplayPartInfo elements with length @ref count.
} TL_GuidanceDisplayPartInfoList;


/// Retrieves the supported display parts by the user guidance of the given scanner. 
///
/// @param scanner                  a valid handle to a scanner
/// @param[out] displayPartList     a pointer to receive the list of supported display parts. It is of type (struct) "TL_GuidanceDisplayPartInfoList*". The result struct is created on heap, so it is mandatory to call TL_Guidance_FreeDisplayPartList() on it after work is finished!
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b displayPartList* parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// // Example how to get display part info of user guidance
/// TL_GuidanceDisplayPartInfoList* parts;
/// TL_Guidance_GetSupportedDisplayParts(m_scanner, &parts);
/// std::cout << __FUNCTION__ << "(): Got display parts: ";
/// for (int i = 0; i < parts->count; i++)
/// {
///     char desc[TL_MAX_STRING_SIZE];
///     cdp = parts->displayParts[i]->ident;
///     if(TL_Guidance_GetDisplayPartDescription(m_scanner, cdp, desc))
///     {
///       std::cout << desc << ", ";
///     }
/// }
/// std::cout << std::endl;
/// TL_Guidance_FreeDisplayPartList(parts);
/// @endcode
GUIDANCE_API bool TL_Guidance_GetSupportedDisplayParts(const TL_ScannerHandle scanner, TL_GuidanceDisplayPartInfoList** displayPartList);

/// Frees the given list of display parts. a call to this is mandatory after getting the list via TL_Guidance_GetSupportedDisplayParts() !
///
/// @param[out] displayPartList     a pointer to the list to be freed. 
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b displayPartList parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
GUIDANCE_API void TL_Guidance_FreeDisplayPartList(TL_GuidanceDisplayPartInfoList* displayPartList);

/// Retrieves the description for one given supported display part by the user guidance of the given scanner. 
///
/// @param scanner                      a valid handle to a scanner
/// @param displayPart                  The display part you want to get the description for.
/// @param[out] displayPartDescription  a pointer to receive the buffer where description is in. Must not be null, a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b displayPartDescription* parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
GUIDANCE_API bool TL_Guidance_GetDisplayPartDescription(const TL_ScannerHandle scanner, TL_GuidanceDisplayPart displayPart, char* displayPartDescription);

/// Registers a new listener at the user guidance of the given scanner. There are multiple listeners possible for one scanner, contrary to the "callback"-concept.
/// For example, you can register multiple listeners to get user guidance rendered in different image sizes, see "requestedWidth" and "requestedHeight".
///
/// @param scanner              a valid handle to a scanner
/// @param requestedWidth       the requested width for the guidance images. User guidance will render new images in that size. Can be "0", then @b requestedHeight is not allowed to be "0", width is automatically calculated according to aspect ratio.
/// @param requestedHeight      the requested height for the guidance images. User guidance will render new images in that size. Can be "0", then @b requestedWidth is not allowed to be "0", height is automatically calculated according to aspect ratio.
/// @param displayPartFilter    the display part you want to get information in the callback for. If a special filter is given, the callback will only be fired, if new guidance image matches the display part defined in that filter. @ref TL_DISPLAYPART_ALL can also be used, then the callback will be fired only for the whole screen (all parts drawn together).
/// @param callback             the callback to be fired in case of a new guidance image was rendered.  
/// @param context              a user-defined context, this is given to the callback if fired.
/// @param[out] handle          a pointer to receive the handle of the new created listener, is not allowed to be null.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b handle parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// // Example how to define callback for user guidance
/// void QMyWindowInstance::guidanceCallback(void* context, TL_ImageHandle image, TL_GuidanceDisplayPart displayPart, float time)
/// {
///     QMyWindowInstance* page = static_cast<QMyWindowInstance*>(context);
///     page->emitGuidanceImageAvailable(makeQImage(image), displayPart);
/// }
/// 
/// QImage QMyWindowInstance::makeQImage(TL_ImageHandle imageHandle)
/// {
///     bool succeded;
///     int width;
///     succeded = TL_Image_GetWidth(imageHandle, &width);
/// 
///     int height;
///     succeded = TL_Image_GetHeight(imageHandle, &height);
/// 
///     unsigned char* data;
///     succeded = TL_Image_GetData(imageHandle, &data);
/// 
///     TL_ImageFormat imgFormat;
///     succeded = TL_Image_GetFormat(imageHandle, &imgFormat);
/// 
///     QImage::Format qimgFormat;
/// 
///     switch (imgFormat)
///     {
///       case TL_ImageFormat::TL_IMAGEFORMAT_ARGB32:
///       {
///         qimgFormat = QImage::Format_ARGB32;
///         break;
///       }
///     default:
///         qimgFormat = QImage::Format_Indexed8;
///         break;
///     }
/// 
///     QImage image(data, width, height, qimgFormat);
///     if (image.isNull()) {
///         throw std::runtime_error(std::string(__FUNCTION__) + "(): conversion failed.");
///     }
///     if (qimgFormat == QImage::Format_Indexed8)
///     {
///         QVector<QRgb>  palette(256);
///         for (int i = 0; i < palette.size(); ++i) {
///             palette[i] = qRgb(i, i, i);
///         }
///         image.setColorTable(palette);
///     }
///     //  dpm(res) := res * 1000/inch2mm(1)
///     const int dotsPerMeter = static_cast<int>(std::lround(500.0 * 1000.0 / 25.4));
///     image.setDotsPerMeterX(dotsPerMeter);
///     image.setDotsPerMeterY(dotsPerMeter);
/// 
///     QImage resImg = image.copy();
/// 
///     TL_Image_Release(imageHandle);
/// 
///     return resImg;
/// }
/// @endcode
///
/// @code
/// // Example how to work with listeners for user guidance
/// TL_GuidanceListenerImageAvailableHandle guidanceListener;
/// TL_GuidanceDisplayPart displayPart;
/// if (TL_Guidance_RegisterListenerImageAvailable(m_myData->m_scannerHandle, 0, 500, displayPart, &QMyWindowInstance::guidanceCallback, this, &guidanceListener))
/// {
///   // guidanceListener is valid and can be used...
/// } else 
/// {
///   // error registering the listener 
/// }
/// // if the listener is no longer needed, then it should be removed with "TL_Guidance_UnregisterListenerImageAvailable" or "TL_Guidance_RemoveAllListenersImageAvailable"
/// @endcode
GUIDANCE_API bool TL_Guidance_RegisterListenerImageAvailable(const TL_ScannerHandle scanner, int requestedWidth, int requestedHeight, TL_GuidanceDisplayPart displayPartFilter, TL_CallbackGuidanceImageAvailable callback, void* context, TL_GuidanceListenerImageAvailableHandle *handle);

/// Unregisters (removes) a previous created listener at the user guidance of the given scanner.
///
/// @param scanner  a valid handle to a scanner
/// @param handle   a valid handle to the listener to be removed from listener list.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b handle parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
GUIDANCE_API bool TL_Guidance_UnregisterListenerImageAvailable(const TL_ScannerHandle scanner, TL_GuidanceListenerImageAvailableHandle handle);

/// Clears (removes all) created listeners at the user guidance of the given scanner.
///
/// @param scanner  a valid handle to a scanner
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_LOGIC if TL_Initialize() not called
GUIDANCE_API bool TL_Guidance_RemoveAllListenersImageAvailable(const TL_ScannerHandle scanner);

/// Retrieves the information, if the given listener exists at the user guidance of the given scanner.
///
/// @param scanner          a valid handle to a scanner
/// @param handle           a valid handle to a listener
/// @param[out] exists      a pointer to a bool variable to receive result information if exists or not.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b exists parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
GUIDANCE_API bool TL_Guidance_ExistsListenerImageAvailable(const TL_ScannerHandle scanner, TL_GuidanceListenerImageAvailableHandle handle, bool* exists);

/// Set display content from file to display part of the given scanner.
///
/// @param scanner          a valid handle to a scanner
/// @param displayPart      a valid handle to a display part
/// @param path             a pointer to a path of media content
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b displayPart parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b path parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @note Note that this function supports only PNG-, SVG- and AVI-files.
///
/// @code
/// // Example how to set a new display content, from png or svg file
/// TL_GuidanceDisplayPart displayPart;
/// // please retrieve the display part as required from function "TL_Guidance_GetSupportedDisplayParts"
/// std::string testFilePathAndName = "[path and name of my file], png, svg and avi are supported";
/// if (TL_Guidance_SetDisplayContentFromFile(m_scanner, displayPart, testFilePathAndName.c_str()))
/// {
///   // new display content is valid...
/// } else 
/// {
///   // error setting new display content 
/// }
/// @endcode
GUIDANCE_API bool TL_Guidance_SetDisplayContentFromFile(const TL_ScannerHandle scanner, TL_GuidanceDisplayPart displayPart, const char* path);

/// Fill display content with given RGB color to display part of the given scanner.
///
/// @param scanner          a valid handle to a scanner
/// @param displayPart      a valid handle to a display part
/// @param redValue         a valid RGB color value for red
/// @param greenValue       a valid RGB color value for green
/// @param blueValue        a valid RGB color value for blue
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if invalid @b exists parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// // Example how to set a new display content, from plain color
/// TL_GuidanceDisplayPart displayPart;
/// // please retrieve the display part as required from function "TL_Guidance_GetSupportedDisplayParts"
/// if (TL_Guidance_FillDisplay(m_scanner, displayPart, 255, 0, 0))
/// {
///   // new display content is red...
/// } else 
/// {
///   // error setting new display content 
/// }
/// @endcode
GUIDANCE_API bool TL_Guidance_FillDisplay(const TL_ScannerHandle scanner, TL_GuidanceDisplayPart displayPart, int redValue, int greenValue, int blueValue);


#ifdef __cplusplus
}
#endif 
