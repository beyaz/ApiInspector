using System.Windows.Threading;
using Notifications.Wpf;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Static Fields
        /// <summary>
        ///     The notification manager
        /// </summary>
        static readonly NotificationManager notificationManager = new NotificationManager();
        #endregion

        #region Constructors
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Shows the error notification.
        /// </summary>
        public static void ShowErrorNotification(string message)
        {
            ShowNotification(null, message, NotificationType.Error);
        }

        /// <summary>
        ///     Shows the success notification.
        /// </summary>
        public static void ShowSuccessNotification(string message)
        {
            ShowNotification(null, message, NotificationType.Success);
        }
        #endregion

        #region Methods
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

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorNotification(e.Exception.Message);

            // Setting 'Handled' to 'true' will prevent the application from terminating.
            e.Handled = true;
        }
        #endregion
    }
}