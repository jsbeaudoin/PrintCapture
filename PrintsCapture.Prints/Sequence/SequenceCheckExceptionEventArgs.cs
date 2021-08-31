namespace PrintsCapture.Prints.Sequence
{
    using System;

    public class SequenceCheckExceptionEventArgs : EventArgs
    {
        public SequenceCheckExceptionEventArgs(string failedOperation, Exception innerEx)
        {
            this.Exception = innerEx;
            this.Operation = failedOperation;
        }

        public Exception Exception { get; private set; }

        public string Operation { get; private set; }
    }
}
