using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Device.LivescanThales.Plugin
{
    using System.Drawing;

    using PrintsCapture.Device.LivescanThales.Sdk;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;

    internal class PrintCaptureInformation
    {
        public PrintCaptureInformation(PrintMapping mappedPrint, uint scanObjectType, PrintResolution resolution, bool isFlatCapture)
        {
            this.Resolution = resolution;
            this.MappedPrint = mappedPrint;
            this.ScanObjectType = scanObjectType;
            this.CaptureSize = PrintInfoExtension.GetSize(
                mappedPrint.HandPart,
                HandPartKind.Unknown,
                mappedPrint.ScanKind,
                resolution,
                isFlatCapture);
        }

        public uint ScanObjectType { get; private set; }

        public PrintResolution Resolution { get; private set; }

        public PrintMapping MappedPrint { get; private set; }

        public Size CaptureSize { get; private set; }
    }
}
