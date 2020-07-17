using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiInspector.Serialization
{
    class Serializer
    {
        public T Clone<T>( T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        static string SerializeToJson(object value, bool ignoreDefaultValues )
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
                Converters           = new List<JsonConverter> {new DecimalConverter()}
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        public string SerializeToJsonDoNotIgnoreDefaultValues(object value)
        {
            return SerializeToJson(value,false);
        }

        public string SerializeToJson(object value)
        {
            return SerializeToJson(value,true);
        }
    }
}