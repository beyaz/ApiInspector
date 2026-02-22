namespace ApiInspector;

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;

static class JsonInternalAssigner
{
    // İsteğe göre dışarıdan geçirilebilir ama burada sabitleyelim:
    private static readonly JsonSerializerSettings InternalJsonSettings = new JsonSerializerSettings
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        // Non-public setter/field görünsün diye custom resolver
        ContractResolver = new InternalWritableContractResolver(includeNonPublicFields: false),
        // CamelCase JSON'larda kolaylık:
        // NamingStrategy = new CamelCaseNamingStrategy(true, false), // İstersen aç
        NullValueHandling = NullValueHandling.Include,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    /// <summary>
    /// JSON'dan gelen değerlerle instance'ın internal/private (ve public) property'lerini, 
    /// mevcut değeri null ise, tipine göre deserialize edip atar.
    /// HttpContext tipinde ise DefaultHttpContext üretip uygun alanları doldurur.
    /// </summary>
    internal static object tryAssignInternalProps(object instance, string jsonForInstance)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        if (string.IsNullOrWhiteSpace(jsonForInstance)) return instance;

        var type = instance.GetType();
        var jToken = JToken.Parse(jsonForInstance);
        if (jToken is not JObject jObj) return instance; // Beklediğimiz kök bir obje

        foreach (var jp in jObj.Properties())
        {
            // Property adını case-insensitive yakala
            var prop = GetPropertyCaseInsensitive(type, jp.Name);
            if (prop is null) continue;

            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter is null) continue; // set edilemiyor

            // İstersen burayı "mevcut değer null ise ata" yerine "json veriyorsa ata" yapabilirsin.
            var currentValue = prop.CanRead ? prop.GetValue(instance) : null;
            if (currentValue is not null) continue;

            var targetType = prop.PropertyType;

            // HttpContext özeli
            if (typeof(HttpContext).IsAssignableFrom(targetType))
            {
                var ctx = CreateHttpContextFromToken(jp.Value);
                if (ctx is not null)
                {
                    setter.Invoke(instance, new object?[] { ctx });
                }
                continue;
            }

