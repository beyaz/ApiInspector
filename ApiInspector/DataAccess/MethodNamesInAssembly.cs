using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    class MethodNamesInAssembly
    {
        #region Static Fields
        public static readonly DataKey<IReadOnlyList<MethodDefinition>> Key = new DataKey<IReadOnlyList<MethodDefinition>>(nameof(MethodNamesInAssembly));
        #endregion

        #region Public Methods
        public static void Load(DataContext context)
        {
            var assemblyName  = context.Get(DataKeys.AssemblyName);
            var fullClassName = context.Get(DataKeys.ClassName);

            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = context.Get(DataKeys.AssemblySearchDirectory);

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

            context.Update(Key, typeDefinition.Methods.ToList());
        }
        #endregion
    }
}