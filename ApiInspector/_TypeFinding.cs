using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiInspector.Application;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector
{
    /// <summary>
    ///     The 
    /// </summary>
    static partial class _
    {

        #region Public Methods
        
        
        public static Assembly FindAssembly(string assemblyFileNameWithoutExtension)
        {
            var trace = fun((string message) => Console.WriteLine(message));

            trace($"Trying to find assembly: {assemblyFileNameWithoutExtension}");

            if (assemblyFileNameWithoutExtension == "BOA.Integration.Connector")
            {
                assemblyFileNameWithoutExtension += ".ModifiedVersionForApiInspector";
            }

            var searchDirectories = new List<string>
            {
                @"D:\BOA\server\bin",
                @"D:\BOA\client\bin",
                @"D:\BOA\client\bin\en",
                @"D:\BOA\HSM\server\bin",
                @"D:\BOA\BOA.Integration\server\bin"
            };
            foreach (var searchDirectory in searchDirectories)
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
        #endregion

    }
}