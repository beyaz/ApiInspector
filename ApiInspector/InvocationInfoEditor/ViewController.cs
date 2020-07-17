using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.Models;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
    {
        #region Public Methods
        /// <summary>
        ///     Called when [assembly name changed].
        /// </summary>
        public void OnAssemblyNameChanged(ViewData viewData)
        {
            var assemblyFilePath = GetAssemblyFilePath(viewData.InvocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                viewData.Logs.Add($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = viewData.InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(new Logger().Log, new List<string> {assemblySearchDirectory});

            viewData.ItemSourceList.ClassNameList = typeVisitor.GeTypeDefinitions(assemblyFilePath).Select(x => x.FullName).ToList();
        }

        /// <summary>
        ///     Called when [assembly search directory changed].
        /// </summary>
        public void OnAssemblySearchDirectoryChanged(ViewData viewData)
        {
            if (!Directory.Exists(viewData.InvocationInfo.AssemblySearchDirectory))
            {
                return;
            }

            viewData.ItemSourceList.AssemblyNameList = Directory.GetFiles(viewData.InvocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
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

            var typeVisitor = new TypeVisitor(new Logger().Log, new List<string> {assemblySearchDirectory});

            var typeDefinition = viewData.typeDefinition = typeVisitor.FindType(assemblyFilePath, invocationInfo.ClassName);
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
        public void OnMethodNameSelected(ViewData viewData)
        {
            var invocationInfo = viewData.InvocationInfo;

            var methodDefinition = viewData.typeDefinition.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);

            if (methodDefinition == null)
            {
                return;
            }

            viewData.methodDefinition = methodDefinition;

            var panel = viewData.ParametersPanel;

            new ParameterPanelIntegration().Connect(invocationInfo, panel, methodDefinition);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the assembly file path.
        /// </summary>
        static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }
        #endregion
    }
}