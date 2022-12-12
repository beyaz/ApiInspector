﻿using ApiInspector.WebUI.Components;
using ReactWithDotNet.Libraries.PrimeReact;
using ReactWithDotNet.Libraries.react_free_scrollbar;
using ReactWithDotNet.Libraries.uiw.react_codemirror;

namespace ApiInspector.WebUI;

class MainWindowModel
{
    public string AssemblyDirectory { get; set; } = @"d:\boa\server\bin\";
    public string AssemblyFileName { get; set; } = @"BOA.Process.Kernel.DebitCard.dll";

    public string SelectedMethodTreeFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string ResponseAsJson { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }
    public MethodReference SelectedMethod { get; set; }
    public bool IsInstanceEditorActive { get; set; }
    public string AAA { get; set; }
}
class MainWindow: ReactComponent<MainWindowModel>
{
    protected override Element render()
    {
        ArrangeEditors();

        var borderColor = "#d5d5d8";
        
        return new FlexRow(Padding(10), WidthHeightMaximized, Background("#eff3f8"))
        {
            new style
            {
                @"
.ͼ1.cm-editor.cm-focused {
    outline: none;
}"
            },
            new FlexColumn(Border($"1px solid {borderColor}"), WidthHeightMaximized, Background("white"))
            {
                new link { rel = "stylesheet", href = "https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/themes/saga-blue/theme.css" },
                new link { rel = "stylesheet", href = "https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/primereact.min.css" },
                new link { rel = "stylesheet", href = "https://cdn.jsdelivr.net/npm/primeicons@5.0.0/primeicons.css" },

                
                   new FlexRow(PaddingLeftRight(30), PaddingTopBottom(5), BorderBottom($"1px solid {borderColor}"))
                   {
                       new FlexRow(Gap(5))
                       {
                           AlignItemsCenter,
                           new h3{"Api Inspector"}, new h5{ " (.net method invoker) " }
                       }
                   } ,
                   
                   new FlexRow(HeightMaximized)
                   {
                       new FlexColumn(Width(500),Gap(10), Margin(10),MarginTop(20))
                       {
                           new FlexColumn(MarginLeftRight(3))
                           {
                               new Label{Text           ="Assembly Directory"},
                               new InputText{ valueBind = ()=>state.AssemblyDirectory} 
                           },

                           new FlexColumn(PaddingLeftRight(3))
                           {
                               new Label{Text="Assembly"},
                               
                               new FlexRow(WidthMaximized, Gap(3))
                               {
                                   new AssemblySelector
                                   {
                                       AssemblyDirectoryPath = state.AssemblyDirectory,
                                       AssemblyFileName = state.AssemblyFileName,
                                       SelectionChanged = x=>state.AssemblyFileName=x
                                   }
                               }
                           },

                           new MethodSelectionView
                           {
                               
                               Filter                    = state.SelectedMethodTreeFilter,
                               SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                               SelectionChanged          = OnElementSelected,
                               AssemblyFilePath          = state.AssemblyDirectory+state.AssemblyFileName,
                           }


                       },
                       
                       new FlexColumn(FlexGrow(1),Gap(10), MarginRight(10))
                       {
                           new FlexColumn
                           {
                               // h e a d e r
                               new FlexRow(CursorPointer, TextAlignCenter)
                               {
                                   new Label{Text ="Instance json",style =
                                   {
                                       Padding(10),
                                       FlexGrow(1)
                                   }},

                                   new Label{Text ="Parameters json",style =
                                   {
                                       Padding(10),
                                       FlexGrow(1)
                                   }}
                               },
                
                               // c o n t e n t
                               new FlexRow(WidthHeightMaximized, Gap(5))
                               {
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
                               
                           },
                           new FlexColumn(FlexGrow(1), Gap(10))
                            {
                                new FlexRow(Height(50), Gap(30))
                                {
                                    new ExecuteButton{Click = OnExecuteClicked, IsProcessing = IsInvocationStarted},
                                    new DebugButton{Click   = OnDebugClicked},
                                },

                               new FlexColumn(WidthHeightMaximized)
                               {
                                   new Label{Text ="Response as json"},
                                   
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
                       }
                       
                   }
            }
        };
        
    }

    public bool IsInvocationStarted { get; set; }
    void OnExecuteClicked()
    {
        if (IsInvocationStarted)
        {
            IsInvocationStarted = false;
            
            var fullAssemblyPath = state.AssemblyDirectory + state.AssemblyFileName;

            state.ResponseAsJson = External.InvokeMethod(fullAssemblyPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters, false);
        }
        else
        {
            IsInvocationStarted = true;
            Client.GotoMethod(100, OnExecuteClicked);
        }
        
    }

    void OnDebugClicked()
    {
        var fullAssemblyPath = state.AssemblyDirectory + state.AssemblyFileName;

        state.ResponseAsJson = External.InvokeMethod(fullAssemblyPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters,true);
    }

    void OnElementSelected((string value, string filter) e)
    {
        SaveState();

        state.SelectedMethod = null;

        state.JsonTextForDotNetInstanceProperties = null;
        state.JsonTextForDotNetMethodParameters = null;

        state.SelectedMethodTreeNodeKey = e.value;
        state.SelectedMethodTreeFilter = e.filter;

        var fullAssemblyPath = state.AssemblyDirectory + state.AssemblyFileName;

        var node = MethodSelectionView.FindTreeNode(fullAssemblyPath, state.SelectedMethodTreeNodeKey);
        if (node is not null)
        {
            if (node.IsMethod)
            {
                state.SelectedMethod = node.MethodReference;

                //state = StateCache.TryRead(state.SelectedMethod) ?? state;
            }
        }

        if (canShowInstanceEditor() && canShowParametersEditor() == false)
        {
            state.IsInstanceEditorActive = true;
        }

        if (canShowParametersEditor() && canShowInstanceEditor() == false)
        {
            state.IsInstanceEditorActive = false;
        }

        if (state.SelectedMethod != null)
        {
            try
            {

                var (jsonForInstance, jsonForParameters) = External.GetEditorTexts(fullAssemblyPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters);

                state.JsonTextForDotNetInstanceProperties = jsonForInstance;
                state.JsonTextForDotNetMethodParameters   = jsonForParameters;

            }
            catch (Exception exception)
            {
                state.ResponseAsJson = exception.ToString();
            }
            
            ArrangeEditors();
        }
        
        
    }

    void ArrangeEditors()
    {
        var lines            = (state.JsonTextForDotNetInstanceProperties + string.Empty).Split(Environment.NewLine);
        
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

    void SaveState()
    {
        
    }

    bool canShowInstanceEditor()
    {
        if (state.SelectedMethod?.IsStatic == true)
        {
            return false;
        }

        return true;
    }

    bool canShowParametersEditor()
    {
        if (state.SelectedMethod?.Parameters.Count > 0)
        {
            return true;
        }

        return false;
    }
}