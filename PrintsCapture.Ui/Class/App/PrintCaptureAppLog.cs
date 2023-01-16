namespace PrintsCapture.Ui.Class
{
    using System;

    using NLog;

    using XL_ID.Utilities.Log;    

    public  static class PrintCaptureAppLog
    {
        private static readonly Logger CurrentLogger;

        static PrintCaptureAppLog()
        {
            CurrentLogger = LogManager.GetCurrentClassLogger();
            LogDispatcher.LogReceived += LogDispatcherOnLogReceived;
        }

        private static void LogDispatcherOnLogReceived(string msg, LogEventLevel level, Exception ex)
        {
            var consoleText = msg;
            if (ex != null)
            {
                consoleText += "\n" + ex.ToString();
            }

            Console.WriteLine(@"{0:yyyy-MM-dd HH:mm:ss} - DeveiApiCrossmatch - {1}", DateTime.Now, consoleText);
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
