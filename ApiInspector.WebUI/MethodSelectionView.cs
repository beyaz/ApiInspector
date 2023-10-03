using System.IO;
using System.Threading.Tasks;

namespace ApiInspector.WebUI;

[Serializable]
public sealed class MetadataNode 
{
    public List<MetadataNode> children { get; } = new();
    public bool IsClass { get; set; }
    public bool IsMethod { get; set; }
    public bool IsNamespace { get; set; }
    public MethodReference MethodReference { get; set; }

    public string NamespaceReference { get; set; }
    public TypeReference TypeReference { get; set; }
    
    public string label { get; set; }
}

[Serializable]
public sealed class MethodSelectionViewState
{
    public string AssemblyFilePath { get; set; }

    public string ClassFilter { get; set; }

    public string MethodFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }
    
    public IReadOnlyList<MetadataNode> Nodes{ get; set; }
}

class MethodSelectionView : ReactComponent<MethodSelectionViewState>
{
    
    public static MetadataNode FindTreeNode(string AssemblyFilePath, string treeNodeKey, string classFilter, string methodFilter)
    {
        if (string.IsNullOrWhiteSpace(AssemblyFilePath) || string.IsNullOrWhiteSpace(treeNodeKey))
        {
            return null;
        }

        if (!File.Exists(AssemblyFilePath))
        {
            return null;
        }

        var nodes = External.GetMetadataNodes(AssemblyFilePath, classFilter, methodFilter).ToArray();

        return FindTreeNode(nodes, x => HasMatch(x, treeNodeKey));
    }
    
    
    public string AssemblyFilePath { get; set; }

    public string ClassFilter { get; set; }

    public string MethodFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    protected override Task constructor()
    {
        state = new MethodSelectionViewState
        {
            AssemblyFilePath = AssemblyFilePath,
            ClassFilter      = ClassFilter,
            MethodFilter     = MethodFilter,
            SelectedMethodTreeNodeKey = SelectedMethodTreeNodeKey
        };


        state.Nodes = FetchNodes(state.AssemblyFilePath, state.ClassFilter, state.MethodFilter);
        
        return Task.CompletedTask;
    }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }

    static MetadataNode FindTreeNode(IEnumerable<MetadataNode> nodes, Func<MetadataNode, bool> hasMatch)
    {
        if (nodes == null)
        {
            return null;
        }

        foreach (var childNode in nodes)
        {
            var found = FindTreeNode(childNode, hasMatch);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    static MetadataNode FindTreeNode(MetadataNode node, Func<MetadataNode, bool> hasMatch)
    {
        if (node == null)
        {
            return null;
        }

        if (hasMatch(node))
        {
            return node;
        }

        return FindTreeNode(node.children, hasMatch);
    }

    static bool HasMatch(MetadataNode node, string treeNodeKey)
    {
        if (node.IsClass)
        {
            return node.TypeReference.FullName == treeNodeKey;
        }

        if (node.IsMethod)
        {
            return node.MethodReference.MetadataToken.ToString() == treeNodeKey;
        }

        return false;
    }

    Element AsTreeItem(MetadataNode node)
    {
        if (node.IsMethod)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Method")), wh(11), mt(5), ml(20) },

                new div { Text(node.label), MarginLeft(5), FontSize13 },

                Id(node.MethodReference.MetadataToken),

                arrangeBackground
            };
        }

        if (node.IsClass)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Class")), wh(14), ml(10) },

                new div { Text(node.label), MarginLeft(5), FontSize13 }
            };
        }

        if (node.IsNamespace)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Namespace")), wh(14) },

                new div { Text(node.label), MarginLeft(5), FontSize13 }
            };
        }

        return new div();

        void arrangeBackground(HtmlElement el)
        {
            el += new[]
            {
                Hover(BackgroundImage(linear_gradient(90, "#d5d5c1", "whitesmoke")), BorderRadius(3)),

                When(HasMatch(node, state.SelectedMethodTreeNodeKey), BackgroundImage(linear_gradient(90, "#d1d1c8", "whitesmoke")), BorderRadius(3))
            };

            el.onClick = OnTreeItemClicked;
        }
    }

    Element AsTreeView(IReadOnlyList<MetadataNode> nodes)
    {
        return new Fragment
        {
            nodes.Select(toItem)
        };

        Element toItem(MetadataNode node)
        {
            if (node.children?.Count > 0)
            {
                var parent = AsTreeItem(node);
                var chldrn = AsTreeView(node.children);

                return new Fragment
                {
                    parent + OnClick(OnTreeItemClicked),
                    chldrn
                };
            }

            return AsTreeItem(node);
        }
    }

    protected override Task OverrideStateFromPropsBeforeRender()
    {
        
        if (state.AssemblyFilePath != AssemblyFilePath||
            state.ClassFilter != ClassFilter||
            state.MethodFilter != MethodFilter||
            state.SelectedMethodTreeNodeKey != SelectedMethodTreeNodeKey)
        {
            state.AssemblyFilePath          = AssemblyFilePath;
            state.ClassFilter               = ClassFilter;
            state.MethodFilter              = MethodFilter;
            state.SelectedMethodTreeNodeKey = SelectedMethodTreeNodeKey;
            state.Nodes                     = FetchNodes(AssemblyFilePath, ClassFilter, MethodFilter);
        }

        return Task.CompletedTask;
    }
    
   
    
    protected override Element render()
    {
        var nodes = state.Nodes;
        
        return new div(MarginLeftRight(3), OverflowYScroll, CursorPointer, Padding(5), Border(Solid(1, "rgb(217, 217, 217)")), BorderRadius(3))
        {
            AsTreeView(nodes),
            WidthMaximized, HeightMaximized
        };
    }

    
    static IReadOnlyList<MetadataNode> FetchNodes(string AssemblyFilePath, string ClassFilter, string MethodFilter)
    {
        if (!string.IsNullOrWhiteSpace(AssemblyFilePath) && File.Exists(AssemblyFilePath))
        {
            return External.GetMetadataNodes(AssemblyFilePath, ClassFilter, MethodFilter).ToList();
        }

        return Array.Empty<MetadataNode>();
    }

    
    
    void OnTreeItemClicked(MouseEvent e)
    {
        DispatchEvent(() => SelectionChanged, e.FirstNotEmptyId);
    }
}