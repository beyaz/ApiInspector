using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {
        #region Fields
        DataContext context;
        #endregion

        #region Constructors
        public View()
        {
            InitializeComponent();

            InitializeContext();

            currentInvocationInfo.Context = context;

            Loaded += OnLoad;
        }
        #endregion

        #region Methods
        void InitializeContext()
        {
            var builder = new ContextBuilder();

            context = builder.Build();
        }

        void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) => { context.Update(DataKeys.TargetEnvironment, environmentIntellisenseTextBox.Editor.Text); };
        }
        #endregion
    }
}