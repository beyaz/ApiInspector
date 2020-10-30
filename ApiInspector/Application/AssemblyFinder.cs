using System.IO;
using System.Reflection;
using static ApiInspector.Application.CommonApplicationKeys;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The assembly finder
    /// </summary>
    static class AssemblyFinder
    {
        #region Static Fields
        /// <summary>
        ///     The assembly search directories
        /// </summary>
        public static readonly StringList AssemblySearchDirectories = new StringList(typeof(AssemblyFinder), nameof(AssemblySearchDirectories));
        #endregion

        #region Methods
        /// <summary>
        ///     Tries to find assembly.
        /// </summary>
        internal static Assembly TryToFindAssembly(Scope scope, string assemblyFileNameWithoutExtension)
        {
            var trace                     = scope.Get(Trace);
            var assemblySearchDirectories = scope.Get(AssemblySearchDirectories);

            trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            if (assemblyFileNameWithoutExtension == "BOA.Integration.Connector")
            {
                assemblyFileNameWithoutExtension += ".ModifiedVersionForApiInspector";
            }

            foreach (var searchDirectory in assemblySearchDirectories)
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