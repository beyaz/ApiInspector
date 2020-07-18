using BOA.Base;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The parameter adapter for object helper type
    /// </summary>
    class ParameterAdapterForObjectHelperType : IParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(ObjectHelper))
            {
                input.InvocationValue = new ObjectHelper {Context = input.boaContext.GetObjectHelper().Context};
                return true;
            }

            return false;
        }
        #endregion
    }
}