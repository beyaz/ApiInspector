using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
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

            var items = new List<string>();

            
            var assemblyPath= Path.Combine(assemblyDirectory , assemblyName);

            if (!File.Exists(assemblyPath))
            {
                
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }


            AssemblyDefinition assembly;

            try
            {
                assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            }
            catch (Exception e)
            {
                logger.Log($"File not Loaded. File:{assemblyPath}, Error: {e}");
                return;
            }

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    items.Add(type.FullName);
                }
            }

            context.Update(Key, items);
        }
        #endregion
    }
}