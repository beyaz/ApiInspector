namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for string type
    /// </summary>
    class ParameterAdapterForStringType 
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static  ParameterAdapterInput TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(string) && input.InvocationValue is string)
            {
                return input;
            }

            return null;
        }
        #endregion
    }
}