using System;
using System.Linq;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.DataAccess
{
    class ReflectionUtil
    {
        #region Public Methods
        public static object ReadPropertyPath(object src, string propName)
        {
            if (src == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(propName))
            {
                return src;
            }

            var getProp = fun((string propertyName) =>
            {
                var propertyInfo = src.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    throw new MissingMemberException(propName);
                }

                return propertyInfo;
            });

            if (propName.Contains("."))
            {
                var Split = propName.Split('.');

                var RemainingProperty = propName.Substring(propName.IndexOf('.') + 1);

                return ReadPropertyPath(getProp(Split[0]).GetValue(src, null), RemainingProperty);
            }

            return getProp(propName).GetValue(src, null);
        }
        #endregion

        public static void SaveValueToPropertyPath(object value, object instance, string propertyPath)
        {
            var parts = propertyPath.Split('.');
            
            var prop  = instance.GetType().GetProperty(parts[0]);

            if (prop == null)
            {
                throw new MissingMemberException(instance.GetType().FullName, parts[0]);
            }

            if (parts.Length == 1)
            {
                prop.SetValue(instance, value, null);
            }
            else
            {
                instance = prop.GetValue(instance);
                SaveValueToPropertyPath(value,instance, string.Join(".", parts.Skip(1)));
            }
        }
    }
}