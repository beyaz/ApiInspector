using System.Collections.Generic;
using System.Linq;
using static ApiInspector.Keys;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {
        #region Fields
        internal Scope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            InitializeComponent();

            Loaded += (s, e) => { UpdateSuggestions(); };
            Loaded += (s, e) => { RegisterEvents(); };
        }
        #endregion

        #region Methods
        internal void Connect(Scope scope)
        {
            this.scope = scope;

            scope.Update(GetAssemblySearchDirectory, () => assemblySearchDirectoryIntellisenseTextBox.Editor.Text);
            scope.Update(GetAssemblyFileName, () => assemblyIntellisenseTextBox.Editor.Text);
            scope.Update(GetClassName, () => classNameIntellisenseTextBox.Editor.Text);

            scope.OnUpdate(AssemblyNameSuggestions, () => { assemblyIntellisenseTextBox.Suggestions = scope.Get(AssemblyNameSuggestions); });
            scope.OnUpdate(ClassNameSuggestions, () => { classNameIntellisenseTextBox.Suggestions = scope.Get(ClassNameSuggestions); });
            scope.OnUpdate(MethodNameSuggestions, () => { methodNameIntellisenseTextBox.Suggestions = scope.Get(MethodNameSuggestions); });
            scope.OnUpdate(SelectedInvocationInfo, Connect);
        }

        /// <summary>
        ///     Connects the specified invocation information.
        /// </summary>
        void Connect()
        {
            RefreshValues();
        }

        void OnMethodNameChanged()
        {
            var invocationInfo = scope.TryGet(SelectedInvocationInfo);
            var typeDefinition = scope.Get(SelectedTypeDefinition);

            invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

            scope.Update(SelectedMethodDefinition, typeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName));

            var methodDefinition = scope.TryGet(SelectedMethodDefinition);
            if (methodDefinition == null)
            {
                return;
            }

            new ParameterPanelIntegration().Connect(invocationInfo, parametersPanel, methodDefinition);
        }

        /// <summary>
        ///     Refreshes the values.
        /// </summary>
        void RefreshValues()
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);

            if (invocationInfo == null)
            {
                return;
            }

            environmentIntellisenseTextBox.SetValue(invocationInfo.Environment);
            assemblySearchDirectoryIntellisenseTextBox.SetValue(invocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(invocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(invocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(invocationInfo.MethodName);
        }

        /// <summary>
        ///     Registers the events.
        /// </summary>
        void RegisterEvents()
        {
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) => { ViewController.OnAssemblySearchDirectoryChanged(scope); };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = scope.TryGet(SelectedInvocationInfo);
                invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
            };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) => { ViewController.OnAssemblyNameChanged(scope); };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { ViewController.OnClassNameChanged(scope); };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { OnMethodNameChanged(); };
        }

        /// <summary>
        ///     Updates the suggestions.
        /// </summary>
        void UpdateSuggestions()
        {
            environmentIntellisenseTextBox.Suggestions = new List<string> {"dev", "test", "prep"};
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = new List<string>
            {
                CommonAssemblySearchDirectories.serverBin,
                CommonAssemblySearchDirectories.clientBin
            };
        }
        #endregion
    }
}