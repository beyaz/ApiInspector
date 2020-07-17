using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    class TypeVisitor
    {
        #region Fields
        readonly IReadOnlyList<string> assemblySearchDirectories;
        readonly Action<string>        log;
        #endregion

        #region Constructors
        public TypeVisitor(Action<string> log, IReadOnlyList<string> assemblySearchDirectories)
        {
            this.log                       = log;
            this.assemblySearchDirectories = assemblySearchDirectories;
        }
        #endregion

        #region Public Methods
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