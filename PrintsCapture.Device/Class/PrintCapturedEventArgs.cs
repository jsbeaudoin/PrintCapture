using System;

namespace PrintsCapture.Device
{
    using PrintsCapture.Prints;

    public class PrintCapturedEventArgs : EventArgs
    {
        public PrintInfo Print { get; private set; }

        public PrintCapturedEventArgs(PrintInfo print)
        {
            this.Print = print;
        }

        /// <summary>
        /// Batch behavior. Null means no batch. False means it is part of a batch, but do not sent it yet. 
        /// True means that the entire batch may now be processed
        /// </summary>
        public bool? SendBatch { get; set; }

        public bool KeepOriginalImage { get; set; }
    }
}
