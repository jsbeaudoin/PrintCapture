using System;
using NLog;
using XL_ID.Utilities.Log;

namespace PrintsCapture.Device.LivescanJenetric
{
    public static class DeviceLog
    {
        static DeviceLog()
        {
            Logger = LogManager.GetCurrentClassLogger();
            LogDispatcher.LogReceived += LogDispatcherOnLogReceived;
        }

        private static void LogDispatcherOnLogReceived(string msg, LogEventLevel level, Exception ex)
        {
            switch (level)
            {
                case LogEventLevel.Info:
                    Logger.Info(msg);
                    break;

                case LogEventLevel.Warning:
                    if (ex == null)
                    {
                        Logger.Warn(msg);
                    }
                    else
                    {
                        Logger.Warn(ex, msg);
                    }
                    break;

                case LogEventLevel.Error:
                    if (ex == null)
                    {
                        Logger.Error(msg);
                    }
                    else
                    {
                        Logger.Error(ex, msg);
                    }
                    break;

                default:
                    Logger.Warn(msg);
                    break;
            }
        }

        public static Logger Logger { get; private set; }

    }
}
