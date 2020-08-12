using System.Windows;
using ApiInspector.MainWindow;
using ApiInspector.Tracing;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            BoaAssemblyResolver.AttachToCurrentDomain();

            ErrorMonitor = new ErrorMonitor(this);

            ErrorMonitor.StartMonitor();
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the error monitor.
        /// </summary>
        internal ErrorMonitor ErrorMonitor { get; }
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [startup].
        /// </summary>
        void OnStartup(object sender, StartupEventArgs e)
        {
            var injector = new AppInjector();

            MainWindow = injector.Get<View>();

            MainWindow?.Show();
        }
        #endregion
    }
}