using System.Collections.Immutable;
using Mono.Cecil;

namespace ApiInspector.WebUI;

static class MetadataHelper
{
    public static Result<IReadOnlyList<MetadataNode>> GetMetadataNodes(string assemblyFilePath, string classFilter, string methodFilter)
    {
        AssemblyDefinition assemblyDefinition;

        try
        {
            assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFilePath);
        }
        catch (Exception exception)
        {
            return exception;
        }

        return getNamespaceNodes(getAllTypes(assemblyDefinition, classFilter)).ToList();

        IEnumerable<MetadataNode> getNamespaceNodes(IReadOnlyList<TypeDefinition> types)
        {
            var namespaceNodes = new List<MetadataNode>();

            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
            {
                var classNodes = types.Where(x => x.Namespace == namespaceName).Select(classToMetaData).Where(classNode => classNode.HasChild).ToList();

                if (!string.IsNullOrWhiteSpace(methodFilter))
                {
                    classNodes = classNodes.Take(3).OrderByDescending(classNode => classNode.Children.Count).ToList();
                }

                if (classNodes.Count > 0)
                {
                    namespaceNodes.Add(new()
                    {
                        NamespaceReference = namespaceName,
                        IsNamespace        = true,
                        label              = namespaceName,
                        Children           = classNodes.ToImmutableList()
                    });
                }
            }

            return namespaceNodes.Take(5).ToList();

            MetadataNode classToMetaData(TypeDefinition type)
            {
                var classNode = new MetadataNode
                {
                    IsClass       = true,
                    TypeReference = asTypeReference(type),
                    label         = type.Name
                };

                VisitMethods(type, m =>
                {
                    if (classNode.Children.Count >= 5)
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(methodFilter))
                    {
                        if (m.Name.IndexOf(methodFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            classNode = addChild(classNode, convertToMetadataNode(m));
                        }

                        return;
                    }

                    classNode = addChild(classNode, convertToMetadataNode(m));
                });

                return classNode;

                static void VisitMethods(TypeDefinition type, Action<MethodDefinition> visit)
                {
                    foreach (var methodDefinition in type.Methods)
                    {
                        if (Try(() => IsValidForExport(methodDefinition)).value)
                        {
                            visit(methodDefinition);
                        }
                    }

                    return;

                    static bool IsValidForExport(MethodDefinition methodInfo)
                    {
                        if (methodInfo.IsConstructor)
                        {
                            return false;
                        }

                        if (!methodInfo.HasBody)
                        {
                            return false;
                        }

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

                        if (methodInfo.Parameters.Any(p => isNotValidForJson(p.ParameterType)))
                        {
                            return false;
                        }

                        return true;

                        static bool isNotValidForJson(Mono.Cecil.TypeReference t)
                        {
                            if (t.Resolve()?.BaseType?.FullName == typeof(MulticastDelegate).FullName)
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
            return node with { Children = new List<MetadataNode>(node.Children) { child }.ToImmutableList() };
        }

        static MetadataNode convertToMetadataNode(MethodDefinition methodInfo)
        {
            return new()
            {
                IsMethod        = true,
                MethodReference = asMethodReference(methodInfo),
                label           = asMethodReference(methodInfo).FullNameWithoutReturnType
            };

            static MethodReference asMethodReference(MethodDefinition methodInfo)
            {
                return new()
                {
                    Name     = methodInfo.Name,
                    IsStatic = methodInfo.IsStatic,
                    FullNameWithoutReturnType = string.Join(string.Empty, new List<string>
                    {
                        methodInfo.Name,
                        "(",
                        string.Join(", ", methodInfo.Parameters.Select(parameterInfo => parameterInfo.ParameterType.Name + " " + parameterInfo.Name)),
                        ")"
                    }),
                    MetadataToken = methodInfo.MetadataToken.ToInt32(),

                    DeclaringType = asTypeReference(methodInfo.DeclaringType),
                    Parameters    = methodInfo.Parameters.Select(asParameterReference).ToList()
                };

                static ParameterReference asParameterReference(ParameterDefinition parameterInfo)
                {
                    return new()
                    {
                        Name          = parameterInfo.Name,
                        ParameterType = asTypeReference(parameterInfo.ParameterType)
                    };
                }
            }
        }

        static TypeReference asTypeReference(Mono.Cecil.TypeReference x)
        {
            return new()
            {
                FullName      = x.FullName,
                Name          = GetName(x),
                NamespaceName = x.Namespace,
                Assembly      = asReference(x.Scope)
            };

            static string GetName(Mono.Cecil.TypeReference x)
            {
                if (x.IsNested)
                {
                    return GetName(x.DeclaringType) + "+" + x.Name;
                }

                return x.Name;
            }

            static AssemblyReference asReference(IMetadataScope assembly)
            {
                return new() { Name = assembly.Name };
            }
        }

        static List<TypeDefinition> getAllTypes(AssemblyDefinition assembly, string classFilter)
        {
            var types = new List<TypeDefinition>();

            VisitTypes(assembly, visit);

            if (types.Count == 0 && !string.IsNullOrWhiteSpace(classFilter))
            {
                // try search by namespace

                void filterByNamespaceName(TypeDefinition type)
                {
                    if (type.FullName?.IndexOf(classFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        types.Add(type);
                    }
                }

                VisitTypes(assembly, filterByNamespaceName);
            }

            return types;

            void visit(TypeDefinition type)
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

            static void VisitTypes(AssemblyDefinition assembly, Action<TypeDefinition> visit)
            {
                foreach (var moduleDefinition in assembly.Modules)
                {
                    foreach (var typeDefinition in moduleDefinition.Types)
                    {
                        if (typeDefinition.IsInterface)
                        {
                            continue;
                        }

                        visit(typeDefinition);
                    }
                }
            }
        }
    }
}