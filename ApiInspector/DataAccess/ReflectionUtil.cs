using System;
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
                var propertyInfo = src.GetType()?.GetProperty(propertyName);
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
    }
}