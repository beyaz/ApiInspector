using System;
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

            switch (name)
            {
                case ViewEvents.OnAssemblySearchDirectoryChanged:
                {
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
                    ViewController.OnAssemblyNameChanged(scope);

                    break;
                }

                case ViewEvents.OnClassNameChanged:
                {
                    ViewController.OnClassNameChanged(scope);

                    break;
                }

                case ViewEvents.OnMethodNameChanged:
                {
                    OnMethodNameChanged();

                    break;
                }

                default: throw new NotImplementedException(name.ToString());
            }

            
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

            //// force method name change for update parameter panel
            //if (methodNameIntellisenseTextBox.Editor.Text == invocationInfo.MethodName)
            //{
            //    FireEvent(ViewEvents.OnMethodNameChanged);
            //}
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