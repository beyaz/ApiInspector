using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    static class ViewController
    {
        #region Static Fields
        /// <summary>
        ///     The assembly file path
        /// </summary>
        public static DataKey<string> AssemblyFilePath = new DataKey<string>(nameof(AssemblyFilePath));

        /// <summary>
        ///     The type definition related class name
        /// </summary>
        public static DataKey<TypeDefinition> TypeDefinitionRelatedClassName = new DataKey<TypeDefinition>(nameof(TypeDefinitionRelatedClassName));

        /// <summary>
        ///     The types in assembly
        /// </summary>
        public static DataKey<IReadOnlyList<TypeDefinition>> TypesInAssembly = new DataKey<IReadOnlyList<TypeDefinition>>(nameof(TypesInAssembly));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Called when [assembly name changed].
        /// </summary>
        public static void OnAssemblyNameChanged(DataContext context)
        {
            var itemSourceList = context.Get(Data.ItemSourceList);
            var logger         = context.Get(Logger.Key);

            var assemblyFilePath = context.Get(AssemblyFilePath);

            if (!File.Exists(assemblyFilePath))
            {
                logger.Log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            itemSourceList.ClassNameList = context.Get(TypesInAssembly).Select(x => x.FullName).ToList();
        }

        /// <summary>
        ///     Called when [assembly search directory changed].
        /// </summary>
        public static void OnAssemblySearchDirectoryChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            if (!Directory.Exists(invocationInfo.AssemblySearchDirectory))
            {
                return;
            }

            itemSourceList.AssemblyNameList = Directory.GetFiles(invocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public static void OnClassNameChanged(DataContext context)
        {
            var itemSourceList = context.Get(Data.ItemSourceList);

            var typeDefinition = context.Get(TypeDefinitionRelatedClassName);
            if (typeDefinition == null)
            {
                return;
            }

            itemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();
        }

        /// <summary>
        ///     Called when [method name selected].
        /// </summary>
        public static void OnMethodNameSelected(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var typeDefinition = context.Get(TypeDefinitionRelatedClassName);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(Data.MethodDefinition, methodDefinition);

            ParameterPanelIntegration.Connect(context);
        }
        #endregion
    }
}