using ApiInspector.Application;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    class InvocationInfoEditorContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var context = new DataContext();

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