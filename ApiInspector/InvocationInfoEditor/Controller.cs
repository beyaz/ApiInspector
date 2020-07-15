using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.DataFlow;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{

    static class Controller_old
    {
        #region Public Methods
        public static void OnAssemblyNameChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            var assemblyName = invocationInfo.AssemblyName;

            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;

            var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var items = new List<TypeDefinition>();

            CecilHelper.VisitAllTypes(context, assemblyPath, typeDefinition => { items.Add(typeDefinition); });

            itemSourceList.ClassNameList = items.Select(x => x.FullName).ToList();
        }

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

        public static void OnClassNameChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            var logger = context.Get(Logger.Key);

            var assemblyPath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var typeDefinition = CecilHelper.FindType(context, assemblyPath, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                logger.Log($"Type not exists. File:{assemblyPath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            itemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();
        }

        public static void OnMethodNameSelected(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var className        = invocationInfo.ClassName;
            var methodName       = invocationInfo.MethodName;
            var assemblyFilePath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, className);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(Data.MethodDefinition, methodDefinition);

            context.GetInstanceManager().GetService<ParameterPanelRefresher>().UpdateUI(context.Get(Data.ParametersPanel), methodDefinition);
        }
        #endregion
    }
}