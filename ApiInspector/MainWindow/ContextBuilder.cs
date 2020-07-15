using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataFlow
{
    class ContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var defaultAssemblySearchDirectory = @"d:\boa\server\bin\";

            


            var context = new DataContext();
            context.Add(Data.InvocationInfo,new InvocationInfo{ AssemblySearchDirectory = defaultAssemblySearchDirectory});
            context.Add(Data.ItemSourceList,new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string>{  defaultAssemblySearchDirectory },
                EnvironmentNameList = new List<string>{  "dev","test" }
            });


            Domain.BOAContextInitializer.InvocationInfo=            Domain.Invoker.InvocationInfo = Data.InvocationInfo;

            Domain.BOAContextInitializer.BOAExecutionContext = Domain.Invoker.BOAExecutionContext;

            

            // connect view events
            {
                
                context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged,()=>Controller_old.OnAssemblySearchDirectoryChanged(context));
                context.SubscribeEvent(ViewEvents.AssemblyNameChanged,()=>Controller_old.OnAssemblyNameChanged(context));
                context.SubscribeEvent(ViewEvents.ClassNameChanged,()=>Controller_old.OnClassNameChanged(context));
                context.SubscribeEvent(ViewEvents.MethodNameChanged,()=>Controller_old.OnMethodNameSelected(context));
            }

           

            context.Add(Logger.Key, new Logger());


            context.SubscribeEvent(ViewEvents.AssemblyNameChanged, () => Controller_old.OnAssemblyNameChanged(context));
            context.SubscribeEvent(ViewEvents.ClassNameChanged, () => Controller_old.OnClassNameChanged(context));
            context.SubscribeEvent(ViewEvents.MethodNameChanged,  () => Controller_old.OnMethodNameSelected(context));

            context.SetupGet(CecilHelper.AssemblySearchDirectories, c =>
            {
                var assemblySearchDirectory = context.Get(Data.InvocationInfo).AssemblySearchDirectory;

                return new List<string>{assemblySearchDirectory};
            });

            context.SetupGet(CecilHelper.Log, c => message => c.Get(Logger.Key).Log(message));
            context.SetupGet(Controller_old.AssemblyFilePath,GetAssemblyFilePath);
            context.SetupGet(Controller_old.TypesInAssembly,GetTypesInAssembly);
            context.SetupGet(Controller_old.TypeDefinitionRelatedClassName,GeTypeDefinitionRelatedClassName);

            context.SetupGet(MainWindow.View.HistoryDataKey,HistoryManager.GetHistory);
            

            return context;
        }
        #endregion

        static string GetAssemblyFilePath(DataContext context)
        {
            var invocationInfo    = context.Get(Data.InvocationInfo);
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }

        static IReadOnlyList<TypeDefinition> GetTypesInAssembly(DataContext context)
        {
            var assemblyFilePath = context.Get(Controller_old.AssemblyFilePath);

            var items = new List<TypeDefinition>();

            CecilHelper.VisitAllTypes(context, assemblyFilePath, typeDefinition => { items.Add(typeDefinition); });

            return items;
        }

         static TypeDefinition GeTypeDefinitionRelatedClassName(DataContext context)
        {
            var assemblyFilePath = context.Get(Controller_old.AssemblyFilePath);
            var invocationInfo = context.Get(Data.InvocationInfo);
            var logger = context.Get(Logger.Key);


            if (!File.Exists(assemblyFilePath))
            {
                logger.Log($"File not exists. File:{assemblyFilePath}");
                return null;
            }

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                logger.Log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return null;
            }

            return typeDefinition;
        }
    }
}