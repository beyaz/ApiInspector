using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    class CecilHelper
    {

       public static TypeDefinition FindType(DataContext context, string assemblyPath, string typeFullName)
        {
            var typeDefinitions = new List<TypeDefinition>();

            VisitAllTypes(context,assemblyPath, type =>
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

            if (File.Exists(assemblyPath) == false)
            {
                return;
            }

            var resolver = new DefaultAssemblyResolver();

            resolver.AddSearchDirectory(@"d:\boa\server\bin\");

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
    }
}