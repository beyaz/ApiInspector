namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter
    /// </summary>
    interface IParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        bool TryAdapt(ParameterAdapterInput input);
        #endregion
    }
}