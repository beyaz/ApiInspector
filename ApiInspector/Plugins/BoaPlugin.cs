using ApiInspector.Serialization;

namespace ApiInspector.Plugins
{
    public static class BoaPlugin
    {
        public static void Attach()
        {
            _.AddJsonConverter(new MethodDefinitionConverter());
            _.AddJsonConverter(new ObjectHelperConverter());
            _.AddJsonConverter(new DecimalConverter());
            _.AddJsonConverter(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
    }
}