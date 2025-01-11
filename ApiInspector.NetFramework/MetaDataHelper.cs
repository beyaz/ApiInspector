using System.Reflection;

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

static class MetadataHelper
{
    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath, string classFilter, string methodFilter)
    {
        return getNamespaceNodes(getAllTypes(Assembly.LoadFrom(assemblyFilePath), classFilter));

        IEnumerable<MetadataNode> getNamespaceNodes(IReadOnlyList<Type> types)
        {
            var items = new List<MetadataNode>();

            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
            {
                var classNodes = types.Where(x => x.Namespace == namespaceName).Select(classToMetaData).ToList();

                if (!string.IsNullOrWhiteSpace(methodFilter))
                {
                    classNodes = classNodes.Where(classNode => classNode.HasChild).Take(5).OrderByDescending(classNode => classNode.Children.Count).ToList();
                }

                if (classNodes.Count > 0)
                {
                    items.Add(new()
                    {
                        NamespaceReference = namespaceName,
                        IsNamespace        = true,
                        label              = namespaceName,
                        Children           = classNodes
                    });
                }
            }

            return items.Take(5).ToList();

            MetadataNode classToMetaData(Type type)
            {
                var classNode = new MetadataNode
                {
                    IsClass       = true,
                    TypeReference = type.AsReference(),
                    label         = type.Name
                };

                VisitMethods(type, m =>
                {
                    if (!string.IsNullOrWhiteSpace(methodFilter))
                    {
                        if (classNode.Children.Count < 5)
                        {
                            if (m.Name.IndexOf(methodFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                classNode = addChild(classNode, convertToMetadataNode(m));
                            }
                        }

                        return;
                    }

                    classNode = addChild(classNode, convertToMetadataNode(m));
                });

                return classNode;

                static void VisitMethods(Type type, Action<MethodInfo> visit)
                {
                    type.VisitMethods(methodInfo =>
                    {
                        if (Try(() => IsValidForExport(methodInfo)).value)
                        {
                            visit(methodInfo);
                        }
                    });
                    return;

                    static bool IsValidForExport(MethodInfo methodInfo)
                    {
                        if (methodInfo.Name == "render" || methodInfo.Name == "InvokeRender")
                        {
                            return false;
                        }

                        if (methodInfo.Name.StartsWith("set_"))
                        {
                            return false;
                        }

                        if (methodInfo.Name.StartsWith("get_") && !methodInfo.IsStatic)
                        {
                            return false;
                        }

                        if (methodInfo.GetParameters().Any(p => isNotValidForJson(p.ParameterType)))
                        {
                            return false;
                        }

                        return true;

                        static bool isNotValidForJson(Type t)
                        {
                            if (typeof(MulticastDelegate).IsAssignableFrom(t.BaseType))
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    static (Exception exception, T value) Try<T>(Func<T> func)
                    {
                        try
                        {
                            return (default, func());
                        }
                        catch (Exception exception)
                        {
                            return (exception, default);
                        }
                    }
                }
            }
        }

        static MetadataNode addChild(MetadataNode node, MetadataNode child)
        {
            return node with { Children = new List<MetadataNode>(node.Children) { child } };
        }

        static MetadataNode convertToMetadataNode(MethodInfo methodInfo)
        {
            return new()
            {
                IsMethod        = true,
                MethodReference = methodInfo.AsReference(),
                label           = methodInfo.AsReference().FullNameWithoutReturnType
            };
        }

        static List<Type> getAllTypes(Assembly assembly, string classFilter)
        {
            var types = new List<Type>();

            VisitTypes(assembly, visit);

            if (types.Count == 0 && !string.IsNullOrWhiteSpace(classFilter))
            {
                // try search by namespace

                void filterByNamespaceName(Type type)
                {
                    if (type.FullName?.IndexOf(classFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        types.Add(type);
                    }
                }

                VisitTypes(assembly, filterByNamespaceName);
            }

            return types;

            void visit(Type type)
            {
                if (!string.IsNullOrWhiteSpace(classFilter))
                {
                    if (type.Name.IndexOf(classFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        types.Add(type);
                    }

                    return;
                }

                types.Add(type);
            }

            static void VisitTypes(Assembly assembly, Action<Type> visit)
            {
                assembly.VisitTypes(visit);
            }
        }
    }
}