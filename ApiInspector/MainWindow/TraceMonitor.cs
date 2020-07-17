using System.Collections.Generic;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ApiInspector.MainWindow
{
    class TraceMonitor
    {
        readonly RichTextBox  traceViewer;
        readonly Dispatcher   dispatcher;
        readonly List<string> traceMessages;

        public TraceMonitor(RichTextBox traceViewer, Dispatcher dispatcher, List<string> traceMessages)
        {
            this.traceViewer   = traceViewer;
            this.dispatcher    = dispatcher;
            this.traceMessages = traceMessages;
        }

        public void StartToMonitor()
        {
            var timer = new Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

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
    }
}