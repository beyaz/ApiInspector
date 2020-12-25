using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            assemblySearchDirectoryIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {




                var onAssemblySearchDirectoryChanged = fun(() =>
                {
                    var assemblySearchDirectory = scope.Get(GetAssemblySearchDirectory)();
                    var invocationInfo          = scope.Get(SelectedInvocationInfo);

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

                    scope.Update(AssemblyNameSuggestions, getAssemblyListInDirectory());
                });

                onAssemblySearchDirectoryChanged();
            };

            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var invocationInfo = scope.TryGet(SelectedInvocationInfo);
                invocationInfo.Environment = environmentIntellisenseTextBox.Editor.Text;
            };

            assemblyIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var onAssemblyNameChanged = fun(() =>
                {
                    var trace               = scope.Get(Trace);
                    var invocationInfo      = scope.Get(SelectedInvocationInfo);
                    var getAssemblyFileName = scope.Get(GetAssemblyFileName);

                    invocationInfo.AssemblyName = getAssemblyFileName();

                    var getClassNamesOfSelectedAssembly = fun(() =>
                    {
                        var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                        if (!File.Exists(assemblyFilePath))
                        {
                            trace($"File not exists. File:{assemblyFilePath}");
                            return new List<string>();
                        }

                        var assemblySearchDirectories = invocationInfo.GetAssemblySearchDirectories();

                        return GetTypeDefinitionsInAssembly(trace, assemblyFilePath, assemblySearchDirectories).Select(x => x.FullName).ToList();
                    });

                    scope.Update(ClassNameSuggestions, getClassNamesOfSelectedAssembly());
                });
                onAssemblyNameChanged();
            };

            classNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {


                var onClassNameChanged = fun(() =>
                {
                    var trace          = scope.Get(Trace);
                    var invocationInfo = scope.Get(SelectedInvocationInfo);
                    var getClassName   = scope.Get(GetClassName);

                    invocationInfo.ClassName = getClassName();

                    var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                    var findType = fun(() =>
                    {
                        if (!File.Exists(assemblyFilePath))
                        {
                            trace($"File not exists. File:{assemblyFilePath}");
                            return null;
                        }

                        return GetTypeDefinitionsInAssembly(trace, assemblyFilePath, invocationInfo.GetAssemblySearchDirectories()).FirstOrDefault(type => type.FullName == invocationInfo.ClassName);
                    });





                    var typeDefinition = findType();

                    scope.Update(SelectedTypeDefinition, typeDefinition);
                    if (typeDefinition == null)
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

                        return typeDefinition.Methods.Select(x => x.Name).ToList();
                    });

                    var methodNames = getMethodNameListFromSelectedType();

                    scope.Update(MethodNameSuggestions, methodNames);
                });

                onClassNameChanged();

            };

            methodNameIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                var onMethodNameChanged = fun(() =>
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
                });
                onMethodNameChanged();

            };
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