namespace VideoBrowser.Helpers
{
    using System;
    using System.Threading;

    /// <summary>
    /// Defines the <see cref="DelayCall" />.
    /// </summary>
    public class DelayCall : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayCall"/> class.
        /// </summary>
        /// <param name="action">The action<see cref="Action"/>.</param>
        /// <param name="milliSecondDelay">The milliSecondDelay<see cref="int"/>.</param>
        /// <param name="resetTimerOnCall">The resetTimerOnCall<see cref="bool"/>.</param>
        /// <param name="callInUIThread">The callInUIThread<see cref="bool"/>.</param>
        public DelayCall(Action action, int milliSecondDelay, bool resetTimerOnCall = true, bool callInUIThread = false)
        {
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            this.MilliSecondDelay = milliSecondDelay;
            this.IsResetTimerOnCall = resetTimerOnCall;
            this.IsCallInUIThread = callInUIThread;
            this.Timer = new Timer(this.OnTimer_Tick, null, int.MaxValue, int.MaxValue);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether Disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IsCallInUIThread.
        /// </summary>
        private bool IsCallInUIThread { get; }

        /// <summary>
        /// Gets the Action.
        /// </summary>
        private Action Action { get; }

        /// <summary>
        /// Gets or sets a value indicating whether IsResetTimerOnCall.
        /// </summary>
        private bool IsResetTimerOnCall { get; }

        /// <summary>
        /// Gets or sets a value indicating whether IsRunning.
        /// </summary>
        private bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the LastCallTime.
        /// </summary>
        private long LastCallTime { get; set; }

        /// <summary>
        /// Gets or sets the MilliSecondDelay.
        /// </summary>
        private int MilliSecondDelay { get; }

        /// <summary>
        /// Gets the Timer.
        /// </summary>
        private Timer Timer { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Call.
        /// </summary>
        /// <param name="delayInMilliseconds">The delayInMilliseconds<see cref="long"/>.</param>
        public void Call(long delayInMilliseconds = 0)
        {
            if (this.Disposed)
            {
                return;
            }

            this.Stop();
            if (this.IsRunning)
            {
                var elapsedTime = new TimeSpan(DateTime.Now.Ticks - this.LastCallTime);
                if (elapsedTime.Milliseconds > this.MilliSecondDelay)
                {
                    this.Action();
                    return;
                }
            }

            delayInMilliseconds = delayInMilliseconds == 0 ? this.MilliSecondDelay : delayInMilliseconds;
            this.LastCallTime = DateTime.Now.Ticks;
            this.Restart(delayInMilliseconds);
        }

        /// <summary>
        /// The Dispose.
        /// </summary>
        public void Dispose()
        {
            this.Cancel();
            if (this.Disposed)
            {
                return;
            }

            this.Disposed = false;
            this.Timer.Dispose();
        }

        /// <summary>
        /// The Cancel.
        /// </summary>
        private void Cancel()
        {
            this.IsRunning = false;
            this.Stop();
        }

        /// <summary>
        /// The OnTimer_Tick.
        /// </summary>
        /// <param name="state">The state<see cref="object"/>.</param>
        private void OnTimer_Tick(object state)
        {
            this.Cancel();
            if (this.Disposed)
            {
                return;
            }

            this.Action();
        }

        /// <summary>
        /// The Restart.
        /// </summary>
        /// <param name="delayInMilliseconds">The delayInMilliseconds<see cref="long"/>.</param>
        private void Restart(long delayInMilliseconds)
        {
            this.Timer.Change(delayInMilliseconds, delayInMilliseconds);
        }

        /// <summary>
        /// The Stop.
        /// </summary>
        private void Stop()
        {
            if (this.Disposed)
            {
                return;
            }

            this.Timer.Change(int.MaxValue, int.MaxValue);
        }

        #endregion Methods
    }
}