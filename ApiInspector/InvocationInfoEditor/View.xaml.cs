using System;
using System.Windows;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {

        public DataContext Context { get; set; }

        #region Fields
        /// <summary>
        ///     The view controller
        /// </summary>
        internal readonly ViewController viewController = new ViewController();
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
            var invocationInfo = SelectedInvocationInfoKey[Context];

            switch (name)
            {
                case ViewEvents.OnAssemblySearchDirectoryChanged:
                {
                    invocationInfo.AssemblySearchDirectory = assemblySearchDirectoryIntellisenseTextBox.Editor.Text;

                    viewController.OnAssemblySearchDirectoryChanged();

                    break;
                }

                case ViewEvents.OnEnvironmentChanged:
                {
                    invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;

                    break;
                }

                case ViewEvents.OnAssemblyNameChanged:
                {
                    invocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;

                    viewController.OnAssemblyNameChanged();

                    break;
                }

                case ViewEvents.OnClassNameChanged:
                {
                    invocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;

                    viewController.OnClassNameChanged();

                    break;
                }

                case ViewEvents.OnMethodNameChanged:
                {
                    invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                    viewController.OnMethodNameSelected();

                    if (Context.Contains(MethodDefinitionKey))
                    {
                        new ParameterPanelIntegration().Connect(invocationInfo, parametersPanel, MethodDefinitionKey[Context]);
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
            if (!Context.Contains(SelectedInvocationInfoKey))
            {
                return;
            }
            var invocationInfo = SelectedInvocationInfoKey[Context];


            environmentIntellisenseTextBox.SetValue(invocationInfo.Environment);
            assemblySearchDirectoryIntellisenseTextBox.SetValue(invocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(invocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(invocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(invocationInfo.MethodName);
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
            if (Context == null || !Context.Contains(ItemSourceListKey))
            {
                return;
            }

            var source = ItemSourceListKey[Context];

            environmentIntellisenseTextBox.Suggestions             = source.EnvironmentNameList;
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = source.AssemblySearchDirectoryList;
            assemblyIntellisenseTextBox.Suggestions                = source.AssemblyNameList;
            classNameIntellisenseTextBox.Suggestions               = source.ClassNameList;
            methodNameIntellisenseTextBox.Suggestions              = source.MethodNameList;
        }
        #endregion
    }
}