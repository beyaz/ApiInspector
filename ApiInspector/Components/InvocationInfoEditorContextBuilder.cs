using ApiInspector.Application;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    class InvocationInfoEditorContextBuilder
    {
        public DataContext Build()
        {
            var context = new DataContext();

            context.ForwardKey(AssemblyIntellisenseTextBox.Names,AssemblyNames.Key);
            context.ForwardKey(ClassNameIntellisenseTextBox.Names,ClassNamesInAssembly.Key);
            context.ForwardKey(MethodNameIntellisenseTextBox.Names,MethodNamesInAssembly.Key);
            context.Add(Logger.Key,new Logger());

            context.Add(DataKeys.AssemblySearchDirectory, @"d:\boa\server\bin\");


            context.OnUpdate(DataKeys.AssemblyName,()=>ClassNamesInAssembly.Load(context) );
            context.OnUpdate(DataKeys.ClassName,()=>ClassNamesInAssembly.Load(context) );
            context.OnUpdate(DataKeys.MethodName,()=>MethodNamesInAssembly.Load(context) );
            
            
            AssemblyNames.Load(context);


            return context;
        }
    }
}