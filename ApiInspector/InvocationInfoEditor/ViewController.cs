using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using Mono.Cecil;
using static ApiInspector.Keys;
using static ApiInspector.DataAccess.TypeVisitor;
using static ApiInspector.Utility;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
    {
        #region Public Methods
        /// <summary>
        ///     Called when [assembly name changed].
        /// </summary>
        public static void OnAssemblyNameChanged(Scope scope)
        {
            var itemSources = scope.Get(ItemsSources);

            itemSources.ClassNameList = GetClassNamesOfSelectedAssembly(scope);
        }
        
        public static void OnAssemblySearchDirectoryChanged(string assemblySearchDirectory,Action<IReadOnlyList<string>> setAssemblyNames)
        {
            setAssemblyNames( GetAssemblyListInDirectory(assemblySearchDirectory));
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public static void OnClassNameChanged(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var log            = scope.Get(Trace);
            var itemSources    = scope.Get(ItemsSources);

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            var typeDefinition = FindType(scope);

            scope.Update(Keys.TypeDefinition, typeDefinition);
            if (typeDefinition == null)
            {
                log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            itemSources.MethodNameList = GetMethodNameListFromSelectedType(typeDefinition, invocationInfo.AssemblySearchDirectory);
        }

        /// <summary>
        ///     Called when [method name selected].
        /// </summary>
        public static void OnMethodNameSelected(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var typeDefinition = scope.Get(Keys.TypeDefinition);

            scope.Update(Keys.MethodDefinition, typeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName));
        }
        #endregion

        #region Methods
        static IReadOnlyList<string> GetAssemblyListInDirectory(string assemblySearchDirectory)
        {
            if (!Directory.Exists(assemblySearchDirectory))
            {
                return new string[0];
            }

            var assemblyNameList = Directory.GetFiles(assemblySearchDirectory).Select(Path.GetFileName).ToList();

            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                return assemblyNameList.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("BOA.EOD.")).ToList();
            }

            return assemblyNameList;
        }

        static List<string> GetMethodNameListFromSelectedType(TypeDefinition typeDefinition, string assemblySearchDirectory)
        {
            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                return new List<string>
                {
                    EndOfDay.MethodAccessText
                };
            }

            return typeDefinition.Methods.Select(x => x.Name).ToList();
        }
        #endregion
    }
}

namespace ApiInspector
{
    partial class Utility
    {
        #region Public Methods
        public static TypeDefinition FindType(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var log            = scope.Get(Trace);

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return null;
            }

            scope.OpenNewLayer("Searching type definition");

            scope.Add(AssemblySearchDirectories, GetAssemblySearchDirectories(invocationInfo));

            scope.Add(AssemblyPath, GetAssemblyFilePath(invocationInfo));

            var trace                     = scope.Get(Trace);
            var assemblyPath              = scope.Get(AssemblyPath);
            var assemblySearchDirectories = scope.Get(AssemblySearchDirectories);

            var typeDefinition = GetTypeDefinitionsInAssembly(trace, assemblyPath, assemblySearchDirectories).FirstOrDefault(type => type.FullName == invocationInfo.ClassName);

            scope.CloseCurrentLayer();

            return typeDefinition;
        }

        public static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }

        public static List<string> GetAssemblySearchDirectories(InvocationInfo invocationInfo)
        {
            return ListOf(invocationInfo.AssemblySearchDirectory);
        }

        public static IReadOnlyList<string> GetClassNamesOfSelectedAssembly(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var log            = scope.Get(Trace);

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return new List<string>();
            }

            scope.OpenNewLayer("Searching assembly");

            scope.Add(AssemblySearchDirectories, GetAssemblySearchDirectories(invocationInfo));
            scope.Add(AssemblyPath, GetAssemblyFilePath(invocationInfo));

            var trace                     = scope.Get(Trace);
            var assemblyPath              = scope.Get(AssemblyPath);
            var assemblySearchDirectories = scope.Get(AssemblySearchDirectories);

            var names                     = GetTypeDefinitionsInAssembly(trace, assemblyPath, assemblySearchDirectories).Select(x => x.FullName).ToList();

            scope.CloseCurrentLayer();

            return names;
        }
        #endregion
    }
}