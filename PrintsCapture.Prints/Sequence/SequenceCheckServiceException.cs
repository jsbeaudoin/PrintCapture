namespace PrintsCapture.Prints.Sequence
{
    using System;

    public class SequenceCheckServiceException : ApplicationException
    {
        public SequenceCheckServiceException(string message, Exception innerEx) : base(message, innerEx) {  }

        public SequenceCheckServiceException(string message) : base(message) { }
    }
}
