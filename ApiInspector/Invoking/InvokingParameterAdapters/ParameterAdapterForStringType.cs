namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    

    class ParameterAdapter
    {
        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static  ParameterAdapterInput TryAdaptForStringType(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(string) && input.InvocationValue is string)
            {
                return input;
            }

            return null;
        }
    }
}