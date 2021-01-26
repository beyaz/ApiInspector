using System;
using System.Linq;
using System.Reflection;
using static ApiInspector.Application.BoaAssemblyResolver;

namespace ApiInspector
{
    static partial class _
    {
        
    }
}

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The type finder
    /// </summary>
    static class TypeFinder
    {
        #region Public Methods
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