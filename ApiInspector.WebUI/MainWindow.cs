using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using ApiInspector.WebUI.Components;
using ReactWithDotNet.ThirdPartyLibraries.MonacoEditorReact;
using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;
using static System.Environment;

namespace ApiInspector.WebUI;

enum ActionButtonStatus
{
    Ready,
    Executing,
    Success,
    Fail
}

class ExternalProcessManager
{
    public static Process CurrentProcess { get; set; }

    public static Task CurrentProcessTask { get; set; }

    public static string ResponseAsJson { get; set; }

    public static Exception ResponseException { get; set; }
}

class MainWindow : Component<MainWindowModel>
{
    public ActionButtonStatus DebugButtonStatus { get; set; }
    
    public ActionButtonStatus ExecuteButtonStatus { get; set; }

    public bool HistoryDialogVisible { get; set; }

    public bool IsInitializingSelectedMethod { get; set; }

    string AssemblyFileFullPath => Path.Combine(state.AssemblyDirectory, state.AssemblyFileName);

    ScenarioModel SelectedScenarioModel
    {
        get
        {
            if (state.ScenarioListSelectedIndex >= 0 && state.ScenarioListSelectedIndex < state.ScenarioList.Count)
            {
                return state.ScenarioList[state.ScenarioListSelectedIndex];
            }

            return null;
        }
    }

    protected override Task constructor()
    {
        state = StateCache.ReadState() ?? new MainWindowModel
        {
            AssemblyDirectory = Path.GetDirectoryName(DotNetFrameworkInvokerExePath),
            AssemblyFileName  = "ApiInspector.exe",
            MethodFilter      = "GetHelpMessage"
        };

        state.RuntimeName = GetDefaultRuntimeNameFromAssembly(AssemblyFileFullPath);

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        return new FlexRow(Padding(10), SizeFull, Background(Theme.BackgroundColor))
        {
            new FlexColumn
            {
                applicationTopPanel,
                createContent,

                new Style
                {
                    Border(Solid(1, Theme.BorderColor)),
                    SizeFull,
                    Background(Theme.WindowBackgroundColor),
                    BorderRadius(10),
                    BoxShadow(0, 30, 30, 0, rgba(69, 42, 124, 0.15))
                },
                NotificationHost
            }
        };

        Element createContent()
        {
            if (HistoryDialogVisible)
            {
                return new FlexColumnCentered(HeightFull)
                {
                    new HistoryPanel
                    {
                        Closed = () =>
                        {
                            HistoryDialogVisible = false;

                            return Task.CompletedTask;
                        },
                        SelectionChanged = selectedMethod =>
                        {
                            HistoryDialogVisible = false;

                            state = StateCache.TryRead(selectedMethod) ?? state;

                            return Task.CompletedTask;
                        }
                    },
                    SpaceY(200)
                };
            }

            return new SplitRow
            {
                sizes = [30, 70],
                children =
                {
                    searchPanel,
                    new FlexRow(SizeFull)
                    {
                        ActiveSelectedMethod,
                        addRemovePanel
                    }
                }
            };
        }

        Element applicationTopPanel()
        {
            return new FlexRow
            {
                new FlexRow(AlignItemsCenter, Gap(5))
                {
                    new h3 { "Api Inspector" }, new h5 { " (.net method invoker) ", MarginTop(5) }
                },

                new FlexRow(Gap(20))
                {
                    GetEnvironment,
                    new LogoutButton()
                },

                new Style
                {
                    JustifyContentSpaceBetween,
                    AlignItemsCenter,
                    BorderBottom(Solid(1, Theme.BorderColor)),
                    Padding(5, 30)
                }
            };
        }

        Element scenarioFilterInput()
        {
            return new input
            {
                type                     = "text",
                valueBind                = () => state.ScenarioFilterText,
                valueBindDebounceTimeout = 700,
                valueBindDebounceHandler = OnScenarioFilterTextKeypressFinished,
                style                    = { InputStyle, BoxShadowNone, PaddingY(4), PaddingX(10), BorderRadius(16) },
                autoComplete             = "off",
                placeholder              = "filter by scenario"
            };
        }

        Element addRemovePanel()
        {
            static bool hasMatch(ScenarioModel scenarioModel, string filterText)
            {
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    return true;
                }

                return filterText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(x =>
                {
                    if (scenarioModel.JsonTextForDotNetMethodParameters?.Contains(x, StringComparison.OrdinalIgnoreCase) is true)
                    {
                        return true;
                    }

                    if (scenarioModel.JsonTextForDotNetInstanceProperties?.Contains(x, StringComparison.OrdinalIgnoreCase) is true)
                    {
                        return true;
                    }

                    if (scenarioModel.ResponseAsJson?.Contains(x, StringComparison.OrdinalIgnoreCase) is true)
                    {
                        return true;
                    }

                    return false;
                });
            }

            return new FlexColumn(Width(30), PaddingRight(10), Gap(10), JustifyContentFlexStart, AlignItemsCenter, PaddingTopBottom(10))
            {
                state.ScenarioList.Select((scenario, i) => new CircleButton
                {
                    Index      = i,
                    Label      = i.ToString(),
                    IsSelected = i == state.ScenarioListSelectedIndex,
                    Clicked = e =>
                    {
                        state.ScenarioListSelectedIndex = Convert.ToInt32(e.currentTarget.id);
                        return Task.CompletedTask;
                    }
                } + When(!hasMatch(scenario, state.ScenarioFilterText), DisplayNone)),

                new CircleButton
                {
                    Label = "+",
                    Clicked = _ =>
                    {
                        state.ScenarioList              = state.ScenarioList.Add(new());
                        state.ScenarioListSelectedIndex = state.ScenarioList.Count - 1;
                        TryInitializeDefaultJsonInputs();

                        return Task.CompletedTask;
                    },
                    TooltipText = "Add new test scenario"
                },
                state.ScenarioList.Count > 1 ? new CircleButton
                {
                    Label = "-",
                    Clicked = _ =>
                    {
                        state.ScenarioList              = state.ScenarioList.RemoveAt(state.ScenarioListSelectedIndex);
                        state.ScenarioListSelectedIndex = state.ScenarioList.Count - 1;

                        return Task.CompletedTask;
                    },
                    TooltipText = "Remove selected test scenario"
                } : null
            };
        }

