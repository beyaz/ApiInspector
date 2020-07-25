using System;
using System.Windows;
using ApiInspector.Models;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {

        public DataContext Context;

        #region Fields
        /// <summary>
        ///     The view controller
        /// </summary>
        readonly ViewController viewController = new ViewController();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            InitializeComponent();

            Loaded += RegisterEvents;
            Loaded += (s, e) => { UpdateSuggestions(); };
        }
        #endregion

        #region Enums
        enum ViewEvents
        {
            OnAssemblySearchDirectoryChanged,
            OnEnvironmentChanged,
            OnAssemblyNameChanged,
            OnClassNameChanged,
            OnMethodNameChanged
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the model.
        /// </summary>
        InvocationEditorViewModel Model => MainWindowViewModelKey[Context].InvocationEditor;
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => Model.InvocationInfo;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Called when [invocation information changed].
        /// </summary>
        public void OnInvocationInfoChanged()
        {
            RefreshValues();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Afters the controller call.
        /// </summary>
        void AfterControllerCall()
        {
            UpdateSuggestions();
        }

        /// <summary>
        ///     Fires the event.
        /// </summary>
        void FireEvent(ViewEvents name)
        {
            switch (name)
            {
                case ViewEvents.OnAssemblySearchDirectoryChanged:
                {
                    InvocationInfo.AssemblySearchDirectory = assemblySearchDirectoryIntellisenseTextBox.Editor.Text;

                    viewController.OnAssemblySearchDirectoryChanged(Model);

                    break;
                }

                case ViewEvents.OnEnvironmentChanged:
                {
                    InvocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;

                    break;
                }

                case ViewEvents.OnAssemblyNameChanged:
                {
                    InvocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;

                    viewController.OnAssemblyNameChanged(Model);

                    break;
                }

                case ViewEvents.OnClassNameChanged:
                {
                    InvocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;

                    viewController.OnClassNameChanged(Model);

                    break;
                }

                case ViewEvents.OnMethodNameChanged:
                {
                    InvocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                    viewController.OnMethodNameSelected(Model);

                    if (Model.MethodDefinition != null)
                    {
                        new ParameterPanelIntegration().Connect(InvocationInfo, parametersPanel, Model.MethodDefinition);
                    }

                    break;
                }

                default: throw new NotImplementedException(name.ToString());
            }

            AfterControllerCall();
        }

        /// <summary>
        ///     Refreshes the values.
        /// </summary>
        void RefreshValues()
        {
            if (InvocationInfo == null)
            {
                return;
            }

            environmentIntellisenseTextBox.SetValue(InvocationInfo.Environment);
            assemblySearchDirectoryIntellisenseTextBox.SetValue(InvocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(InvocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(InvocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(InvocationInfo.MethodName);
        }

        /// <summary>
        ///     Registers the events.
        /// </summary>
        void RegisterEvents(object sender, RoutedEventArgs routedEventArgs)
        {
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) => { FireEvent(ViewEvents.OnAssemblySearchDirectoryChanged); };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) => { FireEvent(ViewEvents.OnEnvironmentChanged); };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) => { FireEvent(ViewEvents.OnAssemblyNameChanged); };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { FireEvent(ViewEvents.OnClassNameChanged); };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { FireEvent(ViewEvents.OnMethodNameChanged); };
        }

        /// <summary>
        ///     Updates the suggestions.
        /// </summary>
        void UpdateSuggestions()
        {
            if (Context == null || Model == null || Model.ItemSourceList == null)
            {
                return;
            }

            var source = Model.ItemSourceList;

            environmentIntellisenseTextBox.Suggestions             = source.EnvironmentNameList;
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = source.AssemblySearchDirectoryList;
            assemblyIntellisenseTextBox.Suggestions                = source.AssemblyNameList;
            classNameIntellisenseTextBox.Suggestions               = source.ClassNameList;
            methodNameIntellisenseTextBox.Suggestions              = source.MethodNameList;
        }
        #endregion
    }
}