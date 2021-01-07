using ApiInspector.MainWindow;
using ApiInspector.Tracing;
using Ninject;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The application injector
    /// </summary>
    class AppInjector : StandardKernel
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppInjector" /> class.
        /// </summary>
        public AppInjector(ErrorMonitor errorMonitor)
        {
            Bind<TraceQueue>().ToSelf().InSingletonScope();
            
            Bind<View>().ToSelf().InSingletonScope();
            Bind<ErrorMonitor>().ToMethod(x => errorMonitor);

            var scope = new Scope
            {
                {Keys.TraceQueue, Get<TraceQueue>()},
                {Keys.ErrorMonitor, Get<ErrorMonitor>()}
            };

            Bind<Scope>().ToMethod(x => scope);
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets this instance.
        /// </summary>
        public T Get<T>()
        {
            return ResolutionExtensions.Get<T>(this);
        }
        #endregion
    }
}