using System.Collections.Immutable;
using System.IO;

namespace ApiInspector.WebUI;

[Serializable]
public record MetadataNode 
{
    public ImmutableList<MetadataNode> Children { get; init; } = ImmutableList.Create<MetadataNode>();
    
    public bool IsClass { get; init; }
    public bool IsMethod { get; init; }
    public bool IsNamespace { get; init; }
    public MethodReference MethodReference { get; init; }

    public string NamespaceReference { get; init; }
    public TypeReference TypeReference { get; init; }
    
    public string label { get; init; }
    
    public bool HasChild => Children.Count > 0;
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

class MethodSelectionView : Component<MethodSelectionViewState>
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
    public Func<string,Task> SelectionChanged { get; set; }

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

        return FindTreeNode(node.Children, hasMatch);
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
        return AsTreeItem(node, SelectedMethodTreeNodeKey, OnTreeItemClicked);
    }
    
    static Element AsTreeItem(MetadataNode node, string SelectedMethodTreeNodeKey, MouseEventHandler OnTreeItemClicked)
    {
        if (node.IsMethod)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Method")), Size(11), MarginTop(5), MarginLeft(20) },

                new div { Text(node.label), MarginLeft(5), FontSize13 },

                Id(node.MethodReference.MetadataToken),

                arrangeBackground
            };
        }

        if (node.IsClass)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Class")), Size(14), MarginLeft(10) },

                new div { Text(node.label), MarginLeft(5), FontSize13 }
            };
        }

        if (node.IsNamespace)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Namespace")), Size(14) },

                new div { Text(node.label), MarginLeft(5), FontSize13 }
            };
        }

        return new div();

        void arrangeBackground(HtmlElement el)
        {
            var isSelected = HasMatch(node, SelectedMethodTreeNodeKey);
            
            if (isSelected)
            {
                el += BackgroundImage(linear_gradient(90,  rgb(136, 195, 242), rgb(242, 246, 249))) + BorderRadius(3);
            }
            else
            {
                el += Hover(BackgroundImage(linear_gradient(90, rgb(190, 220, 244), rgb(242, 246, 249)))+ BorderRadius(3));
            }

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
            if (node.HasChild)
            {
                var parent = AsTreeItem(node);
                var chldrn = AsTreeView(node.Children);

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
            WidthFull, HeightFull
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

    
    
    Task OnTreeItemClicked(MouseEvent e)
    {
        DispatchEvent(SelectionChanged, [e.currentTarget.id]);
        
        return Task.CompletedTask;
    }
}