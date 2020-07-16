using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using ApiInspector.CardSystemOldAndNewApiCall;
using ApiInspector.DataAccess;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.Base;
using BOA.DataFlow;
using Mono.Cecil;
using Invoker = ApiInspector.Invoking.Invoker;

namespace ApiInspector.MainWindow
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


                 View.InvocationInfo =      Invoker.InvocationInfo = Data.InvocationInfo;

                 View.BOAExecutionContext = Invoker.BOAExecutionContext;

            context.SetupGet(Invoker.BOAExecutionContext,GetExecutionDataContext);
            context.SetupGet(BOAContextInitializer.TargetEnvironment,(c)=>c.Get(Data.InvocationInfo).Environment);

            
           
            // connect view events
            {
                
                context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged,()=>ViewController.OnAssemblySearchDirectoryChanged(context));
                context.SubscribeEvent(ViewEvents.AssemblyNameChanged,()=>ViewController.OnAssemblyNameChanged(context));
                context.SubscribeEvent(ViewEvents.ClassNameChanged,()=>ViewController.OnClassNameChanged(context));
                context.SubscribeEvent(ViewEvents.MethodNameChanged,()=>ViewController.OnMethodNameSelected(context));
            }

            {
                Detection.InvocationInfo = Data.InvocationInfo;
                Detection.MethodDefinition = Data.MethodDefinition;
            }

            {
                MainWindow.View.Trace = Invoker.Trace;
            }

            context.Add(Logger.Key, new Logger());


            context.SubscribeEvent(ViewEvents.AssemblyNameChanged, () => ViewController.OnAssemblyNameChanged(context));
            context.SubscribeEvent(ViewEvents.ClassNameChanged, () => ViewController.OnClassNameChanged(context));
            context.SubscribeEvent(ViewEvents.MethodNameChanged,  () => ViewController.OnMethodNameSelected(context));

            context.SetupGet(CecilHelper.AssemblySearchDirectories, c =>
            {
                var assemblySearchDirectory = context.Get(Data.InvocationInfo).AssemblySearchDirectory;

                return new List<string>{assemblySearchDirectory};
            });

            context.SetupGet(CecilHelper.Log, c => message => c.Get(Logger.Key).Log(message));
            context.SetupGet(ViewController.AssemblyFilePath,GetAssemblyFilePath);
            context.SetupGet(ViewController.TypesInAssembly,GetTypesInAssembly);
            context.SetupGet(ViewController.TypeDefinitionRelatedClassName,GeTypeDefinitionRelatedClassName);

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
            var assemblyFilePath = context.Get(ViewController.AssemblyFilePath);

            var items = new List<TypeDefinition>();

            CecilHelper.VisitAllTypes(context, assemblyFilePath, typeDefinition => { items.Add(typeDefinition); });

            return items;
        }

         static TypeDefinition GeTypeDefinitionRelatedClassName(DataContext context)
        {
            var assemblyFilePath = context.Get(ViewController.AssemblyFilePath);
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


         static ObjectHelper GetExecutionDataContext(DataContext context)
         {
             if (context.Contains(BOAContextInitializer.BOAExecutionContext))
             {
                 return context.Get(BOAContextInitializer.BOAExecutionContext);
             }

             BOAContextInitializer.Initialize(context);

             return context.Get(BOAContextInitializer.BOAExecutionContext);
         }
    }
}