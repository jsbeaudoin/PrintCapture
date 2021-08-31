using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using NLog;

namespace PrintsCapture.RemoteSeqCheck
{
    public class ActionQueue : IDisposable
    {
        private const int IdleWaitDelay = 100;
        private const int ActionPauseDelay = 50;

        private readonly ConcurrentQueue<Action> actionQueue;

        private bool isDisposing;

        private BackgroundWorker bgWorker;

        public ActionQueue()
        {
            actionQueue = new ConcurrentQueue<Action>();

            this.StartThread();
        }

        public void Add(Action action)
        {
            this.actionQueue.Enqueue(action);
        }


        public void Run()
        {
            try
            {
                while (!this.isDisposing)
                {
                    if (actionQueue.IsEmpty)
                    {
                        Thread.Sleep(IdleWaitDelay);
                    }
                    else
                    {
                        Action currentAction;
                        if (actionQueue.TryDequeue(out currentAction))
                        {
                            currentAction.Invoke();
                            Thread.Sleep(ActionPauseDelay);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.isDisposing)
                {
                    return;
                }
                LogManager.GetCurrentClassLogger().Error(ex, "Action queue encountered an exception.");
                Thread.Sleep(IdleWaitDelay);
                this.StartThread();
            }
            
        }

        public void Dispose()
        {
            isDisposing = true;
            bgWorker.CancelAsync();
        }

        private void StartThread()
        {
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.DoWork += (sender, args) => this.Run();
            this.bgWorker.RunWorkerAsync();
        }
    }
}
