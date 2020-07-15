using System;
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
            context.Add(Data.InvocationInfo,new InvocationInfo{ AssemblySearchDirectory = defaultAssemblySearchDirectory});
            context.Add(Data.ItemSourceList,new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string>{  defaultAssemblySearchDirectory },
                EnvironmentNameList = new List<string>{  "dev","test" }
            });
            

            // connect view events
            {
                
                context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged,()=>Controller_old.OnAssemblySearchDirectoryChanged(context));
                context.SubscribeEvent(ViewEvents.AssemblyNameChanged,()=>Controller_old.OnAssemblyNameChanged(context));
                context.SubscribeEvent(ViewEvents.ClassNameChanged,()=>Controller_old.OnClassNameChanged(context));
                context.SubscribeEvent(ViewEvents.MethodNameChanged,()=>Controller_old.OnMethodNameSelected(context));
            }

           

            context.Add(Logger.Key, new Logger());


            context.SubscribeEvent(EventNames.AssemblyNameChanged, () => Controller_old.OnAssemblyNameChanged(context));
            context.SubscribeEvent(EventNames.ClassNameChanged, () => Controller_old.OnClassNameChanged(context));
            context.SubscribeEvent(EventNames.MethodNameChanged,  () => Controller_old.OnMethodNameSelected(context));

            context.SetupGet(CecilHelper.AssemblySearchDirectories, c =>
            {
                var assemblySearchDirectory = context.Get(Data.InvocationInfo).AssemblySearchDirectory;

                return new List<string>{assemblySearchDirectory};
            });

            context.SetupGet(CecilHelper.Log, c => message => c.Get(Logger.Key).Log(message));
            

            return context;
        }
        #endregion
    }
}