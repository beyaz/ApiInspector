using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Serialization;

namespace ApiInspector.Util
{
    /// <summary>
    ///     The json sorter
    /// </summary>
    class JsonSorter
    {
        #region Public Methods
        /// <summary>
        ///     Sorts the specified json content.
        /// </summary>
        public static void Sort(string jsonFilePath, string classFullName, Dictionary<string, string> sortOptions)
        {
            var jsonContent = File.ReadAllText(jsonFilePath);

            var serializer = new Serializer();

            var typeFinder = new TypeFinder();

            var type = typeFinder.Find(classFullName);
            if (type == null)
            {
                throw new ArgumentException(nameof(type));
            }

            var instance = serializer.Deserialize(jsonContent, type);
            if (sortOptions != null)
            {
                foreach (var pair in sortOptions)
                {
                    OrderListByProperty(instance, pair.Key, pair.Value);
                }
            }

            var sortedJson = serializer.SerializeToJsonDoNotIgnoreDefaultValues(instance);

            Utility.WriteAllText(jsonFilePath + ".Formatted.json", sortedJson);
        }
        #endregion

        #region Methods
        static object GetPropertyValue(object instance, string propertyName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var type = instance.GetType();

            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new MissingMemberException(type.FullName + ":" + propertyName);
            }

            return propertyInfo.GetValue(instance);
        }

        static List<object> OrderBy(List<object> list, string orderPropertyName)
        {
            return list.OrderBy(item => GetPropertyValue(item, orderPropertyName)).ToList();
        }

        static void OrderListByProperty(object instance, string enumerablePropertyName, string orderPropertyName)
        {
            if (instance == null)
            {
                return;
            }

            var propertyInfo = instance.GetType().GetProperty(enumerablePropertyName);
            if (propertyInfo == null)
            {
                throw new MissingMemberException(enumerablePropertyName);
            }

            var enumerable = propertyInfo.GetValue(instance) as IEnumerable;
            if (enumerable == null)
            {
                throw new ArgumentException($"Property is nor enumarable. PropertyName is {enumerablePropertyName}");
            }

            var list = new List<object>();

            foreach (var item in enumerable)
            {
                list.Add(item);
            }

            list = OrderBy(list, orderPropertyName);

            if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                dynamic items = propertyInfo.GetValue(instance);

                items.Clear();

                items.AddRange(list);

                return;
            }

            throw new NotImplementedException();
        }
        #endregion
    }
}