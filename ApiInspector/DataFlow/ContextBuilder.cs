using ApiInspector.Application;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.Domain;
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
            var context = new DataContext();
            context.Add(DataKeys.InvocationInfo,new InvocationInfo());

            Data.InvocationInfo = DataKeys.InvocationInfo;
            Data.    ExecutionDataContext = DataKeys.ExecutionDataContext;
            Data.ExecutionResponse = DataKeys.ExecutionResponse;

            context.ForwardKey(AssemblyIntellisenseTextBox.Names, AssemblyNames.Key);
            context.ForwardKey(ClassNameIntellisenseTextBox.Names, ClassNamesInAssembly.Key);
            context.ForwardKey(MethodNameIntellisenseTextBox.Names, MethodNamesInAssembly.Key);
            context.Add(Logger.Key, new Logger());

            context.Add(DataKeys.AssemblySearchDirectory, @"d:\boa\server\bin\");

            context.OnUpdate(DataKeys.AssemblyName, () => Controller.OnAssemblyNameChanged(context));
            context.OnUpdate(DataKeys.ClassName, () => Controller.OnClassNameChanged(context));
            context.OnUpdate(DataKeys.MethodName, () => Controller.OnMethodNameSelected(context));

            AssemblyNames.Load(context);

            return context;
        }
        #endregion
    }
}