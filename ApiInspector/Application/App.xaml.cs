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
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [startup].
        /// </summary>
        void OnStartup(object sender, StartupEventArgs e)
        {
            var errorMonitor = new ErrorMonitor(this);

            errorMonitor.StartMonitor();

            var injector = new AppInjector(errorMonitor);

            MainWindow = injector.Get<View>();

            MainWindow?.Show();
        }
        #endregion
    }
}