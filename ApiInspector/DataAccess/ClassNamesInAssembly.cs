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
        public static readonly DataKey<IReadOnlyList<TypeDefinition>> Key = new DataKey<IReadOnlyList<TypeDefinition>>(nameof(ClassNamesInAssembly));
        #endregion

        #region Public Methods
        public static void Load(DataContext context)
        {
            var assemblyName = context.Get(DataKeys.AssemblyName);

            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = context.Get(DataKeys.AssemblySearchDirectory);

            var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var items = new List<TypeDefinition>();

            CecilHelper.VisitAllTypes(context, assemblyPath, typeDefinition => { items.Add(typeDefinition); });

            context.Update(Key, items);
        }
        #endregion
    }
}