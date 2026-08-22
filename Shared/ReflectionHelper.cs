using System.IO;
using System.Reflection;

namespace ApiInspector;

static class ReflectionHelper
{
    public static void AttachToAssemblyResolveSameDirectory(string fullAssemblyPath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CreateAssemblyResolver(fullAssemblyPath);
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

    internal static ResolveEventHandler CreateAssemblyResolver(string fullAssemblyPath)
    {
        return (_, args) =>
        {
            var requestedName = new AssemblyName(args.Name);

            // I s   A l r e a d y   L o a d e d
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
            }

            // U s e r   D i r e c t o r i e s
            {
                var assemblyFilePath = Path.Combine(Path.GetDirectoryName(fullAssemblyPath) ?? string.Empty, $"{requestedName.Name}.dll");
                if (File.Exists(assemblyFilePath))
                {
                    try
                    {
                        return Assembly.LoadFrom(assemblyFilePath);
                    }
                    catch (Exception exception)
                    {
                        WriteLog(exception.ToString());

                        throw;
                    }
                }
            }

            // R u n t i m e   D i r e c t o r i e s
            {
                var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

                if (!string.IsNullOrWhiteSpace(runtimeDir))
                {
                    var candidate = FindBestAssembly(requestedName, [runtimeDir]);
                    if (candidate != null)
                    {
                        return Assembly.LoadFrom(candidate);
                    }
                }
            }

            // F r a m e w o r k   D i r e c t o r i e s
            {
                if (IsFrameworkAssembly(requestedName))
                {
                    var frameworkDirs = GetFrameworkDirectories();

                    var candidate = FindBestAssembly(requestedName, frameworkDirs);

                    if (candidate != null)
                    {
                        return Assembly.LoadFrom(candidate);
                    }
                }
            }

            return null;

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
                var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                void SafeAdd(string path)
                {
                    if (!string.IsNullOrWhiteSpace(path)
                        && Directory.Exists(path))
                    {
                        result.Add(path);
                    }
                }

                SafeAdd(Path.GetDirectoryName(
                    typeof(object).Assembly.Location));

                SafeAdd(Environment.GetEnvironmentVariable(
                    "DOTNET_ROOT"));

                SafeAdd(Environment.GetEnvironmentVariable(
                    "DOTNET_ROOT(x86)"));

                SafeAdd(@"C:\Program Files\dotnet");

                SafeAdd(@"C:\Program Files (x86)\dotnet");

                foreach (var root in result.ToList())
                {
                    try
                    {
                        foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
                        {
                            result.Add(dir);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return result;
            }

            static string FindBestAssembly(AssemblyName requested, IEnumerable<string> roots)
            {
                var requestedVersion = requested.Version ?? new Version(0, 0);

                var requestedPkt = requested.GetPublicKeyToken();

                string bestPath = null;

                var bestScore = long.MinValue;

                foreach (var root in roots)
                {
                    if (!Directory.Exists(root))
                    {
                        continue;
                    }

                    WriteLog($"Trying to find '{requested.Name}' in '{root}'");

                    IEnumerable<string> files;

                    try
                    {
                        files = Directory.EnumerateFiles(root, $"{requested.Name}.dll", SearchOption.AllDirectories);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var file in files)
                    {
                        try
                        {
                            var candidateName = AssemblyName.GetAssemblyName(file);

                            if (!string.Equals(candidateName.Name, requested.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            //
                            // Strong name uyumu
                            //
                            if (requestedPkt is { Length: > 0 })
                            {
                                var candidatePkt = candidateName.GetPublicKeyToken();

                                if (candidatePkt == null || !candidatePkt.SequenceEqual(requestedPkt))
                                {
                                    continue;
                                }
                            }

                            var version = candidateName.Version ?? new Version(0, 0);

                            long score;

                            if (version == requestedVersion)
                            {
                                score = long.MaxValue;

                                return file;
                            }

                            score = -Math.Abs(version.CompareTo(requestedVersion));

                            if (score > bestScore)
                            {
                                bestScore = score;

                                bestPath = file;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                return bestPath;
            }
        };
    }
}