namespace PrintsCapture.Ui.Exceptions
{
    using System;

    public class LiveScanException : ApplicationException
    {
        public LiveScanException(string message, System.Exception innerException) : base(message, innerException)
        {  }


    }
}
