using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    /// <summary>
    ///     The type visitor
    /// </summary>
    static class TypeVisitor
    {
        #region Public Methods
        /// <summary>
        ///     Gets the type definitions in assembly.
        /// </summary>
        public static IEnumerable<TypeDefinition> GetTypeDefinitionsInAssembly(Action<Exception> onError, string assemblyPath, IEnumerable<string> assemblySearchDirectories)
        {
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
                onError(new Exception($"File not Loaded. File:{assemblyPath}",e));
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