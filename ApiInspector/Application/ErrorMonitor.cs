using System.Windows.Threading;
using Notifications.Wpf;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The error monitor
    /// </summary>
    class ErrorMonitor
    {
        #region Static Fields
        /// <summary>
        ///     The notification manager
        /// </summary>
        static readonly NotificationManager notificationManager = new NotificationManager();
        #endregion

        #region Fields
        /// <summary>
        ///     The application
        /// </summary>
        readonly System.Windows.Application application;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorMonitor" /> class.
        /// </summary>
        public ErrorMonitor(System.Windows.Application application)
        {
            this.application = application;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts the monitor.
        /// </summary>
        public void StartMonitor()
        {
            application.DispatcherUnhandledException -= OnDispatcherUnhandledException;
            application.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Shows the error notification.
        /// </summary>
         public void ShowErrorNotification(string message)
        {
            ShowNotification(null, message, NotificationType.Error);
        }

        /// <summary>
        ///     Shows the notification.
        /// </summary>
        static void ShowNotification(string title, string message, NotificationType notificationType)
        {
            notificationManager.Show(new NotificationContent
            {
                Title   = title,
                Message = message,
                Type    = notificationType
            });
        }

        /// <summary>
        ///     Called when [dispatcher unhandled exception].
        /// </summary>
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorNotification(e.Exception.Message);

            // Setting 'Handled' to 'true' will prevent the application from terminating.
            e.Handled = true;
        }
        #endregion
    }
}