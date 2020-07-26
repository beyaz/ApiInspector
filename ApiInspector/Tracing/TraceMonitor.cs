using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ApiInspector.Tracing
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
        ///     The trace queue
        /// </summary>
        readonly TraceQueue traceQueue;

        /// <summary>
        ///     The trace viewer
        /// </summary>
        readonly RichTextBox traceViewer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMonitor" /> class.
        /// </summary>
        public TraceMonitor(RichTextBox traceViewer, Dispatcher dispatcher, TraceQueue traceQueue)
        {
            this.traceViewer = traceViewer;
            this.dispatcher  = dispatcher;
            this.traceQueue  = traceQueue;
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
                var traceMessages = traceQueue.GetAllMessagesInQueue();
                traceQueue.ClearQueue();
                foreach (var message in traceMessages)
                {
                    traceViewer.AppendText("\r" + message);
                    traceViewer.ScrollToEnd();
                }
            });
        }
        #endregion
    }
}