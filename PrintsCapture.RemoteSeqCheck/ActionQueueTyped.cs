using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace PrintsCapture.RemoteSeqCheck
{
    public class ActionQueue<T> : IDisposable
    {
        private const int MaxWaitDelay = 20000;
        private const int IdleWaitDelay = 100;
        private const int ActionPauseDelay = 50;
        private readonly T actionDependant;
        private readonly int paralleRunLimit;

        private readonly ConcurrentQueue<Action<T>> actionQueue;
        private readonly ConcurrentDictionary<Action<T>, bool> awaitedActionList;

        private bool isDisposing;

        private BackgroundWorker bgWorker;
        private int actionRunning;

        public ActionQueue(T dependant, int paralleRunLimit = 1)
        {
            actionDependant = dependant;
            this.paralleRunLimit = paralleRunLimit;
            actionQueue = new ConcurrentQueue<Action<T>>();
            this.awaitedActionList = new ConcurrentDictionary<Action<T>, bool>();
            this.StartThread();
        }

        public void Add(Action<T> action)
        {
            this.actionQueue.Enqueue(action);
        }

        public void AddAndWait(Action<T> action)
        {
            bool errorAwait = !this.awaitedActionList.TryAdd(action, false);
            this.actionQueue.Enqueue(action);

            if (errorAwait)
            {
                throw new ApplicationException("Could not wait for action completion. Error adding to the await list.");
            }

            var waitTotal = 0;
            while (!this.awaitedActionList[action] && waitTotal < MaxWaitDelay)
            {
                Thread.Sleep(IdleWaitDelay);
                waitTotal += IdleWaitDelay;
            }

            bool isWaiting;
            this.awaitedActionList.TryRemove(action, out isWaiting);

            if (waitTotal >= MaxWaitDelay)
            {
                throw new ApplicationException("Maximum wait time for ActionQueue reached.");
            }
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
                        this.RunAction();
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

        private void RunAction()
        {
            while (actionRunning >= this.paralleRunLimit)
            {
                Thread.Sleep(ActionPauseDelay);
            }
            
            actionRunning += 1;
            Action<T> currentAction;
            if (actionQueue.TryDequeue(out currentAction))
            {
                var runActionCommand = new Action(() =>
                {
                    currentAction.Invoke(actionDependant);
                    actionRunning -= 1;
                    if (this.awaitedActionList.ContainsKey(currentAction))
                    {
                        Thread.Sleep(ActionPauseDelay);
                        this.awaitedActionList[currentAction] = true;
                    }
                });

                Task.Factory.StartNew(runActionCommand);
            }
            
            Thread.Sleep(ActionPauseDelay);
        }

        private void StartThread()
        {
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.DoWork += (sender, args) => this.Run();
            this.bgWorker.RunWorkerAsync();
        }
    }
}
