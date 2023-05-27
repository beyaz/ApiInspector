using System.IO;
using System.Threading.Tasks;
using ApiInspector.WebUI.Components;
using ReactWithDotNet.ThirdPartyLibraries.PrimeReact;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;
using ReactWithDotNet.ThirdPartyLibraries.UIW.ReactCodemirror;
using static System.Environment;

namespace ApiInspector.WebUI;

class MainWindowModel
{
    public string AssemblyDirectory { get; set; }

    public string AssemblyFileName { get; set; }

    public string ClassFilter { get; set; }

    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }

    public string MethodFilter { get; set; }

    public string ResponseAsJson { get; set; }

    public MethodReference SelectedMethod { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }
}

class MainWindow : ReactComponent<MainWindowModel>
{
    public bool DebugButtonStatusIsFail { get; set; }

    public bool DebugButtonStatusIsSuccess { get; set; }
    public bool ExecuteButtonStatusIsFail { get; set; }

    public bool ExecuteButtonStatusIsSuccess { get; set; }

    public bool HistoryDialogVisible { get; set; }
    public bool IsDebugStarted { get; set; }
    public bool IsExecutionStarted { get; set; }

    public bool IsInitializingSelectedMethod { get; set; }

    static StyleModifier ComponentBoxShadow => BoxShadow("6px 6px 20px 0px rgb(69 42 124 / 15%)");

    string AssemblyFileFullPath => Path.Combine(state.AssemblyDirectory, state.AssemblyFileName);

    protected override async Task constructor()
    {
        state = await StateCache.ReadState() ?? new MainWindowModel
        {
            AssemblyDirectory = Path.GetDirectoryName(DotNetFrameworkInvokerExePath),
            AssemblyFileName  = "ApiInspector.exe",
            MethodFilter      = "GetHelpMessage"
        };
    }

