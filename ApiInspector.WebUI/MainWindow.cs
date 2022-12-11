using ApiInspector.WebUI.Components;
using ReactWithDotNet.Libraries.PrimeReact;
using ReactWithDotNet.Libraries.react_free_scrollbar;
using ReactWithDotNet.Libraries.ReactSuite;
using ReactWithDotNet.Libraries.uiw.react_codemirror;

namespace ApiInspector.WebUI;

class MainWindowModel
{
    public string AssemblyDirectory { get; set; } = @"D:\work\git\ApiInspector\ApiInspector\bin\Debug\";
    public string AssemblyFileName { get; set; } = @"ApiInspector.exe";

    public string SelectedMethodTreeFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string ResponseAsJson { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }
    public MethodReference SelectedMethod { get; set; }
    public bool IsInstanceEditorActive { get; set; }
}
class MainWindow: ReactComponent<MainWindowModel>
{
    protected override Element render()
    {
        ArrangeEditors();
        
        const int width = 500;

        var borderColor = "#d5d5d8";
        
        return new FlexRow(Padding(10), PositionAbsolute, Top(0),Bottom(0),Left(0),Right(0), Background("#eff3f8"))
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
                   
                   new FlexRow(HeightMaximized,BorderRight($"1px solid {borderColor}"))
                   {
                       new FlexColumn(Width(700),Gap(10), HeightMaximized, Margin(10),MarginTop(20))
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
                                   new InputText{ valueBind = ()=>state.AssemblyFileName, style = { FlexGrow(1) }},

                                   new RefreshButton{  }
                               }
                           },

                           new MethodSelectionView
                           {
                               
                               Filter                    = state.SelectedMethodTreeFilter,
                               SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                               SelectionChanged          = OnElementSelected,
                               AssemblyFilePath          = state.AssemblyDirectory+state.AssemblyFileName,
                               //Width                     = width
                           } + HeightMaximized + OverflowYAuto + MarginBottom(10)


                       },
                       
                       new FlexColumn(FlexGrow(1))
                       {
                           new FlexColumn(FlexGrow(1))
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
                               
                           }
                           
                           ,new FlexColumn(FlexGrow(1), Gap(10))
                            {
                                new FlexRow(Height(50), Gap(30))
                                {
                                    new ExecuteButton{Click = OnExecuteClicked},
                                    new DebugButton{Click   = OnDebugClicked},
                                },

                               new FlexColumn(WidthHeightMaximized)
                               {
                                   new Label{Text ="Response as json"},
                                   
                                  

                                   new FreeScrollBar
                                   {
                                        Height(210), PaddingBottom(10),
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

    void OnExecuteClicked()
    {
        var fullAssemblyPath = state.AssemblyDirectory + state.AssemblyFileName;
        
        state.ResponseAsJson = External.InvokeMethod(fullAssemblyPath,state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters, false);
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
            var (jsonForInstance, jsonForParameters) = External.GetEditorTexts(fullAssemblyPath, state.SelectedMethod, state.JsonTextForDotNetInstanceProperties, state.JsonTextForDotNetMethodParameters);

            state.JsonTextForDotNetInstanceProperties = jsonForInstance;
            state.JsonTextForDotNetMethodParameters   = jsonForParameters;

            ArrangeEditors();
        }
        
        
    }

    void ArrangeEditors()
    {
        var lines            = (state.JsonTextForDotNetInstanceProperties + string.Empty).Split(Environment.NewLine);
        
        var optimumLineCount = 19;
        if (lines.Length < optimumLineCount)
        {
            state.JsonTextForDotNetInstanceProperties += string.Join(Environment.NewLine, Enumerable.Range(0, optimumLineCount - lines.Length).Select(x => string.Empty));
        }

        lines = (state.JsonTextForDotNetMethodParameters + string.Empty).Split(Environment.NewLine);
        if (lines.Length < optimumLineCount)
        {
            state.JsonTextForDotNetMethodParameters += string.Join(Environment.NewLine, Enumerable.Range(0, optimumLineCount - lines.Length).Select(x => string.Empty));
        }

        lines = (state.ResponseAsJson + string.Empty).Split(Environment.NewLine);
        if (lines.Length < 13)
        {
            state.ResponseAsJson += string.Join(Environment.NewLine, Enumerable.Range(0, 13 - lines.Length).Select(x => string.Empty));
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