using System.Collections.Generic;
using System.IO;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.Base;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.MainWindow
{
    class ContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var defaultAssemblySearchDirectory = @"d:\boa\server\bin\";

            var context = new DataContext();
            context.Add(Data.InvocationInfo, new InvocationInfo {AssemblySearchDirectory = defaultAssemblySearchDirectory});
            context.Add(Data.ItemSourceList, new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string> {defaultAssemblySearchDirectory},
                EnvironmentNameList         = new List<string> {"dev", "test"}
            });

            View.InvocationInfo  = Data.InvocationInfo;

            

            context.SetupGet(BOAContextInitializer.TargetEnvironment, c => c.Get(Data.InvocationInfo).Environment);


           

            

            context.Add(Logger.Key, new Logger());


           
            

            return context;
        }
        #endregion

        #region Methods
      
       

       

       
        #endregion
    }
}