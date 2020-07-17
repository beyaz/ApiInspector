using BOA.UnitTestHelper;

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
            BOAAssemblyResolver.AttachToCurrentDomain();

            new ErrorMonitor(this).StartMonitor();
        }
        #endregion
    }
}