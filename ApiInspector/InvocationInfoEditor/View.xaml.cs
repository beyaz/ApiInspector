using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.InvocationInfoEditor
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
            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
            };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;
            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;
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