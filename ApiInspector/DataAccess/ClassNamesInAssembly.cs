using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using ApiInspector.Components;
using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    class MethodNamesInAssembly
    {
        #region Static Fields
        public static readonly DataKey<IReadOnlyList<string>> Key = new DataKey<IReadOnlyList<string>>(nameof(AssemblyNamesAll));
        #endregion

        #region Public Methods
        public static void Load(DataContext context)
        {
            var assemblyName = context.Get(DataKeys.AssemblyName);
            var className = context.Get(DataKeys.ClassName);

            Load(context,assemblyName,className);
        }

        public static void Load(DataContext context, string assemblyName, string fullClassName)
        {
            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = context.Get(AssemblyDirectory.Key);

            var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

      

            var typeDefinition = CecilHelper.FindType(context, assemblyPath, fullClassName);
            if (typeDefinition == null)
            {
                logger.Log($"Type not exists. File:{assemblyPath}, fullClassName:{fullClassName}");
                return; 
            }

            var items = new List<string>();

            foreach (var methodDefinition in typeDefinition.Methods)
            {
                items.Add(methodDefinition.Name);
            }


            context.Update(Key, items);
        }
        #endregion
    }
}

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
            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = context.Get(AssemblyDirectory.Key);

            var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var items = new List<string>();

            CecilHelper.VisitAllTypes(context, assemblyPath, typeDefinition => { items.Add(typeDefinition.FullName); });

            context.Update(Key, items);
        }

        public static void Load(DataContext context)
        {
            var assemblyName = context.Get(DataKeys.AssemblyName);

            Load(context,assemblyName);

        }
        #endregion
    }
}