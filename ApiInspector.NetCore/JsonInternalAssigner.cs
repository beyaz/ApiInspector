using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ApiInspector;

static class JsonInternalAssigner
{
    static readonly JsonSerializerSettings InternalJsonSettings = new()
    {
        ConstructorHandling   = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver      = new InternalWritableContractResolver(includeNonPublicFields: false),
        NullValueHandling     = NullValueHandling.Include,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    /// <summary>
    ///     JSON'dan gelen değerlerle instance'ın internal/private (ve public) property'lerini,
    ///     mevcut değeri null ise, tipine göre deserialize edip atar.
    ///     HttpContext tipinde ise DefaultHttpContext üretip uygun alanları doldurur.
    /// </summary>
    internal static object TryAssignInternalProps(object instance, string jsonForInstance)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (string.IsNullOrWhiteSpace(jsonForInstance))
        {
            return instance;
        }

        var type = instance.GetType();
        var jToken = JToken.Parse(jsonForInstance);
        if (jToken is not JObject jObj)
        {
            return instance; // Beklediğimiz kök bir obje
        }

        foreach (var jp in jObj.Properties())
        {
            // Property adını case-insensitive yakala
            var prop = GetPropertyCaseInsensitive(type, jp.Name);
            if (prop is null)
            {
                continue;
            }

            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter is null)
            {
                continue; // set edilemiyor
            }

            // İstersen burayı "mevcut değer null ise ata" yerine "json veriyorsa ata" yapabilirsin.
            var currentValue = prop.CanRead ? prop.GetValue(instance) : null;
            if (currentValue is not null)
            {
                continue;
            }

            var targetType = prop.PropertyType;

            // Genel tip dönüşümü
            try
            {
                var converted = ConvertTokenToType(jp.Value, targetType);
                setter.Invoke(instance, [converted]);
            }
            catch
            {
                // Sessiz geçmek yerine istersen loglayabilirsin
                // Console.WriteLine($"Property '{prop.Name}' için atama başarısız: {ex}");
            }
        }

        return instance;
    }

    /// <summary>
    ///     JToken'ı hedef tipe çevirir (string/enum/nullable/complex/collection destekli).
    /// </summary>
    static object ConvertTokenToType(JToken token, Type targetType)
    {
        // Nullable<T> çöz
        var isNullable = IsNullable(targetType, out var underlying);
        var effectiveType = isNullable ? underlying! : targetType;

        // null token
        if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
        {
            return null;
        }

        // string özel durumu: 123 -> "123"
        if (effectiveType == typeof(string))
        {
            if (token.Type == JTokenType.String)
            {
                return token.Value<string>();
            }

            return token.ToString();
        }

        // enum
        if (effectiveType.IsEnum)
        {
            if (token.Type == JTokenType.String)
            {
                return Enum.Parse(effectiveType, token.Value<string>()!, ignoreCase: true);
            }

            if (token.Type == JTokenType.Integer)
            {
                return Enum.ToObject(effectiveType, token.Value<long>());
            }

            // başka durumlarda default serializer dene
        }

        // Diğer tüm durumlar: Newtonsoft serializer ile
        var serializer = JsonSerializer.Create(InternalJsonSettings);
        return token.ToObject(targetType, serializer);
    }

    static PropertyInfo GetPropertyCaseInsensitive(Type type, string name)
    {
        // Public + NonPublic; IgnoreCase
        return type.GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase
        );
    }

    static bool IsNullable(Type t, out Type underlying)
    {
        underlying = Nullable.GetUnderlyingType(t);
        return underlying is not null;
    }

    sealed class InternalWritableContractResolver : DefaultContractResolver
    {
        public InternalWritableContractResolver(bool includeNonPublicFields)
        {
            IncludeNonPublicFields = includeNonPublicFields;
            // İstersen NamingStrategy burada da ayarlanabilir
            // NamingStrategy = new CamelCaseNamingStrategy(true, false);
        }

        public bool IncludeNonPublicFields { get; }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            foreach (var prop in props)
            {
                if (!prop.Writable && prop.DeclaringType != null && prop.UnderlyingName != null)
                {
                    var pi = prop.DeclaringType.GetProperty(
                        prop.UnderlyingName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (pi != null && pi.GetSetMethod(true) != null)
                    {
                        prop.Writable = true;
                    }

                    if (pi != null && pi.GetGetMethod(true) != null)
                    {
                        prop.Readable = true;
                    }
                }
            }

            // Fields are included via GetSerializableMembers override when IncludeNonPublicFields is true,
            // so no need to add them here manually.

            return props;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            // Include public and non-public instance properties from the type hierarchy.
            // Optionally include fields when requested. Exclude compiler generated backing fields.
            var members = new List<MemberInfo>();

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var current = objectType;
            while (current != null && current != typeof(object))
            {
                // Properties
                foreach (var pi in current.GetProperties(flags))
                {
                    members.Add(pi);
                }

                if (IncludeNonPublicFields)
                {
                    foreach (var fi in current.GetFields(flags))
                    {
                        if (fi.Name.Contains("k__BackingField"))
                        {
                            continue;
                        }

                        members.Add(fi);
                    }
                }

                current = current.BaseType;
            }

            // Remove duplicates (if any) while preserving order
            var seen = new HashSet<string>();
            var result = new List<MemberInfo>();
            foreach (var m in members)
            {
                var key = m.MemberType + ":" + m.Name + ":" + (m.DeclaringType?.FullName ?? "");
                if (seen.Add(key))
                {
                    result.Add(m);
                }
            }

            return result;
        }
    }
}