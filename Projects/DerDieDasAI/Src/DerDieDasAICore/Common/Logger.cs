// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Common
{
    using System;
    using System.Text;

    public class Logger
    {
        #region Fields

        public static readonly Logger Instance = new();

        #endregion Fields

        #region Events

        public event EventHandler<string> LoggerUpdated;

        #endregion Events

        #region Properties

        public static Action<string> Log { get; set; }

        public int MaxLength { get; set; } = 100;

        public bool ShowInConsole { get; set; }

        #endregion Properties

        private StringBuilder _log = new();

        public Logger()
        {
#if DEBUG
            this.ShowInConsole = true;
#else
            this.ShowInConsole = false;
#endif
        }

        public void WriteLine(string message)
        {
            _log.AppendLine(message);
            LoggerUpdated?.Invoke(this, message);
            if (_log.Length > MaxLength)
            {
                _log.Remove(0, 1);
            }

            if (this.ShowInConsole)
            {
                System.Diagnostics.Trace.WriteLine(message);
            }
        }
    }
}