    Element GetEnvironment()
    {
        return Flow(AssemblyFileFullPath, External.GetEnvironment, str => new FlexRowCentered
        {
            str
        });
    }
    protected override Element render()
    {
        ArrangeEditors();

        const string borderColor = "#d5d5d8";

        return new FlexRow(Padding(10), WidthHeightMaximized, Background("#eff3f8"))
        {
            new Dialog
            {
                visible  = HistoryDialogVisible,
                header   = new div("Select Method From History"),
                closable = true,
                onHide   = () => HistoryDialogVisible = false,
                children =
                {
                    new style
                    {
                        text = @"
.p-dialog .p-dialog-header {
    background: transparent;
}
.p-dialog .p-dialog-content{
   background: transparent;
}
"
                    },
                    new HistoryPanel
                    {
                        SelectionChanged = selectedMethod =>
                        {
                            HistoryDialogVisible = false;

                            state = StateCache.TryRead(selectedMethod) ?? state;
                        }
                    }
                },
                style = { Border($"1px solid {borderColor}"), BackdropFilterBlur(12), Background("rgba(255, 255, 255, 0.4)") }
            },
            new style
            {
                @"
.ͼ1.cm-editor.cm-focused {
    outline: none;
}
/* string */
.ͼ1a{
    color: #f44336;
    font-weight: bold;
}
/* number */
.ͼ18 {
    color: #141413;
    font-weight: bold;
}
/* boolean */
.ͼ1f {
    color: #2c1aeb;
    font-weight: bold;
}


/* prime react splitter */
.p-splitter-gutter{
     z-index: 9;
}
"
            },
            
            new FlexColumn(Border(Solid(1,borderColor)),
                           WidthHeightMaximized,
                           Background(rgba(255, 255, 255, 0.4)),
                           BorderRadius(10),
                           BoxShadow(0, 30, 30, 0, rgba(69, 42, 124, 0.15)),
                           BackdropFilterBlur(30)
                          )
            {
                new FlexRow(PaddingLeftRight(30), PaddingTopBottom(5), BorderBottom(Solid(1, borderColor)))
                {
                    JustifyContentSpaceBetween,
                    AlignItemsCenter,
                    
                    new FlexRow(Gap(5))
                    {
                        AlignItemsCenter,
                        new h3 { "Api Inspector" }, new h5 { " (.net method invoker) " }
                    },
                    
                    new FlexRow(Gap(20))
                    {
                        GetEnvironment,
                        new LogoutButton()
                    }
                },

                new FlexRow(HeightMaximized)
                {
                    searchPanel,
                    ActiveSelectedMethod
                }
            }
        };

        Element searchPanel()
        {
            return new FlexColumn(Width(500), Gap(10), Padding(10), MarginTop(20), PositionRelative)
            {
                new HistoryButton
                {
                    Click = _ => HistoryDialogVisible = true,
                    style = { PositionAbsolute , Right(10) , Top(-13) , ComponentBoxShadow }
                },

                new FlexColumn
                {
                    new Label { Text = "Assembly Directory" },

                    new DirectorySelector
                    {
                        DirectoryPath    = state.AssemblyDirectory,
                        SelectionChanged = x => state.AssemblyDirectory = x,
                        style = { ComponentBoxShadow }
                    }
                },

                new FlexColumn
                {
                    new Label { Text = "Assembly" },

                    new AssemblySelector
                    {
                        AssemblyDirectoryPath = state.AssemblyDirectory,
                        AssemblyFileName      = state.AssemblyFileName,
                        SelectionChanged      = x => state.AssemblyFileName = x,
                        style                 = { ComponentBoxShadow }
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by class name" },

                    new InputText
                    {
                        valueBind                = () => state.ClassFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = { ComponentBoxShadow }
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by method name" },

                    new InputText
                    {
                        valueBind                = () => state.MethodFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = { ComponentBoxShadow }
                    }
                },

                new MethodSelectionView
                {
                    ClassFilter               = state.ClassFilter,
                    MethodFilter              = state.MethodFilter,
                    SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                    SelectionChanged          = OnElementSelected,
                    AssemblyFilePath          = AssemblyFileFullPath,
                    style                     = { ComponentBoxShadow }
                }
            };
        }
        
        Element ActiveSelectedMethod()
        {
            if (IsInitializingSelectedMethod)
            {
                return new FlexRowCentered(FlexGrow(1))
                {
                    new LoadingIcon { wh(100) }
                };
            }

            return new FlexColumn(FlexGrow(1), Gap(10), PaddingRight(10))
            {
                new Splitter
                {
                    ComponentBoxShadow,
                    MarginTop(5),
                    BorderRadius(5),
                    
                    new SplitterPanel
                    {
                        new FlexColumn(AlignItemsCenter)
                        {
                            new Label
                            {
                                Text = "Instance json", 
                                style =
                                {
                                    Padding(10),
                                    FlexGrow(1)
                                }
                            },
                            new FreeScrollBar
                            {
                                FreeScrollBar.Modify(x=>x.autohide = true),
                                
                                Height(300), PaddingBottom(10),
                                BorderTop(Solid(1,"#d9d9d9")),
                                BorderBottomLeftRadius(3),
                                WidthMaximized,
                                FlexGrow(1),
                                new CodeMirror
                                {
                                    extensions = { "json", "githubLight" },
                                    valueBind  = () => state.JsonTextForDotNetInstanceProperties,
                                    basicSetup =
                                    {
                                        highlightActiveLine       = false,
                                        highlightActiveLineGutter = false,
                                    }
                                }
                            }
                        },
                    },

                    new SplitterPanel
                    {
                        new FlexColumn(AlignItemsCenter)
                        {
                            new Label
                            {
                                Text = "Parameters json",
                                style =
                                {
                                    Padding(10),
                                    FlexGrow(1)
                                }
                            },
                            new FreeScrollBar
                            {
                                FreeScrollBar.Modify(x=>x.autohide = true),
                                
                                Height(300), PaddingBottom(10),
                                BorderTop(Solid(1,"#d9d9d9")),
                                BorderBottomRightRadius(3),
                                WidthMaximized,
                                FlexGrow(1),

                                new CodeMirror
                                {
                                    extensions = { "json", "githubLight" },
                                    valueBind  = () => state.JsonTextForDotNetMethodParameters,
                                    basicSetup =
                                    {
                                        highlightActiveLine       = false,
                                        highlightActiveLineGutter = false,
                                    }
                                }
                            }
                        }
                    }
                },
                new FlexColumn(FlexGrow(1), Gap(10))
                {
                    new FlexRow(Height(50), Gap(30))
                    {
                        new ExecuteButton
                        {
                            Click               = OnExecuteClicked,
                            IsProcessing        = IsExecutionStarted,
                            ShowStatusAsSuccess = ExecuteButtonStatusIsSuccess,
                            ShowStatusAsFail    = ExecuteButtonStatusIsFail
                        } + ComponentBoxShadow,
                        new DebugButton
                        {
                            Click               = OnDebugClicked,
                            IsProcessing        = IsDebugStarted,
                            ShowStatusAsSuccess = DebugButtonStatusIsSuccess,
                            ShowStatusAsFail    = DebugButtonStatusIsFail
                        } + ComponentBoxShadow,

                        new MethodReferenceView { MethodReference = state.SelectedMethod } + ComponentBoxShadow
                    },

                    new FlexColumn(WidthHeightMaximized)
                    {
                        new Label { Text = "Response as json" },

                        new FreeScrollBar
                        {
                            FreeScrollBar.Modify(x=>x.autohide = true),
                            ComponentBoxShadow,
                            Height("calc(100% - 28px)"), PaddingBottom(10),
                            Border("1px solid #d9d9d9"),
                            BorderRadius(5),
                            WidthMaximized,

                            new CodeMirror
                            {
                                extensions = { "json", "githubLight" },
                                valueBind  = () => state.ResponseAsJson,
                                basicSetup =
                                {
                                    highlightActiveLine       = false,
                                    highlightActiveLineGutter = false,
                                }
                            }
                        }
                    }
                }
            };
        }
    }

    void ArrangeEditors()
    {
        const int optimumLineCount = 19;

        state.JsonTextForDotNetInstanceProperties = arrange(state.JsonTextForDotNetInstanceProperties, optimumLineCount);
        state.JsonTextForDotNetMethodParameters   = arrange(state.JsonTextForDotNetMethodParameters, optimumLineCount);
        state.ResponseAsJson                      = arrange(state.ResponseAsJson, optimumLineCount);

        static string arrange(string value, int optimumLineCount)
        {
            var lineCount = getLineCount(value);

            if (lineCount < optimumLineCount)
            {
                return value + string.Join(NewLine, Enumerable.Range(0, optimumLineCount - lineCount).Select(_ => string.Empty));
            }

            return value;

            static int getLineCount(string value)
            {
                return (value + string.Empty).Split('\n').Length;
            }
        }
    }

    void ClearActionButtonStates()
    {
        DebugButtonStatusIsFail    = false;
        DebugButtonStatusIsSuccess = false;

        ExecuteButtonStatusIsFail    = false;
        ExecuteButtonStatusIsSuccess = false;
    }

    void OnDebugClicked()
    {
        if (state.SelectedMethod is null)
        {
            state.ResponseAsJson = "Please select any method from left side.";
            return;
        }

        SaveState();

        state.ResponseAsJson = null;

        ClearActionButtonStates();

        if (IsDebugStarted)
        {
            IsDebugStarted = false;

            try
            {
                state.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters, true);

                DebugButtonStatusIsSuccess = true;
            }
            catch (Exception exception)
            {
                DebugButtonStatusIsFail = true;

                state.ResponseAsJson = exception.Message;
            }

            Client.GotoMethod(2000, ClearActionButtonStates);
        }
        else
        {
            IsDebugStarted = true;
            Client.GotoMethod(100, OnDebugClicked);
        }
    }

    void OnElementSelected(string keyOfSelectedTreeNode)
    {
        state.SelectedMethodTreeNodeKey = keyOfSelectedTreeNode;

        IsInitializingSelectedMethod = true;

        Client.GotoMethod(OnElementSelected);
    }

    void OnElementSelected()
    {
        IsInitializingSelectedMethod = false;

        state.SelectedMethod = null;

        state.JsonTextForDotNetInstanceProperties = null;
        state.JsonTextForDotNetMethodParameters   = null;
        state.ResponseAsJson                      = null;

        var node = MethodSelectionView.FindTreeNode(AssemblyFileFullPath, state.SelectedMethodTreeNodeKey, state.ClassFilter, state.MethodFilter);
        if (node is not null)
        {
            if (node.IsMethod)
            {
                state.SelectedMethod = node.MethodReference;

                var currentState = state;

                var cachedState = StateCache.TryRead(state.SelectedMethod);
                if (cachedState is not null)
                {
                    state = cachedState;

                    state.AssemblyDirectory = currentState.AssemblyDirectory;
                    state.AssemblyFileName  = currentState.AssemblyFileName;
                    state.ClassFilter       = currentState.ClassFilter;
                    state.MethodFilter      = currentState.MethodFilter;
                }
            }
        }

        if (state.SelectedMethod != null)
        {
            if (state.JsonTextForDotNetInstanceProperties.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                SafeInvoke(() => External.GetInstanceEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties))
                   .Then(json => state.JsonTextForDotNetInstanceProperties = json, printError);
            }

            if (state.JsonTextForDotNetMethodParameters.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                SafeInvoke(() => External.GetParametersEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, state.JsonTextForDotNetMethodParameters))
                   .Then(json => state.JsonTextForDotNetMethodParameters = json, printError);
            }

            ArrangeEditors();

            void printError(Exception exception)
            {
                state.ResponseAsJson = exception + NewLine + state.ResponseAsJson;
            }
        }
    }

    void OnExecuteClicked()
    {
        if (state.SelectedMethod is null)
        {
            state.ResponseAsJson = "Please select any method from left side.";
            return;
        }

        SaveState();

        state.ResponseAsJson = null;

        ClearActionButtonStates();

        if (IsExecutionStarted)
        {
            IsExecutionStarted = false;

            try
            {
                state.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters, false);

                ExecuteButtonStatusIsSuccess = true;
            }
            catch (Exception exception)
            {
                state.ResponseAsJson = exception.Message;

                ExecuteButtonStatusIsFail = true;
            }

            Client.GotoMethod(2000, ClearActionButtonStates);
        }
        else
        {
            IsExecutionStarted = true;
            Client.GotoMethod(100, OnExecuteClicked);
        }
    }

    void OnFilterTextKeypressCompleted()
    {
    }

    void SaveState()
    {
        HistoryOfSearchDirectories.AddIfNotExists(state.AssemblyDirectory);

        StateCache.Save(state);

        if (state.SelectedMethod is not null)
        {
            StateCache.Save(state.SelectedMethod, state);
        }
    }
}