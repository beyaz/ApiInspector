using System.IO;
using ApiInspector.WebUI.Components;
using ReactWithDotNet.Libraries.PrimeReact;
using ReactWithDotNet.Libraries.react_free_scrollbar;
using ReactWithDotNet.Libraries.uiw.react_codemirror;

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
    public bool IsDebugStarted { get; set; }
    public bool IsExecutionStarted { get; set; }

    public bool IsInitializingSelectedMethod { get; set; }

    string AssemblyFileFullPath => Path.Combine(state.AssemblyDirectory, state.AssemblyFileName);

    protected override void constructor()
    {
        state = StateCache.ReadState() ?? new MainWindowModel
        {
            AssemblyDirectory = Path.GetDirectoryName(Config.DotNetFrameworkInvokerExePath) + Path.DirectorySeparatorChar,
            AssemblyFileName  = "ApiInspector.exe",
            MethodFilter      = "GetHelpMessage"
        };
    }

    protected override Element render()
    {
        ArrangeEditors();

        const string borderColor = "#d5d5d8";

        return new FlexRow(Padding(10), WidthHeightMaximized, Background("#eff3f8"))
        {
            new style
            {
                @"
.ͼ1.cm-editor.cm-focused {
    outline: none;
}"
            },
            new FlexColumn(Border($"1px solid {borderColor}"), WidthHeightMaximized, Background("white"),BorderRadius(3))
            {
                new FlexRow(PaddingLeftRight(30), PaddingTopBottom(5), BorderBottom($"1px solid {borderColor}"))
                {
                    new FlexRow(Gap(5))
                    {
                        AlignItemsCenter,
                        new h3 { "Api Inspector" }, new h5 { " (.net method invoker) " }
                    }
                },

                new FlexRow(HeightMaximized)
                {
                    new FlexColumn(Width(500), Gap(10), Margin(10), MarginTop(20))
                    {
                        new FlexColumn(MarginLeftRight(3))
                        {
                            new Label { Text = "Assembly Directory" },

                            new DirectorySelector
                            {
                                DirectoryPath    = state.AssemblyDirectory,
                                SelectionChanged = x => state.AssemblyDirectory = x
                            }
                        },

                        new FlexColumn(PaddingLeftRight(3))
                        {
                            new Label { Text = "Assembly" },

                            new AssemblySelector
                            {
                                AssemblyDirectoryPath = state.AssemblyDirectory,
                                AssemblyFileName      = state.AssemblyFileName,
                                SelectionChanged = x =>
                                {
                                    state.AssemblyFileName = x;
                                    SaveState();
                                }
                            }
                        },
                        new FlexColumn(MarginLeftRight(3))
                        {
                            new Label { Text = "Filter by class name" },

                            new InputText
                            {
                                valueBind                = () => state.ClassFilter,
                                valueBindDebounceTimeout = 700,
                                valueBindDebounceHandler = OnFilterTextKeypressCompleted
                            }
                        },
                        new FlexColumn(MarginLeftRight(3))
                        {
                            new Label { Text = "Filter by method name" },

                            new InputText
                            {
                                valueBind                = () => state.MethodFilter,
                                valueBindDebounceTimeout = 700,
                                valueBindDebounceHandler = OnFilterTextKeypressCompleted
                            }
                        },

                        new MethodSelectionView
                        {
                            ClassFilter               = state.ClassFilter,
                            MethodFilter              = state.MethodFilter,
                            SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                            SelectionChanged          = OnElementSelected,
                            AssemblyFilePath          = AssemblyFileFullPath
                        }
                    },
                    When(IsInitializingSelectedMethod, new FlexRowCentered(FlexGrow(1))
                    {
                        new LoadingIcon { wh(100) }
                    }),
                    When(!IsInitializingSelectedMethod, () => new FlexColumn(FlexGrow(1), Gap(10), MarginRight(10))
                    {
                        new style
                        {
                            text = ".p-splitter-gutter{z-index:9;}"
                        },
                        new Splitter
                        {
                            new SplitterPanel(PaddingRight(3))
                            {
                                new FlexColumn(AlignItemsCenter)
                                {
                                    new Label
                                    {
                                        Text = "Instance json", style =
                                        {
                                            Padding(10),
                                            FlexGrow(1)
                                        }
                                    },
                                    new FreeScrollBar
                                    {
                                        Height(300), PaddingBottom(10),
                                        Border("1px solid #d9d9d9"),
                                        BorderRadius(3),
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
                                            },
                                            style =
                                            {
                                                BorderRadius(3),
                                                //Border("1px solid #d9d9d9"),
                                                FontSize11,
                                                WidthMaximized
                                            }
                                        }
                                    }
                                },
                            },

                            new SplitterPanel(PaddingLeft(3))
                            {
                                new FlexColumn(AlignItemsCenter)
                                {
                                    new Label
                                    {
                                        Text = "Parameters json", style =
                                        {
                                            Padding(10),
                                            FlexGrow(1)
                                        }
                                    },
                                    new FreeScrollBar
                                    {
                                        Height(300), PaddingBottom(10),
                                        Border("1px solid #d9d9d9"),
                                        BorderRadius(3),
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
                                            },
                                            style =
                                            {
                                                BorderRadius(3),
                                                //Border("1px solid #d9d9d9"),
                                                FontSize11,
                                                WidthMaximized
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
                                },
                                new DebugButton
                                {
                                    Click               = OnDebugClicked,
                                    IsProcessing        = IsDebugStarted,
                                    ShowStatusAsSuccess = DebugButtonStatusIsSuccess,
                                    ShowStatusAsFail    = DebugButtonStatusIsFail
                                }
                            },

                            new FlexColumn(WidthHeightMaximized)
                            {
                                new Label { Text = "Response as json" },

                                new FreeScrollBar
                                {
                                    Height("calc(100% - 28px)"), PaddingBottom(10),
                                    Border("1px solid #d9d9d9"),
                                    BorderRadius(3),
                                    WidthMaximized,

                                    new CodeMirror
                                    {
                                        extensions = { "json", "githubLight" },
                                        valueBind  = () => state.ResponseAsJson,
                                        basicSetup =
                                        {
                                            highlightActiveLine       = false,
                                            highlightActiveLineGutter = false,
                                        },
                                        style =
                                        {
                                            FontSize11
                                        }
                                    }
                                }
                            }
                        }
                    })
                }
            }
        };
    }

    void ArrangeEditors()
    {
        var lines = (state.JsonTextForDotNetInstanceProperties + string.Empty).Split(Environment.NewLine);

        var optimumLineCount = 19;
        if (lines.Length < optimumLineCount)
        {
            state.JsonTextForDotNetInstanceProperties += string.Join(Environment.NewLine, Enumerable.Range(0, optimumLineCount - lines.Length).Select(_ => string.Empty));
        }

        lines = (state.JsonTextForDotNetMethodParameters + string.Empty).Split(Environment.NewLine);
        if (lines.Length < optimumLineCount)
        {
            state.JsonTextForDotNetMethodParameters += string.Join(Environment.NewLine, Enumerable.Range(0, optimumLineCount - lines.Length).Select(_ => string.Empty));
        }

        lines = (state.ResponseAsJson + string.Empty).Split(Environment.NewLine);
        if (lines.Length < optimumLineCount)
        {
            state.ResponseAsJson += string.Join(Environment.NewLine, Enumerable.Range(0, optimumLineCount - lines.Length).Select(_ => string.Empty));
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

                state.ResponseAsJson = exception.ToString();
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
        SaveState();

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

        var node = MethodSelectionView.FindTreeNode(AssemblyFileFullPath, state.SelectedMethodTreeNodeKey);
        if (node is not null)
        {
            if (node.IsMethod)
            {
                state.SelectedMethod = node.MethodReference;

                state = StateCache.TryRead(state.SelectedMethod) ?? state;
            }
        }

        if (state.SelectedMethod != null)
        {
            if (state.JsonTextForDotNetInstanceProperties.HasNoValue() &&
                state.JsonTextForDotNetMethodParameters.HasNoValue())
            {
                try
                {
                    var (jsonForInstance, jsonForParameters) = External.GetEditorTexts(AssemblyFileFullPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters);

                    state.JsonTextForDotNetInstanceProperties = jsonForInstance;
                    state.JsonTextForDotNetMethodParameters   = jsonForParameters;
                }
                catch (Exception exception)
                {
                    state.ResponseAsJson = exception.ToString();
                }
            }

            ArrangeEditors();
        }
    }

    void OnExecuteClicked()
    {
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
                state.ResponseAsJson = exception.ToString();

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