using System;
using System.Windows;
using System.Windows.Input;
using ApiInspector.MainWindow;
using ApiInspector.Plugins;
using ApiInspector.Tracing;
using static ApiInspector._;
using static ApiInspector.MainWindow.Mixin;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    partial class App
    {

        public static System.Windows.Input.Cursor DefaultCursor => System.Windows.Input.Cursors.Arrow;


        #region Static Fields
        internal static readonly Scope AppScope = new Scope();

        static ErrorMonitor errorMonitor;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            AttachBoaSystemAssemblyResolverToCurrentDomain();
            errorMonitor = new ErrorMonitor(this);

            Mouse.OverrideCursor = DefaultCursor;
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
                    "Themes/Metro/Light/Theme.Colors.xaml",
                    "Themes/Metro/Light/Styles.Shared.xaml",
                    "Themes/Metro/Light/Styles.WPF.xaml"
                    // "Themes/Metro/Light/Metro.MSControls.Toolkit.Implicit.xaml"
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
            BoaPlugin.Attach();

            ApplySkin();

            errorMonitor.StartMonitor();

            AppScope.Add(ShowErrorNotificationKey, errorMonitor.ShowErrorNotification);

            MainWindow = new View();

            MainWindow.Show();
        }
        #endregion
    }
}