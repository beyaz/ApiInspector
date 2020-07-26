namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The parameter adapter for string type
    /// </summary>
    class ParameterAdapterForStringType : IParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(string) && input.InvocationValue is string)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}