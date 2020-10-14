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
    ///     The json sorter sort input
    /// </summary>
    class JsonSorterSortInput
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the full name of the class.
        /// </summary>
        public string ClassFullName { get; set; }

        /// <summary>
        ///     Gets or sets the json file path.
        /// </summary>
        public string JsonFilePath { get; set; }

        /// <summary>
        ///     Gets or sets the sort by property maps.
        /// </summary>
        public Dictionary<string, string> SortByPropertyMaps { get; set; }
        #endregion
    }

    /// <summary>
    ///     The json sorter
    /// </summary>
    class JsonSorter
    {
        #region Public Methods
        /// <summary>
        ///     Sorts the specified json content.
        /// </summary>
        public static void Sort(JsonSorterSortInput input)
        {
            var jsonFilePath       = input.JsonFilePath;
            var classFullName      = input.ClassFullName;
            var sortByPropertyMaps = input.SortByPropertyMaps;

            var jsonContent = File.ReadAllText(jsonFilePath);

            var serializer = new Serializer();

            var typeFinder = new TypeFinder();

            var type = typeFinder.Find(classFullName);
            if (type == null)
            {
                throw new ArgumentException(nameof(type));
            }

            var instance = serializer.Deserialize(jsonContent, type);
            if (sortByPropertyMaps != null)
            {
                foreach (var pair in sortByPropertyMaps)
                {
                    OrderListByProperty(instance, pair.Key, pair.Value);
                }
            }

            var sortedJson = serializer.SerializeToJsonDoNotIgnoreDefaultValues(instance);

            Utility.WriteAllText(jsonFilePath + ".Formatted.json", sortedJson);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the property value.
        /// </summary>
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

        /// <summary>
        ///     Orders the by.
        /// </summary>
        static List<object> OrderBy(List<object> list, string orderPropertyName)
        {
            return list.OrderBy(item => GetPropertyValue(item, orderPropertyName)).ToList();
        }

        /// <summary>
        ///     Orders the list by property.
        /// </summary>
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

                foreach (var item in list)
                {
                    items.GetType().GetMethod("Add").Invoke(items, new[] {item});
                }

                return;
            }

            throw new NotImplementedException(propertyInfo.PropertyType.FullName);
        }
        #endregion
    }
}