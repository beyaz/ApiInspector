using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;

namespace ApiInspector.WebUI;

static partial class Extensions
{
    public static string BluePrimary => "#1976d2";

    public static StyleModifier PrimaryBackground => Background("rgb(249, 249, 249)");

    public static string GetSvgUrl(string svgFileName)
    {
        var resourceFilePathInAssembly = $"ApiInspector.WebUI.Resources.{svgFileName}.svg";

        return getDataUriFromSvgBytes(getEmbeddedFile(resourceFilePathInAssembly));

        static string getDataUriFromSvgBytes(byte[] bytesOfSvgFile)
        {
            var imageBase64 = Convert.ToBase64String(bytesOfSvgFile);

            return "data:image/svg+xml;base64," + imageBase64;
        }

        static byte[] getEmbeddedFile(string resourceFilePathInAssembly)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceStream = assembly.GetManifestResourceStream(resourceFilePathInAssembly);
            if (resourceStream == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();

            resourceStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }

    public static (bool isDotNetCore, bool isDotNetFramework) GetTargetFramework(FileInfo dll)
    {
        var CompiledNetCoreRegex      = new Regex(@".NETCoreApp,Version=v[0-9\.]+", RegexOptions.Compiled);
        var CompiledNetFrameworkRegex = new Regex(@".NETFramework,Version=v[0-9\.]+", RegexOptions.Compiled);

        var contents = File.ReadAllText(dll.FullName);

        var match = CompiledNetCoreRegex.Match(contents);
        if (match.Success)
        {
            return (true, false);
        }

        match = CompiledNetFrameworkRegex.Match(contents);
        if (match.Success)
        {
            return (false, true);
        }

        return (false, false);
    }

    public static bool HasNoValue(this string value) => string.IsNullOrWhiteSpace(value);

    public static bool HasValue(this string value) => !string.IsNullOrWhiteSpace(value);

    public static void OnBrowserInactive(this Client client)
    {
        client.DispatchEvent(nameof(OnBrowserInactive));
    }

    public static void OnBrowserInactive(this Client client, Action handlerAction)
    {
        client.ListenEvent(OnBrowserInactive, handlerAction);
    }

    public static readonly IModifier AutoHideScrollbar = FreeScrollBar.Modify(x => x.autohide = true);


    public static readonly IEnumerable<MetadataNode> EmptyMetadataNodes = new ImmutableArray<MetadataNode>();
    
    
    /// <summary>
    ///     Removes value from start of str
    /// </summary>
    public static string RemoveFromStart(this string data, string value)
    {
        return RemoveFromStart(data, value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Removes value from start of str
    /// </summary>
    public static string RemoveFromStart(this string data, string value, StringComparison comparison)
    {
        if (data == null)
        {
            return null;
        }

        if (data.StartsWith(value, comparison))
        {
            return data.Substring(value.Length, data.Length - value.Length);
        }

        return data;
    }
}
