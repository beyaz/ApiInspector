using System.Collections.Generic;
using System.Windows;
using ApiInspector.Application;
using ApiInspector.Components;
using ApiInspector.DataAccess;

namespace ApiInspector.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region SelectedAssemlyName
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(MainWindowModel), typeof(MainWindow), new PropertyMetadata(default(MainWindowModel)));

        public MainWindowModel Model
        {
            get { return (MainWindowModel) GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        #endregion


        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            Model = new MainWindowModel
            {
                CurrentInvocationInfoEditorModel = new InvocationInfoEditorModel
                {
                    AssemblyDirectory = @"d:\boa\server\bin",
                    AssemblyNames = new List<string>(){"u","a","abc"}
                }
            };
            
            //App.Context.Add(AssemblyDirectory.Key, @"d:\boa\server\bin");
            
            //AssemblyNamesAll.Load(App.Context);
        }
        #endregion



    }
}