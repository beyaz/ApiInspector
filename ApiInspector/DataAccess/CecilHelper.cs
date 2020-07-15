using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    static class CecilHelper
    {
        public static  DataKey<IReadOnlyList<string>> AssemblySearchDirectories = new DataKey<IReadOnlyList<string>>(nameof(AssemblySearchDirectories));

        #region Public Methods
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

        public static void VisitAllTypes(DataContext context, string assemblyPath, Action<TypeDefinition> action)
        {
            var logger = context.Get(Logger.Key);
            
            var assemblySearchDirectory = context.Get(Data.InvocationInfo).AssemblySearchDirectory;

            var assemblySearchDirectories = new List<string>{assemblySearchDirectory};
            

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
                logger.Log($"File not Loaded. File:{assemblyPath}, Error: {e}");
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