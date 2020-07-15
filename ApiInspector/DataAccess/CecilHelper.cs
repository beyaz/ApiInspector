using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    /// <summary>
    ///     The cecil helper
    /// </summary>
    static class CecilHelper
    {
        #region Static Fields
        /// <summary>
        ///     The assembly search directories
        /// </summary>
        public static DataKey<IReadOnlyList<string>> AssemblySearchDirectories = new DataKey<IReadOnlyList<string>>(nameof(AssemblySearchDirectories));

        /// <summary>
        ///     The log
        /// </summary>
        public static DataKey<Action<string>> Log = new DataKey<Action<string>>(nameof(Log));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Finds the type.
        /// </summary>
        public static TypeDefinition FindType(DataContext context, string assemblyPath, string typeFullName)
        {
            var typeDefinitions = new List<TypeDefinition>();

            VisitAllTypes(context, assemblyPath, type =>
            {
                if (type.FullName == typeFullName)
                {
                    typeDefinitions.Add(type);
                }
            });

            return typeDefinitions.FirstOrDefault();
        }

        /// <summary>
        ///     Visits all types.
        /// </summary>
        public static void VisitAllTypes(DataContext context, string assemblyPath, Action<TypeDefinition> action)
        {
            var log = context.Get(Log);

            var assemblySearchDirectories = context.Get(AssemblySearchDirectories);

            if (File.Exists(assemblyPath) == false)
            {
                return;
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
                return;
            }

            foreach (var moduleDefinition in assemblyDefinition.Modules)
            {
                foreach (var type in moduleDefinition.Types)
                {
                    if (type.Name.Contains("<"))
                    {
                        continue;
                    }

                    action(type);
                }
            }
        }
        #endregion
    }
}