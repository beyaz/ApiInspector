using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using Mono.Cecil;
using static ApiInspector.Keys;
using static FunctionalPrograming.FPExtensions;
using static ApiInspector.DataAccess.TypeVisitor;
using static ApiInspector.WPFExtensions;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     Interaction logic for View.xaml
    /// </summary>
    class View:UserControl
    {
        
        internal Scope scope;

        readonly IntellisenseTextBox environmentIntellisenseTextBox = new IntellisenseTextBox(),
                                     assemblySearchDirectoryIntellisenseTextBox = new IntellisenseTextBox(),
                                     assemblyIntellisenseTextBox = new IntellisenseTextBox(),
                                     classNameIntellisenseTextBox = new IntellisenseTextBox(),
                                     methodNameIntellisenseTextBox = new IntellisenseTextBox();


        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            var buildUI = Fun(() =>
            {
                var fontWeight = FontWeights.Bold;

                var createInput = Fun((string label, IntellisenseTextBox editor) =>
                {
                    var lbl = new Label
                    {
                        FontWeight = fontWeight,
                        Content    = label
                    };

                    return NewStackPanel(lbl, editor);
                });

                var crateMethodNamePart = Fun(() =>
                {
                    var lbl = new Label
                    {
                        FontWeight = fontWeight,
                        Content    = "Method Name"
                    };

                    lbl.Loaded += (s, e) =>
                    {
                        scope?.OnUpdate(SelectedMethodDefinition, () =>
                        {
                            var returnTypeFullName = scope.TryGet(SelectedMethodDefinition)?.ReturnType.FullName;
                            
                            if (returnTypeFullName == null)
                            {
                                lbl.Content = "Method Name";
                                return;
                            }

                            // beautify returnTypeFullName
                            {
                                var replaceMap = new Dictionary<string,string>
                                {
                                    {"`1",string.Empty},
                                    {"`2",string.Empty},
                                    {"`3",string.Empty},
                                    {"`4",string.Empty},
                                
                                    {"System.Collections.Generic.",string.Empty },
                                    {"System.Collections.",string.Empty },
                                    {"BOA.Common.Types.",string.Empty },
                                    {"System.",string.Empty },

                                };

                                foreach (var pair in replaceMap)
                                {
                                    returnTypeFullName = returnTypeFullName.Replace(pair.Key, pair.Value);
                                }
                            }
                            
                            lbl.Content = $"Method Name returns: {returnTypeFullName}";
                        });
                    };


                    return NewStackPanel(lbl, methodNameIntellisenseTextBox);
                });

                var labelSeparatedGridSplitter = Fun((GridSplitter gridSplitter) =>
                {
                    methodNameIntellisenseTextBox.Loaded += (s, e) =>
                    {
                        gridSplitter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                        gridSplitter.Height = methodNameIntellisenseTextBox.ActualHeight;
                    };

                });

                return NewGroupBox(NewBoldTextBlock("Method Information"), 
                                   NewStackPanel(10,
                                                 NewGridWithColumns(new []{2,3,5},
                                                                    createInput("Target BOA Environment (Dev,Test)", environmentIntellisenseTextBox),
                                                                    createInput("Assembly Search Directory", assemblySearchDirectoryIntellisenseTextBox),
                                                                    createInput("Assembly Name", assemblyIntellisenseTextBox))
                                                 ,
                                                 NewColumnSplittedGrid(createInput("Class Name", classNameIntellisenseTextBox),
                                                                       crateMethodNamePart(),labelSeparatedGridSplitter))
                                                );
            });

            Content = buildUI();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += (s, e) => { UpdateSuggestions(); };
                Loaded += (s, e) => { RegisterEvents(); };
            }
          
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
            var getAssemblySearchDirectory = Fun(() => assemblySearchDirectoryIntellisenseTextBox.Editor.Text);
            var getSelectedInvocationInfo  = Fun(() => scope.Get(SelectedInvocationInfo));
            var getAssemblyFileName        = Fun(() => assemblyIntellisenseTextBox.Editor.Text);
            var getClassName               = Fun(() => classNameIntellisenseTextBox.Editor.Text);
            var trace                      = scope.Get(Trace);

            var getUpdateSuggestionsFunc = Fun((IntellisenseTextBox textBox) => { return Fun((IReadOnlyList<string> suggestions) => textBox.Suggestions = suggestions); });

            var updateAssemblyNameSuggestions = getUpdateSuggestionsFunc(assemblyIntellisenseTextBox);
            var updateClassNameSuggestions    = getUpdateSuggestionsFunc(classNameIntellisenseTextBox);
            var updateMethodNameSuggestions   = getUpdateSuggestionsFunc(methodNameIntellisenseTextBox);

            TypeDefinition   selectedTypeDefinition   = null;
            MethodDefinition selectedMethodDefinition = null;

            var getTypeDefinitionsInAssembly = Fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                var assemblySearchDirectories = invocationInfo.GetAssemblySearchDirectories();

                return GetTypeDefinitionsInAssembly(e => trace(e.ToString()), assemblyFilePath, assemblySearchDirectories);
            });

            var onAssemblySearchDirectoryChanged = Fun(() =>
            {
                var assemblySearchDirectory = getAssemblySearchDirectory();
                var invocationInfo          = getSelectedInvocationInfo();

                invocationInfo.AssemblySearchDirectory = assemblySearchDirectory;

                var getAssemblyListInDirectory = Fun(() =>
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

            var onAssemblyNameChanged = Fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.AssemblyName = getAssemblyFileName();

                var getClassNamesOfSelectedAssembly = Fun(() =>
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

            var updateSelectedMethodDefinition = Fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                bool searchMethod(MethodDefinition md)
                {
                    if (md.GetMethodNameWithSignature() == invocationInfo.MethodName)
                    {
                        return true;
                    }

                    return md.Name == invocationInfo.MethodName;
                }

                selectedMethodDefinition = selectedTypeDefinition?.Methods.FirstOrDefault(searchMethod);

                if (selectedMethodDefinition!= null)
                {
                    if (invocationInfo.MethodName != selectedMethodDefinition.GetMethodNameWithSignature())
                    {
                        invocationInfo.MethodName = selectedMethodDefinition.GetMethodNameWithSignature();
                        methodNameIntellisenseTextBox.SetValue(invocationInfo.MethodName);

                    }
                }
               
                
                scope.Update(Keys.SelectedMethodDefinition,selectedMethodDefinition);
            });

            var onClassNameChanged = Fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.ClassName = getClassName();

                var assemblyFilePath = invocationInfo.GetAssemblyFilePath();

                var findType = Fun(() =>
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

                var getMethodNameListFromSelectedType = Fun(() =>
                {
                    if (invocationInfo.AssemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
                    {
                        return new List<string>
                        {
                            EndOfDay.MethodAccessText
                        };
                    }

                    var filter = Fun((MethodDefinition methodDefinition) =>
                    {
                        if (methodDefinition.IsGetter || methodDefinition.IsSetter || methodDefinition.IsConstructor )
                        {
                            return false;
                        }


                        return true;
                    });

                    return selectedTypeDefinition.Methods.Where(filter).Select(CecilHelper.GetMethodNameWithSignature).ToList();
                });

                var methodNames = getMethodNameListFromSelectedType();

                updateMethodNameSuggestions(methodNames);

                if (!string.IsNullOrWhiteSpace(invocationInfo.MethodName) &&  invocationInfo.MethodName == methodNameIntellisenseTextBox.Editor.Text)
                {
                    updateSelectedMethodDefinition();
                }
            });

           

            var onMethodNameChanged = Fun(() =>
            {
                var invocationInfo = getSelectedInvocationInfo();

                invocationInfo.MethodName = methodNameIntellisenseTextBox.Editor.Text;

                updateSelectedMethodDefinition();
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
                CommonAssemblySearchDirectories.clientBin,
                CommonAssemblySearchDirectories.IntegrationServerBin
            };
        }
        #endregion
    }
}