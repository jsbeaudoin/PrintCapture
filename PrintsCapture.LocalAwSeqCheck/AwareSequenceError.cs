// Takes the Exception message string and extracts the error number. error enum from Aware below.
namespace PrintsCapture.LocalAwSeqCheck
{
    using System;

    using Aware.AwSequence;

    internal static class AwareSequenceError
    {
        public static awSequenceCheck.errorCode ExceptionToError(Exception Ex)
        {
            int noError = 0;
            string msg = Ex.Message;
            int posDelim = msg.LastIndexOf(':');
            //AwareSeqError result = AwareSeqError.AWSEQ_NO_ERRORS;

            if (posDelim > 0)
            {
                int.TryParse(msg.Substring(posDelim + 1), out noError);
            }

            return (awSequenceCheck.errorCode)noError;
        }
    }
}