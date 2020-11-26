using System;
using System.Collections.Generic;
using ApiInspector.Models;
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

        #region Enums
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

        #region Public Methods
        /// <summary>
        ///     Connects the specified invocation information.
        /// </summary>
        public void Connect(InvocationInfo invocationInfo)
        {
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
            var invocationInfo = scope.TryGet(SelectedInvocationInfo);

            if (invocationInfo == null)
            {
                return; // TODO: nasıl olabilir
            }
            var trace = scope.Get(Trace);
            switch (name)
            {
                case ViewEvents.OnAssemblySearchDirectoryChanged:
                {
                    var data = new Scope
                    {
                        {GetAssemblySearchDirectory, () => assemblySearchDirectoryIntellisenseTextBox.Editor.Text},
                        {SelectedInvocationInfo, invocationInfo}
                    };
                    data.OnInsert(AssemblyNameSuggestions, () =>
                    {
                        assemblyIntellisenseTextBox.Suggestions = data.Get(AssemblyNameSuggestions);
                    });

                    ViewController.OnAssemblySearchDirectoryChanged(scope);

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

                    

                    
                    ViewController.OnAssemblyNameChanged(invocationInfo,trace,x=>classNameIntellisenseTextBox.Suggestions = x);

                    break;
                }

                case ViewEvents.OnClassNameChanged:
                {
                    invocationInfo.ClassName = classNameIntellisenseTextBox.Editor.Text;
                        
                    

                    ViewController.OnClassNameChanged(invocationInfo,trace,x=>methodNameIntellisenseTextBox.Suggestions=x,x=>scope.Update(TypeDefinition, x));

                    break;
                }

                case ViewEvents.OnMethodNameChanged:
                {
                    invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                    ViewController.OnMethodNameSelected(scope);

                    var methodDefinition = scope.TryGet(MethodDefinition);
                    if (methodDefinition != null)
                    {
                        new ParameterPanelIntegration().Connect(invocationInfo, parametersPanel, methodDefinition);
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

            environmentIntellisenseTextBox.Suggestions = new List<string> {"dev", "test","prep"};
            assemblySearchDirectoryIntellisenseTextBox.Suggestions = new List<string>
            {
                CommonAssemblySearchDirectories.serverBin,
                CommonAssemblySearchDirectories.clientBin
            };
            
        }
        #endregion
    }
}