using System.Collections;
using System.IO;
using System.Reflection;
using static ApiInspector.FpExtensions;

namespace ApiInspector;

static class ReflectionHelper
{
    public static void AttachAssemblyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssemblyInSameFolder;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyInSameFolder;
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

    static Assembly LoadAssemblyFile(string filePath)
    {
        return SafeInvoke(() => Assembly.LoadFrom(filePath)).TraceError(traceError).Unwrap();

        void traceError(Exception exception) => FileHelper.WriteLog($"Assembly load failed. @filePath: {filePath}, @exception: {exception}");
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

    
    static Assembly ResolveAssemblyInSameFolder(object _, ResolveEventArgs e)
    {
        var requestedAssemblyName = new AssemblyName(e.Name);

        var currentEnvironmentIsDotNetFramework = GetTargetFramework(new FileInfo(typeof(ReflectionHelper).Assembly.Location)).isDotNetFramework;

        if (requestedAssemblyName.Name.StartsWith("System.",StringComparison.OrdinalIgnoreCase))
        {
            if (currentEnvironmentIsDotNetFramework)
            {
                if (Directory.Exists("C:\\Program Files\\dotnet\\sdk\\"))
                {
                    foreach (var folderPath in Directory.GetDirectories("C:\\Program Files\\dotnet\\sdk\\").OrderByDescending(x=>x))
                    {
                        if (Path.GetFileName(folderPath).StartsWith(requestedAssemblyName.Version.Major.ToString(),StringComparison.OrdinalIgnoreCase))
                        {
                            var finalFolderPath = Path.Combine(folderPath, "Containers", "tasks", "net472");

                            var finalFilePath = Path.Combine(finalFolderPath, requestedAssemblyName.Name + ".dll");
                            if (File.Exists(finalFilePath))
                            {
                                return LoadAssemblyFile(finalFilePath);
                            }
                            
                        }
                    }
                }
            }
        }
        
        var searchDirectories = new List<string>();

        var fileNameWithoutExtension = requestedAssemblyName.Name;

        SafeInvoke(() => Path.GetDirectoryName(e.RequestingAssembly?.Location)).Then(directoryName =>
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

        var extensions = new[] { ".dll", ".exe" };

        foreach (var searchDirectory in searchDirectories)
        {
            foreach (var fileExtension in extensions)
            {
                var filePath = Path.Combine(searchDirectory, fileNameWithoutExtension + fileExtension);
                if (File.Exists(filePath))
                {
                    return LoadAssemblyFile(filePath);
                }
            }
        }

        foreach (var fileExtension in extensions)
        {
            var fileName = fileNameWithoutExtension + fileExtension;

            var fullFilePath = Plugin.TryFindFullFilePathOfAssembly(fileName);
            if (fullFilePath is not null)
            {
                return LoadAssemblyFile(fullFilePath);
            }
        }

        FileHelper.WriteLog($"Assembly not resolved. @fileNameWithoutExtension: {fileNameWithoutExtension}");

        return null;
    }
}