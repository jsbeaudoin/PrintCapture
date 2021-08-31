namespace Livescan.Scanners.DriverEssential.Sdk
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Container to hold image data together with meta information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageData
    {
        /// <summary>
        /// Pointer to image buffer
        /// </summary>
        public IntPtr Buffer;

        /// <summary>
        /// Image horizontal size
        /// </summary>
        public uint Width;

        /// <summary>
        /// Image vertical size
        /// </summary>
        public uint Height;

        /// <summary>
        /// Horizontal image resolution (in PPI)
        /// </summary>
        public double ResolutionX;

        /// <summary>
        /// Vertical image resolution (in PPI)
        /// </summary>
        public double ResolutionY;

        /// <summary>
        /// Image acquisition time (in seconds)
        /// This value contains the time taken for acquisition from device (excluding processing time)
        /// </summary>
        public double FrameTime;

        /// <summary>       
        /// Image line pitch (in Bytes)
        /// Positive values indicate top down line order, Negative values mean bottom up line order
        /// </summary>
        public int Pitch;

        /// <summary>
        /// Number of Bits per pixel
        /// </summary>
        public byte BitsPerPixel;

        /// <summary>
        /// Image color format
        /// </summary>
        public emumImageFormat Format;

        /// <summary>
        /// Marks image as finally processed
        /// A value of "FALSE" disqualifies image for further processing. (e.g. interim or preprocessed result images)
        /// </summary>
        public int IsFinal;
    }


    /// <summary>
    /// WINAPI tool for Color reference structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        /// <summary>
        /// Red
        /// </summary>
        public byte R;

        /// <summary>
        /// Green
        /// </summary>
        public byte G;

        /// <summary>
        /// Blue
        /// </summary>
        public byte B;

        /// <summary>            
        /// int to send to Win32 API because, there's no COLORREF structure in native Win32. 
        /// It is typedef-ed to DWORD, which means that in the managed world its direct counterpart is System.Int32 (aka int in C#). 
        /// So, when faced with interop involving COLORREF'S you'd better treat them as int's. 
        /// Also have in mind that the color components are stored in reverse order, i.e. 
        /// the Red component is in the lowest-byte. In short, the format is 0x00BBGGRR. 
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>int to send to Win 32 API</returns>
        /// <remarks>For converting COLORREF from and to .net's Color use 'ColorTranslator.ToWin32' [System.Drawing] and 'ColorTranslator.FromWin32' methods.</remarks>
        public static int MakeCOLORREF(byte r, byte g, byte b)
        {
            return (int)(((uint)r) | (((uint)g) << 8) | (((uint)b) << 16));
        }

        /// <summary>
        /// int to send to Win32 API because, there's no COLORREF structure in native Win32. 
        /// It is typedef-ed to DWORD, which means that in the managed world its direct counterpart is System.Int32 (aka int in C#). 
        /// So, when faced with interop involving COLORREF'S you'd better treat them as int's. 
        /// Also have in mind that the color components are stored in reverse order, i.e. 
        /// the Red component is in the lowest-byte. In short, the format is 0x00BBGGRR.
        /// </summary>
        /// <returns>int to send to Win 32 API</returns>
        /// <remarks>For converting COLORREF from and to .net's Color use 'ColorTranslator.ToWin32' [System.Drawing] and 'ColorTranslator.FromWin32' methods.</remarks>
        public int MakeCOLORREF()
        {
            return (int)(((uint)this.R) | (((uint)this.G) << 8) | (((uint)this.B) << 16));
        }

    }

    /// <summary>
    /// Container to hold version information.
    /// </summary>        
    [StructLayout(LayoutKind.Sequential)]
    public struct LScanApiVersion
    {
        /// <summary>
        /// Product version string
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string Product;

        /// <summary>
        /// File version string
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string File;
    }

    /// <summary>
    /// Basic device information structure. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LScanDeviceInfo
    {
        /// <summary>
        /// Device serial number
        /// </summary>  
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string serialNumber;

        /// <summary>
        /// Device type name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string typeName;

        /// <summary>
        /// Device interface type (USB, Firewire)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string interfaceType;

    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LSCAN_Controls_KeyValue
    {
        /// <summary>
        /// a unique key for identifying values within a given template
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string key;

        /// <summary>
        /// a value associated to a given @e key
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string value;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LSCAN_Controls_KeyValue2
    {
        /// <summary>
        /// a unique key for identifying values within a given template
        /// </summary>
        public IntPtr key;

        /// <summary>
        /// a value associated to a given @e key
        /// </summary>
        public IntPtr value;
    }

    /// <summary>
    /// String to pass as an "out" parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LScanResultString
    {
        /// <summary>
        /// The return string
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LSE_Constants.LSCAN_MAX_STR_LEN)]
        public string returnString;
    }

    /// Image format constants.
    public enum emumImageFormat
    {
        /// <summary>
        /// Gray scale image
        /// </summary>
        IMG_FORMAT_GRAY,
        /// <summary>
        /// 24 bit RGB color image
        /// </summary>
        IMG_FORMAT_RGB24,
        /// <summary>
        /// True color RGB image
        /// </summary>
        IMG_FORMAT_RGB32,
        /// <summary>
        /// Format not set or unknown
        /// </summary>
        IMG_FORMAT_UNKNOWN
    }

    
}
