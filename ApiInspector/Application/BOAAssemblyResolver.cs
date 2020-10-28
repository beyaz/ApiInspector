using System;
using System.Collections.Generic;
using System.Reflection;
using static ApiInspector.Application.AssemblyFinder;
using static ApiInspector.Application.CommonApplicationKeys;

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
        public static IReadOnlyList<string> AssemblySearchDirectories_2 = new[]
        {
            @"D:\BOA\server\bin",
            @"D:\BOA\client\bin",
            @"D:\BOA\client\bin\en",
            @"D:\BOA\HSM\server\bin",
            @"D:\BOA\BOA.Integration\server\bin"
        };
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the invocation search directory.
        /// </summary>
        public static string InvocationSearchDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the trace.
        /// </summary>
        public static Action<string> TraceHandler { get; set; } = message => { };
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
        ///     Finds the assembly.
        /// </summary>
        internal static Assembly FindAssembly(string assemblyFileNameWithoutExtension)
        {

            IReadOnlyList<string> GetDirectories()
            {
                var directories = new List<string>(AssemblySearchDirectories_2);
                if (InvocationSearchDirectory != null)
                {
                    directories.Add(InvocationSearchDirectory);
                }

                return directories;
            }

            var scope = new Scope
            {
                { AssemblyFinder.AssemblySearchDirectories, GetDirectories()},
                { Trace, TraceHandler}
            };
            return TryToFindAssembly(scope, assemblyFileNameWithoutExtension);
        }

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
        #endregion
    }
}