// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILivescanDevice.cs" company="Solutions XL-ID inc">
//   Update text
// </copyright>
// <summary>
//   Defines the CapturedPrintHandler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PrintsCapture.Device.Interface
{
    using System.Collections.Generic;
    using System.Drawing;

    using PrintsCapture.Device;    
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    public enum DeviceMessageKind
    {
        Information,
        Error
    }

    public delegate void CapturedPrintHandler(PrintResolution resolution, Bitmap printImage);

    public delegate void CapturePreviewHandler(Bitmap previewImage);

    public delegate void DeviceMessageHandler(string text, DeviceMessageKind kind, bool showResumeButton);

    public delegate void DeviceOpenedHandler(DeviceOpenStatus openResult);

    public delegate void CaptureQualityHandler(IEnumerable<PrintCaptureQuality> qualityList);

    public interface ILivescanDevice : ICaptureDevice
    {
        #region Properties        
        
        CustomPropertyList Properties { get; }

        SdkException LastException { get; }       

        PrintResolution FingerResolution { get; set; }

        PrintResolution PalmResolution { get; set; }

        string ExternalTool { get; }        

        #endregion

        #region Events

        event DeviceMessageHandler DeviceSendMessage;

        event DeviceOpenedHandler DeviceOpened;

        event CapturedPrintHandler PrintCaptured;

        event CaptureQualityHandler CaptureQualityChanged;

        #endregion

        #region Methods        

        /// <summary>
        /// Initializes information for capture preview.
        /// </summary>        
        /// <param name="handParts">all hand parts, so missing can be determined</param>
        /// <param name="preview">
        /// The preview image channel.
        /// </param>
        /// <param name="previewWindowHandle">
        /// Handle of Window used to display preview images if channel is not provided
        /// </param>
        /// <param name="isFlat">Indicate that the capture print set is flat only</param>
        /// <returns>
        /// <see cref="bool"/> indicating if initialization was successful.
        /// </returns>
        bool InitializeCapture(IEnumerable<PhysicalHandPart> handParts, CapturePreviewHandler preview, int previewWindowHandle, bool isFlat);

        /// <summary>
        /// Captures a single print.
        /// </summary>
        /// <param name="resolution">
        /// The resolution.
        /// </param>
        /// <param name="printHand"></param>
        /// <param name="handPart"></param>
        /// <param name="scanKind"></param>
        bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind);

        bool StopCapture();              

        #endregion
    }
}
