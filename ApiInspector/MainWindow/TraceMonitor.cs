using System.Collections.Generic;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The trace monitor
    /// </summary>
    class TraceMonitor
    {
        #region Fields
        /// <summary>
        ///     The dispatcher
        /// </summary>
        readonly Dispatcher dispatcher;

        /// <summary>
        ///     The trace messages
        /// </summary>
        readonly List<string> traceMessages;

        /// <summary>
        ///     The trace viewer
        /// </summary>
        readonly RichTextBox traceViewer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMonitor" /> class.
        /// </summary>
        public TraceMonitor(RichTextBox traceViewer, Dispatcher dispatcher, List<string> traceMessages)
        {
            this.traceViewer   = traceViewer;
            this.dispatcher    = dispatcher;
            this.traceMessages = traceMessages;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts to monitor.
        /// </summary>
        public void StartToMonitor()
        {
            var timer = new Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [timed event].
        /// </summary>
        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            dispatcher.Invoke(() =>
            {
                foreach (var message in traceMessages)
                {
                    traceViewer.AppendText("\r" + message);
                    traceViewer.ScrollToEnd();
                }

                traceMessages.Clear();
            });
        }
        #endregion
    }
}