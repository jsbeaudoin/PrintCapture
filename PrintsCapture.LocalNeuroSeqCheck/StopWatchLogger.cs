

namespace PrintsCapture.LocalNeuroSeqCheck
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using NLog;

    public static class StopwatchLogger
    {
        public static void Run(Action action, string actionName, string additionalInfo = "")
        {
            var sw = Stopwatch.StartNew();

            try
            {
                action();
            }
            finally
            {
                sw.Stop();

                LogManager.GetCurrentClassLogger().Trace("{0} | {1}ms | {2}", actionName, sw.ElapsedMilliseconds, additionalInfo);
            }
        }

        public static TResult Run<TResult>(Func<TResult> action, string actionName, string additionalInfo = "")
        {
            TResult result;
            var sw = Stopwatch.StartNew();

            try
            {
                result = action();
            }
            finally
            {
                sw.Stop();

                LogManager.GetCurrentClassLogger().Trace("{0} | {1}ms | {2}", actionName, sw.ElapsedMilliseconds, additionalInfo);
            }

            return result;
        }

        public static TResult RunStopwatch<TObject, TResult>(this TObject obj, Expression<Func<TObject, TResult>> action, string actionName, string additionalInfo = "")
        {
            TResult result;
            var sw = Stopwatch.StartNew();

            try
            {
                result = action.Compile()(obj);
            }
            finally
            {
                sw.Stop();

                LogManager.GetCurrentClassLogger().Trace("{0} | {1}ms | {2}", actionName, sw.ElapsedMilliseconds, additionalInfo);
            }

            return result;
        }

        public static void RunStopwatch<TObject>(this TObject obj, Expression<Action<TObject>> action, string actionName, string additionalInfo = "")
        {
            var sw = Stopwatch.StartNew();

            try
            {
                action.Compile()(obj);
            }
            finally
            {
                sw.Stop();

                LogManager.GetCurrentClassLogger().Trace("{0} | {1}ms | {2}", actionName, sw.ElapsedMilliseconds, additionalInfo);
            }
        }
    }
}
