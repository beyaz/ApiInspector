using System.Windows;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    /// <summary>
    ///     Interaction logic for InvocationInfoEditorView.xaml
    /// </summary>
    public partial class InvocationInfoEditorView
    {
        DataContext context;
        public DataContext Context
        {
            get
            {
                return context;
            }
            set
            {
                assemblyIntellisenseTextBox.Context = value;
                classNameIntellisenseTextBox.Context = value;
                methodNameIntellisenseTextBox.Context = value;

                value.Update(DataKeys.ParametersPanel,parametersPanel);

                context = value;
            }
        }

        #region Constructors
        public InvocationInfoEditorView()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

        void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                Context.Update(DataKeys.AssemblyName, assemblyIntellisenseTextBox.Editor.Text);
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, ee) =>
            {
                Context.Update(DataKeys.ClassName, classNameIntellisenseTextBox.Editor.Text);
            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, ee) =>
            {
                Context.Update(DataKeys.MethodName, methodNameIntellisenseTextBox.Editor.Text);
            };
        }
        #endregion

        #region InvocationInfoEditorModel Model
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(InvocationInfoEditorModel), typeof(InvocationInfoEditorView), new PropertyMetadata(default(InvocationInfoEditorModel)));

        public InvocationInfoEditorModel Model
        {
            get { return (InvocationInfoEditorModel) GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        #endregion
    }
}