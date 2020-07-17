using System.Windows;
using ApiInspector.Models;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {
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
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the model.
        /// </summary>
        public InvocationEditorViewModel Model { get; set; }
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
        ///     Refreshes the values.
        /// </summary>
        void RefreshValues()
        {
            Model.ParametersPanel = parametersPanel;

            environmentIntellisenseTextBox.SetValue(InvocationInfo.Environment);
            assemblySearchDirectoryIntellisenseTextBox.SetValue(InvocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(InvocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(InvocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(InvocationInfo.MethodName);
        }

        void AfterControllerCall()
        {
            UpdateSuggestions();
        }

        /// <summary>
        ///     Registers the events.
        /// </summary>
        void RegisterEvents(object sender, RoutedEventArgs routedEventArgs)
        {
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                InvocationInfo.AssemblySearchDirectory = assemblySearchDirectoryIntellisenseTextBox.Editor.Text;

                viewController.OnAssemblySearchDirectoryChanged(Model);

                AfterControllerCall();
            };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) => { InvocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text; };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                InvocationInfo.AssemblyName = assemblyIntellisenseTextBox.Editor.Text;

                viewController.OnAssemblyNameChanged(Model);

                AfterControllerCall();
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                InvocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;

                viewController.OnClassNameChanged(Model);

                AfterControllerCall();
            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                InvocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                viewController.OnMethodNameSelected(Model);

                AfterControllerCall();
            };
        }

        /// <summary>
        ///     Updates the suggestions.
        /// </summary>
        void UpdateSuggestions()
        {
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