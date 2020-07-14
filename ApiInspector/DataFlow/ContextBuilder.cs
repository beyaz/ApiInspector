using System.Collections.Generic;
using ApiInspector.Application;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.DataFlow
{
    class ContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var defaultAssemblySearchDirectory = @"d:\boa\server\bin\";
            var context = new DataContext();
            context.Add(DataAccess.Data.AssemblySearchDirectoryList,new List<string>{  defaultAssemblySearchDirectory });
            context.Add(DataAccess.Data.InvocationInfo,new InvocationInfo{ AssemblySearchDirectory = defaultAssemblySearchDirectory});

            // connect view events
            {
                
                context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged,()=>Controller.OnAssemblySearchDirectoryChanged(context));
                context.SubscribeEvent(ViewEvents.AssemblyNameChanged,()=>Controller.OnAssemblyNameChanged(context));
                context.SubscribeEvent(ViewEvents.ClassNameChanged,()=>Controller.OnClassNameChanged(context));
                context.SubscribeEvent(ViewEvents.MethodNameChanged,()=>Controller.OnMethodNameSelected(context));
            }

           Domain. Data.InvocationInfo = DataKeys.InvocationInfo;
           Domain. Data.    ExecutionDataContext = DataKeys.ExecutionDataContext;
           Domain.Data.ExecutionResponse = DataKeys.ExecutionResponse;

            context.ForwardKey(AssemblyIntellisenseTextBox.Names, DataAccess.Data.AssemblyNames);
            context.ForwardKey(ClassNameIntellisenseTextBox.Names, ClassNamesInAssembly.Key);
            context.ForwardKey(MethodNameIntellisenseTextBox.Names, MethodNamesInAssembly.Key);
            context.Add(Logger.Key, new Logger());

            context.Add(DataKeys.AssemblySearchDirectory, defaultAssemblySearchDirectory);

            context.SubscribeEvent(EventNames.AssemblyNameChanged, () => Controller.OnAssemblyNameChanged(context));
            context.OnUpdate(DataKeys.ClassName, () => Controller.OnClassNameChanged(context));
            context.OnUpdate(DataKeys.MethodName, () => Controller.OnMethodNameSelected(context));

            

            return context;
        }
        #endregion
    }
}