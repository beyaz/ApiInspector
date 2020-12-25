using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using static ApiInspector.Keys;
using static ApiInspector.DataAccess.TypeVisitor;
using static ApiInspector.Utility;
using static FunctionalPrograming.Extensions;


namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    static class Extensions
    {
        #region Public Methods





        #region Methods


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