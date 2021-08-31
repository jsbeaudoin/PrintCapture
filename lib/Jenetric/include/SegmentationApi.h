/// @brief  Definitions and functions of the TOUCHLAB SDK's finger segmentation API.
/// @file   SegmentationApi.h
///
/// @copyright  (C) JENETRIC GmbH
///
/// Usage example (without error handling):
/// @code
/// void PrintAllFourLeftFingerRects( int width, int height, unsigned char* data)
/// {
///     TL_SegmentsHandle  segments;
///     int  segmentsCount, i;
///     TL_FingerSetId  finger;
///     int  rectX, rectY, rectW, rectH;
///
///     TL_Segmentation_ExtractSegmentsFromImage( TL_SID_LEFT_FOUR_FINGERS, width, height, data, TL_FID_NONE, &segments );
///     TL_Segmentation_GetSegmentsCount( segments, &segmentsCount );
///     printf( "%d segment(s) found\n", segmentsCount );
///     for( i = 0; i < segmentsCount; i++ )
///     {
///         TL_Segmentation_GetFingerSet( segments, i, &finger );
///         TL_Segmentation_GetSegmentOrigin( segments, i, &rectX, &rectY );
///         TL_Segmentation_GetImageWidth( segments, i, &rectW );
///         TL_Segmentation_GetImageHeight( segments, i, &rectH );
///         printf( "segment[%d]: set %d at (%d, %d) has dimensions %dx%d\n", i, (int)finger, rectX, rectY, rectW, rectH );
///     }
///     TL_Segmentation_ReleaseSegments( segments );
/// }
/// @endcode

#pragma once


#ifdef DLL_TOUCHLAB_EXPORTS
    #define SEGMENTATION_API __declspec(dllexport)         ///< exports DLL symbols
#else
    #define SEGMENTATION_API __declspec(dllimport)         ///< imports DLL symbols
#endif


