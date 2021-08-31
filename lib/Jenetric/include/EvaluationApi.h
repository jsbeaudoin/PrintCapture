/// @brief  Definitions and functions of the TOUCHLAB SDK's image evaluation API.
/// @file   EvaluationApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#ifdef DLL_TOUCHLAB_EXPORTS
    #define EVALUATION_API __declspec(dllexport)       ///< exports DLL symbols
#else
    #define EVALUATION_API __declspec(dllimport)       ///< imports DLL symbols
#endif

#ifdef __cplusplus
extern "C" {
#endif

/// Handle to evaluation data of an image as delivered by @ref TL_CallbackScanImageAvailable.
typedef int TL_EvaluationHandle;

/// Checks whether presentation attack detection was executed when evaluating an image.
///
/// @param      eval     a valid scan image evaluation handle to check on
/// @param[out] wasExecuted  a pointer receiving the check result (must not be null): @c true if executed, @c false otherwise
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid handle was given as @b eval parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b wasExecuted parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
EVALUATION_API
bool  TL_Evaluation_WasAttackDetectionExecuted( const TL_EvaluationHandle eval, bool* wasExecuted );

/// Checks whether a presentation attack was detected while evaluating an image.
///
/// @param      eval     a valid scan image evaluation handle to check on
/// @param[out] wasDetected  a pointer receiving the check result (must not be null): @c true if detected, @c false otherwise
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid handle was given as @b eval parameter
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b wasDetected parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
EVALUATION_API
bool  TL_Evaluation_WasAttackDetected( const TL_EvaluationHandle eval, bool* wasDetected );

/// Returns the count of detected fingers. 
/// The order of fingers reflect the positioning of fingerprints detected on the surface, from left to right. 
/// (The leftmost finger as seen by user when looking down on scanner having index 0 etc.)
///
/// @param      eval    a valid scan image evaluation handle to check on
/// @param[out] count   a pointer receiving the number of detected fingers
///
/// @return @ref TL_LOGIC if TL_Initialize() not called
EVALUATION_API
bool TL_Evaluation_GetFingerCount(TL_EvaluationHandle eval, int* count);

/// Returns the bounding box of a finger given by index. 
///
/// @param  eval    a valid scan image evaluation handle to check on
/// @param  index   index of finger (The leftmost finger as seen by user when looking down on scanner having index 0 etc.)
/// @param[out] left     a pointer receiving the bounding box's left column (must not be null)
/// @param[out] top      a pointer receiving the bounding box's top row (must not be null)
/// @param[out] width    a pointer receiving the segment's width  (must not be null)
/// @param[out] height   a pointer receiving the segment's height (must not be null)
///
/// @return @ref TL_INVALID_ARGUMENT if invalid handle was given as @b eval parameter
/// @return @ref TL_OUT_OF_RANGE if index is out of range
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b left, @b top, @b width or @b height parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
EVALUATION_API
bool TL_Evaluation_GetFingerBoundingBox(TL_EvaluationHandle eval, int index, int* left, int* top, int* width, int* height);

/// Returns the measured NFIQ2 quality score for finger given by index. 
/// 
/// @param  eval    a valid scan image evaluation handle to check on
/// @param  index   index of finger (The leftmost finger as seen by user when looking down on scanner having index 0 etc.)
/// @param  score   a pointer to receive the score (must not be null). A valid score is between 0 to 100. A value of 255 indicates a failed NFIQ2 call.
///
/// @return @ref TL_INVALID_ARGUMENT if invalid handle was given as @b eval parameter
/// @return @ref TL_OUT_OF_RANGE if index is out of range
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b score parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
EVALUATION_API
bool TL_Evaluation_GetNFIQ2Score(TL_EvaluationHandle eval, int index, int* score);

#ifdef __cplusplus
}
#endif
