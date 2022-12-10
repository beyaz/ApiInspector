using ReactWithDotNet.Libraries.PrimeReact;
using ReactWithDotNet.Libraries.ReactSuite;
using ReactWithDotNet.Libraries.uiw.react_codemirror;

namespace ApiInspector.WebUI;

class MainWindowModel
{
    public string AssemblyDirectory { get; set; } = @"d:\boa\server\bin\";
    public string AssemblyFileName { get; set; } = @"BOA.Process.Kernel.DebitCard.dll";

    public string SelectedMethodTreeFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }
    public MethodReference SelectedMethod { get; set; }
    public bool IsInstanceEditorActive { get; set; }
}
class MainWindow: ReactComponent<MainWindowModel>
{
    protected override Element render()
    {
        const int width = 500;

        var borderColor = "#d5d5d8";
        
        return new FlexRow(Padding(10), PositionAbsolute, Top(0),Bottom(0),Left(0),Right(0), Background("#eff3f8"))
        {
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
                       new FlexColumn(Width(300), HeightMaximized)
                       {
                           "LeftMenu",
                           new FlexColumn
                           {
                               "Assembly Directory",
                               new InputText{ valueBind = ()=>state.AssemblyDirectory}
                           },

                           new FlexColumn
                           {
                               "Assembly",
                               new FlexRow
                               {
                                   new InputText{ valueBind = ()=>state.AssemblyFileName, style = { FlexGrow(1)}}, new Button{label = "Refresh"}
                               }
                           },

                           new MethodSelectionView
                           {
                               Filter                    = state.SelectedMethodTreeFilter,
                               SelectedMethodTreeNodeKey = state.SelectedMethodTreeNodeKey,
                               SelectionChanged          = OnElementSelected,
                               AssemblyFilePath          = state.AssemblyDirectory+state.AssemblyFileName,
                               Width                     = width
                           }

                           
                       },
                       
                       new FlexColumn(FlexGrow(1))
                       {
                           new FlexColumn(FlexGrow(1), BorderBottom($"1px solid {borderColor}"))
                           {
                               // h e a d e r
                               new FlexRow(Color("#6c757d"), CursorPointer, TextAlignCenter)
                               {
                                   new div(Text("Instance json"))
                                   {
                                       FontWeight600,
                                       Padding(10),
                                       FlexGrow(1),
                                       FontSize13
                                   },

                                   new div(Text("Parameters json"))
                                   {
                                       FontWeight600,
                                       Padding(10),
                                       FlexGrow(1),
                                       FontSize13
                                   }
                               },
                
                               // c o n t e n t
                               new FlexRow(WidthHeightMaximized, Gap(5))
                               {
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
                                           Border("1px solid #d9d9d9"),
                                           FontSize11,
                                           WidthMaximized
                                       }
                                   },
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
                                           Border("1px solid #d9d9d9"),
                                           FontSize11,
                                           WidthMaximized
                                       }
                                   }
                               }
                               
                           }
                           
                           ,new FlexRowCentered(FlexGrow(1))
                            {
                                "response"
                            }
                       }
                       
                   }
            }
        };
        
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