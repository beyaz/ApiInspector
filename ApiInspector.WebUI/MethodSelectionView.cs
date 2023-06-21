using System.IO;
using ReactWithDotNet.ThirdPartyLibraries.PrimeReact;

namespace ApiInspector.WebUI;

class MetadataNode : TreeNode
{
    public List<MetadataNode> children { get; } = new();
    public bool IsClass { get; set; }
    public bool IsMethod { get; set; }
    public bool IsNamespace { get; set; }
    public MethodReference MethodReference { get; set; }

    public string NamespaceReference { get; set; }
    public TypeReference TypeReference { get; set; }
}

class MethodSelectionView : ReactComponent
{
    public string AssemblyFilePath { get; set; }

    public string ClassFilter { get; set; }

    public string MethodFilter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }

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

        return SingleSelectionTree<MetadataNode>.FindNodeByKey(nodes, treeNodeKey);
    }

    protected override Element render()
    {
        IReadOnlyList<MetadataNode> nodes;

        try
        {
            nodes = GetNodes();
        }
        catch (Exception exception)
        {
            return new div { "Error occured: " + exception };
        }

        // expand if there are few elements
        {
            if (nodes.Count == 1) // namespace
            {
                if (nodes[0].children.Count == 1) // class
                {
                    if (nodes[0].children[0].children.Count < 3)
                    {
                        nodes[0].expanded             = true;
                        nodes[0].children[0].expanded = true;
                    }
                }
            }
        }

        var tree = new SingleSelectionTree<MetadataNode>
        {
            nodeTemplate      = nodeTemplate,
            value             = nodes,
            onSelectionChange = OnSelectionChanged,
            selectionKeys     = SelectedMethodTreeNodeKey,
            style             = { WidthMaximized, HeightMaximized }
        };

        return tree;
    }

    static Element nodeTemplate(MetadataNode node)
    {
        if (node.IsMethod)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Method")), wh(14), mt(5) },

                new div { Text(node.label), MarginLeft(5), FontSize13 }
            };
        }

        if (node.IsClass)
        {
            return new FlexRow(AlignItemsCenter)
            {
                new img { Src(GetSvgUrl("Class")), wh(14) },

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
    }

    IReadOnlyList<MetadataNode> GetNodes()
    {
        if (!string.IsNullOrWhiteSpace(AssemblyFilePath) && File.Exists(AssemblyFilePath))
        {
            return External.GetMetadataNodes(AssemblyFilePath, ClassFilter, MethodFilter).ToList();
        }

        return Array.Empty<MetadataNode>();
    }

    void OnSelectionChanged(SingleSelectionTreeSelectionParams e)
    {
        DispatchEvent(() => SelectionChanged, e.value);
    }
}