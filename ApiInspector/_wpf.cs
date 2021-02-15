using System.Windows;

namespace ApiInspector
{
    /// <summary>
    ///     The
    /// </summary>
    static partial class _
    {
        #region Public Methods
        /// <summary>
        ///     Shutdowns the application when closed.
        /// </summary>
        public static void ShutdownApplicationWhenClosed(Window window)
        {
            window.Closed += (s, e) => { System.Windows.Application.Current.Shutdown(); };
        }
        #endregion
    }
}