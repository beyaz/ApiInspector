using System.Collections.Immutable;
using System.IO;
using System.Linq.Expressions;
using ApiInspector.WebUI.Components;
using ReactWithDotNet.ThirdPartyLibraries.MonacoEditorReact;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;
using static System.Environment;

namespace ApiInspector.WebUI;

class MainWindow : Component<MainWindowModel>
{
    const string borderColor = "#d5d5d8";
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
        return new FlexRow(Padding(10), WidthHeightMaximized, Background("#eff3f8"))
        {
            new FlexColumn(Border(Solid(1, borderColor)),
                           WidthHeightMaximized,
                           Background(rgba(255, 255, 255, 0.4)),
                           BorderRadius(10),
                           BoxShadow(0, 30, 30, 0, rgba(69, 42, 124, 0.15)),
                           BackdropFilterBlur(30)
                          )
            {
                applicationHeader,

                HistoryDialogVisible
                    ? new FlexColumnCentered(HeightMaximized)
                    {
                        new HistoryPanel
                        {
                            Closed = ()=>
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
                        
                    }
                    : new FlexRow(HeightMaximized)
                    {
                        searchPanel,
                        addRemovePanel,
                        ActiveSelectedMethod
                    }
            }
        };

        Element applicationHeader()
        {
            return new FlexRow(PaddingLeftRight(30), PaddingTopBottom(5), BorderBottom(Solid(1, borderColor)))
            {
                JustifyContentSpaceBetween,
                AlignItemsCenter,

                new FlexRow(Gap(5))
                {
                    AlignItemsCenter,
                    new h3 { "Api Inspector" }, new h5 { " (.net method invoker) ", MarginTop(5) }
                },

                new FlexRow(Gap(20))
                {
                    GetEnvironment,
                    new LogoutButton()
                }
            };
        }
        Element addRemovePanel()
        {
            return new FlexColumn(Width(30), PaddingRight(10), Gap(10), JustifyContentFlexStart, AlignItemsCenter, PaddingTopBottom(10))
            {
                state.ScenarioList.Select((_, i) => new CircleButton
                {
                    Index      = i,
                    Label      = i.ToString(),
                    IsSelected = i == state.ScenarioListSelectedIndex,
                    Clicked    = e =>
                    {
                        state.ScenarioListSelectedIndex = Convert.ToInt32(e.FirstNotEmptyId);
                        return Task.CompletedTask;
                    }
                }),

                new CircleButton
                {
                    Label = "+", Clicked = _ =>
                    {
                        state.ScenarioList              = state.ScenarioList.Add(new ScenarioModel());
                        state.ScenarioListSelectedIndex = state.ScenarioList.Count - 1;
                        TryInitializeDefaultJsonInputs();
                        
                        return Task.CompletedTask;
                    }
                },
                When(state.ScenarioList.Count > 1, new CircleButton
                {
                    Label = "-", Clicked = _ =>
                    {
                        state.ScenarioList              = state.ScenarioList.RemoveAt(state.ScenarioListSelectedIndex);
                        state.ScenarioListSelectedIndex = state.ScenarioList.Count - 1;
                        
                        return Task.CompletedTask;
                    }
                })
            };
        }

        Element searchPanel()
        {
            return new FlexColumn(Width(500), Gap(10), Padding(10), MarginTop(20), PositionRelative)
            {
                new HistoryButton
                {
                    Click = _ =>
                    {
                        HistoryDialogVisible = true;
                        
                        return Task.CompletedTask;
                    },
                    style = { PositionAbsolute, Right(10), Top(-13), ComponentBoxShadow }
                },

                new FlexColumn
                {
                    new Label { Text = "Assembly Directory" },

                    new DirectorySelector
                    {
                        DirectoryPath = state.AssemblyDirectory,
                        SelectionChanged = x =>
                        {
                            state.AssemblyDirectory = x;
                            Client.OnAssemblyChanged(AssemblyFileFullPath);
                            
                            return Task.CompletedTask;
                        },
                        style = {ComponentBoxShadow, FontSize12, Padding(8), Border(Solid(1,"#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057") }
                    }
                },

                new FlexColumn
                {
                    new Label { Text = "Assembly" },

                    new AssemblySelector
                    {
                        AssemblyDirectoryPath = state.AssemblyDirectory,
                        AssemblyFileName      = state.AssemblyFileName,
                        SelectionChanged = x =>
                        {
                            state.AssemblyFileName = x;
                            Client.OnAssemblyChanged(AssemblyFileFullPath);
                            
                            return Task.CompletedTask;
                        },
                        style = { ComponentBoxShadow }
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by class name" },
                    
                    new input
                    {
                        type                     ="text",
                        valueBind                = () => state.ClassFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = {ComponentBoxShadow, FontSize12, Padding(8), Border(Solid(1,"#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057") }
                    }
                },
                new FlexColumn
                {
                    new Label { Text = "Filter by method name" },
                    
                    new input
                    {
                        type                     ="text",
                        valueBind                = () => state.MethodFilter,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                        style                    = {ComponentBoxShadow, FontSize12, Padding(8), Border(Solid(1,"#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057") }
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
                    lineNumbers = false
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

            var scenarioIndex = state.ScenarioListSelectedIndex;

            return new FlexColumn(FlexGrow(1), Gap(10), PaddingRight(10))
            {
                new FlexRow(WidthMaximized)
                {
                    ComponentBoxShadow,
                    MarginTop(5),
                    BorderRadius(5),

                    new FlexColumn(AlignItemsCenter, FlexGrow(1), When(state.SelectedMethod?.IsStatic == true, DisplayNone))
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
                            AutoHideScrollbar,

                            Height(300), PaddingBottom(10),
                            BorderTop(Solid(1, "#d9d9d9")),
                            BorderBottomLeftRadius(3),
                            WidthMaximized,
                            FlexGrow(1),

                            NewJsonEditor(() => state.ScenarioList[scenarioIndex].JsonTextForDotNetInstanceProperties)
                        }
                    },

                    new FlexColumn(AlignItemsCenter,FlexGrow(1))
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
                            AutoHideScrollbar,

                            Height(300), PaddingBottom(10),
                            BorderTop(Solid(1, "#d9d9d9")),
                            BorderBottomRightRadius(3),
                            WidthMaximized,
                            FlexGrow(1),

                            NewJsonEditor(() => state.ScenarioList[scenarioIndex].JsonTextForDotNetMethodParameters)
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
                            AutoHideScrollbar,

                            ComponentBoxShadow,
                            Height("calc(100% - 28px)"), PaddingBottom(10),
                            Border("1px solid #d9d9d9"),
                            BorderRadius(5),
                            WidthMaximized,

                            NewJsonEditor(() => state.ScenarioList[scenarioIndex].ResponseAsJson)
                        }
                    }
                }
            };
        }
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
                scenario.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties, scenario.JsonTextForDotNetMethodParameters, true);

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

        state.ScenarioList              = ImmutableList<ScenarioModel>.Empty.Add(new ScenarioModel());
        state.ScenarioListSelectedIndex = 0;

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

        TryInitializeDefaultJsonInputs();
        return Task.CompletedTask;
    }

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
                scenario.ResponseAsJson = External.InvokeMethod(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties, scenario.JsonTextForDotNetMethodParameters, false);

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
                SafeInvoke(() => External.GetInstanceEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetInstanceProperties))
                    .Then(json => scenario.JsonTextForDotNetInstanceProperties = json, printError);
            }

