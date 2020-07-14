using System;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;

namespace ApiInspector.DataFlow
{
    /// <summary>
    ///     The instance manager
    /// </summary>
    interface IInstanceManager
    {
        #region Public Methods
        /// <summary>
        ///     Gets the instance.
        /// </summary>
        T GetInstance<T>();
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
        public static IInstanceManager GetInstanceManager(this DataContext context)
        {
            return new InstanceManager(context);
        }
        #endregion
    }

    /// <summary>
    ///     The instance manager
    /// </summary>
    class InstanceManager : IInstanceManager
    {
        #region Fields
        /// <summary>
        ///     The context
        /// </summary>
        readonly DataContext context;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstanceManager" /> class.
        /// </summary>
        public InstanceManager(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public T GetInstance<T>()
        {
            var targetType = typeof(T);

            if (targetType == typeof(CecilTypeVisitor))
            {
                return (T) (object) new CecilTypeVisitor(context.Get(Logger.Key));
            }

            if (targetType == typeof(IInstanceManager))
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