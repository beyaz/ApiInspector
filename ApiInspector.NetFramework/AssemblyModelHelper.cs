using System.Reflection;

namespace ApiInspector;

static class AssemblyModelHelper
{
    public static TypeReference AsReference(this Type x)
    {
        return new()
        {
            FullName      = x.FullName,
            Name          = GetName(x),
            NamespaceName = x.Namespace,
            Assembly      = asReference(x.Assembly)
        };

        static string GetName(Type x)
        {
            if (x.IsNested)
            {
                return GetName(x.DeclaringType) + "+" + x.Name;
            }

            return x.Name;
        }

        static AssemblyReference asReference(Assembly assembly)
        {
            return new() { Name = assembly.GetName().Name };
        }
    }

    public static MethodReference AsReference(this MethodInfo methodInfo)
    {
        return new()
        {
            Name     = methodInfo.Name,
            IsStatic = methodInfo.IsStatic,
            FullNameWithoutReturnType = string.Join(string.Empty, new List<string>
            {
                methodInfo.Name,
                "(",
                string.Join(", ", methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType.Name + " " + parameterInfo.Name)),
                ")"
            }),
            MetadataToken = methodInfo.MetadataToken,

            DeclaringType = methodInfo.DeclaringType.AsReference(),
            Parameters    = methodInfo.GetParameters().Select(AsReference).ToList()
        };
    }

    public static ParameterReference AsReference(this ParameterInfo parameterInfo)
    {
        return new()
        {
            Name          = parameterInfo.Name,
            ParameterType = parameterInfo.ParameterType.AsReference()
        };
    }

    public static Type TryLoadFrom(this Assembly assembly, TypeReference typeReference)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (typeReference == null)
        {
            throw new ArgumentNullException(nameof(typeReference));
        }
        
        return assembly.GetType(typeReference.FullName, throwOnError: false, ignoreCase: true);
    }

    public static MethodInfo TryLoadFrom(this Assembly assembly, MethodReference methodReference)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (methodReference == null)
        {
            throw new ArgumentNullException(nameof(methodReference));
        }

        var type = assembly.GetType(methodReference.DeclaringType.FullName, throwOnError: false, ignoreCase: true);
        if (type == null)
        {
            return null;
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        methods = methods.Where(m => m.Name == methodReference.Name).ToArray();
        if (methods.Length == 1)
        {
            return methods[0];
        }

        return methods.FirstOrDefault(m => methodReference.Equals(AsReference(m)));
    }

    public static void VisitMethods(this Type type, Action<MethodInfo> visitAction)
    {
        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        if (IsStaticClass(type))
        {
            flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        }

        foreach (var methodInfo in type.GetMethods(flags))
        {
            if (methodInfo.DeclaringType == typeof(object))
            {
                continue;
            }

            visitAction(methodInfo);
        }

        return;

        static bool IsStaticClass(Type type)
        {
            return type.IsClass && type.IsAbstract && type.IsSealed;
        }
    }

    public static void VisitTypes(this Assembly assembly, Action<Type> visitAction)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (visitAction == null)
        {
            throw new ArgumentNullException(nameof(visitAction));
        }

        foreach (var type in TryGetTypes(assembly))
        {
            visitType(type);
        }

        return;

        void visitType(Type type)
        {
            visitAction(type);

            foreach (var nestedType in type.GetNestedTypes())
            {
                visitType(nestedType);
            }
        }

        static IEnumerable<Type> TryGetTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(isValidForAnalyze);

            static bool isValidForAnalyze(Type type)
            {
                var skipTypeList = new[]
                {
                    "Microsoft.CodeAnalysis.EmbeddedAttribute",
                    "System.Runtime.CompilerServices.RefSafetyRulesAttribute"
                };

                if (skipTypeList.Contains(type.FullName))
                {
                    return false;
                }

                return true;
            }
        }
    }
}