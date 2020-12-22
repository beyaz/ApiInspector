using System;
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
        public static ParameterAdapterInput TryAdaptForSerializableTypes(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType.IsClass && input.InvocationValue is string invocationValueAsString)
            {
                var invocationValue = JsonConvert.DeserializeObject(invocationValueAsString, targetParameterType);

                return input.WithInvocationValue(invocationValue);
            }

            return null;
        }

        public static ParameterAdapterInput TryAdaptForDateTime(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (input.InvocationValue is string invocationValueAsString)
            {
                if (targetParameterType == typeof(DateTime?))
                {
                    if (string.IsNullOrWhiteSpace(invocationValueAsString))
                    {
                        return input.WithInvocationValue(null);
                    }

                    return input.WithInvocationValue(DateTime.Parse(invocationValueAsString));
                }

                if (targetParameterType == typeof(DateTime))
                {
                    return input.WithInvocationValue(DateTime.Parse(invocationValueAsString));
                }
            }

            return null;
        }
        #endregion
    }
}