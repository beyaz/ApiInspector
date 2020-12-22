using System;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for convertible types
    /// </summary>
    class ParameterAdapterForConvertibleTypes 
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static  ParameterAdapterInput TryAdaptForIConvertibleTypes(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            var invocationValue = Convert.ChangeType(input.InvocationValue, targetParameterType);

            return input.WithInvocationValue(invocationValue);

        }
        #endregion
    }
}