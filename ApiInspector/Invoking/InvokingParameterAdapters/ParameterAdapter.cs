using System;
using BOA.Base;
using Newtonsoft.Json;
using static ApiInspector._;

namespace ApiInspector.Invoking.InvokingParameterAdapters
{
    /// <summary>
    ///     The parameter adapter
    /// </summary>
    static class ParameterAdapter
    {
        #region Public Methods
        /// <summary>
        ///     Tries the adapt for date time.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForDateTime(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (input.InvocationValue is string invocationValueAsString)
            {
                if (targetParameterType == typeof(DateTime?))
                {
                    if (string.IsNullOrWhiteSpace(invocationValueAsString))
                    {
                        return input.WithInvocationValue(null);
                    }

                    return input.WithInvocationValue(DateTime.Parse(invocationValueAsString));
                }

                if (targetParameterType == typeof(DateTime))
                {
                    return input.WithInvocationValue(DateTime.Parse(invocationValueAsString));
                }
            }

            return null;
        }

        

        /// <summary>
        ///     Tries the adapt for i convertible types.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForIConvertibleTypes(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            var invocationValue = Convert.ChangeType(input.InvocationValue, targetParameterType);

            return input.WithInvocationValue(invocationValue);
        }

        /// <summary>
        ///     Tries the type of the adapt for object helper.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForObjectHelperType(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(ObjectHelper))
            {
                var invocationValue = input.BoaContext.GetObjectHelper();

                return input.WithInvocationValue(invocationValue);
            }

            return null;
        }

        /// <summary>
        ///     Tries the type of the adapt for object.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForObjectType(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(object))
            {
                if (input.InvocationValue is string invocationValueAsString)
                {
                    if (invocationValueAsString.IndexOf("$type", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        };
                        var invocationValue = JsonConvert.DeserializeObject(invocationValueAsString, targetParameterType, settings);

                        return input.WithInvocationValue(invocationValue);
                    }
                }

                return input;
            }

            return null;
        }

        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForSerializableTypes(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            
            if ( !CanPresentSimpleTextBox(GetFullName(targetParameterType)) && input.InvocationValue is string invocationValueAsString)
            {
                var invocationValue = Serialization.Serializer.Deserialize(invocationValueAsString, targetParameterType);

                return input.WithInvocationValue(invocationValue);
            }

            return null;
        }

        /// <summary>
        ///     Tries the adapt.
        /// </summary>
        public static ParameterAdapterInput TryAdaptForStringType(ParameterAdapterInput input)
        {
            var targetParameterType = input.ParameterInfo.ParameterType;

            if (targetParameterType == typeof(string))
            {
                return input.WithInvocationValue(input.InvocationValue);
            }

            return null;
        }
        #endregion
    }
}