using System;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector.Plugins
{
    public class MethodDefinitionConverter : JsonConverter<MethodDefinition>
    {
        public override void WriteJson(JsonWriter writer, MethodDefinition value, JsonSerializer serializer)
        {
            var assemblyPath = value.Module.FullyQualifiedName;

            var fullMethodName = value.FullName;

            var jObject = new JObject
            {
                new JProperty("assemblyPath", new JValue(assemblyPath)),
                new JProperty("fullMethodName", new JValue(fullMethodName)),
            };

            jObject.WriteTo(writer);
        }

        public override MethodDefinition ReadJson(JsonReader reader, Type objectType, MethodDefinition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (!jObject.HasValues)
            {
                return null;
            }

            var assemblyPath   = (string) jObject["assemblyPath"];
            var fullMethodName = (string) jObject["fullMethodName"];

            foreach (var moduleDefinition in AssemblyDefinition.ReadAssembly(assemblyPath).Modules)
            {
                foreach (var item in moduleDefinition.Types)
                {
                    foreach (var methodDefinition in item.Methods)
                    {
                        if (methodDefinition.FullName == fullMethodName)
                        {
                            return methodDefinition;
                        }
                    }
                }
            }

            throw new Exception($"{assemblyPath} -> {fullMethodName}");
        }
    }
}