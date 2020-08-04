using System;
using System.Collections.Generic;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The card service method invoker input
    /// </summary>
    class CardServiceMethodInvokerInput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="CardServiceMethodInvokerInput" /> class.
        /// </summary>
        public CardServiceMethodInvokerInput(Type targetType, string methodName, IReadOnlyList<object> invocationParameters)
        {
            TargetType           = targetType;
            MethodName           = methodName;
            InvocationParameters = invocationParameters;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the invocation parameters.
        /// </summary>
        public IReadOnlyList<object> InvocationParameters { get; }

        /// <summary>
        ///     Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        ///     Gets the type of the target.
        /// </summary>
        public Type TargetType { get; }
        #endregion
    }
}