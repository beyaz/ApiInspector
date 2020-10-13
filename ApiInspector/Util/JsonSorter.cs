using System;
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
        public static string Sort(string jsonContent, string classFullName)
        {
            var serializer = new Serializer();

            var typeFinder = new TypeFinder();
            var type       = typeFinder.Find(classFullName);
            if (type == null)
            {
                throw new ArgumentException(nameof(type));
            }

            var instance = serializer.Deserialize(jsonContent, type);

            return serializer.SerializeToJsonDoNotIgnoreDefaultValues(instance);
        }
        #endregion
    }
}