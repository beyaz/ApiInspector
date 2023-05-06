﻿using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

        object instance;
        try
        {
            instance = Activator.CreateInstance(type);
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

    static Assembly ResolveAssemblyInSameFolder(object _, ResolveEventArgs e)
    {
        var searchDirectories = new List<string>();

        var fileNameWithoutExtension = new AssemblyName(e.Name).Name;

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

            var fullFilePath = Plugins.TryFindFullFilePathOfAssembly(fileName);
            if (fullFilePath is not null)
            {
                return LoadAssemblyFile(fullFilePath);
            }
        }

        FileHelper.WriteLog($"Assembly not resolved. @fileNameWithoutExtension: {fileNameWithoutExtension}");

        return null;
    }
}