#ifdef __cplusplus
extern "C" {
#endif


/// A handle representing the (0 or more) extracted distal finger segments.
///
/// Associated to each segment are:
/// - a gray-scale image
/// - an identification code for the finger being imaged
/// - top-left corner coordinates within image where extracted from
///
typedef int  TL_SegmentsHandle;


/// Single finger identifiers.
///
typedef enum
{
    TL_FID_NONE = 0,

    TL_FID_RIGHT_THUMB          = 1 << 0,
    TL_FID_RIGHT_INDEX_FINGER   = 1 << 1,
    TL_FID_RIGHT_MIDDLE_FINGER  = 1 << 2,
    TL_FID_RIGHT_RING_FINGER    = 1 << 3,
    TL_FID_RIGHT_LITTLE_FINGER  = 1 << 4,
    TL_FID_LEFT_THUMB           = 1 << 5,
    TL_FID_LEFT_INDEX_FINGER    = 1 << 6,
    TL_FID_LEFT_MIDDLE_FINGER   = 1 << 7,
    TL_FID_LEFT_RING_FINGER     = 1 << 8,
    TL_FID_LEFT_LITTLE_FINGER   = 1 << 9,
} TL_FingerId;


/// Finger set identifiers.
///
/// Naming is heavily based on the "friction ridge generalized position" concept as defined in
/// "ANSI/NIST-ITL 1-2011: Update 2015".
///
/// Not supported are:
/// - unknown finger (FGP 0)
/// - extra digit codes (FGP 11-14, 16-17)
/// - unknown friction ridge (FGP 18)
/// - EJI or tip (FGP 19)
/// - palm position codes (FGP 20-38, 81-86)
/// - plantar position codes (FGP 60-79)
///
typedef enum {
    //  single finger position codes
    TL_SID_RIGHT_THUMB = TL_FID_RIGHT_THUMB,
    TL_SID_RIGHT_INDEX_FINGER = TL_FID_RIGHT_INDEX_FINGER,
    TL_SID_RIGHT_MIDDLE_FINGER = TL_FID_RIGHT_MIDDLE_FINGER,
    TL_SID_RIGHT_RING_FINGER = TL_FID_RIGHT_RING_FINGER,
    TL_SID_RIGHT_LITTLE_FINGER = TL_FID_RIGHT_LITTLE_FINGER,
    TL_SID_LEFT_THUMB = TL_FID_LEFT_THUMB,
    TL_SID_LEFT_INDEX_FINGER = TL_FID_LEFT_INDEX_FINGER,
    TL_SID_LEFT_MIDDLE_FINGER = TL_FID_LEFT_MIDDLE_FINGER,
    TL_SID_LEFT_RING_FINGER = TL_FID_LEFT_RING_FINGER,
    TL_SID_LEFT_LITTLE_FINGER = TL_FID_LEFT_LITTLE_FINGER,
    TL_SID_LEFT_RIGHT_THUMBS = TL_FID_LEFT_THUMB | TL_FID_RIGHT_THUMB,

    //  multiple finger position codes (2-finger combinations)
    TL_SID_RIGHT_INDEX_MIDDLE = TL_FID_RIGHT_INDEX_FINGER
                              | TL_FID_RIGHT_MIDDLE_FINGER,
    TL_SID_RIGHT_MIDDLE_RING = TL_FID_RIGHT_MIDDLE_FINGER
                             | TL_FID_RIGHT_RING_FINGER,
    TL_SID_RIGHT_RING_LITTLE = TL_FID_RIGHT_RING_FINGER
                             | TL_FID_RIGHT_LITTLE_FINGER,
    TL_SID_LEFT_INDEX_MIDDLE = TL_FID_LEFT_INDEX_FINGER
                             | TL_FID_LEFT_MIDDLE_FINGER,
    TL_SID_LEFT_MIDDLE_RING = TL_FID_LEFT_MIDDLE_FINGER
                            | TL_FID_LEFT_RING_FINGER,
    TL_SID_LEFT_RING_LITTLE = TL_FID_LEFT_RING_FINGER
                            | TL_FID_LEFT_LITTLE_FINGER,
    TL_SID_RIGHT_INDEX_LEFT_INDEX = TL_FID_LEFT_INDEX_FINGER
                                  | TL_FID_RIGHT_INDEX_FINGER,

    //  multiple finger position codes (3-finger combinations)
    TL_SID_RIGHT_INDEX_MIDDLE_RING = TL_FID_RIGHT_INDEX_FINGER
                                   | TL_FID_RIGHT_MIDDLE_FINGER
                                   | TL_FID_RIGHT_RING_FINGER,
    TL_SID_RIGHT_MIDDLE_RING_LITTLE = TL_FID_RIGHT_MIDDLE_FINGER
                                    | TL_FID_RIGHT_RING_FINGER
                                    | TL_FID_RIGHT_LITTLE_FINGER,
    TL_SID_LEFT_INDEX_MIDDLE_RING = TL_FID_LEFT_INDEX_FINGER
                                  | TL_FID_LEFT_MIDDLE_FINGER
                                  | TL_FID_LEFT_RING_FINGER,
    TL_SID_LEFT_MIDDLE_RING_LITTLE = TL_FID_LEFT_MIDDLE_FINGER
                                   | TL_FID_LEFT_RING_FINGER
                                   | TL_FID_LEFT_LITTLE_FINGER,

    //  multiple finger position codes (4-finger combinations)
    TL_SID_RIGHT_FOUR_FINGERS = TL_FID_RIGHT_INDEX_FINGER
                              | TL_FID_RIGHT_MIDDLE_FINGER
                              | TL_FID_RIGHT_RING_FINGER
                              | TL_FID_RIGHT_LITTLE_FINGER,
    TL_SID_LEFT_FOUR_FINGERS = TL_FID_LEFT_INDEX_FINGER
                             | TL_FID_LEFT_MIDDLE_FINGER
                             | TL_FID_LEFT_RING_FINGER
                             | TL_FID_LEFT_LITTLE_FINGER,

    //  multiple finger position codes (5-finger combinations)
    TL_SID_RIGHT_FOUR_FINGERS_AND_THUMB = TL_FID_RIGHT_INDEX_FINGER
                                        | TL_FID_RIGHT_MIDDLE_FINGER
                                        | TL_FID_RIGHT_RING_FINGER
                                        | TL_FID_RIGHT_LITTLE_FINGER
                                        | TL_FID_RIGHT_THUMB,
    TL_SID_LEFT_FOUR_FINGERS_AND_THUMB = TL_FID_LEFT_INDEX_FINGER
                                       | TL_FID_LEFT_MIDDLE_FINGER
                                       | TL_FID_LEFT_RING_FINGER
                                       | TL_FID_LEFT_LITTLE_FINGER
                                       | TL_FID_LEFT_THUMB,
} TL_FingerSetId;


/// Extracts the distal segments ("finger tips") from a given fingerprint image.
///
/// @param      fingerSet       finger set to extract segments from
/// @param      width           image width
/// @param      height          image height
/// @param      data            pointer to user-provided pixel data (width*height many bytes)
/// @param      missingFingers  a combination of @ref TL_FingerId values describing which fingers are missing in image (if any)
/// @param[out] segments        a pointer receiving a handle to the extracted segments, needs to be released after use by calling TL_Segmentation_ReleaseSegments()
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if all fingers of @b fingerSet are part of @b missingFingers
/// @return @ref TL_INVALID_ARGUMENT if any of @b missingFingers is not part of @b fingerSet
/// @return @ref TL_INVALID_ARGUMENT if invalid @b fingerSet value was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b segments parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_ExtractSegmentsFromImage( TL_FingerSetId fingerSet, int width, int height, const unsigned char* data, int missingFingers, TL_SegmentsHandle* segments );

/// Releases the handle's segments and invalidates the handle.
/// Has no effect when called on an invalid handle.
///
/// @param segments  a handle provided by TL_Segmentation_ExtractSegmentsFromImage()
///
SEGMENTATION_API void  TL_Segmentation_ReleaseSegments( TL_SegmentsHandle segments );

/// Gets the number of extracted segments.
///
/// @param      segments a valid handle to extracted segments
/// @param[out] count    a pointer receiving the number of segments (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b count parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetSegmentsCount( TL_SegmentsHandle segments, int* count );

/// Gets the position code of a segment.
///
/// @param      segments   a valid handle to extracted segments
/// @param      index      a zero-based segment index
/// @param[out] fingerSet  a pointer receiving the finger set id (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_OUT_OF_RANGE if @b index is not in range [0, ..., count-1] defined by TL_Segmentation_GetSegmentsCount()
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b fingerSet parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetFingerSet( TL_SegmentsHandle segments, int index, TL_FingerSetId* fingerSet );

/// Gets a segment's origin location.
///
/// Contains the top-left corner coordinate of the extracted image rectangle.
///
/// @param      segments a valid handle to extracted segments
/// @param      index    a zero-based segment index
/// @param[out] left     a pointer receiving the segment's left column within originating image (must not be null)
/// @param[out] top      a pointer receiving the segment's top row within originating image (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_OUT_OF_RANGE if @b index is not in range [0, ..., count-1] defined by TL_Segmentation_GetSegmentsCount()
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for any of @b left or @b top parameters
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetSegmentOrigin( TL_SegmentsHandle segments, int index, int* left, int* top );

/// Gets the image width of a segment.
///
/// @param      segments a valid handle to extracted segments
/// @param      index    a zero-based segment index
/// @param[out] width    a pointer receiving the image width (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_OUT_OF_RANGE if @b index is not in range [0, ..., count-1] defined by TL_Segmentation_GetSegmentsCount()
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b width parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetImageWidth( TL_SegmentsHandle segments, int index, int* width );

/// Gets the image height of a segment.
///
/// @param      segments a valid handle to extracted segments
/// @param      index    a zero-based segment index
/// @param[out] height   a pointer receiving the image height (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_OUT_OF_RANGE if @b index is not in range [0, ..., count-1] defined by TL_Segmentation_GetSegmentsCount()
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b height parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetImageHeight( TL_SegmentsHandle segments, int index, int* height );

/// Gets access to the image pixel data of a segment.
///
/// @param      segments a valid handle to extracted segments
/// @param      index    a zero-based segment index
/// @param[out] data     a pointer receiving a temp. pointer to the image pixel data (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_OUT_OF_RANGE if @b index is not in range [0, ..., count-1] defined by TL_Segmentation_GetSegmentsCount()
/// @return @ref TL_INVALID_ARGUMENT if invalid @b segments handle was given
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b data parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
SEGMENTATION_API bool  TL_Segmentation_GetImageData( TL_SegmentsHandle segments, int index, unsigned char** data );


#ifdef __cplusplus
}
#endif
