using System.Collections.Generic;

namespace ApiInspector.Tracing
{
    /// <summary>
    ///     The tracer
    /// </summary>
    public interface ITracer
    {
        #region Public Methods
        /// <summary>
        ///     Traces the specified message.
        /// </summary>
        void Trace(string message);
        #endregion
    }

    /// <summary>
    ///     The trace queue
    /// </summary>
    class TraceQueue : ITracer
    {
        #region Fields
        /// <summary>
        ///     The trace messages
        /// </summary>
        readonly List<string> traceMessages = new List<string>();
        #endregion

        #region Public Methods
        /// <summary>
        ///     Adds the message.
        /// </summary>
        public void AddMessage(string message)
        {
            traceMessages.Add(message);
        }

        /// <summary>
        ///     Clears the queue.
        /// </summary>
        public void ClearQueue()
        {
            traceMessages.Clear();
        }

        /// <summary>
        ///     Gets all messages in queue.
        /// </summary>
        public IReadOnlyList<string> GetAllMessagesInQueue()
        {
            return traceMessages.ToArray();
        }

        /// <summary>
        ///     Traces the specified message.
        /// </summary>
        public void Trace(string message)
        {
            AddMessage(message);
        }
        #endregion
    }
}