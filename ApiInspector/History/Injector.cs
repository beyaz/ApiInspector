using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using Ninject;

namespace ApiInspector.History
{
    /// <summary>
    ///     The injector
    /// </summary>
    class Injector : StandardKernel
    {
        public Injector()
        {
            Bind<EnvironmentVariable>().ToSelf().InSingletonScope();
        }
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