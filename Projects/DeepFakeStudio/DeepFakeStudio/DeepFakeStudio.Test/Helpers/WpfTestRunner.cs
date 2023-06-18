namespace DeepFakeStudio.Test.Helpers
{
    using System;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Defines the <see cref="WpfTestRunner" />.
    /// </summary>
    public static class WpfTestRunner
    {
        #region Methods

        /// <summary>
        /// The Run.
        /// </summary>
        /// <param name="action">The action<see cref="Action{Application}"/>.</param>
        public static void Run(Action<Application> action)
        {
            var app = Application.Current;
            if (app == null)
            {
                app = new Application { ShutdownMode = ShutdownMode.OnLastWindowClose };
            }

            app.Dispatcher.Invoke(() =>
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                {
                    action(app);
                    app.Shutdown();
                }
                else
                {
                    var thread = new Thread(() =>
                    {
                        action(app);
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
            });
        }

        #endregion Methods
    }
}