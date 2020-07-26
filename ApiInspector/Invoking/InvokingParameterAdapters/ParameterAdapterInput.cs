using System.Reflection;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The parameter adapter input
    /// </summary>
    class ParameterAdapterInput
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the boa context.
        /// </summary>
        public BOAContext BoaContext { get; set; }

        /// <summary>
        ///     Gets or sets the invocation value.
        /// </summary>
        public object InvocationValue { get; set; }

        /// <summary>
        ///     Gets or sets the parameter information.
        /// </summary>
        public ParameterInfo ParameterInfo { get; set; }
        #endregion
    }
}