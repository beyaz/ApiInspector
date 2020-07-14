using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.InvocationInfoEditor;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView
    {
        BOA.DataFlow.DataContext context;

        void InitializeContext()
        {
            var builder = new InvocationInfoEditorContextBuilder();

            context = builder.Build();
        }

        #region Constructors
        public MainWindowView()
        {
            InitializeComponent();

            InitializeContext();

            currentInvocationInfo.Context = context;

            Loaded += OnLoad;

        }
        #endregion
        void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                context.Update(DataKeys.TargetEnvironment, environmentIntellisenseTextBox.Editor.Text);
            };
            
        }


    }
}