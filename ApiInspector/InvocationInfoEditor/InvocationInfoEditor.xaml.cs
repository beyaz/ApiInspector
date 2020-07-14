using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
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
        public View()
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

        #region InvocationInfo Model
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(InvocationInfo), typeof(View), new PropertyMetadata(default(InvocationInfo)));

        public InvocationInfo Model
        {
            get { return (InvocationInfo) GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        #endregion
    }
}