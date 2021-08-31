using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Device.Class
{
    public class CaptureCompletedEventArgs : EventArgs
    {
        public bool HasError { get; }

        public CaptureCompletedEventArgs(bool hasError)
        {
            HasError = hasError;
        }
    }
}