            // Genel tip dönüşümü
            try
            {
                var converted = ConvertTokenToType(jp.Value, targetType);
                setter.Invoke(instance, [converted] );
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
    /// JToken'ı hedef tipe çevirir (string/enum/nullable/complex/collection destekli).
    /// </summary>
    private static object? ConvertTokenToType(JToken token, Type targetType)
    {
        // Nullable<T> çöz
        var isNullable = IsNullable(targetType, out var underlying);
        var effectiveType = isNullable ? underlying! : targetType;

        // null token
        if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            return null;

        // string özel durumu: 123 -> "123"
        if (effectiveType == typeof(string))
        {
            if (token.Type == JTokenType.String)
                return token.Value<string>();
            return token.ToString();
        }

        // enum
        if (effectiveType.IsEnum)
        {
            if (token.Type == JTokenType.String)
                return Enum.Parse(effectiveType, token.Value<string>()!, ignoreCase: true);

            if (token.Type == JTokenType.Integer)
                return Enum.ToObject(effectiveType, token.Value<long>());

            // başka durumlarda default serializer dene
        }

        // Diğer tüm durumlar: Newtonsoft serializer ile
        var serializer = JsonSerializer.Create(InternalJsonSettings);
        return token.ToObject(targetType, serializer);
    }

    /// <summary>
    /// JSON'dan basit bir DefaultHttpContext inşa eder ve alanları doldurur.
    /// </summary>
    private static HttpContext? CreateHttpContextFromToken(JToken token)
    {
        // null ise
        if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            return null;

        var ctx = new DefaultHttpContext();

        if (token is not JObject jo) return ctx; // JSON bir obje değilse boş context döneriz.

        // TraceIdentifier
        if (jo.TryGetValue("traceIdentifier", StringComparison.OrdinalIgnoreCase, out var tIdTok))
            ctx.TraceIdentifier = tIdTok.Type == JTokenType.String ? tIdTok.Value<string>()! : tIdTok.ToString();

        // Items (Dictionary<string, object>)
        if (jo.TryGetValue("items", StringComparison.OrdinalIgnoreCase, out var itemsTok) && itemsTok is JObject itemsObj)
        {
            foreach (var p in itemsObj.Properties())
            {
                ctx.Items[p.Name] = (p.Value as JValue)?.Value ?? p.Value.ToString();
            }
        }

        // Request
        if (jo.TryGetValue("request", StringComparison.OrdinalIgnoreCase, out var reqTok) && reqTok is JObject req)
        {
            if (req.TryGetValue("method", StringComparison.OrdinalIgnoreCase, out var mTok))
                ctx.Request.Method = mTok.Type == JTokenType.String ? mTok.Value<string>()! : mTok.ToString();

            if (req.TryGetValue("scheme", StringComparison.OrdinalIgnoreCase, out var sTok))
                ctx.Request.Scheme = sTok.Type == JTokenType.String ? sTok.Value<string>()! : sTok.ToString();

            if (req.TryGetValue("host", StringComparison.OrdinalIgnoreCase, out var hTok))
                ctx.Request.Host = new HostString(hTok.ToString());

            if (req.TryGetValue("pathBase", StringComparison.OrdinalIgnoreCase, out var pbTok))
                ctx.Request.PathBase = new PathString(pbTok.ToString());

            if (req.TryGetValue("path", StringComparison.OrdinalIgnoreCase, out var pTok))
                ctx.Request.Path = new PathString(pTok.ToString());

            if (req.TryGetValue("queryString", StringComparison.OrdinalIgnoreCase, out var qsTok))
                ctx.Request.QueryString = new QueryString(qsTok.ToString());

            // Headers
            if (req.TryGetValue("headers", StringComparison.OrdinalIgnoreCase, out var hdrTok) && hdrTok is JObject hdrObj)
            {
                foreach (var p in hdrObj.Properties())
                {
                    // Tekil ya da dizi şeklinde gelebilir
                    if (p.Value is JArray arr)
                        ctx.Request.Headers[p.Name] = arr.Select(v => v.ToString()).ToArray();
                    else
                        ctx.Request.Headers[p.Name] = p.Value.ToString();
                }
            }
        }

        // Response (sınırlı)
        if (jo.TryGetValue("response", StringComparison.OrdinalIgnoreCase, out var respTok) && respTok is JObject resp)
        {
            if (resp.TryGetValue("statusCode", StringComparison.OrdinalIgnoreCase, out var scTok) && scTok.Type == JTokenType.Integer)
                ctx.Response.StatusCode = scTok.Value<int>();

            if (resp.TryGetValue("headers", StringComparison.OrdinalIgnoreCase, out var rhTok) && rhTok is JObject rhObj)
            {
                foreach (var p in rhObj.Properties())
                {
                    if (p.Value is JArray arr)
                        ctx.Response.Headers[p.Name] = arr.Select(v => v.ToString()).ToArray();
                    else
                        ctx.Response.Headers[p.Name] = p.Value.ToString();
                }
            }
        }

        return ctx;
    }

    private static PropertyInfo? GetPropertyCaseInsensitive(Type type, string name)
    {
        // Public + NonPublic; IgnoreCase
        return type.GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase
        );
    }

    private static bool IsNullable(Type t, out Type? underlying)
    {
        underlying = Nullable.GetUnderlyingType(t);
        return underlying is not null;
    }
}

/// <summary>
/// Non-public setter'lara yazmaya izin veren ve non-public üyeleri gören resolver.
/// </summary>
public sealed class InternalWritableContractResolver : DefaultContractResolver
{
    public InternalWritableContractResolver(bool includeNonPublicFields)
    {
        DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
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
                    prop.Writable = true;

                if (pi != null && pi.GetGetMethod(true) != null)
                    prop.Readable = true;
            }
        }

        if (IncludeNonPublicFields)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var fi in fields)
            {
                if (fi.Name.Contains("k__BackingField")) continue;

                var jp = base.CreateProperty(fi, memberSerialization);
                jp.Readable = true;
                jp.Writable = true;
                props.Add(jp);
            }
        }

        return props;
    }
}