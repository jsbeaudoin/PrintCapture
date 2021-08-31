namespace PrintsCapture.Device.Interface
{
    using System.Drawing;

    using PrintsCapture.Prints.Enum;

    public interface ICardscanDevice : ICaptureDevice
    {
        /// <summary>
        /// Sets the device serial number
        /// </summary>
        /// <param name="serial"></param>
        void SetSerialNumber(string serial);

        /// <summary>
        /// Scans the zone at given resolution, after a given offset for a given size
        /// </summary>
        /// <param name="resolution">Dot Per inch resolution</param>
        /// <param name="offset">Offset starting from upper bound</param>
        /// <param name="size">Size to scan</param>
        /// <returns>Scanned Bitmap indexed image</returns>
        Bitmap Scan(PrintResolution resolution, float offset, float size);

        //string SearchKey { get; set; }
    }
}
