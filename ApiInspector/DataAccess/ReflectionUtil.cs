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

            var getProp = Fun((string propertyName) =>
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
            while (true)
            {
                var parts = propertyPath.Split('.');

                var propertyInfo = instance.GetType().GetProperty(parts[0]);

                if (propertyInfo == null)
                {
                    throw new MissingMemberException(instance.GetType().FullName, parts[0]);
                }

                if (parts.Length == 1)
                {
                    propertyInfo.SetValue(instance, value, null);
                }
                else
                {
                    var innerInstance = propertyInfo.GetValue(instance);
                    if (innerInstance == null)
                    {
                        innerInstance = Activator.CreateInstance(propertyInfo.PropertyType);

                        propertyInfo.SetValue(instance,innerInstance);
                    }

                    instance     = innerInstance;
                    propertyPath = string.Join(".", parts.Skip(1));

                    continue;
                }

                break;
            }
        }
    }
}