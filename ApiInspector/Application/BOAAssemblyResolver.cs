using System;
using System.Collections.Generic;
using System.Reflection;
using static ApiInspector.Application.App;
using static ApiInspector.Application.AssemblyFinder;
using static ApiInspector.CommonApplicationKeys;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Defines the boa assembly resolver.
    /// </summary>
    public static class BoaAssemblyResolver
    {
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
            var scope = new Scope
            {
                {AssemblySearchDirectories, GetDirectories()},
                {Trace, message => { }}
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

        /// <summary>
        ///     Gets the directories.
        /// </summary>
        static IReadOnlyList<string> GetDirectories()
        {
            var directories = new List<string>
            {
                @"D:\BOA\server\bin",
                @"D:\BOA\client\bin",
                @"D:\BOA\client\bin\en",
                @"D:\BOA\HSM\server\bin",
                @"D:\BOA\BOA.Integration\server\bin"
            };
            var invocationSearchDirectory = ApplicationScope.TryGet(InvocationSearchDirectory);
            if (invocationSearchDirectory != null)
            {
                directories.Add(invocationSearchDirectory);
            }

            return directories;
        }
        #endregion
    }
}