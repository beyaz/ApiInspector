using System;
using System.Linq;
using System.Reflection;
using ApiInspector.Application;

namespace ApiInspector
{
    /// <summary>
    ///     The 
    /// </summary>
    static partial class _
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

                return BoaAssemblyResolver.FindAssembly(assemblyName)?.GetType(fullName);
            }

            return null;
        }
        #endregion

    }
}