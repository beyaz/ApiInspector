using System.Collections;
using System.IO;
using System.Reflection;

namespace ApiInspector;

static class ReflectionHelper
{
    public static void AttachAssemblyResolver()
    {
        //AppDomain.CurrentDomain.AssemblyResolve -= TryResolveAssembly;
        //AppDomain.CurrentDomain.AssemblyResolve += TryResolveAssembly;
    }

    public static void AttachToAssemblyResolveSameDirectory(string fullAssemblyPath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CreateAssemblyResolver([Path.GetDirectoryName(fullAssemblyPath)]);
        
        // AppDomain.CurrentDomain.AssemblyResolve += (_, e) => TryLoadFromSameFolder(fullAssemblyPath, e);
    }

    public static object CreateDefaultValue(Type type)
    {
        if (type == typeof(string))
        {
            return "";
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType is not null)
            {
                return Array.CreateInstance(elementType, 0);
            }
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition.IsSubclassOf(typeof(IList)))
            {
                var genericArgument = type.GetGenericArguments().FirstOrDefault();
                if (genericArgument is not null)
                {
                    return Array.CreateInstance(genericArgument, 0);
                }
            }
        }

        try
        {
            var instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                return null;
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                // avoid circular
                if (propertyInfo.PropertyType == type)
                {
                    continue;
                }

                var existingValue = propertyInfo.GetValue(instance);
                if (existingValue == null)
                {
                    propertyInfo.SetValue(instance, CreateDefaultValue(propertyInfo.PropertyType));
                }
            }

            return instance;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static (bool isDotNetCore, bool isDotNetFramework) GetTargetFramework(FileInfo dll)
    {
        var fileContent = File.ReadAllText(dll.FullName);

        if (fileContent.IndexOf(".NETFramework,Version=v", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return (isDotNetCore: false, isDotNetFramework: true);
        }

        if (fileContent.IndexOf(".NETCoreApp,Version=v", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return (isDotNetCore: true, isDotNetFramework: false);
        }

        return (false, false);
    }

    public static Assembly LoadFrom(string fullAssemblyPath)
    {
        var isMsCorLib = Path.GetFileNameWithoutExtension(typeof(object).Assembly.Location) == Path.GetFileNameWithoutExtension(fullAssemblyPath);
        if (isMsCorLib)
        {
            return typeof(object).Assembly;
        }

        return Assembly.LoadFrom(fullAssemblyPath);
    }

    static Assembly LoadAssemblyFile(string filePath)
    {
        return SafeInvoke(() => Assembly.LoadFrom(filePath)).TraceError(traceError).Unwrap();

        void traceError(Exception exception) => WriteLog($"Assembly load failed. @filePath: {filePath}, @exception: {exception}");
    }

    static (bool success, T value) OnFail<T>(this (bool success, T value) tuple, Action action)
    {
        if (!tuple.success)
        {
            action();
        }

        return tuple;
    }

    static Assembly TryResolveAssembly(object _, ResolveEventArgs e)
    {
        var requestedAssemblyName = new AssemblyName(e.Name);

        WriteLog($"RequestedAssemblyName: {requestedAssemblyName}");

        var fileNameWithoutExtension = requestedAssemblyName.Name;

        var pipe = new[]
        {
            () => tryLoadSystemAssembliesFromSdk(requestedAssemblyName),
            () => tryFindAssemblyByUsingPlugins(fileNameWithoutExtension),
            () => tryLoadFromSearchDirectories(e, fileNameWithoutExtension)
        };

        return run(pipe).OnFail(onFail).ValueOrDefault();

        void onFail()
        {
            var errorMessage = $"Assembly not resolved. @fileNameWithoutExtension: {fileNameWithoutExtension}";
            WriteLog(errorMessage);
        }

        static (bool success, Assembly assembly) tryFindAssemblyByUsingPlugins(string fileNameWithoutExtension)
        {
            var extensions = new[] { ".dll", ".exe" };

            foreach (var fileExtension in extensions)
            {
                var fileName = fileNameWithoutExtension + fileExtension;

                var fullFilePath = Plugin.TryFindFullFilePathOfAssembly(fileName);
                if (fullFilePath is not null && File.Exists(fullFilePath))
                {
                    return (true, LoadAssemblyFile(fullFilePath));
                }
            }

            return default;
        }

        static (bool success, Assembly assembly) tryLoadSystemAssembliesFromSdk(AssemblyName requestedAssemblyName)
        {
            if (requestedAssemblyName.Name?.StartsWith("System.", StringComparison.OrdinalIgnoreCase) is true)
            {
                var currentEnvironmentIsDotNetFramework = GetTargetFramework(new FileInfo(typeof(ReflectionHelper).Assembly.Location)).isDotNetFramework;

                if (currentEnvironmentIsDotNetFramework)
                {
                    if (Directory.Exists("C:\\Program Files\\dotnet\\sdk\\"))
                    {
                        foreach (var folderPath in Directory.GetDirectories("C:\\Program Files\\dotnet\\sdk\\").OrderByDescending(x => x))
                        {
                            var majorVersion = requestedAssemblyName.Version?.Major.ToString();

                            var folderNameHasMatchMajorVersion = !string.IsNullOrWhiteSpace(majorVersion) && Path.GetFileName(folderPath).StartsWith(majorVersion, StringComparison.OrdinalIgnoreCase);
                            if (folderNameHasMatchMajorVersion)
                            {
                                var finalFolderPath = Path.Combine(folderPath, "Containers", "tasks", "net472");

                                var finalFilePath = Path.Combine(finalFolderPath, requestedAssemblyName.Name + ".dll");
                                if (File.Exists(finalFilePath))
                                {
                                    return (true, LoadAssemblyFile(finalFilePath));
                                }
                            }
                        }
                    }

                    if (Directory.Exists(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\"))
                    {
                        foreach (var folderPath in Directory.GetDirectories(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\").OrderByDescending(x => x))
                        {
                            var majorVersion = requestedAssemblyName.Version?.Major.ToString();

                            var folderNameHasMatchMajorVersion = !string.IsNullOrWhiteSpace(majorVersion) && Path.GetFileName(folderPath).StartsWith(majorVersion, StringComparison.OrdinalIgnoreCase);
                            if (folderNameHasMatchMajorVersion)
                            {
                                var finalFilePath = Path.Combine(folderPath, requestedAssemblyName.Name + ".dll");
                                if (File.Exists(finalFilePath))
                                {
                                    return (true, LoadAssemblyFile(finalFilePath));
                                }
                            }
                        }
                    }
                }
            }

            return default;
        }

        static (bool success, Assembly assembly) tryLoadFromSearchDirectories(ResolveEventArgs e, string fileNameWithoutExtension)
        {
            var extensions = new[] { ".dll", ".exe" };

            foreach (var searchDirectory in getSearchDirectories(e.RequestingAssembly))
            {
                foreach (var fileExtension in extensions)
                {
                    var filePath = Path.Combine(searchDirectory, fileNameWithoutExtension + fileExtension);
                    if (File.Exists(filePath))
                    {
                        return (success: true, LoadAssemblyFile(filePath));
                    }
                }
            }

            return default;

            static IReadOnlyList<string> getSearchDirectories(Assembly requestingAssembly)
            {
                var searchDirectories = new List<string>();

                SafeInvoke(() => Path.GetDirectoryName(requestingAssembly?.Location)).Then(directoryName =>
                {
                    if (directoryName != null)
                    {
                        searchDirectories.Insert(0, directoryName);
                    }
                });

                if (GetTargetFramework(new FileInfo(typeof(ReflectionHelper).Assembly.Location)).isDotNetCore)
                {
                    var version = Environment.Version.ToString();

                    var folders = new[]
                    {
                        $"C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App\\{version}",
                        $"C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\{version}",
                        $"C:\\Program Files\\dotnet\\shared\\Microsoft.WindowsDesktop.App\\{version}"
                    };

                    foreach (var folder in folders)
                    {
                        searchDirectories.Add(folder);
                    }
                }

                return searchDirectories;
            }
        }

        static (bool success, T value) run<T>(Func<(bool success, T value)>[] methods)
        {
            foreach (var method in methods)
            {
                var (success, value) = method();
                if (success)
                {
                    return (true, value);
                }
            }

            return default;
        }
    }

    static Assembly TryLoadFromSameFolder(string fullAssemblyPath, ResolveEventArgs e)
    {
        var result = TryLoadFromSameFolder(fullAssemblyPath, new AssemblyName(e.Name).Name);
        if (result.success)
        {
            WriteLog(result.trace);
            return result.assembly;
        }

        WriteLog(result.trace);

        return null;
    }

    static (bool success, Assembly assembly, Exception exception, IReadOnlyList<string> trace) TryLoadFromSameFolder(string fullAssemblyPath, string requestedAssemblyName)
    {
        var directoryInfo = Directory.GetParent(fullAssemblyPath);
        if (directoryInfo is null)
        {
            return new()
            {
                trace = [$"{nameof(TryLoadFromSameFolder)} / FirectoryNotFound / {fullAssemblyPath}"]
            };
        }

        var fullFilePath = Path.Combine(directoryInfo.FullName, requestedAssemblyName + ".dll");
        if (!File.Exists(fullFilePath))
        {
            return new()
            {
                trace = [$"{nameof(TryLoadFromSameFolder)} / FileNotFound / {fullFilePath}"]
            };
        }

        try
        {
            return new()
            {
                success  = true,
                assembly = Assembly.LoadFrom(fullFilePath),
                trace    = [$"Successfully loaded assembly({requestedAssemblyName}) from same folder."]
            };
        }
        catch (Exception exception)
        {
            return new()
            {
                exception = exception,
                trace     = [$"Failed when loading assembly({requestedAssemblyName}) from same folder."]
            };
        }
    }

    static T ValueOrDefault<T>(this (bool success, T value) tuple)
    {
        if (tuple.success)
        {
            return tuple.value;
        }

        return default;
    }


    internal static ResolveEventHandler CreateAssemblyResolver(IReadOnlyList<string> searchDirectories)
    {
        var directories = searchDirectories
                         .Where(Directory.Exists)
                         .Select(Path.GetFullPath)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToArray();

        return (_sender, args) =>
        {
            var requestedName = new AssemblyName(args.Name);

            try
            {
                var alreadyLoaded = AppDomain.CurrentDomain
                                             .GetAssemblies()
                                             .FirstOrDefault(a =>
                                              {
                                                  try
                                                  {
                                                      return AssemblyName.ReferenceMatchesDefinition(a.GetName(), requestedName);
                                                  }
                                                  catch
                                                  {
                                                      return false;
                                                  }
                                              });

                if (alreadyLoaded != null)
                {
                    return alreadyLoaded;
                }

                //
                // System.* / Microsoft.* için önce runtime'a bırak.
                //
                if (IsFrameworkAssembly(requestedName))
                {
                    try
                    {
                        return Assembly.Load(requestedName);
                    }
                    catch
                    {
                    }
                }

                //
                // Kullanıcının verdiği klasörler
                //
                var candidate = FindBestAssembly(
                    requestedName,
                    directories);

                if (candidate != null)
                {
                    return Assembly.LoadFrom(candidate);
                }

                //
                // Runtime dizini
                //
                var runtimeDir = Path.GetDirectoryName(
                    typeof(object).Assembly.Location);

                if (!string.IsNullOrWhiteSpace(runtimeDir))
                {
                    candidate = FindBestAssembly(
                        requestedName,
                        new[] { runtimeDir });

                    if (candidate != null)
                    {
                        return Assembly.LoadFrom(candidate);
                    }
                }

                //
                // Microsoft/System ise bütün runtime ve sdk klasörlerine bak
                //
                if (IsFrameworkAssembly(requestedName))
                {
                    var frameworkDirs = GetFrameworkDirectories();

                    candidate = FindBestAssembly(
                        requestedName,
                        frameworkDirs);

                    if (candidate != null)
                    {
                        return Assembly.LoadFrom(candidate);
                    }

                    //
                    // son kez runtime'a sor
                    //
                    try
                    {
                        return Assembly.Load(requestedName);
                    }
                    catch
                    {
                    }
                }

                return null;

                // ==========================
                // Local Functions
                // ==========================

                static bool IsFrameworkAssembly(AssemblyName name)
                {
                    var n = name.Name ?? "";

                    return n.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                           || n.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                           || n.Equals("System", StringComparison.OrdinalIgnoreCase)
                           || n.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)
                           || n.Equals("netstandard", StringComparison.OrdinalIgnoreCase);
                }

                static IEnumerable<string> GetFrameworkDirectories()
                {
                    var result = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                    void SafeAdd(string? path)
                    {
                        if (!string.IsNullOrWhiteSpace(path)
                            && Directory.Exists(path))
                        {
                            result.Add(path);
                        }
                    }

                    //
                    // Runtime klasörü
                    //
                    SafeAdd(Path.GetDirectoryName(
                        typeof(object).Assembly.Location));

                    //
                    // DOTNET_ROOT
                    //
                    SafeAdd(Environment.GetEnvironmentVariable(
                        "DOTNET_ROOT"));

                    SafeAdd(Environment.GetEnvironmentVariable(
                        "DOTNET_ROOT(x86)"));

                    //
                    // Default install locations
                    //
                    SafeAdd(
                        @"C:\Program Files\dotnet");

                    SafeAdd(
                        @"C:\Program Files (x86)\dotnet");

                    foreach (var root in result.ToList())
                    {
                        try
                        {
                            foreach (var dir in Directory.EnumerateDirectories(
                                         root,
                                         "*",
                                         SearchOption.AllDirectories))
                            {
                                result.Add(dir);
                            }
                        }
                        catch
                        {
                        }
                    }

                    return result;
                }

                static string? FindBestAssembly(
                    AssemblyName requested,
                    IEnumerable<string> roots)
                {
                    var requestedVersion =
                        requested.Version ?? new Version(0, 0);

                    var requestedPkt =
                        requested.GetPublicKeyToken();

                    string? bestPath = null;
                    var bestScore = long.MinValue;

                    foreach (var root in roots)
                    {
                        if (!Directory.Exists(root))
                        {
                            continue;
                        }

                        IEnumerable<string> files;

                        try
                        {
                            files = Directory.EnumerateFiles(
                                root,
                                $"{requested.Name}.dll",
                                SearchOption.AllDirectories);
                        }
                        catch
                        {
                            continue;
                        }

                        foreach (var file in files)
                        {
                            try
                            {
                                var candidateName =
                                    AssemblyName.GetAssemblyName(file);

                                if (candidateName.Version != requested.Version)
                                {
                                    ;
                                }
                                if (!string.Equals(
                                        candidateName.Name,
                                        requested.Name,
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                //
                                // Strong name uyumu
                                //
                                if (requestedPkt is { Length: > 0 })
                                {
                                    var candidatePkt =
                                        candidateName.GetPublicKeyToken();

                                    if (candidatePkt == null ||
                                        !candidatePkt.SequenceEqual(
                                            requestedPkt))
                                    {
                                        continue;
                                    }
                                }

                                var version =
                                    candidateName.Version
                                    ?? new Version(0, 0);

                                long score;

                                if (version == requestedVersion)
                                {
                                    score = long.MaxValue;
                                }
                                else
                                {
                                    score =
                                        -Math.Abs(
                                            version.CompareTo(
                                                requestedVersion));
                                }

                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    bestPath  = file;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    return bestPath;
                }
            }
            catch(Exception exception)
            {
                return null;
            }
        };
    }
    

}