            if (scenario.JsonTextForDotNetMethodParameters.IsNullOrWhiteSpaceOrEmptyJsonObject())
            {
                SafeInvoke(() => External.GetParametersEditorJsonText(AssemblyFileFullPath, state.SelectedMethod, scenario.JsonTextForDotNetMethodParameters))
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
        public MouseEventHandler Clicked { get; set; }
        public int Index { get; set; }
        public bool IsSelected { get; set; }
        public string Label { get; set; }

        protected override Element render()
        {
            return new FlexRowCentered
            {
                Id(Index),
                ComponentBoxShadow,
                OnClick(Clicked),
                Label,
                Border(Solid(1, borderColor)),
                BorderRadius("50%"),
                WidthHeight(30),
                CursorPointer,
                Hover(Border(Solid(1, "#b8b8ea"))),
                When(IsSelected, FontWeightExtraBold, Background(rgb(212, 212, 230)))
            };
        }
    }

    class EnvironmentInfoState
    {
        public string AssemblyFileFullPath { get; set; }
        public string Text { get; set; }
    }

    class EnvironmentInfoView : Component<EnvironmentInfoState>
    {
        public string AssemblyFileFullPath { get; set; }

        protected override Task constructor()
        {
            state = new EnvironmentInfoState();

            OnAssemblyChanged(AssemblyFileFullPath);

            Client.OnAssemblyChanged(OnAssemblyChanged);

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

static class EventExtensions
{
    public static void OnAssemblyChanged(this Client client, string assemblyFileFullPath)
    {
        client.DispatchEvent(nameof(OnAssemblyChanged), assemblyFileFullPath);
    }

    public static void OnAssemblyChanged(this Client client, Func<string,Task> handlerAction)
    {
        client.ListenEvent(OnAssemblyChanged, handlerAction);
    }
}