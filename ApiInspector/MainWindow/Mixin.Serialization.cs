using System;
using ApiInspector.Serialization;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The mixin
    /// </summary>
    static partial class Mixin
    {
        #region Public Methods
        public static string SerializeForMethodParameter(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string valueAsString)
            {
                return valueAsString;
            }

            return Serializer.SerializeToJson(value);
        }

        public static object DeserializeForMethodParameter(string json, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return json;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return targetType.GetDefaultValue();
            }


            return Serializer.Deserialize(json, targetType);
        }

        #endregion
    }
}