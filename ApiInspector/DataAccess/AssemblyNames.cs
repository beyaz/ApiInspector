using System.IO;
using System.Linq;
using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    static class AssemblyNames
    {
        #region Public Methods
        public static void Load(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var assemblyNames = Directory.GetFiles(invocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();

            context.Update(Data.AssemblyNames, assemblyNames);
        }
        #endregion
    }
}