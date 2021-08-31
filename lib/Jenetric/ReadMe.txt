TOUCHLAB SDK
JENETRIC GmbH

--- Documentation ---
The API documentation is provided in the subfolder "help" under the TOUCHLAB SDK <INSTALLATION_FOLDER>. 
Execute "index.html" to read the documentation.

--- CHECKBOX Documentation ---
The CHECKBOX tool documentation is provided online in the JENETRIC Support Center. 

--- Update of Scanner ---
Prior to the update, you need to install the scanner driver!

The firmware updater you can find under:
<INSTALLATION_FOLDER>\FirmwareUpdater.exe

Options:
  -h [ --help ]         Print help messages
  -u [ --update ] arg   Update using the update.json file <arg>
  -s [ --silent ]       Updates silently without confirmation
  -o [ --override ]     Update regardless of version constrains
  -i [ --info ]         Info (provide scanner information)

Please use the "<INSTALLATION_FOLDER>\firmware_files\" folder (incl. the config file named: update.json and all necessary files)

Sample call to get all information form the connected scanner:
FirmwareUpdater.exe -i

Sample call of a normal update:
FirmwareUpdater.exe -u .\firmware_files\update.json

Sample call of a silent update:
FirmwareUpdater.exe -u .\firmware_files\update.json -s

Sample call of an override update (for override the same version or downgrade version):
FirmwareUpdater.exe -u .\firmware_files\update.json -o

The possible error codes:
    STATUS_NO_ERROR              = 0: no error
    STATUS_INVALID_ARGUMENT      = 1: invalid argument or parameter
    STATUS_FILE_NOT_FOUND        = 2: file not found
    STATUS_INVALID_CONFIG        = 3: invalid configuration file
    STATUS_NO_SCANNER            = 4: no scanner detected or scanner is already in use by another program
    STATUS_INVALID_SCANNER_COUNT = 5: more than one scanner detected - only a single one is allowed
    STATUS_CONNECTION_ERROR      = 6: scanner connection was interrupted
    STATUS_INVALID_CHECKSUM      = 7: invalid checksum of file
    STATUS_USER_CANCELED         = 8: user canceled the update
    STATUS_INCORRECT_VERSION     = 9: provided firmware version is equal or lower than the installed firmware
    STATUS_INVALID_REVISION     = 10: provided firmware does not match to hardware revision
    STATUS_UNKNOWN              = 11: unknown error
  
--- Driver ---
The installation of the TOUCHLAB SDK contains the appropriate signed windows driver (We provide two setup variants as msi package: one for x86 and one for x64 driver):

Note: The shortcuts _x86 or _x64 describe the driver version inside the installer package, NOT the SDK API architecture. The API is provided in 32-bit code.

Installation of setup_touchlab_sdk_x86.msi or *_x64.msi.
This package also contains the appropriate driver for x86 or x64 windows operating systems. The driver will be installed automatically! 

To manually install the driver kindly execute the following steps:
    1)   Go to the device manager and search for FX3 device.
    2)   Right-click on „FX3“ and select “installing driver”.
    3)   „Select driver from computer” than go to „<INSTALLATION_FOLDER>\driver\x86“ or „<INSTALLATION_FOLDER>\driver\x64“ (depending on operating system architecture).
    4)   Confirm the security warning by clicking „Install”.
    5)   Done. Now you should find „JENETRIC device“ in the device manager.