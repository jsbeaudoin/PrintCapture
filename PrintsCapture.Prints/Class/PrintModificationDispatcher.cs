namespace PrintsCapture.Prints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NLog;

    using PrintsCapture.Prints.Delegates;

    public class PrintModificationDispatcher
    {        

        private static readonly List<PrintModifiedHandler> WatchList = new List<PrintModifiedHandler>();

        private static readonly object Locker = new object();

        private static Logger logger;

        static PrintModificationDispatcher()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public static void AddWatch(PrintModifiedHandler callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Handler can't be null", "callback");
            }            

            lock (Locker)
            {
                WatchList.Add(callback);
            }
            
        }

        public static void ClearAll()
        {
            lock (Locker)
            {
                WatchList.Clear();
            }
        }

        public static void RemoveWatch(PrintModifiedHandler callback)
        {
            var allToRemove = WatchList.Where(x => x == callback).ToList();

            lock (Locker)
            {
                foreach (var watchInfo in allToRemove)
                {
                    WatchList.Remove(watchInfo);
                }
            }
        }

        public static void PrintModified(PrintInfo info, PrintModifiedHandler ignoredCallback = null)
        {
            logger.Info("Print Modified called for : {0}", info.Key);
            List<PrintModifiedHandler> allToWarn;
            lock (Locker)
            {
                allToWarn = new List<PrintModifiedHandler>(WatchList);
            }


            foreach (var watchInfo in allToWarn)
            {
                try
                {
                    if (watchInfo != ignoredCallback)
                    {
                        watchInfo(null, new PrintModifiedEventArgs(info));
                    }
                    
                }
                catch (Exception ex)
                {
                    // rien
                    LogManager.GetCurrentClassLogger().Error(ex, "PrintModified Exception. Could not execute callback");
                }
            }
            logger.Info("Print Modified ended for : {0}", info.Key);

        }
    }
}
