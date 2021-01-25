using System;
using BOA.Base;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector.Plugins
{
    public class ObjectHelperConverter : JsonConverter<ObjectHelper>
    {
        public override void WriteJson(JsonWriter writer, ObjectHelper value, JsonSerializer serializer)
        {
            
        }

        public override ObjectHelper ReadJson(JsonReader reader, Type objectType, ObjectHelper existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }
    }
}