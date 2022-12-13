using System.IO;
using ReactWithDotNet.Libraries.PrimeReact;


namespace ApiInspector.WebUI;

class MetadataNode : TreeNode
{
    public bool IsClass { get; set; }
    public bool IsMethod { get; set; }
    public bool IsNamespace { get; set; }
    
    public string NamespaceReference { get; set; }
    public MethodReference MethodReference { get; set; }
    public TypeReference TypeReference { get; set; }

    public List<MetadataNode> children { get; } = new();

}

class MethodSelectionModel
{
    public string Filter { get; set; }
}

class MethodSelectionView : ReactComponent<MethodSelectionModel>
{
    public int Width { get; set; }
    
    public string AssemblyFilePath { get; set; }

    public string Filter { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    [ReactCustomEvent]
    public Action<(string value, string filter)> SelectionChanged { get; set; }

    public static MetadataNode FindTreeNode(string AssemblyFilePath, string treeNodeKey)
    {
        if (string.IsNullOrWhiteSpace(AssemblyFilePath) || string.IsNullOrWhiteSpace(treeNodeKey))
        {
            return null;
        }

        if (!File.Exists(AssemblyFilePath))
        {
            return null;
        }

        var nodes = External.GetMetadataNodes(AssemblyFilePath).ToArray();

        return SingleSelectionTree<MetadataNode>.FindNodeByKey(nodes, treeNodeKey);
    }

    protected override void constructor()
    {
        state = new MethodSelectionModel { Filter = Filter };
    }

    protected override Element render()
    {
        var tree = new SingleSelectionTree<MetadataNode>
        {
            filterValueBind   = () => state.Filter,
            filter            = true,
            filterBy          = nameof(MetadataNode.label),
            filterPlaceholder = "Search react components or methods which returns Element",
            nodeTemplate      = nodeTemplate,
            value             = GetNodes(),
            onSelectionChange = OnSelectionChanged,
            selectionKeys     = SelectedMethodTreeNodeKey,
            style             = {  WidthMaximized , HeightMaximized }
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

  


    IEnumerable<MetadataNode> GetNodes()
    {


        
        if (!string.IsNullOrWhiteSpace(AssemblyFilePath) && File.Exists(AssemblyFilePath))
        {
            return External.GetMetadataNodes(AssemblyFilePath);
        }

        return new List<MetadataNode>();
    }

    void OnSelectionChanged(SingleSelectionTreeSelectionParams e)
    {
        DispatchEvent(() => SelectionChanged, (e.value, state.Filter));
    }
}