using System.IO;
using System.Reflection;
using static ApiInspector.Keys;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The assembly finder
    /// </summary>
    static class AssemblyFinder
    {
        #region Methods
        /// <summary>
        ///     Tries to find assembly.
        /// </summary>
        internal static Assembly TryToFindAssembly(Scope scope, string assemblyFileNameWithoutExtension)
        {
            var trace = scope.Get(Trace);

            trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            if (assemblyFileNameWithoutExtension == "BOA.Integration.Connector")
            {
                assemblyFileNameWithoutExtension += ".ModifiedVersionForApiInspector";
            }

            foreach (var searchDirectory in scope.Get(AssemblySearchDirectories))
            {
                var filePath = $@"{searchDirectory}\{assemblyFileNameWithoutExtension}.dll";
                if (File.Exists(filePath))
                {
                    trace($"Loading assembly: {filePath}");
                    return Assembly.LoadFile(filePath);
                }
            }

            trace($"Assembly Not Found: {assemblyFileNameWithoutExtension}");

            return null;
        }
        #endregion
    }
}