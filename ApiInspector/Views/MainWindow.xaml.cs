using System.Windows;
using ApiInspector.Application;
using ApiInspector.DataAccess;

namespace ApiInspector.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region SelectedAssemlyName
        public static readonly DependencyProperty SelectedAssemblyNameProperty = DependencyProperty.Register("SelectedAssemblyName", typeof(string), typeof(MainWindow), new PropertyMetadata(default(string),OnSelectedAssemblyChanged));

        static void OnSelectedAssemblyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selectedAssemblyName = ((MainWindow)d).SelectedAssemblyName;

            ClassNamesInAssembly.Load(App.Context,selectedAssemblyName);
        }

        public string SelectedAssemblyName
        {
            get { return (string) GetValue(SelectedAssemblyNameProperty); }
            set { SetValue(SelectedAssemblyNameProperty, value); }
        }
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            
            App.Context.Add(AssemblyDirectory.Key, @"d:\boa\server\bin");
            
            AssemblyNamesAll.Load(App.Context);
        }
        #endregion
    }
}