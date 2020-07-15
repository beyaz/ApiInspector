using System;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;

namespace ApiInspector.DataFlow
{

    /// <summary>
    ///     The service provider
    /// </summary>
    interface IServiceProvider
    {
        #region Public Methods

        /// <summary>
        ///     Gets the service.
        /// </summary>
        T GetService<T>();
        #endregion
    }

    /// <summary>
    ///     The instance manager extensions
    /// </summary>
    static class InstanceManagerExtensions
    {
        #region Public Methods
        /// <summary>
        ///     Gets the instance manager.
        /// </summary>
        public static IServiceProvider GetInstanceManager(this DataContext context)
        {
            return new ServiceProvider(context);
        }
        #endregion
    }


    /// <summary>
    ///     The service provider
    /// </summary>
    class ServiceProvider : IServiceProvider
    {
        #region Fields
        /// <summary>
        ///     The context
        /// </summary>
        readonly DataContext context;
        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceProvider"/> class.
        /// </summary>
        public ServiceProvider(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public T GetService<T>()
        {
            var targetType = typeof(T);

            if (targetType == typeof(CecilTypeVisitor))
            {
                return (T) (object) new CecilTypeVisitor(context.Get(Logger.Key));
            }

            if (targetType == typeof(IServiceProvider))
            {
                return (T) (object) this;
            }

            if (targetType == typeof(ParameterPanelRefresher))
            {
                return (T) (object) new ParameterPanelRefresher();
            }

            throw new NotImplementedException(targetType.FullName);
        }
        #endregion
    }
}