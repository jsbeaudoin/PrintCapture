using System;

using NLog;

using XL_ID.Utilities.Log;  

namespace PrintsCapture.Device.LivescanI3
{
    public static class DeviceLog
    {
        private static readonly Logger CurrentLogger;

        static DeviceLog()
        {
            CurrentLogger = LogManager.GetCurrentClassLogger();
            LogDispatcher.LogReceived += LogDispatcherOnLogReceived;
        }

        private static void LogDispatcherOnLogReceived(string msg, LogEventLevel level, Exception ex)
        {
            switch (level)
            {
                case LogEventLevel.Info:
                    CurrentLogger.Info(msg);
                    break;

                case LogEventLevel.Warning:
                    if (ex == null)
                    {
                        CurrentLogger.Warn(msg);
                    }
                    else
                    {
                        CurrentLogger.Warn(ex, msg);
                    }
                    break;

                case LogEventLevel.Error:
                    if (ex == null)
                    {
                        CurrentLogger.Error(msg);
                    }
                    else
                    {
                        CurrentLogger.Error(ex, msg);
                    }
                    break;

                default:
                    CurrentLogger.Warn(msg);
                    break;
            }
        }

        public static Logger Logger => CurrentLogger;

    }
}
