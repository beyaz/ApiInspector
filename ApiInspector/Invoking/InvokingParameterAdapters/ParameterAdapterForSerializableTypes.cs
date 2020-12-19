using Newtonsoft.Json;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for serializable types
    /// </summary>
    class ParameterAdapterForSerializableTypes 
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static  ParameterAdapterInput TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType.IsClass && input.InvocationValue is string invocationValueAsString)
            {
                var invocationValue = JsonConvert.DeserializeObject(invocationValueAsString, targetParameterType);

                return input.WithInvocationValue(invocationValue);
            }

            return null;
        }
        #endregion
    }
}