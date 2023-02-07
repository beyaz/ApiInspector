﻿using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
        return SafeInvoke(() => Assembly.LoadFile(filePath)).TraceError(traceError).Unwrap();

        void traceError(Exception exception) => FileHelper.WriteLog($"Assembly load failed. @filePath: {filePath}, @exception: {exception}");
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

        if (RuntimeInformation.FrameworkDescription.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var folders = new[]
            {
                "C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App\\3.1.29",
                "C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.29",
                "C:\\Program Files\\dotnet\\shared\\Microsoft.WindowsDesktop.App\\3.1.29"
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

            var fullFilePath = Plugins.TryFindAssembly(fileName);
            if (fullFilePath is not null)
            {
                return LoadAssemblyFile(fullFilePath);
            }
        }

        FileHelper.WriteLog($"Assembly not resolved. @fileNameWithoutExtension: {fileNameWithoutExtension}");

        return null;
    }
}