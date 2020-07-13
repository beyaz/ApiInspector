using ApiInspector.Components;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        ///     The context
        /// </summary>
        public static readonly  DataContext Context = new DataContext();

        static App()
        {
            Context.ForwardKey(AssemblyIntellisenseTextBox.AssemblyNames,AssemblyNamesAll.Key);
            Context.ForwardKey(ClassNameIntellisenseTextBox.ClassNames,ClassNamesInAssembly.Key);
        }
    }
}