        Element searchPanel()
        {
            return new FlexColumn(Width(500), Gap(10), Padding(10, 0, 10, 10), MarginTop(20), PositionRelative)
            {
                new HistoryButton
                {
                    Click = _ =>
                    {
                        HistoryDialogVisible = true;

                        return Task.CompletedTask;
                    }
                } + PositionAbsolute + Right(10) + Top(-13) + ComponentBoxShadow,

                new FlexColumn
                {
                    new Label { Text = "Assembly Directory" },

                    new DirectorySelector
                    {
                        DirectoryPath = state.AssemblyDirectory,
                        SelectionChanged = x =>
                        {
                            state.AssemblyDirectory = x;

                            state.RuntimeName = GetDefaultRuntimeNameFromAssembly(AssemblyFileFullPath);

                            Client.DispatchEvent(Event.OnAssemblyChanged, [
                                new AssemblyChangedArgs
                                {
                                    AssemblyFileFullPath = AssemblyFileFullPath,
                                    RuntimeName          = state.RuntimeName
                                }
                            ]);

                            return Task.CompletedTask;
                        }
                    }
                },

                new Tooltip
                {
                    Tooltip.Title("Type any .dll or .exe file name"),
                    Tooltip.Placement("top-start"),
                    new FlexColumn
                    {
                        new Label { Text = "Assembly" },

                        AssemblySelector.CreateAssemblySelectorInput(state.AssemblyDirectory, state.AssemblyFileName, x =>
                        {
                            state.AssemblyFileName = x;

                            state.RuntimeName = GetDefaultRuntimeNameFromAssembly(AssemblyFileFullPath);

                            Client.DispatchEvent(Event.OnAssemblyChanged, [
                                new AssemblyChangedArgs
                                {
                                    AssemblyFileFullPath = AssemblyFileFullPath,
                                    RuntimeName          = state.RuntimeName
                                }
                            ]);

                            return Task.CompletedTask;
                        })
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by class name" },

                    new input
                    {
                        type                     = "text",
                        valueBind                = () => state.ClassFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = { InputStyle }
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by method name" },

                    new input
                    {
                        type                     = "text",
                        valueBind                = () => state.MethodFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = { InputStyle }
                    }
                },

                new MethodSelectionView
                {
                    ClassFilter               = state.ClassFilter,
                    MethodFilter              = state.MethodFilter,
                    SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                    SelectionChanged          = OnElementSelected,
                    AssemblyFilePath          = AssemblyFileFullPath
                } + ComponentBoxShadow
            };
        }

        static Element NewJsonEditor(Expression<Func<string>> valueBind)
        {
            return new Editor
            {
                defaultLanguage = "json",
                valueBind       = valueBind,
                options =
                {
                    renderLineHighlight = "none",
                    fontFamily          = "'IBM Plex Mono Medium', 'Courier New', monospace",
                    fontSize            = 11,
                    minimap             = new { enabled = false },
                    formatOnPaste       = true,
                    formatOnType        = true,
                    automaticLayout     = true,
                    lineNumbers         = false,

                    unicodeHighlight = new
                    {
                        ambiguousCharacters = false, // confusable karakterleri vurgulama
                        includeStrings      = false, // (ops.) string içeriklerinde vurgulama
                        invisibleCharacters = false // (ops.) görünmez karakterleri vurgulama
                    }
                }
            };
        }

        Element ActiveSelectedMethod()
        {
            if (IsInitializingSelectedMethod)
            {
                return new FlexRowCentered(FlexGrow(1))
                {
                    new LoadingIcon { Size(100) }
                };
            }

            var scenarioIndex = state.ScenarioListSelectedIndex;

            var isStaticMethod = state.SelectedMethod?.IsStatic == true;

            var instanceEditor = new FlexColumn(AlignItemsCenter, FlexGrow(1), When(isStaticMethod, DisplayNone))
            {
                PositionRelative,
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
                    AutoHideScrollbar,

                    Height(300), PaddingBottom(10),
                    BorderTop(Solid(1, "#d9d9d9")),
                    BorderBottomLeftRadius(3),
                    WidthFull,
                    FlexGrow(1),

                    NewJsonEditor(() => state.ScenarioList[scenarioIndex].JsonTextForDotNetInstanceProperties)
                },

                new Tooltip
                {
                    Tooltip.Title("Format"),
                    new div(CursorDefault, Hover(FontWeightBold), PositionAbsolute, Right(32), Bottom(8))
                    {
                        "{ }",
                        OnClick(FormatDotNetInstanceText)
                    }
                }
            };

            var parametersEditor = new FlexColumn(AlignItemsCenter, FlexGrow(1))
            {
                PositionRelative,

                state.ScenarioList.Count < 3 ? null :
                    scenarioFilterInput() +
                    PositionAbsolute + Right(4) + Top(6),

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
                    AutoHideScrollbar,

                    Height(300), PaddingBottom(10),
                    BorderTop(Solid(1, "#d9d9d9")),
                    BorderBottomRightRadius(3),
                    WidthFull,
                    FlexGrow(1),

                    NewJsonEditor(() => state.ScenarioList[scenarioIndex].JsonTextForDotNetMethodParameters)
                },

                new Tooltip
                {
                    Tooltip.Title("Format"),
                    new div(CursorDefault, Hover(FontWeightBold), PositionAbsolute, Right(22), Bottom(8))
                    {
                        "{ }",
                        OnClick(FormatDotNetParametersText)
                    }
                }
            };

            var partEditors = new FlexRow(WidthFull)
            {
                ComponentBoxShadow,
                MarginTop(5),
                BorderRadius(5),

                isStaticMethod ? parametersEditor :
                    new SplitRow
                    {
                        instanceEditor,

                        parametersEditor
                    }
            };

            var partActionButtons = new FlexRow(Height(50), Gap(30))
            {
                new FlexColumn(Gap(2), AlignItemsFlexStart, CursorDefault)
                {
                    new FlexRowCentered(Gap(4))
                    {
                        new svg(svg.Size(20))
                        {
                            new circle { cx = 10, cy = 10, r = 9, stroke = BluePrimary, strokeWidth = 1, fill = none },

                            state.RuntimeName == RuntimeNames.NetCore ? null :
                                new circle { cx = 10, cy = 10, r = 5, fill = "#f18484" }
                        },

                        "Framework",

                        OnClick(_ =>
                        {
                            state.RuntimeName = RuntimeNames.NetFramework;

                            return Task.CompletedTask;
                        })
                    },

                    new FlexRowCentered(Gap(4))
                    {
                        new svg(svg.Size(20))
                        {
                            new circle { cx = 10, cy = 10, r = 9, stroke = BluePrimary, strokeWidth = 1, fill = none },

                            state.RuntimeName == RuntimeNames.NetFramework ? null :
                                new circle { cx = 10, cy = 10, r = 5, fill = "#f18484" }
                        },

                        "Core",

                        OnClick(_ =>
                        {
                            state.RuntimeName = RuntimeNames.NetCore;

                            return Task.CompletedTask;
                        })
                    }
                },
                new ExecuteButton
                {
                    Click  = OnExecuteClicked,
                    Status = ExecuteButtonStatus
                } + ComponentBoxShadow,
                new DebugButton
                {
                    Click  = OnDebugClicked,
                    Status = DebugButtonStatus
                } + ComponentBoxShadow,

                new MethodReferenceView { MethodReference = state.SelectedMethod } + ComponentBoxShadow
            };

            Element partTrace = null;
            {
                if (DebugButtonStatus == ActionButtonStatus.Executing || DebugButtonStatus == ActionButtonStatus.Fail ||
                    ExecuteButtonStatus == ActionButtonStatus.Executing || ExecuteButtonStatus == ActionButtonStatus.Fail)
                {
                    if (AsyncLogger.logs.Count > 0)
                    {
                        var trace = string.Join(NewLine, AsyncLogger.logs);

                        partTrace  = new FlexColumn(WidthFull, Height(200))
                        {
                            new Label { Text = "Trace" },

                            new FreeScrollBar
                            {
                                AutoHideScrollbar,

                                ComponentBoxShadow,
                                HeightFull,
                                Border("1px solid #d9d9d9"),
                                BorderRadius(5),
                                WidthFull,

                                new textarea
                                {
                                    id         = "textAreaForLogs",
                                    value      = trace,
                                    spellcheck = "false",
                                    readOnly   = true,
                                    wrap       = "off",
                                    style =
                                    {
                                        OutlineNone,
                                        BorderNone,
                                        FontSize11,
                                        FontFamily("'IBM Plex Mono Medium', 'Courier New', monospace"),
                                        SizeFull,
                                        Padding(16)
                                    }
                                }
                            }


                        };

                        Client.RunJavascript
                        (
                            """
                            function scrollTextareaToEnd(id) 
                            {
                              const ta = document.getElementById(id);
                              if (!ta) return;

                              ta.scrollTop = ta.scrollHeight;
                              ta.selectionStart = ta.selectionEnd = ta.value.length;
                              ta.focus();
                            }

                            scrollTextareaToEnd("textAreaForLogs");

                            """
                        );
                    }
                    
                }
               
                
                
            }
            var partResponse = new FlexColumn(SizeFull, PaddingBottom(10))
            {
                new Label { Text = "Response as json" },

                new FreeScrollBar
                {
                    AutoHideScrollbar,

                    ComponentBoxShadow,
                    HeightFull,
                    Border("1px solid #d9d9d9"),
                    BorderRadius(5),
                    WidthFull,

                    NewJsonEditor(() => state.ScenarioList[scenarioIndex].ResponseAsJson)
                },
                
                partTrace
            };

            return new FlexColumn(SizeFull, PaddingRight(10))
            {
                new SplitColumn
                {
                    partEditors,
                    new FlexColumn(SizeFull, Gap(10))
                    {
                        partActionButtons,
                        partResponse
                    }
                }
            };
        }
    }

    static string GetDefaultRuntimeNameFromAssembly(string assemblyFileFullPath)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (fileInfo.Exists)
        {
            var targetRuntimeInfo = GetTargetFramework(fileInfo);
            if (targetRuntimeInfo.IsNetFramework || targetRuntimeInfo.IsNetStandard)
            {
                return RuntimeNames.NetFramework;
            }
        }

        return RuntimeNames.NetCore;
    }

    Task ClearActionButtonStates()
    {
        if (ExternalProcessManager.CurrentProcess is not null)
        {
            IgnoreException(() => ExternalProcessManager.CurrentProcess.Kill(entireProcessTree: true));

            ExternalProcessManager.CurrentProcess = null;

            if (ExternalProcessManager.CurrentProcessTask is not null)
            {
                IgnoreException(() => ExternalProcessManager.CurrentProcessTask.Dispose());

                ExternalProcessManager.CurrentProcessTask = null;
            }
        }

        AsyncLogger.logs.Clear();

        ExternalProcessManager.ResponseException = null;

        ExternalProcessManager.ResponseAsJson = null;

        DebugButtonStatus = ActionButtonStatus.Ready;

        ExecuteButtonStatus = ActionButtonStatus.Ready;

        SaveState();

        return Task.CompletedTask;
    }

    Task FormatDotNetInstanceText(MouseEvent _)
    {
        if (SelectedScenarioModel is null)
        {
            return Task.CompletedTask;
        }

        SelectedScenarioModel.JsonTextForDotNetInstanceProperties = JsonPrettify(SelectedScenarioModel.JsonTextForDotNetInstanceProperties);

        return Task.CompletedTask;
    }

    Task FormatDotNetParametersText(MouseEvent _)
    {
        if (SelectedScenarioModel is null)
        {
            return Task.CompletedTask;
        }

        SelectedScenarioModel.JsonTextForDotNetMethodParameters = JsonPrettify(SelectedScenarioModel.JsonTextForDotNetMethodParameters);

        return Task.CompletedTask;
    }

    Element GetEnvironment()
    {
        return new EnvironmentInfoView
        {
            AssemblyFileFullPath = AssemblyFileFullPath,

            RuntimeName = state.RuntimeName
        };
    }

    Task MonitorDebug()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (ExternalProcessManager.ResponseException is not null)
        {
            DebugButtonStatus = ActionButtonStatus.Fail;

            scenario.ResponseAsJson = ExternalProcessManager.ResponseException.Message;

            ExternalProcessManager.ResponseException = null;

            ExternalProcessManager.ResponseAsJson = null;

            Client.GotoMethod(2000, ClearActionButtonStates);

            return Task.CompletedTask;
        }

        if (ExternalProcessManager.ResponseAsJson is not null)
        {
            DebugButtonStatus = ActionButtonStatus.Success;

            scenario.ResponseAsJson = ExternalProcessManager.ResponseAsJson;

            ExternalProcessManager.ResponseException = null;

            ExternalProcessManager.ResponseAsJson = null;

            Client.GotoMethod(2000, ClearActionButtonStates);

            return Task.CompletedTask;
        }
        
        Client.GotoMethod(100, MonitorDebug);

        return Task.CompletedTask;
    }

    Task MonitorExecution()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (ExternalProcessManager.ResponseException is not null)
        {
            ExecuteButtonStatus = ActionButtonStatus.Fail;

            scenario.ResponseAsJson = ExternalProcessManager.ResponseException.Message;

            ExternalProcessManager.ResponseException = null;

            ExternalProcessManager.ResponseAsJson = null;

            Client.GotoMethod(2000, ClearActionButtonStates);

            return Task.CompletedTask;
        }

        if (ExternalProcessManager.ResponseAsJson is not null)
        {
            ExecuteButtonStatus = ActionButtonStatus.Success;

            scenario.ResponseAsJson = ExternalProcessManager.ResponseAsJson;

            ExternalProcessManager.ResponseException = null;

            ExternalProcessManager.ResponseAsJson = null;

            Client.GotoMethod(2000, ClearActionButtonStates);

            return Task.CompletedTask;
        }
        
        Client.GotoMethod(100, MonitorExecution);

        return Task.CompletedTask;
    }

    async Task OnDebugClicked(MouseEvent _)
    {
        if (DebugButtonStatus == ActionButtonStatus.Executing)
        {
            await ClearActionButtonStates();

            return;
        }

        await ClearActionButtonStates();

        await OnDebugClicked();
    }

    Task OnDebugClicked()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (state.SelectedMethod is null)
        {
            scenario.ResponseAsJson = "Please select any method from left side.";

            DebugButtonStatus = ActionButtonStatus.Ready;

            return Task.CompletedTask;
        }

        scenario.ResponseAsJson = null;

        if (DebugButtonStatus == ActionButtonStatus.Ready)
        {
            DebugButtonStatus = ActionButtonStatus.Executing;

            Client.GotoMethod(100, OnDebugClicked);
        }
        else if (DebugButtonStatus == ActionButtonStatus.Executing)
        {
            ExternalProcessManager.CurrentProcessTask = Task.Run(() =>
            {
                try
                {
                    var input = new ExternalInvokeInput
                    {
                        RuntimeName = state.RuntimeName,

                        AssemblyFileFullPath = AssemblyFileFullPath,

                        MethodReference = state.SelectedMethod,

                        JsonTextForDotNetInstanceProperties = scenario.JsonTextForDotNetInstanceProperties,

                        JsonTextForDotNetMethodParameters = scenario.JsonTextForDotNetMethodParameters,

                        WaitForDebugger = true,

                        OnProcessStarted = process => { ExternalProcessManager.CurrentProcess = process; }
                    };

                    External.InvokeMethod(input).Match
                    (
                        json => ExternalProcessManager.ResponseAsJson         = json,
                        exception => ExternalProcessManager.ResponseException = exception
                    );
                }
                catch (Exception exception)
                {
                    ExternalProcessManager.ResponseException = exception;
                }
            });

            Client.GotoMethod(MonitorDebug);
        }

        return Task.CompletedTask;
    }

    Task OnElementSelected(string keyOfSelectedTreeNode)
    {
        state.SelectedMethodTreeNodeKey = keyOfSelectedTreeNode;

        IsInitializingSelectedMethod = true;

        Client.GotoMethod(OnElementSelected);

        return Task.CompletedTask;
    }

    Task OnElementSelected()
    {
        IsInitializingSelectedMethod = false;

        state.SelectedMethod = null;

        state.ScenarioList              = ImmutableList<ScenarioModel>.Empty.Add(new());
        state.ScenarioListSelectedIndex = 0;

        var nodeResult = MethodSelectionView.FindTreeNode(AssemblyFileFullPath, state.SelectedMethodTreeNodeKey, state.ClassFilter, state.MethodFilter);
        if (nodeResult.HasError)
        {
            this.FailNotification(nodeResult.Error.Message);
        }

        var node = nodeResult.Value;

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

                    if (state.RuntimeName.HasNoValue())
                    {
                        state.RuntimeName = GetDefaultRuntimeNameFromAssembly(AssemblyFileFullPath);
                    }
                }
            }
        }

        TryInitializeDefaultJsonInputs();

        return Task.CompletedTask;
    }

    async Task OnExecuteClicked(MouseEvent _)
    {
        if (ExecuteButtonStatus == ActionButtonStatus.Executing)
        {
            await ClearActionButtonStates();

            return;
        }

        await ClearActionButtonStates();

        await OnExecuteClicked();
    }

    Task OnExecuteClicked()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (state.SelectedMethod is null)
        {
            scenario.ResponseAsJson = "Please select any method from left side.";

            ExecuteButtonStatus = ActionButtonStatus.Ready;

            return Task.CompletedTask;
        }

        scenario.ResponseAsJson = null;

        if (ExecuteButtonStatus == ActionButtonStatus.Ready)
        {
            ExecuteButtonStatus = ActionButtonStatus.Executing;

            Client.GotoMethod(100, OnExecuteClicked);
        }
        else if (ExecuteButtonStatus == ActionButtonStatus.Executing)
        {
            ExternalProcessManager.CurrentProcessTask = Task.Run(() =>
            {
                try
                {
                    var input = new ExternalInvokeInput
                    {
                        RuntimeName = state.RuntimeName,

                        AssemblyFileFullPath = AssemblyFileFullPath,

                        MethodReference = state.SelectedMethod,

                        JsonTextForDotNetInstanceProperties = scenario.JsonTextForDotNetInstanceProperties,

                        JsonTextForDotNetMethodParameters = scenario.JsonTextForDotNetMethodParameters,

                        WaitForDebugger = false,

                        OnProcessStarted = process => { ExternalProcessManager.CurrentProcess = process; }
                    };
                    External.InvokeMethod(input).Match
                    (
                        json => ExternalProcessManager.ResponseAsJson         = json,
                        exception => ExternalProcessManager.ResponseException = exception
                    );
                }
                catch (Exception exception)
                {
                    ExternalProcessManager.ResponseException = exception;
                }
            });

            Client.GotoMethod(MonitorExecution);
        }

        return Task.CompletedTask;
    }

    Task OnFilterTextKeypressCompleted()
    {
        return Task.CompletedTask;
    }

    Task OnScenarioFilterTextKeypressFinished()
    {
        return Task.CompletedTask;
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

    void TryInitializeDefaultJsonInputs()
    {
        if (state.SelectedMethod != null)
        {
            var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

            if (scenario.JsonTextForDotNetInstanceProperties.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                External.GetInstanceEditorJsonText(state.RuntimeName, AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties)
                    .Match(json => scenario.JsonTextForDotNetInstanceProperties = json, printError);
            }

            if (scenario.JsonTextForDotNetMethodParameters.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                External.GetParametersEditorJsonText(state.RuntimeName, AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetMethodParameters)
                    .Match(json => scenario.JsonTextForDotNetMethodParameters = json, printError);
            }

            void printError(Exception exception)
            {
                scenario.ResponseAsJson = exception + NewLine + scenario.ResponseAsJson;
            }
        }
    }

    class CircleButton : ReactPureComponent
    {
        public MouseEventHandler Clicked { get; init; }

        public int Index { get; init; }

        public bool IsSelected { get; init; }

        public string Label { get; init; }

        public string TooltipText { get; init; }

        public bool UseSearchIcon { get; init; }

        protected override Element render()
        {
            return ArrangeTooltip(new FlexRowCentered
            {
                Id(Index),
                ComponentBoxShadow,
                OnClick(Clicked),
                UseSearchIcon ? SearchIcon() : Label,

                Border(Solid(1, Theme.BorderColor)),
                BorderRadius("50%"),
                Size(30),
                CursorPointer,
                Hover(Border(Solid(1, "#b8b8ea"))),
                When(IsSelected, FontWeightExtraBold, Background(rgb(212, 212, 230)))
            });
        }

        static Element SearchIcon()
        {
            return new svg(ViewBox(0, 0, 24, 24), Fill(none), svg.Size(16), Stroke(Gray400))
            {
                new path
                {
                    fillRule = "evenodd",
                    clipRule = "evenodd",
                    d        = "M15 10.5C15 12.9853 12.9853 15 10.5 15C8.01472 15 6 12.9853 6 10.5C6 8.01472 8.01472 6 10.5 6C12.9853 6 15 8.01472 15 10.5ZM14.1793 15.2399C13.1632 16.0297 11.8865 16.5 10.5 16.5C7.18629 16.5 4.5 13.8137 4.5 10.5C4.5 7.18629 7.18629 4.5 10.5 4.5C13.8137 4.5 16.5 7.18629 16.5 10.5C16.5 11.8865 16.0297 13.1632 15.2399 14.1792L20.0304 18.9697L18.9697 20.0303L14.1793 15.2399Z",
                    fill     = Gray400
                }
            };
        }

        Element ArrangeTooltip(Element content)
        {
            if (TooltipText.HasValue())
            {
                return new Tooltip
                {
                    arrow = true,
                    title = TooltipText,
                    children =
                    {
                        content
                    }
                };
            }

            return content;
        }
    }

    class EnvironmentInfoState
    {
        public string AssemblyFileFullPath { get; set; }

        public string RuntimeName { get; set; }

        public string Text { get; set; }
    }

    class EnvironmentInfoView : Component<EnvironmentInfoState>
    {
        public required string AssemblyFileFullPath { get; init; }

        public required string RuntimeName { get; init; }

        protected override Task constructor()
        {
            state = new();

            Client.ListenEvent<AssemblyChangedArgs>(Event.OnAssemblyChanged, OnAssemblyChanged);

            return base.constructor();
        }

        protected override Task OverrideStateFromPropsBeforeRender()
        {
            if (state.AssemblyFileFullPath != AssemblyFileFullPath || state.RuntimeName != RuntimeName)
            {
                state.AssemblyFileFullPath = AssemblyFileFullPath;

                state.RuntimeName = RuntimeName;

                External.GetEnvironment(state.RuntimeName, AssemblyFileFullPath).Match
                (
                    x => state.Text = x,
                    _ => state.Text = null
                );
            }

            return base.OverrideStateFromPropsBeforeRender();
        }

        protected override Element render()
        {
            return new FlexRowCentered
            {
                state.Text
            };
        }

        Task OnAssemblyChanged(AssemblyChangedArgs args)
        {
            state.AssemblyFileFullPath = args.AssemblyFileFullPath;

            state.RuntimeName = args.RuntimeName;

            External.GetEnvironment(args.RuntimeName, args.AssemblyFileFullPath).Match
            (
                x => state.Text = x,
                _ => state.Text = null
            );

            return Task.CompletedTask;
        }
    }
}

enum Event
{
    OnAssemblyChanged
}

sealed record AssemblyChangedArgs
{
    public string AssemblyFileFullPath { get; init; }

    public string RuntimeName { get; init; }
}