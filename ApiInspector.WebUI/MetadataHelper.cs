//using System.Collections.Immutable;
//using System.Reflection;
//using Mono.Cecil;

//namespace ApiInspector.WebUI;

//static class MetadataHelper
//{
//    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath, string classFilter, string methodFilter)
//    {
//        AssemblyDefinition assemblyDefinition;

//        try
//        {
//            assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFilePath);
//        }
//        catch (Exception )
//        {
//            return [];
//        }
        
        
//        return getNamespaceNodes(getAllTypes(assemblyDefinition, classFilter));

//        IEnumerable<MetadataNode> getNamespaceNodes(IReadOnlyList<TypeDefinition> types)
//        {
//            var items = new List<MetadataNode>();

//            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
//            {
//                var classNodes = types.Where(x => x.Namespace == namespaceName).Select(classToMetaData).ToList();

//                if (!string.IsNullOrWhiteSpace(methodFilter))
//                {
//                    classNodes = classNodes.Where(classNode => classNode.HasChild).Take(5).OrderByDescending(classNode => classNode.Children.Count).ToList();
//                }

//                if (classNodes.Count > 0)
//                {
//                    items.Add(new()
//                    {
//                        NamespaceReference = namespaceName,
//                        IsNamespace        = true,
//                        label              = namespaceName,
//                        Children           = classNodes.ToImmutableList()
//                    });
//                }
//            }

//            return items.Take(5).ToList();

//            MetadataNode classToMetaData(TypeDefinition type)
//            {
//                var classNode = new MetadataNode
//                {
//                    IsClass       = true,
//                    // todo: TypeReference = type.AsReference(),
//                    label         = type.Name
//                };

//                VisitMethods(type, m =>
//                {
//                    if (!string.IsNullOrWhiteSpace(methodFilter))
//                    {
//                        if (classNode.Children.Count < 5)
//                        {
//                            if (m.Name.IndexOf(methodFilter, StringComparison.OrdinalIgnoreCase) >= 0)
//                            {
//                                classNode = addChild(classNode, convertToMetadataNode(m));
//                            }
//                        }

//                        return;
//                    }

//                    classNode = addChild(classNode, convertToMetadataNode(m));
//                });

//                return classNode;

//                static void VisitMethods(TypeDefinition type, Action<MethodDefinition> visit)
//                {
//                    foreach (var methodDefinition in type.Methods)
//                    {
//                        if (Try(() => IsValidForExport(methodDefinition)).value)
//                        {
//                            visit(methodDefinition);
//                        }
//                    }
//                    return;

//                    static bool IsValidForExport(MethodDefinition methodInfo)
//                    {
//                        if (methodInfo.Name == "render" || methodInfo.Name == "InvokeRender")
//                        {
//                            return false;
//                        }

//                        if (methodInfo.Name.StartsWith("set_"))
//                        {
//                            return false;
//                        }

//                        if (methodInfo.Name.StartsWith("get_") && !methodInfo.IsStatic)
//                        {
//                            return false;
//                        }

//                        if (methodInfo.Parameters.Any(p => isNotValidForJson(p.ParameterType)))
//                        {
//                            return false;
//                        }

//                        return true;

//                        static bool isNotValidForJson(Mono.Cecil.TypeReference t)
//                        {
//                            if (t.Resolve().BaseType?.FullName == typeof(MulticastDelegate).FullName)
//                            {
//                                return true;
//                            }

//                            return false;
//                        }
//                    }

//                    static (Exception exception, T value) Try<T>(Func<T> func)
//                    {
//                        try
//                        {
//                            return (default, func());
//                        }
//                        catch (Exception exception)
//                        {
//                            return (exception, default);
//                        }
//                    }
//                }
//            }
//        }

//        static MetadataNode addChild(MetadataNode node, MetadataNode child)
//        {
//            return node with { Children = new List<MetadataNode>(node.Children) { child }.ToImmutableList() };
//        }

//        static MetadataNode convertToMetadataNode(MethodDefinition methodInfo)
//        {
//            return new()
//            {
//                IsMethod        = true,
//                //todo: MethodReference = methodInfo.AsReference(),
//                //todo: label           = methodInfo.AsReference().FullNameWithoutReturnType
//            };
//        }

//        static List<TypeDefinition> getAllTypes(AssemblyDefinition assembly, string classFilter)
//        {
//            var types = new List<TypeDefinition>();

//            VisitTypes(assembly, visit);

//            if (types.Count == 0 && !string.IsNullOrWhiteSpace(classFilter))
//            {
//                // try search by namespace

//                void filterByNamespaceName(TypeDefinition type)
//                {
//                    if (type.FullName?.IndexOf(classFilter, StringComparison.OrdinalIgnoreCase) >= 0)
//                    {
//                        types.Add(type);
//                    }
//                }

//                VisitTypes(assembly, filterByNamespaceName);
//            }

//            return types;

//            void visit(TypeDefinition type)
//            {
//                if (!string.IsNullOrWhiteSpace(classFilter))
//                {
//                    if (type.Name.IndexOf(classFilter, StringComparison.OrdinalIgnoreCase) >= 0)
//                    {
//                        types.Add(type);
//                    }

//                    return;
//                }

//                types.Add(type);
//            }

//            static void VisitTypes(AssemblyDefinition assembly, Action<TypeDefinition> visit)
//            {
//                foreach (var moduleDefinition in assembly.Modules)
//                {
//                    foreach (var typeDefinition in moduleDefinition.Types)
//                    {
//                        visit(typeDefinition);
//                    }
//                }
//            }
//        }
//    }
//}