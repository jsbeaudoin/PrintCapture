namespace PrintsCapture.Device
{
    using System;

    using PrintsCapture.Device.Enum;

    public class SdkException : ApplicationException
    {
        public const string ErrorMessagePrefix = "SdkError";

        public string DeviceName { get; private set; }

        public string ApiOperation { get; private set; }

        public SdkErrorKind ErrorKind { get; private set; }

        public  bool IsWarningOnly { get; private set; }

        /// <summary>
        /// Basic Sdk Exception constructor. This kind of error is meant to be handled and translated if possible
        /// </summary>
        /// <param name="deviceName">Device on which the error was caused</param>
        /// <param name="apiOperation">Operation (For log purpose)</param>
        /// <param name="errorKind">Generic Sdk error kind. (Allow for translated error messages)</param>
        /// <param name="message">Message inserted after the translated error, if any</param>
        /// <param name="warnOnly">Error is only a warning, user can ignore it</param>
        /// <param name="innerException"></param>
        public SdkException(string deviceName, string apiOperation, SdkErrorKind errorKind, string message, bool warnOnly = false, Exception innerException = null)
            : base(SetMessage(errorKind, message), innerException)
        {
            this.DeviceName = deviceName;
            this.ApiOperation = apiOperation;
            this.ErrorKind = errorKind;
            this.IsWarningOnly = warnOnly;

        }

        private static string SetMessage(SdkErrorKind errorKind, string message)
        {
            if (errorKind == SdkErrorKind.SpecificError)
            {
                return message;
            }

            var completeMessage = Prints.Language.CommonText.ResourceManager.GetString(ErrorMessagePrefix + errorKind);

            if (!string.IsNullOrEmpty(message))
            {
                completeMessage += " : " + message;
            }

            return completeMessage;
        }
    }

}
