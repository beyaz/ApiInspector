﻿using System.Reflection;

namespace ApiInspector.WebUI;

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

    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        return getNamespaceNodes(GetAllTypes(LoadAssembly(assemblyFilePath)));

        static IReadOnlyList<MetadataNode> getNamespaceNodes(IReadOnlyList<Type> types)
        {
            var items = new List<MetadataNode>();

            foreach (var namespaceName in types.Select(t => t.Namespace).Distinct())
            {
                var nodeForNamespace = new MetadataNode
                {
                    NamespaceReference = namespaceName,
                    IsNamespace        = true,
                    label              = namespaceName,
                    children = types.Where(x => x.Namespace == namespaceName).Select(classToMetaData).ToList()
                };

                

                items.Add(nodeForNamespace);
            }

            return items;
        }

        static MetadataNode classToMetaData(Type x)
        {
            var classNode = new MetadataNode
            {
                IsClass       = true,
                TypeReference = x.AsReference(),
                label         = x.Name
            };

            VisitMethods(x, m =>
            {
                classNode.children ??= new List<MetadataNode>();
                classNode.children.Add(ConvertToMetadataNode(m));
            });

            return classNode;
        }
    }

    public static Assembly LoadAssembly(string assemblyFilePath)
    {
        if (Assembly.GetEntryAssembly()?.Location == assemblyFilePath)
        {
            return Assembly.GetEntryAssembly();
        }

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

    static List<Type> GetAllTypes(Assembly assembly)
    {
        var types = new List<Type>();

        void visit(Type type) => types.Add(type);

        VisitTypes(assembly, visit);

        return types;
    }

    static bool IsValidForExport(MethodInfo methodInfo)
    {
        if (methodInfo.Name == "render" || methodInfo.Name == "InvokeRender")
        {
            return false;
        }

        if ( /*methodInfo.Name.Contains("|") ||*/ methodInfo.Name.StartsWith("set_"))
        {
            return false;
        }

        if (methodInfo.GetParameters().Any(p => isNotValidForJson(p.ParameterType)))
        {
            return false;
        }

        // is function component
        if (methodInfo.ReturnType == typeof(Element) || methodInfo.ReturnType.IsSubclassOf(typeof(Element)))
        {
            return true;
        }

        return false;

        static bool isNotValidForJson(Type t)
        {
            if (t == typeof(Element) || t.BaseType == typeof(HtmlElement))
            {
                return true;
            }

            if (typeof(Element).IsAssignableFrom(t.BaseType))
            {
                return true;
            }

            if (typeof(ReactComponentBase).IsAssignableFrom(t.BaseType))
            {
                return true;
            }

            if (typeof(PureComponent).IsAssignableFrom(t.BaseType))
            {
                return true;
            }

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
        assembly.VisitTypes(type =>
        {
            if (isValidForExport(type))
            {
                visit(type);
            }
        });

        static bool isValidForExport(Type type)
        {
            if (type.IsAbstract)
            {
                return false;
            }

            return type.IsSubclassOf(typeof(ReactComponentBase)) || type.IsSubclassOf(typeof(PureComponent));
        }
    }
}