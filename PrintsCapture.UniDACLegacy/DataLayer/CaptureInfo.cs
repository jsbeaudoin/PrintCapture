namespace PrintsCapture.UniDACLegacy.DataLayer
{
    using System.Collections.Generic;

    /// <summary>
    /// Capture Info XML configuration Layout
    /// </summary>
    public class CaptureInfo
    {
        /// <summary>
        /// Gets or sets the capture device serial number.
        /// </summary>
        /// <value>The capture device serial number.</value>
        public string CaptureDeviceSerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the capture device make.
        /// </summary>
        /// <value>The capture device make.</value>
        public string CaptureDeviceMake { get; set; }

        /// <summary>
        /// Gets or sets the name of the capture device.
        /// </summary>
        /// <value>
        /// The name of the capture device.
        /// </value>
        public string CaptureDeviceModel { get; set; }

        /// <summary>
        /// Gets or sets the capture mode.
        /// </summary>
        /// <value>
        /// The capture mode.
        /// </value>
        public string CaptureMode { get; set; }

        /// <summary>
        /// Gets or sets the file name prefix.
        /// </summary>
        /// <value>
        /// The file name prefix.
        /// </value>
        public string FileNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the fingerprints.
        /// </summary>
        /// <value>
        /// The fingerprints.
        /// </value>

        public List<Fingerprint> Fingerprints { get; set; }

    }
}
