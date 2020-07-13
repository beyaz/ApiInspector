using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    class CecilHelper
    {
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

    class ClassNamesInAssembly
    {
        #region Static Fields
        public static readonly DataKey<IReadOnlyList<string>> Key = new DataKey<IReadOnlyList<string>>(nameof(AssemblyNamesAll));
        #endregion

        #region Public Methods
        public static void Load(DataContext context, string assemblyName)
        {
            var logger = context.Get(Logger.Key);
            var assemblyDirectory = context.Get(AssemblyDirectory.Key);

           

            
            var assemblyPath= Path.Combine(assemblyDirectory , assemblyName);

            if (!File.Exists(assemblyPath))
            {
                
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }


            var items = new List<string>();

            CecilHelper.VisitAllTypes(context,assemblyPath,(typeDefinition)=>{items.Add(typeDefinition.FullName);});

            context.Update(Key, items);
        }
        #endregion
    }
}