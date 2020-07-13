using System.Windows;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    /// <summary>
    ///     Interaction logic for InvocationInfoEditor.xaml
    /// </summary>
    public partial class InvocationInfoEditor
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

                context = value;
            }
        }

        #region Constructors
        public InvocationInfoEditor()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

         void OnLoad(object sender, RoutedEventArgs e)
        {
            assemblyIntellisenseTextBox.Editor.TextChanged += (s, ee) =>
            {
                ClassNamesInAssembly.Load(Context,assemblyIntellisenseTextBox.Editor.Text);
            };
        }
        #endregion

        #region InvocationInfoEditorModel Model
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(InvocationInfoEditorModel), typeof(InvocationInfoEditor), new PropertyMetadata(default(InvocationInfoEditorModel)));

        public InvocationInfoEditorModel Model
        {
            get { return (InvocationInfoEditorModel) GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        #endregion
    }
}