using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using Mono.Cecil;
using static ApiInspector.Keys;
using static ApiInspector.DataAccess.TypeVisitor;
using static ApiInspector.Utility;
using ActionStringList = System.Action<System.Collections.Generic.IReadOnlyList<string>>;
using ActionString = System.Action<string>;

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
            var trace               = scope.Get(Trace);
            var invocationInfo      = scope.Get(SelectedInvocationInfo);
            var getAssemblyFileName = scope.Get(GetAssemblyFileName);

            invocationInfo.AssemblyName = getAssemblyFileName();

            scope.Update(ClassNameSuggestions, GetClassNamesOfSelectedAssembly(invocationInfo, trace));
        }

        public static void OnAssemblySearchDirectoryChanged(Scope scope)
        {
            var assemblySearchDirectory = scope.Get(GetAssemblySearchDirectory)();
            var invocationInfo          = scope.Get(SelectedInvocationInfo);

            invocationInfo.AssemblySearchDirectory = assemblySearchDirectory;

            var assemblyListInDirectory = GetAssemblyListInDirectory(assemblySearchDirectory);

            scope.Update(AssemblyNameSuggestions, assemblyListInDirectory);
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public static void OnClassNameChanged(Scope scope)
        {
            var trace          = scope.Get(Trace);
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var getClassName   = scope.Get(GetClassName);

            invocationInfo.ClassName = getClassName();

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            var typeDefinition = FindType(invocationInfo, trace);

            scope.Update(Keys.TypeDefinition, typeDefinition);
            if (typeDefinition == null)
            {
                trace($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            var methodNames = GetMethodNameListFromSelectedType(typeDefinition, invocationInfo.AssemblySearchDirectory);

            scope.Update(MethodNameSuggestions, methodNames);
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
        /// <summary>
        ///     Gets the assembly list in directory.
        /// </summary>
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

        /// <summary>
        ///     Gets the type of the method name list from selected.
        /// </summary>
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
    /// <summary>
    ///     The utility
    /// </summary>
    partial class Utility
    {
        #region Public Methods
        /// <summary>
        ///     Finds the type.
        /// </summary>
        public static TypeDefinition FindType(InvocationInfo invocationInfo, ActionString trace)
        {
            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                trace($"File not exists. File:{assemblyFilePath}");
                return null;
            }

            return GetTypeDefinitionsInAssembly(trace, assemblyFilePath, GetAssemblySearchDirectories(invocationInfo)).FirstOrDefault(type => type.FullName == invocationInfo.ClassName);
        }

        /// <summary>
        ///     Gets the assembly file path.
        /// </summary>
        public static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }

        /// <summary>
        ///     Gets the assembly search directories.
        /// </summary>
        public static List<string> GetAssemblySearchDirectories(InvocationInfo invocationInfo)
        {
            return ListOf(invocationInfo.AssemblySearchDirectory);
        }

        /// <summary>
        ///     Gets the class names of selected assembly.
        /// </summary>
        public static IReadOnlyList<string> GetClassNamesOfSelectedAssembly(InvocationInfo invocationInfo, ActionString trace)
        {
            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                trace($"File not exists. File:{assemblyFilePath}");
                return new List<string>();
            }

            var assemblySearchDirectories = GetAssemblySearchDirectories(invocationInfo);

            return GetTypeDefinitionsInAssembly(trace, assemblyFilePath, assemblySearchDirectories).Select(x => x.FullName).ToList();
        }
        #endregion
    }
}