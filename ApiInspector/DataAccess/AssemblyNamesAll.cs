using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    static class AssemblyNamesAll
    {
        #region Static Fields
        public static readonly DataKey<IReadOnlyList<string>> Key = new DataKey<IReadOnlyList<string>>(nameof(AssemblyNamesAll));
        #endregion

        #region Public Methods
        public static void Load(DataContext context, string searchDirectory)
        {
            IReadOnlyList<string> assemblyNames = Directory.GetFiles(searchDirectory).Select(Path.GetFileName).ToList();

            context.Update(Key, assemblyNames);
        }
        public static void Load(DataContext context)
        {
            Load(context,context.Get(AssemblyDirectory.Key));
        }
        
        #endregion
    }


    static class AssemblyDirectory
    {
        #region Static Fields
        public static readonly DataKey<string> Key = new DataKey<string>(nameof(AssemblyDirectory));
        #endregion

       
    }
}

