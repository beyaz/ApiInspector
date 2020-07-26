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
        public CardServiceMethodInvokerInput(Type targetType, string methodName, List<object> invocationParameters, Action<string> trace, BOAContext boaContext)
        {
            TargetType           = targetType;
            MethodName           = methodName;
            InvocationParameters = invocationParameters;
            Trace                = trace;
            BoaContext           = boaContext;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the boa context.
        /// </summary>
        public BOAContext BoaContext { get; }

        /// <summary>
        ///     Gets the invocation parameters.
        /// </summary>
        public List<object> InvocationParameters { get; }

        /// <summary>
        ///     Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        ///     Gets the type of the target.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        ///     Gets the trace.
        /// </summary>
        public Action<string> Trace { get; }
        #endregion
    }
}