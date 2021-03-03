using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector
{
    /// <summary>
    ///     The
    /// </summary>
    static partial class _
    {
        #region Public Methods
        /// <summary>
        ///     Attaches the boa system assembly resolver to current domain.
        /// </summary>
        public static void AttachBoaSystemAssemblyResolverToCurrentDomain()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBoaSystemAssembly;
        }

        /// <summary>
        ///     Finds the type.
        /// </summary>
        public static Type FindTypeByFullName(string fullName)
        {
            var type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            var list = new[]
            {
                "BOA.Card.Contracts",
                "BOA.Card.Contracts.Online",
                "BOA.Process.Kernel.Card",
                "BOA.Integration.Model.MobileBranch"
            };

            foreach (var prefix in list)
            {
                if (!fullName.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                type = Assembly.Load(prefix).GetTypes().FirstOrDefault(t => t.FullName == fullName);
                if (type != null)
                {
                    return type;
                }
            }

            var names = fullName.Split('.');
            if (names.FirstOrDefault() == "BOA")
            {
                var destination = new string[names.Length - 1];

                Array.Copy(names, destination, destination.Length);

                var assemblyName = string.Join(".", destination);

                return FindAssembly(assemblyName)?.GetType(fullName);
            }

            return null;
        }

        /// <summary>
        ///     Resolves the boa system assembly.
        /// </summary>
        public static Assembly ResolveBoaSystemAssembly(object sender, ResolveEventArgs args)
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

        public static Assembly ResolveBoaSystemAssembly(object sender, ResolveEventArgs args, string assemblySearchDirectory)
        {
            var name  = args.Name;
            var index = name.IndexOf(',');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }

            var assembly = FindAssembly(name,GetAssemblySearchDirectories(assemblySearchDirectory));
            if (assembly != null)
            {
                return assembly;
            }

            throw new ArgumentException("AssemblyNotFound:" + args.Name);
        }

        #endregion

        #region Methods

        static IReadOnlyList<string> GetAssemblySearchDirectories(string assemblySearchDirectory = null)
        {
            var searchDirectories = new List<string>
            {
                @"D:\BOA\server\bin",
                @"D:\BOA\client\bin",
                @"D:\BOA\client\bin\en",
                @"D:\BOA\HSM\server\bin",
                @"D:\BOA\BOA.Integration\server\bin"
            };

            if (assemblySearchDirectory != null && !searchDirectories.Contains(assemblySearchDirectory))
            {
                searchDirectories.Insert(0,assemblySearchDirectory);
            }

            return searchDirectories;
        }
            

        /// <summary>
        ///     Finds the assembly.
        /// </summary>
        static Assembly FindAssembly(string assemblyFileNameWithoutExtension)
        {
            return FindAssembly(assemblyFileNameWithoutExtension,GetAssemblySearchDirectories());
        }

        static Assembly FindAssembly(string assemblyFileNameWithoutExtension, IReadOnlyList<string> searchDirectories)
        {
            var trace = fun((string message) => Console.WriteLine(message));

            trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            if (assemblyFileNameWithoutExtension == "BOA.Integration.Connector")
            {
                assemblyFileNameWithoutExtension += ".ModifiedVersionForApiInspector";
            }

            foreach (var searchDirectory in searchDirectories)
            {
                var filePath = $@"{searchDirectory}\{assemblyFileNameWithoutExtension}.dll";
                if (File.Exists(filePath))
                {
                    trace($"Loading assembly: {filePath}");
                    // Assembly.Load(File.ReadAllBytes(filePath));
                    return Assembly.LoadFrom(filePath);
                }
            }

            trace($"Assembly Not Found: {assemblyFileNameWithoutExtension}");

            return null;
        }
        #endregion
    }
}