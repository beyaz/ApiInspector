using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void OnAssemblyNameChanged(InvocationEditorViewModel model)
        {
            var assemblyFilePath = GetAssemblyFilePath(model.InvocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                model.Logs.Add($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = model.InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(model.Logs.Add, new List<string> {assemblySearchDirectory});

            model.ItemSourceList.ClassNameList = typeVisitor.GeTypeDefinitions(assemblyFilePath).Select(x => x.FullName).ToList();
        }

        /// <summary>
        ///     Called when [assembly search directory changed].
        /// </summary>
        public void OnAssemblySearchDirectoryChanged(InvocationEditorViewModel model)
        {
            if (!Directory.Exists(model.InvocationInfo.AssemblySearchDirectory))
            {
                return;
            }

            model.ItemSourceList.AssemblyNameList = Directory.GetFiles(model.InvocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public void OnClassNameChanged(InvocationEditorViewModel model)
        {
            var invocationInfo = model.InvocationInfo;

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);
            if (!File.Exists(assemblyFilePath))
            {
                model.Logs.Add($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = model.InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(model.Logs.Add, new List<string> {assemblySearchDirectory});

            var typeDefinition = model.TypeDefinition = typeVisitor.FindType(assemblyFilePath, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                model.Logs.Add($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            model.ItemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();
        }

        /// <summary>
        ///     Called when [method name selected].
        /// </summary>
        public void OnMethodNameSelected(InvocationEditorViewModel model)
        {
            var invocationInfo = model.InvocationInfo;

            var methodDefinition = model.TypeDefinition.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);

            if (methodDefinition == null)
            {
                return;
            }

            model.MethodDefinition = methodDefinition;

            var panel = model.ParametersPanel;

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