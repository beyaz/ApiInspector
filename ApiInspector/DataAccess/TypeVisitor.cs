using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using static ApiInspector.Application.CommonApplicationKeys;

namespace ApiInspector.DataAccess
{
    /// <summary>
    ///     The type visitor
    /// </summary>
    static class TypeVisitor
    {
        #region Public Methods
        /// <summary>
        ///     Finds the type.
        /// </summary>
        public static TypeDefinition FindTypeDefinition(Scope scope, string assemblyPath, string typeFullName)
        {
            return GetTypeDefinitionsInAssembly(scope, assemblyPath).FirstOrDefault(type => type.FullName == typeFullName);
        }

        /// <summary>
        ///     Ges the type definitions.
        /// </summary>
        public static IReadOnlyList<TypeDefinition> GeTypeDefinitions(Scope scope, string assemblyPath)
        {
            return GetTypeDefinitionsInAssembly(scope, assemblyPath).ToList();
        }
        #endregion

        #region Methods

        static AssemblyDefinition GetAssemblyDefinition(IReadOnlyList<string> assemblySearchDirectories, string assemblyPath,Action<string> onError)
        {
            var resolver = new DefaultAssemblyResolver();

            foreach (var directory in assemblySearchDirectories)
            {
                resolver.AddSearchDirectory(directory);
            }

            try
            {
                return AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {AssemblyResolver = resolver});
            }
            catch (Exception e)
            {
                onError($"File not Loaded. File:{assemblyPath}, Error: {e}");
            }

            return null;
        }

        /// <summary>
        ///     Gets the type definitions in assembly.
        /// </summary>
        static IEnumerable<TypeDefinition> GetTypeDefinitionsInAssembly(Scope scope, string assemblyPath)
        {
            var log = scope.Get(Trace);

            IReadOnlyList<string> assemblySearchDirectories = scope.Get(AssemblySearchDirectories);

            {
                if (File.Exists(assemblyPath) == false)
                {
                    yield break;
                }

                var resolver = new DefaultAssemblyResolver();

                foreach (var directory in assemblySearchDirectories)
                {
                    resolver.AddSearchDirectory(directory);
                }

                AssemblyDefinition assemblyDefinition;

                try
                {
                    assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {AssemblyResolver = resolver});
                }
                catch (Exception e)
                {
                    log($"File not Loaded. File:{assemblyPath}, Error: {e}");
                    yield break;
                }

                foreach (var moduleDefinition in assemblyDefinition.Modules)
                {
                    foreach (var type in moduleDefinition.Types)
                    {
                        if (type.Name.Contains("<"))
                        {
                            continue;
                        }

                        yield return type;
                    }
                }
            }
        }
        #endregion
    }
}