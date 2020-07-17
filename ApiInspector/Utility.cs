using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Something.Converter;

namespace Something.Converter
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc cref="JsonConverter"/>
    /// <summary>
    /// Converts an object to and from JSON.
    /// </summary>
    /// <seealso cref="JsonConverter"/>
    public class DecimalConverter : JsonConverter
    {
        /// <summary>
        /// Gets a new instance of the <see cref="DecimalConverter"/>.
        /// </summary>
        public static readonly DecimalConverter Instance = new DecimalConverter();

        /// <inheritdoc cref="JsonConverter"/>
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="JsonConverter"/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        /// <inheritdoc cref="JsonConverter"/>
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        /// <seealso cref="JsonConverter"/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(reader.Value is string value))
            {
                if (objectType == typeof(decimal?))
                {
                    return null;
                }

                return default(decimal);
            }

            // ReSharper disable once StyleCop.SA1126
            if (decimal.TryParse(value, out var result))
            {
                // ReSharper disable once StyleCop.SA1126
                return result;
            }

            if (objectType == typeof(decimal?))
            {
                return null;
            }

            return default(decimal);
        }

        /// <inheritdoc cref="JsonConverter"/>
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <seealso cref="JsonConverter"/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = default(decimal?);

            if (value != null)
            {
                d = value as decimal?;
                if (d.HasValue)
                {
                    d = new decimal(decimal.ToDouble(d.Value));
                }
            }

            JToken.FromObject(d ?? 0).WriteTo(writer);
        }
    }
}

namespace ApiInspector
{
    static class Utility
    {

        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        #region Public Properties
        public static BindingFlags AllBindings
        {
            get { return BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; }
        }
        #endregion

        #region Public Methods
        public static bool IsSuccess<T>(Func<T> action, ref T target)
        {
            try
            {
                target = action();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string SerializeToJson(object value, bool ignoreDefaultValues = true)
        {
            if (value == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                Formatting           = Formatting.Indented,
                DateFormatString     = "yyyy.MM.dd hh:mm:ss",
                Converters = new List<JsonConverter> { new DecimalConverter() }
                
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        public static Exception TryRun(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static void WriteAllText(string filePath, string content)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null)
            {
                throw new ArgumentNullException(nameof(directoryName));
            }

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(filePath, content);
        }
        #endregion
    }
}