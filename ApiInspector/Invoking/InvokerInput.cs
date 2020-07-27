using System;
using System.Collections.Generic;
using System.Reflection;
using ApiInspector.Models;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoker input
    /// </summary>
    class InvokerInput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvokerInput" /> class.
        /// </summary>
        public InvokerInput(InvocationInfo invocationInfo)
        {
            InvocationInfo = invocationInfo;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        public InvocationInfo InvocationInfo { get; }

        /// <summary>
        ///     Gets or sets the invocation parameters.
        /// </summary>
        public List<object> InvocationParameters { get; set; }

        /// <summary>
        ///     Gets or sets the method information.
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        ///     Gets or sets the type of the target.
        /// </summary>
        public Type TargetType { get; set; }

        #endregion
    }
}