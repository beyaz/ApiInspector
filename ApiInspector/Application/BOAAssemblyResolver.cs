using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static FunctionalPrograming.FPExtensions;

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
            var trace = fun((string message) => Console.WriteLine(message));

            trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            if (assemblyFileNameWithoutExtension == "BOA.Integration.Connector")
            {
                assemblyFileNameWithoutExtension += ".ModifiedVersionForApiInspector";
            }

            foreach (var searchDirectory in GetDirectories())
            {
                var filePath = $@"{searchDirectory}\{assemblyFileNameWithoutExtension}.dll";
                if (File.Exists(filePath))
                {
                    trace($"Loading assembly: {filePath}");
                    return Assembly.Load(File.ReadAllBytes(filePath));
                }
            }

            trace($"Assembly Not Found: {assemblyFileNameWithoutExtension}");

            return null;
        }

        /// <summary>
        ///     Domains the assembly resolve.
        /// </summary>
        public static Assembly DomainAssemblyResolve(object sender, ResolveEventArgs args)
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
            return new List<string>
            {
                @"D:\BOA\server\bin",
                @"D:\BOA\client\bin",
                @"D:\BOA\client\bin\en",
                @"D:\BOA\HSM\server\bin",
                @"D:\BOA\BOA.Integration\server\bin"
            };
        }
        #endregion
    }
}