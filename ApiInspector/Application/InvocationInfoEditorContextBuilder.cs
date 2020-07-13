using ApiInspector.Components;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    class InvocationInfoEditorContextBuilder
    {
        public DataContext Build()
        {
            DataContext context = new DataContext();

            context.ForwardKey(AssemblyIntellisenseTextBox.AssemblyNames,AssemblyNamesAll.Key);
            context.ForwardKey(ClassNameIntellisenseTextBox.ClassNames,ClassNamesInAssembly.Key);
            context.Add(Logger.Key,new Logger());

            context.Add(AssemblyDirectory.Key, @"d:\boa\server\bin");
            
            AssemblyNamesAll.Load(context);


            return context;
        }
    }
}