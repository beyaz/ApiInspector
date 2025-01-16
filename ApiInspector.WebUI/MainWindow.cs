using System.Collections.Immutable;
using System.Linq.Expressions;
using ApiInspector.WebUI.Components;
using ReactWithDotNet.ThirdPartyLibraries.MonacoEditorReact;
using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;
using static System.Environment;

namespace ApiInspector.WebUI;

class MainWindow : Component<MainWindowModel>
{
    public bool DebugButtonStatusIsFail { get; set; }

    public bool DebugButtonStatusIsSuccess { get; set; }
    public bool ExecuteButtonStatusIsFail { get; set; }

    public bool ExecuteButtonStatusIsSuccess { get; set; }

    public bool HistoryDialogVisible { get; set; }
    public bool IsDebugStarted { get; set; }
    public bool IsExecutionStarted { get; set; }

    public bool IsInitializingSelectedMethod { get; set; }

    string AssemblyFileFullPath => Path.Combine(state.AssemblyDirectory, state.AssemblyFileName);

    protected override Task constructor()
    {
        state = StateCache.ReadState() ?? new MainWindowModel
        {
            AssemblyDirectory = Path.GetDirectoryName(DotNetFrameworkInvokerExePath),
            AssemblyFileName  = "ApiInspector.exe",
            MethodFilter      = "GetHelpMessage"
        };

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
                } + When(!hasMatch(scenario, state.ScenarioFilterText), DisplayNone) ),

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
                } + PositionAbsolute+ Right(10)+ Top(-13)+ ComponentBoxShadow,

                new FlexColumn
                {
                    new Label { Text = "Assembly Directory" },

                    new DirectorySelector
                    {
                        DirectoryPath = state.AssemblyDirectory,
                        SelectionChanged = x =>
                        {
                            state.AssemblyDirectory = x;
                            Client.DispatchEvent(Event.OnAssemblyChanged, [AssemblyFileFullPath]);

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

                            Client.DispatchEvent(Event.OnAssemblyChanged, [AssemblyFileFullPath]);

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
                    lineNumbers         = false
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
                        
                state.ScenarioList.Count < 3 ? null:
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
            };

            var partResponse = new FlexColumn(SizeFull)
            {
                new Label { Text = "Response as json" },

                new FreeScrollBar
                {
                    AutoHideScrollbar,

                    ComponentBoxShadow,
                    Height("calc(100% - 28px)"), PaddingBottom(10),
                    Border("1px solid #d9d9d9"),
                    BorderRadius(5),
                    WidthFull,

                    NewJsonEditor(() => state.ScenarioList[scenarioIndex].ResponseAsJson)
                }
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

    Task ClearActionButtonStates()
    {
        DebugButtonStatusIsFail    = false;
        DebugButtonStatusIsSuccess = false;

        ExecuteButtonStatusIsFail    = false;
        ExecuteButtonStatusIsSuccess = false;

        return Task.CompletedTask;
    }

    Element GetEnvironment()
    {
        return new EnvironmentInfoView
        {
            AssemblyFileFullPath = AssemblyFileFullPath
        };
    }

    Task OnDebugClicked(MouseEvent _) => OnDebugClicked();
    
    async Task OnDebugClicked()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (state.SelectedMethod is null)
        {
            scenario.ResponseAsJson = "Please select any method from left side.";
            return;
        }

        SaveState();

        scenario.ResponseAsJson = null;

        await ClearActionButtonStates();

        if (IsDebugStarted)
        {
            IsDebugStarted = false;

            try
            {
                scenario.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties, scenario.JsonTextForDotNetMethodParameters, true).Unwrap();

                DebugButtonStatusIsSuccess = true;
            }
            catch (Exception exception)
            {
                DebugButtonStatusIsFail = true;

                scenario.ResponseAsJson = exception.Message;
            }

            Client.GotoMethod(2000, ClearActionButtonStates);
        }
        else
        {
            IsDebugStarted = true;
            Client.GotoMethod(100, OnDebugClicked);
        }
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
        if (nodeResult.Fail)
        {
            this.FailNotification(nodeResult.InfoCollection);
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
                }
            }
        }

        TryInitializeDefaultJsonInputs();
        
        return Task.CompletedTask;
    }

    Task OnExecuteClicked(MouseEvent _) => OnExecuteClicked();
    
    async Task OnExecuteClicked()
    {
        var scenario = state.ScenarioList[state.ScenarioListSelectedIndex];

        if (state.SelectedMethod is null)
        {
            scenario.ResponseAsJson = "Please select any method from left side.";
            return;
        }

        SaveState();

        scenario.ResponseAsJson = null;

        await ClearActionButtonStates();

        if (IsExecutionStarted)
        {
            IsExecutionStarted = false;

            try
            {
                scenario.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties, scenario.JsonTextForDotNetMethodParameters, false).Unwrap();

                ExecuteButtonStatusIsSuccess = true;
            }
            catch (Exception exception)
            {
                scenario.ResponseAsJson = exception.Message;

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
                External.GetInstanceEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties)
                    .Then(json => scenario.JsonTextForDotNetInstanceProperties = json, printError);
            }

            if (scenario.JsonTextForDotNetMethodParameters.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                External.GetParametersEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetMethodParameters)
                    .Then(json => scenario.JsonTextForDotNetMethodParameters = json, printError);
            }

            void printError(Exception exception)
            {
                scenario.ResponseAsJson = exception + NewLine + scenario.ResponseAsJson;
            }
        }
    }

    class CircleButton : ReactPureComponent
    {
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
        public string Text { get; set; }
    }

    class EnvironmentInfoView : Component<EnvironmentInfoState>
    {
        public string AssemblyFileFullPath { get; init; }

        protected override Task constructor()
        {
            state = new();

            OnAssemblyChanged(AssemblyFileFullPath);

            Client.ListenEvent<string>(Event.OnAssemblyChanged, OnAssemblyChanged);

            return base.constructor();
        }

        protected override Task OverrideStateFromPropsBeforeRender()
        {
            if (state.AssemblyFileFullPath != AssemblyFileFullPath)
            {
                state.AssemblyFileFullPath = AssemblyFileFullPath;
                state.Text                 = Flow(AssemblyFileFullPath, External.GetEnvironment, str => str);
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

        Task OnAssemblyChanged(string assemblyFileFullPath)
        {
            state.AssemblyFileFullPath = assemblyFileFullPath;

            Flow(assemblyFileFullPath, External.GetEnvironment, str => state.Text = str);

            return Task.CompletedTask;
        }
    }
}

enum Event
{
    OnAssemblyChanged
}