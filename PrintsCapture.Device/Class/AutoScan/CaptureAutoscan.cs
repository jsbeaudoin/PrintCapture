namespace PrintsCapture.Device.AutoScan
{
    using System;
    using System.Timers;

    public class OngoingTimeoutArgs : EventArgs
    {
        public OngoingTimeoutArgs(int secondElapsed, int secondRemaining, bool noTimeLeft, bool interrupted = false)
        {
            this.SecondElapsed = secondElapsed;
            this.NoTimeLeft = noTimeLeft;
            this.SecondRemaining = secondRemaining;
            this.Interrupted = interrupted;
        }

        public int SecondElapsed { get; private set; }
        public int SecondRemaining { get; private set; }
        public bool NoTimeLeft { get; private set; }
        public bool Interrupted { get; private set; }
    }

    public class CaptureAutoScan
    {
        const int OngoingInterval = 500;

        private readonly bool canTimeout;        

        private DateTime startTime = DateTime.MinValue;

        private bool timeoutOccured;
        private bool captureStarted;
        
        private Timer elapsedTimer;

        public delegate void OngoingTimeoutDelegate(object sender, OngoingTimeoutArgs e);

        /// <summary>
        /// Occurs at each second when a capture timeout is about to be triggered until it has been triggered.
        /// </summary>
        public event OngoingTimeoutDelegate OngoingTimeOut;

        public CaptureAutoScan(bool canTimeout, int delay)
        {
            this.canTimeout = canTimeout;
            this.CaptureDelay = delay;            
        }


        public void InitializeCapture(int expectedFingers)
        {            
            this.CapturedPrintExpected = expectedFingers;
            this.startTime = DateTime.MinValue;
            this.timeoutOccured = false;
            this.captureStarted = false;
        }

        /// <summary>
        /// Gets or sets the capture delay in second before triggering a manual capture.
        /// </summary>
        /// <value>
        /// The capture delay.
        /// </value>
        public int CaptureDelay { get; set; }

        /// <summary>
        /// Gets or sets the nomber of captured print expected.
        /// </summary>
        /// <value>
        /// The number of captured print expected.
        /// </value>
        public int CapturedPrintExpected { get; set; }        

        /// <summary>
        /// Gets a value indicating whether the capture timed out at the last VerifyTimeout call
        /// </summary>
        /// <value>
        ///   <c>true</c> if [capture timed out]; otherwise, <c>false</c>.
        /// </value>
        public bool CaptureTimedOut { get { return this.timeoutOccured; } }

        public bool HasStarted
        {
            get
            {
                return this.captureStarted;
            }
        }

        /// <summary>
        /// Starts the timeout if the number of print matches the expected count
        /// </summary>
        /// <param name="printCount">Count of print on the plate.</param>
        /// <returns></returns>
        public bool StartTimeout(int printCount)
        {
            bool result = false;
            Console.WriteLine("CaptureAutoScan.StartTimeout called");
            // if there is no timeout set, or the finger cannot trigger a timeout, stop
            if (this.timeoutOccured || !this.canTimeout)
            {
                return false;
            }

            if (printCount == this.CapturedPrintExpected)
            {
                if (!this.captureStarted)
                {                    
                    this.startTime = DateTime.Now;
                    this.captureStarted = true;
                    result = true;
                    this.StartOrResetTimer();
                }
            }
            else if (this.captureStarted)
            {
                this.captureStarted = false;
                this.StopTimeout();
            }

            return result;
        }

        public void StopTimeout()
        {
            
            if (this.elapsedTimer != null)
            {
                Console.WriteLine("CaptureAutoScan.StopTimeout");
                this.captureStarted = false;
                this.elapsedTimer.Stop();
                var args = new OngoingTimeoutArgs(0, this.CaptureDelay, false, true);

                this.OnOngoingTimeOut(args);
            }
        }


        private void StartOrResetTimer()
        {
            Console.WriteLine("CaptureAutoScan.StartOrResetTimer called");
            if (this.elapsedTimer != null)
            {
                this.elapsedTimer.Stop();
            }

            this.elapsedTimer = new Timer(this.CaptureDelay * 1000) { Interval = OngoingInterval };
            this.elapsedTimer.Elapsed += this.TimeElapsed;
            this.elapsedTimer.Start();
        }

        private void TimeElapsed(object sender, ElapsedEventArgs e)
        {
            var secondsElapsed = (int)e.SignalTime.Subtract(this.startTime).TotalSeconds;
            var timeLeft = this.CaptureDelay - secondsElapsed;
            if (timeLeft < 0)
            {
                timeLeft = 0;
            }

            if (timeLeft == 0)
            {
                this.timeoutOccured = true;
                this.StopTimeout();
            }

            var args = new OngoingTimeoutArgs(secondsElapsed, timeLeft, this.timeoutOccured);

            this.OnOngoingTimeOut(args);
        }

        private void OnOngoingTimeOut(OngoingTimeoutArgs e)
        {
            var handler = this.OngoingTimeOut;
            if (handler == null)
            {
                return;
            }

            handler(this, e);
        }

    }
}
