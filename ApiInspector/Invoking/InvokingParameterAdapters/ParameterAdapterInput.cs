using System.Reflection;
using ApiInspector.Invoking.BoaSystem;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter input
    /// </summary>
    class ParameterAdapterInput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParameterAdapterInput" /> class.
        /// </summary>
        public ParameterAdapterInput(ParameterInfo parameterInfo, BOAContext boaContext, string invocationValue)
        {
            ParameterInfo   = parameterInfo;
            BoaContext      = boaContext;
            InvocationValue = invocationValue;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the boa context.
        /// </summary>
        public BOAContext BoaContext { get; }

        public string InvocationValue { get; }

        /// <summary>
        ///     Gets or sets the invocation value.
        /// </summary>
        public object AdaptedInvocationValue { get; private set; }


        /// <summary>
        ///     Gets or sets the parameter information.
        /// </summary>
        public ParameterInfo ParameterInfo { get; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Withes the invocation value.
        /// </summary>
        public ParameterAdapterInput WithInvocationValue(object adaptedInvocationValue)
        {
            return new ParameterAdapterInput(ParameterInfo, BoaContext, InvocationValue){ AdaptedInvocationValue = adaptedInvocationValue};
        }
        #endregion
    }
}