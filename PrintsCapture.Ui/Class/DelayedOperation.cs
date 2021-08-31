namespace PrintsCapture.Ui.Class
{
    using System;
    using System.Timers;

    /// <summary>
    /// Raise OperationCalled event after a time of [interval ms] has ellapsed since last call to DoOperation
    /// </summary>
    public class DelayedOperation
    {
        private readonly Timer delayedNotificationTimer;

        private bool enabled;

        /// <summary>
        /// Public Constructor of delayed operation
        /// </summary>
        /// <param name="intervalMs"></param>
        public DelayedOperation(int intervalMs)
        {
            this.enabled = true;
            this.delayedNotificationTimer = new Timer();
            this.delayedNotificationTimer.Interval = intervalMs;
            this.delayedNotificationTimer.Elapsed += this.OperationTimerOnElapsed;
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
                if (!value)
                {
                    this.delayedNotificationTimer.Stop();
                }
            }
        }

        /// <summary>
        /// When the Rule object properties are changed, after last changed and a delay the event is raised. This allow to change multiple values without changing on the fly.
        /// </summary>
        public event EventHandler OperationCalled;

        public void DoOperation()
        {
            if (!this.Enabled)
            {
                return;
            }

            this.delayedNotificationTimer.Stop();
            this.delayedNotificationTimer.Start();
        }

        private void OperationTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            this.delayedNotificationTimer.Stop();

            var handler = this.OperationCalled;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    
}

