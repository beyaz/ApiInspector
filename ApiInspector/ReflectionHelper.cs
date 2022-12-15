using System.Collections;
using System.IO;
using System.Reflection;

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

        object instance;
        try
        {
            instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }

        foreach (var propertyInfo in type.GetProperties())
        {
            if (propertyInfo.GetIndexParameters().Length > 0)
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

    public static bool IsStaticClass(this Type type)
    {
        return type.IsClass && type.IsAbstract && type.IsSealed;
    }

    static Assembly ResolveAssemblyInSameFolder(object _, ResolveEventArgs e)
    {
        var searchDirectories = new List<string>();

        foreach (var plugin in Plugins.ListOfPlugins)
        {
            if (plugin.AssemblySearchDirectories != null)
            {
                searchDirectories.AddRange(plugin.AssemblySearchDirectories);
            }
        }

        var fileNameWithoutExtension = new AssemblyName(e.Name).Name;

        var directoryName = Path.GetDirectoryName(e.RequestingAssembly?.Location);
        if (directoryName != null)
        {
            searchDirectories.Insert(0, directoryName);
        }

        foreach (var searchDirectory in searchDirectories)
        {
            foreach (var fileExtension in new[] { ".dll", ".exe" })
            {
                var filePath = Path.Combine(searchDirectory, fileNameWithoutExtension + fileExtension);
                if (File.Exists(filePath))
                {
                    return Assembly.LoadFile(filePath);
                }
            }
        }

        return null;
    }
}