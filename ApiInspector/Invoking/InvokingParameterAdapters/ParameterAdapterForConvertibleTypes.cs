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
        public static  bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            input.InvocationValue = Convert.ChangeType(input.InvocationValue, targetParameterType);

            return true;
        }
        #endregion
    }
}