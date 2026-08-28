using System.Reflection;
using System.Text.RegularExpressions;

namespace ApiInspector.WebUI;

sealed class TargetRuntimeInfo
{
    public bool IsNetCore { get; init; }

    public bool IsNetFramework { get; init; }

    public bool IsNetStandard { get; init; }

    public string NetCoreVersion { get; init; }
}

static class TargetRuntimeIndentifier
{
    static Result<MethodInfo> GetStaticPublicMethodFromString(string fullName)
    {
        var arr = fullName.Split('>', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (arr.Length != 3)
        {
            return new TypeLoadException($"Invalid method specification: {fullName}");
        }
        
        var assemblyName = arr[0];
        var typeName = arr[1];
        var methodName = arr[2];

        var assembly = Assembly.Load(assemblyName);

        var type = assembly.GetType(typeName);
        if (type is null)
        {
            return new TypeLoadException($"Type not found: {typeName}");
        }

        var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        if (methodInfo is null)
        {
            return new MissingMethodException($"Method not found: {methodName}");
        }

        return methodInfo;
    }

    public static Result<string> GetInvokerExePath(string filePath)
    {
        {
            return 
            from path in Result.NotNull(filePath)
                from methodInfo in Config.InvokerAppFinderMethod is null ? Result.Success<MethodInfo>(null) : GetStaticPublicMethodFromString(Config.InvokerAppFinderMethod)
                from appFolderName in methodInfo is null ? Result.Success<string>(null) : Result.From(() => (string)methodInfo.Invoke(null, [filePath]))
                let appName = $"ApiInspector.{appFolderName}/ApiInspector.exe"
                let finalPath = Config.InvocationHandlerExePaths.FirstOrDefault(x => x.EndsWith(appName, StringComparison.OrdinalIgnoreCase))
                from y in Result.NotNull(finalPath)

                select y;

            
        }
        {
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new ArgumentNullException(nameof(filePath));
            }
        
            string appFolderName = null;

            if (Config.InvokerAppFinderMethod is not null)
            {
                var methodInfo = GetStaticPublicMethodFromString(Config.InvokerAppFinderMethod);
                if (methodInfo.HasError)
                {
                    return methodInfo.Error;
                }
            
                appFolderName = (string)methodInfo.Value.Invoke(null, [filePath]);
            }

            appFolderName ??= GetInvokerAppFolderName(filePath);

            var appName = $"ApiInspector.{appFolderName}/ApiInspector.exe";

            var path = Config.InvocationHandlerExePaths.FirstOrDefault(x => x.EndsWith(appName, StringComparison.OrdinalIgnoreCase));
            if (path is null)
            {
                return new InvalidOperationException($"AppFolder not found: {appFolderName}");
            }

            return path;
        }
    }

    static string GetInvokerAppFolderName(string filePath)
    {
        var targetRuntimeInfo = GetTargetRuntimeInfo(filePath);
        if (targetRuntimeInfo is null)
        {
            return null;
        }

        if (targetRuntimeInfo.IsNetFramework)
        {
            return "NetFramework.net48";
        }

        return $"NetCore.{targetRuntimeInfo.NetCoreVersion}";
    }

    static TargetRuntimeInfo GetTargetRuntimeInfo(string filePath)
    {
        var assembly = MetadataHelper.ReadAssembly(filePath);

        // 1) TargetFrameworkAttribute varsa al
        foreach (var attribute in assembly.CustomAttributes)
        {
            if (attribute.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
            {
                var raw = attribute.ConstructorArguments.Count > 0
                    ? attribute.ConstructorArguments[0].Value?.ToString()
                    : null;

                if (!string.IsNullOrEmpty(raw))
                {
                    return ParseFromFrameworkString(raw);
                }
            }
        }

        // 2) Basit isim/referans fallback (attribute yoksa)
        var asmName = assembly.Name?.Name ?? string.Empty;
        if (asmName.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase))
        {
            return new TargetRuntimeInfo { IsNetCore = true }; // versiyon yok
        }

        if (asmName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
        {
            return new TargetRuntimeInfo { IsNetFramework = true };
        }

        foreach (var reference in assembly.MainModule.AssemblyReferences)
        {
            if (reference.Name.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase))
            {
                return new TargetRuntimeInfo { IsNetCore = true };
            }

            if (reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                return new TargetRuntimeInfo { IsNetFramework = true };
            }

            if (reference.Name.IndexOf("netstandard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new TargetRuntimeInfo { IsNetStandard = true };
            }
        }

        // Bulunamadı
        return null;
    }

    // Basit regex parser: hem ".NETCoreApp,Version=v3.1" hem "net6.0" "net48" gibi formatları ele alır.
    static TargetRuntimeInfo ParseFromFrameworkString(string raw)
    {
        var s = raw.Trim();

        var lower = s.ToLowerInvariant();

        if (lower.Contains("netstandard"))
        {
            return new() { IsNetStandard = true };
        }

        if (lower.Contains("netframework"))
        {
            return new TargetRuntimeInfo { IsNetFramework = true };
        }

        // netcoreapp,Version=v3.1 gibi formatten Version çek
        var mVer = Regex.Match(lower, @"version=v(?<v>\d+(\.\d+)*)", RegexOptions.IgnoreCase);
        if (mVer.Success)
        {
            // Eğer "netcoreapp" içeriyorsa netcore, değilse netframework/netstandard kontrol ettik zaten
            if (lower.Contains("netcoreapp"))
            {
                return new TargetRuntimeInfo { IsNetCore = true, NetCoreVersion = mVer.Groups["v"].Value };
            }
            // Eğer netcoreapp yok ama version varsa ve kısa form "net5.0" vs. geleceği için aşağıya düşecek
        }

        // Kısa TFM formatı: net6.0, net48, net5.0, netcoreapp3.1 gibi
        // Önce "netcoreappX" den versiyon çek
        var mCoreApp = Regex.Match(lower, @"netcoreapp(?<v>\d+(\.\d+)*)");
        if (mCoreApp.Success)
        {
            return new TargetRuntimeInfo { IsNetCore = true, NetCoreVersion = mCoreApp.Groups["v"].Value };
        }

        // Kısa "netX" formunu yakala
        var mNetShort = Regex.Match(lower, @"\bnet(?<v>\d+(\.\d+)*)\b");
        if (mNetShort.Success)
        {
            var verStr = mNetShort.Groups["v"].Value; // örn "6.0" veya "48"
            // Eğer "48" gibi iki basamaksa "4.8" olarak yorumla
            if (Regex.IsMatch(verStr, @"^\d{2}$"))
            {
                verStr = verStr.Insert(1, ".");
            }

            // net5+ -> .NET (yeni unified runtime) => netcore olarak ele alıyoruz
            if (int.TryParse(verStr.Split('.')[0], out var major) && major >= 5)
            {
                return new TargetRuntimeInfo { IsNetCore = true, NetCoreVersion = verStr };
            }

            return new TargetRuntimeInfo { IsNetFramework = true };
        }

        // Son çare: eğer içinde "core" geçiyorsa netcore, "framework" geçiyorsa netframework
        if (lower.Contains("core"))
        {
            return new TargetRuntimeInfo { IsNetCore = true };
        }

        if (lower.Contains("framework"))
        {
            return new TargetRuntimeInfo { IsNetFramework = true };
        }

        return null;
    }
}