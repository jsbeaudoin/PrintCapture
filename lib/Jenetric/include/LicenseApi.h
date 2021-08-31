/// @brief  Definitions and functions of the TOUCHLAB SDK's license handling API.
/// @file   LicenseApi.h
///
/// @copyright  (C) JENETRIC GmbH

#pragma once

#include "TouchLabApi.h"


#ifdef DLL_TOUCHLAB_EXPORTS
    #define LICENSE_API __declspec(dllexport)       ///< exports DLL symbols
#else
#define LICENSE_API __declspec(dllimport)       ///< imports DLL symbols
#endif 

#ifdef __cplusplus
extern "C" {
#endif 


/// License.
typedef struct {
    char*  product;     ///< product name
    char*  version;     ///< product version
    char*  expiryDate;  ///< expiry date
    char*  reference;   ///< reference number
    char*  signature;   ///< signature hash
    int    optionFlags; ///< options
} TL_License;

/// Contains a list of licenses
typedef struct {
    int count; ///< count of licenses
    TL_License** licenses; ///< array of licenses structs
} TL_LicenseList;


/// Gets a List of local stored licenses.
/// It allocates memory to store the licenses which must be freed by calling TL_License_FreeLicenses()
///
/// @param[out] licenseList   a pointer receiving the list of licenses (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b licenseList parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
LICENSE_API bool TL_License_GetLocalLicenses( TL_LicenseList** licenseList );

/// Gets a List of licenses from the scanner.
/// It allocates memory to store the licenses which must be freed by calling TL_License_FreeLicenses()
///
/// @param scanner              a valid handle to a scanner 
/// @param[out] licenseList     a pointer receiving the list of licenses (must not be null)
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b licenseList parameter
/// @return @ref TL_LOGIC if TL_Initialize() not called
///
/// @code
/// // Example how to work with the scanners licenseList
/// TL_LicenseList* licenseList;
/// if( !TL_License_GetScannerLicenses( m_myData->m_scannerHandle, &licenseList ) ) {
///     // maybe scanner with older firmware without license stream
///     return;
/// }
/// for( int i = 0; i < licenseList->count; i++ ){
///     std::string product = (*licenseList).licenses[i]->product;
///     std::string version = (*licenseList).licenses[i]->version;
///     std::string optionString;
///     if( (*licenseList).licenses[i]->optionFlags && TL_OPTION_SIGNATURE ) {
///         optionString += "signature";
///     }
///     std::string expiryDate = (*licenseList).licenses[i]->expiryDate;
///     std::string reference = (*licenseList).licenses[i]->reference;
/// }
/// TL_License_FreeLicenses( licenseList );
/// @endcode
LICENSE_API bool TL_License_GetScannerLicenses( const TL_ScannerHandle scanner, TL_LicenseList** licenseList );

/// Frees the list allocated by the call of TL_License_GetLocalLicenses() or TL_License_GetScannerLicenses().
///
/// @param[in] licenseList a pointer to the license list
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null was given for @b licenseList parameter
LICENSE_API bool TL_License_FreeLicenses( TL_LicenseList* licenseList );

/// Adds a license and store it in the public ProgramData directory under "JENETRIC GmbH\licenses".
///
/// @param[in] path a path to a license file
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null or an invalid path was given for @b path parameter
/// @return @ref TL_RUNTIME if the license file contained an invalid license or the license is expired
LICENSE_API bool TL_License_AddLocalLicense( const char* path );

/// Adds a license and write it permanently into the scanner.
///
/// @param scanner      a valid handle to a scanner 
/// @param[in] path     a path to a license file
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null or an invalid path was given for @b path parameter
/// @return @ref TL_RUNTIME if the license file contained an invalid license or the license is expired 
/// or the license's serial number does not match with the one of the scanner
LICENSE_API bool TL_License_AddScannerLicense( const TL_ScannerHandle scanner, const char* path );

/// Deletes a local stored license.
///
/// @param[in] license      a license
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if null or an invalid license was given for @b license parameter
/// @return @ref TL_RUNTIME if the license could not be deleted
LICENSE_API bool TL_License_DeleteLocalLicense( TL_License* license );

/// Deletes a license stored in the scanner.
///
/// @param scanner          a valid handle to a scanner 
/// @param[in] license      a license
///
/// @return @c true if succeeded. Otherwise use TL_GetLastError() for more information:
/// @return @ref TL_INVALID_ARGUMENT if invalid @b scanner handle was given
/// @return @ref TL_INVALID_ARGUMENT if null or an invalid license was given for @b license parameter
/// @return @ref TL_RUNTIME if the license could not be deleted
LICENSE_API bool TL_License_DeleteScannerLicense( const TL_ScannerHandle scanner, TL_License* license );


#ifdef __cplusplus
}
#endif 
