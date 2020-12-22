namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for object type
    /// </summary>
    class ParameterAdapterForObjectType 
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForObjectType(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(object))
            {
                return input;
            }

            return null;
        }
        #endregion
    }
}