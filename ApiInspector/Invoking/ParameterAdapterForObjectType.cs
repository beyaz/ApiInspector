namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The parameter adapter for object type
    /// </summary>
    class ParameterAdapterForObjectType : IParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(object))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}