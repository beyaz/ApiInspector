using System.Windows;
using ApiInspector.DataAccess;
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
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.AssemblySearchDirectory = assemblySearchDirectoryIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.AssemblySearchDirectoryChanged);
            };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.EnvironmentChanged);
            };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.AssemblyNameChanged);
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.ClassNameChanged);
            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(DataKeys.InvocationInfo);
                invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.MethodNameChanged);
            };
        }
        #endregion

    }
}