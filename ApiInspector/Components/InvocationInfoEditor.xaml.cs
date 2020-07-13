using System.Windows;

namespace ApiInspector.Components
{
    /// <summary>
    ///     Interaction logic for InvocationInfoEditor.xaml
    /// </summary>
    public partial class InvocationInfoEditor
    {
        #region Constructors
        public InvocationInfoEditor()
        {
            InitializeComponent();
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