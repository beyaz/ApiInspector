using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
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

        /// <summary>
        ///     Called when [assembly search directory changed].
        /// </summary>
        public static void OnAssemblySearchDirectoryChanged(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var itemSources    = scope.Get(ItemsSources);

            var assemblySearchDirectory = invocationInfo.AssemblySearchDirectory;
            if (!Directory.Exists(assemblySearchDirectory))
            {
                return;
            }

            itemSources.AssemblyNameList = Directory.GetFiles(assemblySearchDirectory).Select(Path.GetFileName).ToList();

            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                itemSources.AssemblyNameList = itemSources.AssemblyNameList.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("BOA.EOD.")).ToList();
            }
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
            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            scope.OpenNewLayer("Searching type definition");

            scope.Add(AssemblySearchDirectories, GetAssemblySearchDirectories(invocationInfo));
            scope.Add(AssemblyPath, GetAssemblyFilePath(invocationInfo));

            var typeDefinition = FindTypeDefinition(scope, invocationInfo.ClassName);

            scope.CloseCurrentLayer();

            scope.Update(TypeDefinition, typeDefinition);
            if (typeDefinition == null)
            {
                log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            itemSources.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();

            if (invocationInfo.AssemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                itemSources.MethodNameList = new List<string>
                {
                    EndOfDay.MethodAccessText
                };
            }
        }

        /// <summary>
        ///     Called when [method name selected].
        /// </summary>
        public static void OnMethodNameSelected(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var typeDefinition = scope.Get(TypeDefinition);

            scope.Update(MethodDefinition, typeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName));
        }
        #endregion
    }
}

namespace ApiInspector
{
    partial class Utility
    {
        #region Public Methods
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

            var names = GeTypeDefinitions(scope).Select(x => x.FullName).ToList();

            scope.CloseCurrentLayer();

            return names;
        }
        #endregion
    }
}