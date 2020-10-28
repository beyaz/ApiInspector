using System;
using System.Collections.Generic;
using ApiInspector.Models;
using static ApiInspector.Components.ComboBoxEditor;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    public partial class View
    {
        #region Fields
        /// <summary>
        ///     The model
        /// </summary>
        internal readonly ViewModel model = new ViewModel
        {
            ItemSourceList = new ItemSourceList(),
            InvocationInfo = new InvocationInfo()
        };
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

        void Initialize()
        {
            var scope = new Scope
            {
                {Components.EditorProp.ItemsSource, new List<string> {"dev", "test", "prep"}},
                {Components.EditorProp.DataContext, model.InvocationInfo},
                {Components.EditorProp.BindingPath,nameof(model.InvocationInfo.Environment)}
            };
            InitializeComboBox(scope, environmentComboBox);
        }

        #region Enums
        /// <summary>
        ///     The view events
        /// </summary>
        enum ViewEvents
        {
            /// <summary>
            ///     The on assembly search directory changed
            /// </summary>
            OnAssemblySearchDirectoryChanged,

            /// <summary>
            ///     The on environment changed
            /// </summary>
            OnEnvironmentChanged,

            /// <summary>
            ///     The on assembly name changed
            /// </summary>
            OnAssemblyNameChanged,

            /// <summary>
            ///     The on class name changed
            /// </summary>
            OnClassNameChanged,

            /// <summary>
            ///     The on method name changed
            /// </summary>
            OnMethodNameChanged
        }
        #endregion

        #region Properties
        /// <summary>
        ///     The view controller
        /// </summary>
        internal ViewController viewController => new ViewController(model);
        #endregion

        #region Public Methods
        /// <summary>
        ///     Connects the specified invocation information.
        /// </summary>
        public void Connect(InvocationInfo invocationInfo)
        {
            model.InvocationInfo = invocationInfo;

            UpdateSuggestions();

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
            var invocationInfo = model.InvocationInfo;

            if (invocationInfo == null)
            {
                return; // TODO: nasıl olabilir
            }

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

                    if (model.MethodDefinition != null)
                    {
                        new ParameterPanelIntegration().Connect(invocationInfo, parametersPanel, model.MethodDefinition);
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
            var invocationInfo = model.InvocationInfo;

            if (invocationInfo == null)
            {
                return;
            }

            environmentIntellisenseTextBox.SetValue(invocationInfo.Environment);
            assemblySearchDirectoryIntellisenseTextBox.SetValue(invocationInfo.AssemblySearchDirectory);
            assemblyIntellisenseTextBox.SetValue(invocationInfo.AssemblyName);
            classNameIntellisenseTextBox.SetValue(invocationInfo.ClassName);
            methodNameIntellisenseTextBox.SetValue(invocationInfo.MethodName);
            
            // force method name change for update parameter panel
            if (methodNameIntellisenseTextBox.Editor.Text == invocationInfo.MethodName)
            {
                FireEvent(ViewEvents.OnMethodNameChanged);
            }
        }

        /// <summary>
        ///     Registers the events.
        /// </summary>
        void RegisterEvents()
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
            var source = model.ItemSourceList;

            environmentIntellisenseTextBox.Suggestions             = source.EnvironmentNameList;
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = source.AssemblySearchDirectoryList;
            assemblyIntellisenseTextBox.Suggestions                = source.AssemblyNameList;
            classNameIntellisenseTextBox.Suggestions               = source.ClassNameList;
            methodNameIntellisenseTextBox.Suggestions              = source.MethodNameList;
        }
        #endregion
    }
}