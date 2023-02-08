using System.Reflection;

namespace ApiInspector;

class MetadataNode
{
    public List<MetadataNode> children { get; } = new();

    public bool IsClass { get; set; }

    public bool IsMethod { get; set; }

    public bool IsNamespace { get; set; }

    public string label { get; set; }

    public MethodReference MethodReference { get; set; }

    public string NamespaceReference { get; set; }

    public TypeReference TypeReference { get; set; }
}

static class MetadataHelper
{
    public static MethodInfo FindMethodInfo(Assembly assembly, MetadataNode node)
    {
        MethodInfo returnMethodInfo = null;

        VisitTypes(assembly, visitType);

        void visitType(Type type)
        {
            if (returnMethodInfo == null)
            {
                VisitMethods(type, visitMethodInfo);
            }
        }

        void visitMethodInfo(MethodInfo methodInfo)
        {
            if (returnMethodInfo == null)
            {
                if (ConvertToMetadataNode(methodInfo).MethodReference.MetadataToken == node.MethodReference.MetadataToken)
                {
                    returnMethodInfo = methodInfo;
                }
            }
        }

        return returnMethodInfo;
    }

    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath, string classFilter, string methodFilter)
    {
        return getNamespaceNodes(GetAllTypes(LoadAssembly(assemblyFilePath), classFilter));

        IReadOnlyList<MetadataNode> getNamespaceNodes(IReadOnlyList<Type> types)
        {
            var items = new List<MetadataNode>();

            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
            {
                var nodeForNamespace = new MetadataNode
                {
                    NamespaceReference = namespaceName,
                    IsNamespace        = true,
                    label              = namespaceName
                };

                var classNodes = types.Where(x => x.Namespace == namespaceName).Select(classToMetaData);

                if (!string.IsNullOrWhiteSpace(methodFilter))
                {
                    classNodes = classNodes.Where(classNode => classNode.children.Count > 0).Take(5).OrderByDescending(classNode => classNode.children.Count);
                }

                nodeForNamespace.children.AddRange(classNodes);

                if (nodeForNamespace.children.Count > 0)
                {
                    items.Add(nodeForNamespace);
                }
            }

            return items.Take(5).ToList();
        }

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
                    if (classNode.children.Count < 5)
                    {
                        if (m.Name.IndexOf(methodFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            classNode.children.Add(ConvertToMetadataNode(m));
                        }
                    }

                    return;
                }

                classNode.children.Add(ConvertToMetadataNode(m));
            });

            return classNode;
        }
    }

    public static Assembly LoadAssembly(string assemblyFilePath)
    {
        ReflectionHelper.AttachAssemblyResolver();
        return Assembly.LoadFrom(assemblyFilePath);
    }

    static MetadataNode ConvertToMetadataNode(MethodInfo methodInfo)
    {
        return new MetadataNode
        {
            IsMethod        = true,
            MethodReference = methodInfo.AsReference(),
            label           = methodInfo.AsReference().FullNameWithoutReturnType
        };
    }

    static List<Type> GetAllTypes(Assembly assembly, string classFilter)
    {
        var types = new List<Type>();

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
    }

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

    static void VisitMethods(Type type, Action<MethodInfo> visit)
    {
        type.VisitMethods(methodInfo =>
        {
            if (IsValidForExport(methodInfo))
            {
                visit(methodInfo);
            }
        });
    }

    static void VisitTypes(Assembly assembly, Action<Type> visit)
    {
        assembly.VisitTypes(visit);
    }
}