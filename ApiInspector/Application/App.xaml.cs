using System;
using System.Windows;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.MainWindow;
using ApiInspector.Tracing;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    partial class App
    {
        #region Static Fields
        
        static ErrorMonitor errorMonitor;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            BoaAssemblyResolver.AttachToCurrentDomain();
            errorMonitor = new ErrorMonitor(this);


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
            ApplySkin();

            errorMonitor.StartMonitor();

            MainWindow = new View
            {
                ShowErrorNotification = errorMonitor.ShowErrorNotification
            };

            MainWindow.Show();
        }
        #endregion
    }
}