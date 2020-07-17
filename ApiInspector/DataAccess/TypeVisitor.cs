using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    /// <summary>
    ///     The type visitor
    /// </summary>
    class TypeVisitor
    {
        #region Fields
        /// <summary>
        ///     The assembly search directories
        /// </summary>
        readonly IReadOnlyList<string> assemblySearchDirectories;

        /// <summary>
        ///     The log
        /// </summary>
        readonly Action<string> log;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeVisitor" /> class.
        /// </summary>
        public TypeVisitor(Action<string> log, IReadOnlyList<string> assemblySearchDirectories)
        {
            this.log                       = log;
            this.assemblySearchDirectories = assemblySearchDirectories;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Finds the type.
        /// </summary>
        public TypeDefinition FindType(string assemblyPath, string typeFullName)
        {
            var typeDefinitions = new List<TypeDefinition>();

            VisitAllTypes(assemblyPath, type =>
            {
                if (type.FullName == typeFullName)
                {
                    typeDefinitions.Add(type);
                }
            });

            return typeDefinitions.FirstOrDefault();
        }

        /// <summary>
        ///     Ges the type definitions.
        /// </summary>
        public IReadOnlyList<TypeDefinition> GeTypeDefinitions(string assemblyFilePath)
        {
            var items = new List<TypeDefinition>();

            VisitAllTypes(assemblyFilePath, typeDefinition => { items.Add(typeDefinition); });

            return items;
        }

        /// <summary>
        ///     Visits all types.
        /// </summary>
        public void VisitAllTypes(string assemblyPath, Action<TypeDefinition> action)
        {
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