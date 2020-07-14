using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    /// <summary>
    ///     The cecil type visitor
    /// </summary>
    class CecilTypeVisitor
    {
        #region Fields
        /// <summary>
        ///     The logger
        /// </summary>
        readonly Logger _logger;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="CecilTypeVisitor" /> class.
        /// </summary>
        public CecilTypeVisitor(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Visits all types.
        /// </summary>
        public void VisitAllTypes(IReadOnlyList<string> assemblySearchDirectories, string assemblyPath, Action<TypeDefinition> action)
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
                _logger.Log($"File not Loaded. File:{assemblyPath}, Error: {e}");
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