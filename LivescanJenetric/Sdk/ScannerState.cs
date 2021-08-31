using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintsCapture.Device.LivescanJenetric.Sdk
{
    /// <summary>
    /// Represents the scanner state.
    /// </summary>
    public enum ScannerState
    {
        /// <summary>
        /// The scanner is closed.
        /// </summary>
        Closed,
        /// <summary>
        /// The scanner is idle.
        /// </summary>
        Idle,
        /// <summary>
        /// The scanner begins the open-operation
        /// </summary>
        BeginOpen,
        /// <summary>
        /// The scanner begins the beep-operation.
        /// </summary>
        BeginBeep,
        /// <summary>
        /// The scanner begins to add-logo-operation.
        /// </summary>
        BeginAddLogo,
        /// <summary>
        /// The scanner begins the preview-operation.
        /// </summary>
        BeginPreview,
        /// <summary>
        /// The scanner begins the abort-operation.
        /// </summary>
        BeginAbort,
        /// <summary>
        /// The scanner begins the manual-scan-operation.
        /// </summary>
        BeginManualScan,
        /// <summary>
        /// The scanner begins the auto-scan-operation.
        /// </summary>
        BeginAutoScan,
        /// <summary>
        /// The scanner begins the touch-operation.
        /// </summary>
        BeginTouch,
        /// <summary>
        /// The scanner begins
        /// </summary>
        BeginSignature,
        /// <summary>
        /// The scanner begins the close-operation.
        /// </summary>
        BeginClose,
        /// <summary>
        /// The scanner is in preview mode.
        /// </summary>
        Preview,
        /// <summary>
        /// The scanner is in auto-scan mode.
        /// </summary>
        AutoScan,
        /// <summary>
        /// The scanner is in signature-recording mode.
        /// </summary>
        Signature,
        /// <summary>
        /// The scanner is in touch-display mode.
        /// </summary>
        Touch,
    }
}
