using System.Collections.Generic;
using ApiInspector.Application;
using ApiInspector.Components;
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
            context.Add(DataAccess.Data.InvocationInfo,new InvocationInfo{ AssemblySearchDirectory = defaultAssemblySearchDirectory});
            context.Add(DataAccess.Data.ItemSourceList,new ItemSourceList{ AssemblySearchDirectoryList = new List<string>{  defaultAssemblySearchDirectory }});
            

            // connect view events
            {
                
                context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged,()=>Controller.OnAssemblySearchDirectoryChanged(context));
                context.SubscribeEvent(ViewEvents.AssemblyNameChanged,()=>Controller.OnAssemblyNameChanged(context));
                context.SubscribeEvent(ViewEvents.ClassNameChanged,()=>Controller.OnClassNameChanged(context));
                context.SubscribeEvent(ViewEvents.MethodNameChanged,()=>Controller.OnMethodNameSelected(context));
            }

           

            context.Add(Logger.Key, new Logger());


            context.SubscribeEvent(EventNames.AssemblyNameChanged, () => Controller.OnAssemblyNameChanged(context));
            context.SubscribeEvent(EventNames.ClassNameChanged, () => Controller.OnClassNameChanged(context));
            context.SubscribeEvent(EventNames.MethodNameChanged,  () => Controller.OnMethodNameSelected(context));

            

            return context;
        }
        #endregion
    }
}