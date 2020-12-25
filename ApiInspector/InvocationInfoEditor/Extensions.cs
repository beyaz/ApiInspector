using System.Collections.Generic;
using System.IO;
using ApiInspector.Models;
using static ApiInspector.Utility;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    static class Extensions
    {
        #region Public Methods
        /// <summary>
        ///     Gets the assembly file path.
        /// </summary>
        public static string GetAssemblyFilePath(this InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }

        /// <summary>
        ///     Gets the assembly search directories.
        /// </summary>
        public static List<string> GetAssemblySearchDirectories(this InvocationInfo invocationInfo)
        {
            return ListOf(invocationInfo.AssemblySearchDirectory);
        }
        #endregion
    }
}