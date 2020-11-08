using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using static ApiInspector.Keys;

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
        public static TypeDefinition FindTypeDefinition(Scope scope, string typeFullName)
        {
            return GeTypeDefinitions(scope).FirstOrDefault(type => type.FullName == typeFullName);
        }

        /// <summary>
        ///     Ges the type definitions.
        /// </summary>
        public static IEnumerable<TypeDefinition> GeTypeDefinitions(Scope scope)
        {
            var trace                     = scope.Get(Trace);
            var assemblyPath              = scope.Get(AssemblyPath);
            var assemblySearchDirectories = scope.Get(AssemblySearchDirectories);

            return GetTypeDefinitionsInAssembly((trace, assemblyPath, assemblySearchDirectories));
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the type definitions in assembly.
        /// </summary>
        static IEnumerable<TypeDefinition> GetTypeDefinitionsInAssembly((Action<string> trace, string assemblyPath, IReadOnlyList<string> assemblySearchDirectories) scope)
        {
            var (trace, assemblyPath, assemblySearchDirectories) = scope;

            var resolver = new DefaultAssemblyResolver();

            foreach (var directory in assemblySearchDirectories)
            {
                resolver.AddSearchDirectory(directory);
            }

            if (File.Exists(assemblyPath) == false)
            {
                yield break;
            }

            AssemblyDefinition assemblyDefinition = null;
            try
            {
                assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {AssemblyResolver = resolver});
            }
            catch (Exception e)
            {
                trace($"File not Loaded. File:{assemblyPath}, Error: {e}");
                yield break;
            }

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