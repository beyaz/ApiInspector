using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    static class CecilHelper
    {
        #region Public Methods
        public static TypeDefinition FindType(DataContext context, string assemblyPath, string typeFullName)
        {
            var typeDefinitions = new List<TypeDefinition>();

            VisitAllTypes(context, assemblyPath, type =>
            {
                if (type.FullName == typeFullName)
                {
                    typeDefinitions.Add(type);
                }
            });

            return typeDefinitions.FirstOrDefault();
        }

        public static void VisitAllTypes(DataContext context, string assemblyPath, Action<TypeDefinition> action)
        {
            var logger = context.Get(Logger.Key);
            
            var assemblySearchDirectory = context.Get(DataAccess.Data.InvocationInfo).AssemblySearchDirectory;


            new CecilTypeVisitor(logger).VisitAllTypes(new List<string>{assemblySearchDirectory}, assemblyPath,action);

        }
        #endregion
    }
}