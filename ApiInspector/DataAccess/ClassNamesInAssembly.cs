using System.Collections.Generic;
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
            var items = new List<string>();

            var assembly = AssemblyDefinition.ReadAssembly(assemblyName);

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    items.Add(type.Name);
                }
            }

            context.Update(Key, items);
        }
        #endregion
    }
}