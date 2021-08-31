/// @brief  Definitions and functions of the TOUCHLAB SDK's top-level API.
/// @file   TouchLabApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once 

#ifdef DLL_TOUCHLAB_EXPORTS
    #define TOUCHLAB_API __declspec(dllexport)      ///< exports DLL symbols
#else
    #define TOUCHLAB_API __declspec(dllimport)      ///< imports DLL symbols
#endif 


#ifdef __cplusplus
extern "C" {
#endif 


/// Types a product version that can be used by all API functions.
typedef struct {
    int major;  ///< major version
    int minor;  ///< minor version
    int patch;  ///< patch version
    int build;  ///< build version
} TL_ProductVersion;

/// Contains a list of serial numbers of connected scanners
typedef struct {
    int count; ///< count of connected scanners
    char** serialNumbers; ///< array of serial number strings
} TL_ScannerList;

/// Type of the scanner, the hardware - model.
typedef enum {
    TL_SCANNERTYPE_UNKNOWN = 0,                     ///< The scanner type is not known!
    TL_SCANNERTYPE_LT_QUATTRO = 150,                ///< The scanner is of type "LIVETOUCH quattro"
    TL_SCANNERTYPE_LT_QUATTRO_COMPACT = 170,        ///< The scanner is of type "LIVETOUCH quattro Compact"
} TL_ScannerType;

/// Handle to a scanner device. Usually created by TL_OpenScanner().
typedef int TL_ScannerHandle;

/// A user-defined function to be called after a scanner was plugged in/out.
///
/// @param context whatever was passed to @ref TL_RegisterCallbackScannerListChanged
/// @param count        the new count of currently connected scanners
/// @param serialNumbers  a list of serial numbers of the connected scanners
/// @note If you want to use the @b serialNumbers parameter passed to the callback, *after* the callback *returns*, then make a deep copy of the array. 
/// (The array data passed will no longer be valid after the callback returns).
typedef void( *TL_CallbackScannerListChanged )(void *context, const int count, char** serialNumbers);

/// A user-defined function to be called on some scanning functions.
///
/// @param context  a user-defined context as given on callback registration
/// @param scanner  a valid handle to a scanner
typedef void(*TL_Callback )( void *context, const TL_ScannerHandle scanner );

/// The maximum buffer size of strings returned by API functions (includes terminating null character).
#define TL_MAX_STRING_SIZE 255

/// Provides information about the version's of the API without TL_Initialize().
///
/// @param[out] version  a valid version of the API into TL_ProductVersion format.
/// @return @c true if succeeded. Otherwise @c false if null was given for @b version parameter
/// @note It is not possible to use TL_GetLastError() for more information.
TOUCHLAB_API bool TL_GetVersion( TL_ProductVersion* version );

/// Initializes the TOUCHLAB SDK's top-level API. 
///
/// This initialization is required before using any other API function. Call TL_Finalize() to deinitialize.
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_LOGIC if API is already initialized
///
/// @code
/// bool ok = TL_Initialize();
/// if(!ok)
///     checkError("TL_Initialize()")
/// @endcode
TOUCHLAB_API bool TL_Initialize();

/// Deinitializes the TOUCHLAB SDK's top-level API and cleans up if necessary.
///
/// Should be called when API access is no longer needed.
TOUCHLAB_API void TL_Finalize();

/// Gets the List of supported scanners currently connected to this computer.
/// It allocates memory to store the serial numbers which must be freed by calling TL_FreeScannerList()
///
/// @param[out] scannerList   a pointer receiving the list of serial numbers (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b scannerList parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// TL_ScannerList * S;
/// bool ret = TL_GetScannerList(&S);
///
/// if (!ret)
///     checkError("TL_GetScannerList()");
///
/// int cnt = S ? S->count : 0;
/// char ** serials = S ? S->serialNumbers : 0;
/// @endcode
TOUCHLAB_API bool TL_GetScannerList( TL_ScannerList** scannerList );

/// Frees the list allocated by the call of TL_GetScannerList().
///
/// @param[in] scannerList a pointer to the scanner list
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b scannerList parameter
///
/// @code
/// TL_ScannerList * S = myScannerList();
/// bool ret = TL_FreeScannerList(S);
/// if (!ret)
///     checkError("TL_FreeScannerList()");
/// @endcode
TOUCHLAB_API bool TL_FreeScannerList( TL_ScannerList* scannerList );

/// Registers a user function to be called when the scanner count has changed, for example after (dis-)connecting a device. 
///
/// @param callback a user-defined callback function (use null to unregister)
/// @param context  an arbitrary pointer that will be passed to the callback when it's called. You can use this in client code for whatever you want. 
/// It may be useful for example to pass the *this* pointer from a calling class if the client is written in c++
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// void tClient::onScannerListChanged(void * context, int count, char ** serialNumbers)
/// {
///     tClient * that = static_cast<tClient*>(context);
///     qDebug() << " onScannerListChanged ... now have " << count;
/// }
///
/// bool tClient::regOnScannerListChanged()
/// {
///     void * context = this;
///     bool ret = TL_RegisterCallbackScannerListChanged(tClient::onScannerListChanged, context);
///     if (!ret)
///         checkError("regOnScannerListChanged()");
///
///     return ret;
/// }
/// @endcode
TOUCHLAB_API bool TL_RegisterCallbackScannerListChanged( TL_CallbackScannerListChanged callback, void* context );

/// Opens a scanner with the specified serial number, which creates a new @b scanner @b handle, the value of which will be stored at the address (pointer) specified by @b scanner.
///
/// @param[out] scanner a handle to the opened valid scanner, needs to be closed after use by calling TL_CloseScanner()
/// @param serialNumber a string of serial number contained in the list of connected scanners
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b scanner parameter
/// @return @ref TL_INVALID_ARGUMENT if scanner of given @b serialNumber is already opened
/// @return @ref TL_INVALID_ARGUMENT if @b serialNumber is NULL
/// @return @ref TL_INVALID_ARGUMENT if there is no scanner with the given @b serialNumber
/// @return @ref TL_LOGIC if TL_Initialize() not called
/// @return @ref TL_RUNTIME if the SDK does not support the scanner and needs to be updated
///
/// @note The SDK resource folder must be located beside the TouchLab assembly.
///
/// @code
/// QStringList Serials = mySerials();
/// foreach(QString Ser, Serials)
/// {
///     TL_ScannerHandle Dev;
///     QByteArray Ba = Ser.toUtf8();
///     const char * ser = Ba.constData();
///
///     bool ok = TL_OpenScanner(&Dev, ser);
///
///     if (!ok)
///         checkError(QString("updateDevs with Serial").arg(Ser));
///     else  
///     {
///         // can now eg. register callbacks for Dev .. via TL_Scanner_RegisterCallbackScanImageAvailable(),  TL_Scanner_RegisterCallbackPreviewImage()
///     }
/// }
/// @endcode
TOUCHLAB_API bool TL_OpenScanner( TL_ScannerHandle* scanner, const char* serialNumber );

/// Closes a valid scanner.
/// Has no effect when called on invalid scanner.
///
/// @param scanner  a valid handle to a scanner
///
/// @code
/// TL_ScannerHandle dev = myDev();
/// TL_CloseScanner(dev);
/// @endcode
TOUCHLAB_API void TL_CloseScanner( const TL_ScannerHandle scanner );

/// Retrieves the type of the given scanner. Use this to determine the type before opening the scanner, therefore this is done by serial number.
///
/// @param serialNumber  a valid serial number of the scanner to check
///
/// @param scannerType  contains result information about the scanner type. See @ref TL_ScannerType for details.
///
/// @return @c true if succeeded. Otherwise false.
TOUCHLAB_API bool TL_GetScannerType(const char* serialNumber, TL_ScannerType*  scannerType);

/// Retrieves extended type information of the given scanner type. Use this to determine the type name as defined from manufacturer.
///
/// @param scannerType  a valid scanner type
///
/// @param[out] scannerTypeName  a buffer receiving the type name of the scanner type (must not be null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters. Human readable name for the type.
/// @param[out] scannertTypeIdent a buffer receiving the type ident of the scanner type (must not be null), a null-terminated string of up to @ref TL_MAX_STRING_SIZE characters. This is an unique string representation for the type.
///
/// @return @c true if succeeded. Otherwise false.
TOUCHLAB_API bool TL_GetScannerTypeInformation(const TL_ScannerType scannerType, char* scannerTypeName, char* scannertTypeIdent);


#ifdef __cplusplus
}
#endif
