using ApiInspector.Components;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    class InvocationInfoEditorContextBuilder
    {
        public DataContext Build()
        {
            var context = new DataContext();

            context.ForwardKey(AssemblyIntellisenseTextBox.Names,AssemblyNamesAll.Key);
            context.ForwardKey(ClassNameIntellisenseTextBox.Names,ClassNamesInAssembly.Key);
            context.ForwardKey(MethodNameIntellisenseTextBox.Names,MethodNamesInAssembly.Key);
            context.Add(Logger.Key,new Logger());

            context.Add(AssemblyDirectory.Key, @"d:\boa\server\bin");


            context.OnUpdate(DataKeys.AssemblyName,()=>ClassNamesInAssembly.Load(context) );
            context.OnUpdate(DataKeys.ClassName,()=>ClassNamesInAssembly.Load(context) );
            context.OnUpdate(DataKeys.MethodName,()=>MethodNamesInAssembly.Load(context) );
            
            
            AssemblyNamesAll.Load(context);


            return context;
        }
    }
}