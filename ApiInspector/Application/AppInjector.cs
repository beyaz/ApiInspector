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
            Bind<ITracer>().To<TraceQueue>();
            Bind<View>().ToSelf().InSingletonScope();
            Bind<ErrorMonitor>().ToMethod(x => errorMonitor);
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