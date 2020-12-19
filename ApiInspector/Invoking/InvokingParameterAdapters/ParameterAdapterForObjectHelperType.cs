using BOA.Base;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter for object helper type
    /// </summary>
    class ParameterAdapterForObjectHelperType 
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static  bool TryAdapt(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(ObjectHelper))
            {
                input.InvocationValue = new ObjectHelper {Context = input.BoaContext.GetObjectHelper().Context};
                return true;
            }

            return false;
        }
        #endregion
    }
}