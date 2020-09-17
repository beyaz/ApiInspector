using System;
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
        ///     Applies the skin.
        /// </summary>
        void ApplySkin()
        {
            try
            {
                var xamlPathList = new[]
                {
                    "Themes/Metro/Light/Metro.MSControls.Core.Implicit.xaml",
                    "Themes/Metro/Light/Metro.MSControls.Toolkit.Implicit.xaml"
                };

                foreach (var pathInAssembly in xamlPathList)
                {
                    Resources.MergedDictionaries.Add(new ResourceDictionary {Source = new Uri($"pack://application:,,,/ApiInspector;component/{pathInAssembly}")});
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Called when [startup].
        /// </summary>
        void OnStartup(object sender, StartupEventArgs e)
        {
            ApplySkin();

            var errorMonitor = new ErrorMonitor(this);

            errorMonitor.StartMonitor();

            var injector = new AppInjector(errorMonitor);

            MainWindow = injector.Get<View>();

            MainWindow?.Show();
        }
        #endregion
    }
}