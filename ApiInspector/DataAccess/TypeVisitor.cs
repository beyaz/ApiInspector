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
            return GeTypeDefinitions(scope, assemblyPath).FirstOrDefault(type => type.FullName == typeFullName);
        }

        /// <summary>
        ///     Ges the type definitions.
        /// </summary>
        public static IEnumerable<TypeDefinition> GeTypeDefinitions(Scope scope, string assemblyPath)
        {
            return GetTypeDefinitionsInAssembly(scope, assemblyPath);
        }
        #endregion

        #region Methods
        static Func<AssemblyDefinition> GetAssemblyDefinition(IReadOnlyList<string> assemblySearchDirectories, Action<string> trace, string assemblyPath)
        {
            return () =>
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
                    trace($"File not Loaded. File:{assemblyPath}, Error: {e}");
                }

                return null;
            };
        }

        /// <summary>
        ///     Gets the type definitions in assembly.
        /// </summary>
        static IEnumerable<TypeDefinition> GetTypeDefinitionsInAssembly(Scope scope, string assemblyPath)
        {
            return GetTypeDefinitionsInAssembly(GetAssemblyDefinition(scope.Get(AssemblySearchDirectories), scope.Get(Trace), assemblyPath), assemblyPath);
        }

        static IEnumerable<TypeDefinition> GetTypeDefinitionsInAssembly(Func<AssemblyDefinition> getAssemblyDefinition, string assemblyPath)
        {
            if (File.Exists(assemblyPath) == false)
            {
                yield break;
            }

            var assemblyDefinition = getAssemblyDefinition();
            if (assemblyDefinition == null)
            {
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
        #endregion
    }
}