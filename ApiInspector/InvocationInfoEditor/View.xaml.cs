using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using static ApiInspector.Keys;
using static FunctionalPrograming.Extensions;
using static ApiInspector.DataAccess.TypeVisitor;

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

            scope.OnUpdate(SelectedInvocationInfo, RefreshValues);
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
            var getAssemblySearchDirectory    = fun(() => assemblySearchDirectoryIntellisenseTextBox.Editor.Text);
            var getSelectedInvocationInfo     = fun(() => scope.Get(SelectedInvocationInfo));
            var getAssemblyFileName           = fun(() => assemblyIntellisenseTextBox.Editor.Text);
            var getClassName                  = fun(() => classNameIntellisenseTextBox.Editor.Text);
            var trace                         = scope.Get(Trace);
            var updateAssemblyNameSuggestions = fun((IReadOnlyList<string> items) => assemblyIntellisenseTextBox.Suggestions = items);
            var updateClassNameSuggestions    = fun((IReadOnlyList<string> items) => classNameIntellisenseTextBox.Suggestions = items);
            var updateMethodNameSuggestions   = fun((IReadOnlyList<string> items) => methodNameIntellisenseTextBox.Suggestions = items);
            
            TypeDefinition   selectedTypeDefinition   = null;
            MethodDefinition selectedMethodDefinition = null;

            var getTypeDefinitionsInAssembly = fun(() =>
            {
                var invocationInfo   = getSelectedInvocationInfo();

                var assemblyFilePath = invocationInfo.GetAssemblyFilePath();
                
                var assemblySearchDirectories = invocationInfo.GetAssemblySearchDirectories();

                return GetTypeDefinitionsInAssembly(trace, assemblyFilePath, assemblySearchDirectories);
            });

            var onAssemblySearchDirectoryChanged = fun(() =>
            {
                var assemblySearchDirectory = getAssemblySearchDirectory();
                var invocationInfo          = getSelectedInvocationInfo();

                invocationInfo.AssemblySearchDirectory = assemblySearchDirectory;

                var getAssemblyListInDirectory = fun(() =>
                {
                    if (!Directory.Exists(assemblySearchDirectory))
                    {
                        return new List<string>();
                    }

                    var assemblyNameList = Directory.GetFiles(assemblySearchDirectory).Select(Path.GetFileName).ToList();

                    if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
                    {
                        return assemblyNameList.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("BOA.EOD.")).ToList();
                    }

                    return assemblyNameList;
                });

                updateAssemblyNameSuggestions(getAssemblyListInDirectory());
            });

            var onAssemblyNameChanged = fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.AssemblyName = getAssemblyFileName();

                var getClassNamesOfSelectedAssembly = fun(() =>
                {
                    var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                    if (!File.Exists(assemblyFilePath))
                    {
                        trace($"File not exists. File:{assemblyFilePath}");
                        return new List<string>();
                    }

                    return getTypeDefinitionsInAssembly().Select(x => x.FullName).ToList();
                });

                updateClassNameSuggestions(getClassNamesOfSelectedAssembly());
            });

            var onClassNameChanged = fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.ClassName = getClassName();

                var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                var findType = fun(() =>
                {
                    if (!File.Exists(assemblyFilePath))
                    {
                        trace($"File not exists. File:{assemblyFilePath}");
                        return null;
                    }

                    return getTypeDefinitionsInAssembly().FirstOrDefault(type => type.FullName == invocationInfo.ClassName);
                });

                selectedTypeDefinition = findType();

                if (selectedTypeDefinition == null)
                {
                    trace($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                    return;
                }

                var getMethodNameListFromSelectedType = fun(() =>
                {
                    if (invocationInfo.AssemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
                    {
                        return new List<string>
                        {
                            EndOfDay.MethodAccessText
                        };
                    }

                    return selectedTypeDefinition.Methods.Select(x => x.Name).ToList();
                });

                var methodNames = getMethodNameListFromSelectedType();

                updateMethodNameSuggestions(methodNames);
            });

            var onMethodNameChanged = fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                selectedMethodDefinition = selectedTypeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);
                if (selectedMethodDefinition == null)
                {
                    return;
                }

                new ParameterPanelIntegration().Connect(invocationInfo, parametersPanel, selectedMethodDefinition);
            });

            // attach
            {
                assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) => { onAssemblySearchDirectoryChanged(); };

                environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
                {
                    var invocationInfo = getSelectedInvocationInfo();
                    invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
                };

                assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) => { onAssemblyNameChanged(); };

                classNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { onClassNameChanged(); };

                methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) => { onMethodNameChanged(); };
            }
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