using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    static class AssemblyNames
    {
        #region Static Fields
        public static readonly DataKey<IReadOnlyList<string>> Key = new DataKey<IReadOnlyList<string>>(nameof(AssemblyNames));
        #endregion

        #region Public Methods
        public static void Load(DataContext context)
        {
            var assemblySearchDirectory = context.Get(DataKeys.AssemblySearchDirectory);

            IReadOnlyList<string> assemblyNames = Directory.GetFiles(assemblySearchDirectory).Select(Path.GetFileName).ToList();

            context.Update(Key, assemblyNames);
        }
        #endregion
    }
}