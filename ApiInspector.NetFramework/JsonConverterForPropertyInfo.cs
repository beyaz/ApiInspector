using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

sealed class JsonConverterForPropertyInfo : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(PropertyInfo).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string propertyName = null;
        string declaringType = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                if ("propertyName".Equals((string)reader.Value, StringComparison.OrdinalIgnoreCase))
                {
                    reader.Read();
                    propertyName = (string)reader.Value;
                }

                if ("declaringType".Equals((string)reader.Value, StringComparison.OrdinalIgnoreCase))
                {
                    reader.Read();
                    declaringType = (string)reader.Value;
                }
            }
        }

        if (declaringType is null || propertyName is null)
        {
            return null;
        }

        return Type.GetType(declaringType)?.GetProperty(propertyName);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var propertyInfo = (PropertyInfo)value;
        if (propertyInfo == null)
        {
            writer.WriteNull();
            return;
        }

        var assemblyQualifiedName = propertyInfo.DeclaringType?.AssemblyQualifiedName;
        if (assemblyQualifiedName is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("propertyName");
        writer.WriteValue(propertyInfo.Name);

        writer.WritePropertyName("declaringType");
        writer.WriteValue(assemblyQualifiedName);

        writer.WriteEndObject();
    }
}