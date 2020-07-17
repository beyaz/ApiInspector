using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    public class ViewData
    {
        public ItemSourceList ItemSourceList { get; set; }
        public InvocationInfo InvocationInfo { get; set; }
        public List<string> Logs { get; set; } = new List<string>();
    }
    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
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
        
        public void OnAssemblySearchDirectoryChanged(ViewData viewData)
        {
            if (!Directory.Exists(viewData.InvocationInfo.AssemblySearchDirectory))
            {
                return;
            }

            viewData.ItemSourceList.AssemblyNameList = Directory.GetFiles(viewData.InvocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();
        }

        static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }

        public void OnAssemblyNameChanged(ViewData viewData)
        {
            var assemblyFilePath = GetAssemblyFilePath(viewData.InvocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                viewData.Logs.Add($"File not exists. File:{assemblyFilePath}");
                return;
            }
            
            var assemblySearchDirectory = viewData.InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(new Logger().Log,new List<string> {assemblySearchDirectory});

            viewData.ItemSourceList.ClassNameList = typeVisitor.GeTypeDefinitions(assemblyFilePath).Select(x => x.FullName).ToList();
        }

        

        public void OnClassNameChanged(ViewData viewData)
        {
            var invocationInfo = viewData.InvocationInfo;

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);
            if (!File.Exists(assemblyFilePath))
            {
                viewData.Logs.Add($"File not exists. File:{assemblyFilePath}");
                return;
            }
            
            var assemblySearchDirectory = viewData.InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(new Logger().Log,new List<string> {assemblySearchDirectory});
            
            var typeDefinition = typeVisitor.FindType(assemblyFilePath, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                viewData.Logs.Add($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            viewData.ItemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();

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

            var panel            = context.Get(Data.ParametersPanel);

            new ParameterPanelIntegration().Connect(invocationInfo, panel, methodDefinition);
        }
        #endregion
    }
}