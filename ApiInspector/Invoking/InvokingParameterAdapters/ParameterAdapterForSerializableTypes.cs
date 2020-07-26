using Newtonsoft.Json;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for serializable types
    /// </summary>
    class ParameterAdapterForSerializableTypes : IParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType.IsClass && input.InvocationValue is string)
            {
                input.InvocationValue = JsonConvert.DeserializeObject((string) input.InvocationValue, targetParameterType);
                return true;
            }

            return false;
        }
        #endregion
    }
}