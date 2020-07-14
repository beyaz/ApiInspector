using ApiInspector.Application;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.Domain;
using BOA.DataFlow;

namespace ApiInspector.InvocationInfoEditor
{
    class ContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var context = new DataContext();


            Data.InvocationInfo = DataKeys.InvocationInfo;
            Data.    ExecutionDataContext = DataKeys.ExecutionDataContext;
            Data.ExecutionResponse = DataKeys.ExecutionResponse;

            context.ForwardKey(AssemblyIntellisenseTextBox.Names, AssemblyNames.Key);
            context.ForwardKey(ClassNameIntellisenseTextBox.Names, ClassNamesInAssembly.Key);
            context.ForwardKey(MethodNameIntellisenseTextBox.Names, MethodNamesInAssembly.Key);
            context.Add(Logger.Key, new Logger());

            context.Add(DataKeys.AssemblySearchDirectory, @"d:\boa\server\bin\");

            context.OnUpdate(DataKeys.AssemblyName, () => Controller.OnAssemblyNameChanged(context));
            context.OnUpdate(DataKeys.ClassName, () => MethodNamesInAssembly.Load(context));
            context.OnUpdate(DataKeys.MethodName, () => Controller.OnMethodNameSelected(context));

            AssemblyNames.Load(context);

            return context;
        }
        #endregion
    }
}