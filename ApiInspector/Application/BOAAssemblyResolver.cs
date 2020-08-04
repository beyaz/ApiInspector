using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Defines the boa assembly resolver.
    /// </summary>
    public static class BoaAssemblyResolver
    {
        #region Static Fields
        /// <summary>
        ///     The search directories
        /// </summary>
        public static IReadOnlyList<string> AssemblySearchDirectories = new[]
        {
            @"D:\BOA\server\bin",
            @"D:\BOA\client\bin",
            @"D:\BOA\client\bin\en",
            @"D:\BOA\HSM\server\bin"
        };
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the trace.
        /// </summary>
        public static Action<string> Trace { get; set; } = message => { };
        #endregion

        #region Public Methods
        /// <summary>
        ///     Attaches to current domain.
        /// </summary>
        public static void AttachToCurrentDomain()
        {
            AppDomain.CurrentDomain.AssemblyResolve += DomainAssemblyResolve;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Domains the assembly resolve.
        /// </summary>
        static Assembly DomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name  = args.Name;
            var index = name.IndexOf(',');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }

            var assembly = FindAssembly(name);
            if (assembly != null)
            {
                return assembly;
            }

            throw new ArgumentException("AssemblyNotFound:" + args.Name);
        }

        /// <summary>
        ///     Finds the assembly.
        /// </summary>
        static Assembly FindAssembly(string assemblyFileNameWithoutExtension)
        {
            Trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            foreach (var searchDirectory in AssemblySearchDirectories)
            {
                var filePath = $@"{searchDirectory}\{assemblyFileNameWithoutExtension}.dll";
                if (File.Exists(filePath))
                {
                    Trace($"Loading assembly: {filePath}");
                    return Assembly.LoadFile(filePath);
                }
            }

            Trace($"Assembly Not Found: {assemblyFileNameWithoutExtension}");

            return null;
        }
        #endregion
    }
}