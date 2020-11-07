using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using Mono.Cecil;
using static ApiInspector.Keys;
using static ApiInspector.DataAccess.TypeVisitor;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
    {
        #region Public Methods
        public static void HandleEvent(ViewEvents e, InvocationInfo invocationInfo, ItemSourceList itemSources, Action<string> log)
        {
            void OnAssemblySearchDirectoryChanged()
            {
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

             void OnAssemblyNameChanged( )
            {
                

                var assemblyFilePath = GetAssemblyFilePath(invocationInfo);

                if (!File.Exists(assemblyFilePath))
                {
                    log($"File not exists. File:{assemblyFilePath}");
                    return;
                }

                var assemblySearchDirectory = invocationInfo.AssemblySearchDirectory;

                var scope2 = new Scope
                {
                    {
                        AssemblySearchDirectories, new List<string>
                        {
                            assemblySearchDirectory
                        }
                    },

                    {AssemblyPath, assemblyFilePath}
                };

                itemSources.ClassNameList = GeTypeDefinitions(scope2).Select(x => x.FullName).ToList();
            }

            switch (e)
            {
                case ViewEvents.OnAssemblySearchDirectoryChanged:
                {
                    OnAssemblySearchDirectoryChanged();
                    return;
                }
                case ViewEvents.OnAssemblyNameChanged:
                {
                    OnAssemblyNameChanged();
                    return;
                }
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

            var assemblySearchDirectory = invocationInfo.AssemblySearchDirectory;

            var scope2 = new Scope
            {
                {
                    AssemblySearchDirectories, new List<string>
                    {
                        assemblySearchDirectory
                    }
                },

                {AssemblyPath, assemblyFilePath}
            };
            var typeDefinition = FindTypeDefinition(scope2, invocationInfo.ClassName);
            scope.Update(Keys.TypeDefinition, typeDefinition);
            if (typeDefinition == null)
            {
                log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            itemSources.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();

            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
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
            var typeDefinition = scope.Get(Keys.TypeDefinition);

            scope.Update(Keys.MethodDefinition, FindMatchedFunction(invocationInfo, typeDefinition));
        }
        #endregion

        #region Methods
        static MethodDefinition FindMatchedFunction(InvocationInfo invocationInfo, TypeDefinition typeDefinition)
        {
            return typeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);
        }

        /// <summary>
        ///     Gets the assembly file path.
        /// </summary>
        static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }
        #endregion
    }
}