using Ninject;

namespace ApiInspector.History
{
    /// <summary>
    ///     The injector
    /// </summary>
    class Injector : StandardKernel
    {
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