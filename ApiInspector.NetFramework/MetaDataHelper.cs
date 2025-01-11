namespace ApiInspector;

sealed record MetadataNode
{
    public IReadOnlyList<MetadataNode> Children { get; init; } = new List<MetadataNode>();

    public bool HasChild => Children.Count > 0;

    public bool IsClass { get; init; }

    public bool IsMethod { get; init; }

    public bool IsNamespace { get; init; }

    public string label { get; init; }

    public MethodReference MethodReference { get; init; }

    public string NamespaceReference { get; init; }

    public TypeReference TypeReference { get; init; }
}