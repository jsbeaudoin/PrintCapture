/// @brief  Definitions and functions of the TOUCHLAB SDK's error handling API.
/// @file   ErrorApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "TouchLabApi.h"

#ifdef DLL_TOUCHLAB_EXPORTS
    #define ERROR_API __declspec(dllexport)         ///< exports DLL symbols
#else
    #define ERROR_API __declspec(dllimport)         ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif

/// Types of error states that can be reported by all API functions.
enum TL_ErrorType
{
    TL_NO_ERROR,            ///< no error occurred
    TL_RUNTIME,             ///< error that can only be detected during runtime
    TL_INVALID_ARGUMENT,    ///< an invalid argument was given
    TL_OUT_OF_RANGE,        ///< error occurs by attempt to access elements out of defined range
    TL_SYSTEM,              ///< conditions originating during runtime from the operating system or other low-level application program interfaces which have an associated error_code
    TL_LOGIC,               ///< error in the internal logical of the program, such as violation of logical preconditions or class invariants
    TL_UNKNOWN              ///< general error
};

/// Provides information about the error state of the latest API call within the caller's thread.
///
/// @param[out] type        a pointer receiving the error type (ignored if null) which is @ref TL_LOGIC if top-level API not initialized.
/// @param[out] message     a buffer receiving the error message (ignored if null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters
/// @param[out] code        a pointer receiving the system error code in case of @ref TL_SYSTEM otherwise user specific error code (ignored if null)
///
/// @code
/// void checkError(QString Mesg)
/// {
///     int code;
///     TL_ErrorType Et;
///     char mesg[TL_MAX_STRING_SIZE];
///     TL_GetLastError(&Et, mesg, &code);
///     if (Et == TL_ErrorType::TL_NO_ERROR) {
///         qDebug() << Mesg << "... No Last Error:  ";
///     else
///         qDebug() << Mesg<< "... Last Error:  " << Et << ", " << code << ", " << mesg;
/// }
/// @endcode
ERROR_API void TL_GetLastError( enum TL_ErrorType* type, char* message, int* code );


#ifdef __cplusplus
}
#endif
