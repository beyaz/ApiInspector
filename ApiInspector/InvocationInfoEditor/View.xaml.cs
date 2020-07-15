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
                value.Update(Data.ParametersPanel,parametersPanel);
                

                context = value;

                RegisterEvents();

                UpdateSuggestions();
            }
        }

        #region Constructors
        public View()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

        void RegisterEvents()
        {
            context.SubscribeEvent(ViewEvents.AssemblySearchDirectoryChanged, UpdateSuggestions);
            context.SubscribeEvent(ViewEvents.AssemblyNameChanged, UpdateSuggestions);
            context.SubscribeEvent(ViewEvents.ClassNameChanged, UpdateSuggestions);
            context.SubscribeEvent(ViewEvents.MethodNameChanged, UpdateSuggestions);
            context.OnUpdate(Data.InvocationInfo,RefreshValues);


           
        }

        public void RefreshValues()
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            environmentIntellisenseTextBox.SetValue(invocationInfo.Environment);

            assemblySearchDirectoryIntellisenseTextBox.SetValue(invocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(invocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(invocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(invocationInfo.MethodName);
        }

        void UpdateSuggestions()
        {
            var source = context.Get(Data.ItemSourceList);

            environmentIntellisenseTextBox.Suggestions             = source.EnvironmentNameList;
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = source.AssemblySearchDirectoryList;
            assemblyIntellisenseTextBox.Suggestions = source.AssemblyNameList;
            classNameIntellisenseTextBox.Suggestions = source.ClassNameList;
            methodNameIntellisenseTextBox.Suggestions = source.MethodNameList;
        }

        void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(Data.InvocationInfo);
                invocationInfo.AssemblySearchDirectory = assemblySearchDirectoryIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.AssemblySearchDirectoryChanged);
            };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(Data.InvocationInfo);
                invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.EnvironmentChanged);
            };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(Data.InvocationInfo);
                invocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.AssemblyNameChanged);
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(Data.InvocationInfo);
                invocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.ClassNameChanged);
            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = context.Get(Data.InvocationInfo);
                invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;
                context.PublishEvent(ViewEvents.MethodNameChanged);
            };
        }
        #endregion

    }
}