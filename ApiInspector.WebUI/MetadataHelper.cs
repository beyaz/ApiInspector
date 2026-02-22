using Mono.Cecil;
using System.Collections.Immutable;
using System.Reflection;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace ApiInspector.WebUI;

static class MetadataHelper
{
    public static Result<IReadOnlyList<MetadataNode>> GetMetadataNodes(string assemblyFilePath, string classFilter, string methodFilter)
    {
        try
        {
            using var assemblyDefinition = ReadAssembly(assemblyFilePath);

            return getNamespaceNodes(getAllTypes(assemblyDefinition, classFilter)).ToList();
        }
        catch (Exception exception)
        {
            return exception;
        }

        static List<TypeDefinition> GetAllTypes(IEnumerable<TypeDefinition> types)
        {
            var list = new List<TypeDefinition>();

            foreach (var typeDefinition in types)
            {
                list.Add(typeDefinition);

                foreach (var typeDefinitionNestedType in typeDefinition.NestedTypes)
                {
                    if (typeDefinitionNestedType.Name.StartsWith("<")) // Skip compiler generated classes
                    {
                        continue;
                    }

                    list.Add(typeDefinitionNestedType);
                }
            }

            return list;
        }
        IEnumerable<MetadataNode> getNamespaceNodes(IReadOnlyList<TypeDefinition> types)
        {
            var namespaceNodes = new List<MetadataNode>();

            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
            {
                var classNodes = GetAllTypes(types.Where(x => x.Namespace == namespaceName)).Select(classToMetaData).Where(classNode => classNode.HasChild).ToList();

                if (!string.IsNullOrWhiteSpace(methodFilter))
                {
                    classNodes = classNodes.OrderByDescending(classNode => classNode.Children.Count).ToList();
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

            return namespaceNodes.Take(3).ToList();

          
            
            MetadataNode classToMetaData(TypeDefinition type)
            {
                var classNode = new MetadataNode
                {
                    IsClass       = true,
                    TypeReference = asTypeReference(type),
                    label         = GetLabel(type)
                };

                var methods = getMethods(type);

                if (string.IsNullOrWhiteSpace(methodFilter))
                {
                    foreach (var methodDefinition in methods.Take(3))
                    {
                        classNode = addChild(classNode, convertToMetadataNode(methodDefinition));
                    }
                }
                else
                {
                    foreach (var methodDefinition in methods.Where(m => m.Name.Contains(methodFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        classNode = addChild(classNode, convertToMetadataNode(methodDefinition));
                    }
                }

                return classNode;

                static IReadOnlyList<MethodDefinition> getMethods(TypeDefinition typeDefinition)
                {
                    var returnList = new List<MethodDefinition>();

                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (Try(() => IsValidForExport(methodDefinition)).value)
                        {
                            returnList.Add(methodDefinition);
                        }
                    }

                    return returnList;

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
                            if (t.IsGenericParameter)
                            {
                                return true;
                            }

                            if (t.Name.Contains("<>c__DisplayClass", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            if (t.IsGenericInstance && t.Name.StartsWith("ValueTuple`") && t.Namespace =="System") 
                            {
                                return false;
                            }
                            if (t.Name.Contains("`"))
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
                
                static string GetLabel(TypeDefinition typeDefinition)
                {
                    if (typeDefinition.IsNested)
                    {
                        return GetLabel(typeDefinition.DeclaringType) + "/" + typeDefinition.Name;
                    }

                    return typeDefinition.Name;
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
                        string.Join(", ", methodInfo.Parameters.Select(CalculateName)),
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

                static string CalculateName(ParameterDefinition parameterDefinition)
                {
                    var typeReference = parameterDefinition.ParameterType;
                    if (typeReference.Name.StartsWith("ValueTuple`") && typeReference.Namespace == nameof(System))
                    {
                        return typeReference.FullName.RemoveFromStart(typeReference.Namespace+".");
                    }

                    return GetTypeName(parameterDefinition.ParameterType) + " " + parameterDefinition.Name;
                }
            }
        }

        static TypeReference asTypeReference(Mono.Cecil.TypeReference x)
        {
            return new()
            {
                FullName      = x.FullName,
                Name          = GetTypeName(x),
                NamespaceName = x.Namespace,
                Assembly      = asReference(x.Scope)
            };


            static AssemblyReference asReference(IMetadataScope assembly)
            {
                return new() { Name = assembly.Name.RemoveFromEnd(".dll").RemoveFromEnd(".exe") };
            }
        }

        static string GetTypeName(Mono.Cecil.TypeReference typeReference)
        {
            if (typeReference.IsNested)
            {
                return GetTypeName(typeReference.DeclaringType) + "+" + typeReference.Name;
            }

            if (typeReference.Name== "Nullable`1")
            {
                if (typeReference is GenericInstanceType genericInstanceType)
                {
                    return GetTypeName(genericInstanceType.GenericArguments[0]) + "?";
                }
            }

           

            return typeReference.Name;
        }

        static List<TypeDefinition> getAllTypes(AssemblyDefinition assembly, string classFilter)
        {
            var types = assembly.MainModule.Types.Where(canExport).ToList();

            if (string.IsNullOrWhiteSpace(classFilter))
            {
                return types;
            }

            // try match from fullName
            {
                if (classFilter.Contains('.', StringComparison.CurrentCulture))
                {
                    var fullNameMatchedType = types.FirstOrDefault(t => t.FullName == classFilter);
                    if (fullNameMatchedType is not null)
                    {
                        return [fullNameMatchedType];
                    }
                }
            }
            
            var classFilters = classFilter.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            var selectedTypes = types.Where(t => containsFilter(t.Name)).ToList();
            if (selectedTypes.Count > 0)
            {
                return selectedTypes;
            }

            return types.Where(t => containsFilter(t.Namespace)).ToList();

            bool containsFilter(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return false;
                }

                foreach (var filter in classFilters)
                {
                    if (name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

           

            static bool canExport(TypeDefinition typeDefinition)
            {
                if (typeDefinition.IsInterface)
                {
                    return false;
                }

                return true;
            }
        }
    }

    public static AssemblyDefinition ReadAssembly(string assemblyFilePath)
    {
        var assemblyResolver = new DefaultAssemblyResolver();

        assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFilePath));

        return AssemblyDefinition.ReadAssembly(assemblyFilePath, new ReaderParameters
        {
            AssemblyResolver = assemblyResolver,
            InMemory = true,
            ReadWrite = false
        });
    }
}