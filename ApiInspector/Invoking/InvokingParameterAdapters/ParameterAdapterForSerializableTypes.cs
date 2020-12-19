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
        public static  bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType.IsClass && input.InvocationValue is string invocationValueAsString)
            {
                input.InvocationValue = JsonConvert.DeserializeObject(invocationValueAsString, targetParameterType);
                return true;
            }

            return false;
        }
        #endregion
    }
}