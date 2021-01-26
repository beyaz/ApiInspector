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

            var assembly = _.FindAssembly(name);
            if (assembly != null)
            {
                return assembly;
            }

            throw new ArgumentException("AssemblyNotFound:" + args.Name);
        }

        
        #endregion
    }
}