using System.Reflection;

namespace ApiInspector;

[Serializable]
public sealed class AssemblyReference
{
    public string Name { get; set; }

    public bool Equals(AssemblyReference other)
    {
        return Name == other?.Name;
    }

    public override string ToString()
    {
        return Name;
    }
}

[Serializable]
public sealed class TypeReference
{
    public AssemblyReference Assembly { get; set; }

    public string FullName { get; set; }

    public string Name { get; set; }

    public string NamespaceName { get; set; }

    public bool Equals(TypeReference other)
    {
        return Assembly.Equals(other?.Assembly) && FullName == other?.FullName;
    }

    public override string ToString()
    {
        return $"{FullName},{Assembly}";
    }
}

[Serializable]
public sealed class MethodReference
{
    public TypeReference DeclaringType { get; set; }

    public string FullNameWithoutReturnType { get; set; }

    public bool IsStatic { get; set; }

    public int MetadataToken { get; set; }

    public string Name { get; set; }

    public IReadOnlyList<ParameterReference> Parameters { get; set; }

    public bool Equals(MethodReference other)
    {
        if (DeclaringType is not null)
        {
            if (DeclaringType.Equals(other?.DeclaringType) == false)
            {
                return false;
            }
        }

        if (FullNameWithoutReturnType != other?.FullNameWithoutReturnType)
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return $"{DeclaringType.FullName}::{FullNameWithoutReturnType}";
    }
}

[Serializable]
public sealed class ParameterReference
{
    public string Name { get; set; }

    public TypeReference ParameterType { get; set; }

    public override string ToString()
    {
        return $"{ParameterType} {Name}";
    }
}

static class AssemblyModelHelper
{
    public static AssemblyReference AsReference(this Assembly assembly)
    {
        return new AssemblyReference { Name = assembly.GetName().Name };
    }

    public static TypeReference AsReference(this Type x)
    {
        return new TypeReference
        {
            FullName      = x.FullName,
            Name          = GetName(x),
            NamespaceName = x.Namespace,
            Assembly      = x.Assembly.AsReference()
        };

        static string GetName(Type x)
        {
            if (x.IsNested)
            {
                return GetName(x.DeclaringType) + "+" + x.Name;
            }

            return x.Name;
        }
    }

    public static MethodReference AsReference(this MethodInfo methodInfo)
    {
        return new MethodReference
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
        return new ParameterReference
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

        Type foundedType = null;

        assembly.VisitTypes(type =>
        {
            if (foundedType == null)
            {
                if (typeReference.Equals(type.AsReference()))
                {
                    foundedType = type;
                }
            }
        });

        return foundedType;
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

        var sameNames = new List<MethodInfo>();
        var fullSame  = new List<MethodInfo>();

        assembly.VisitTypes(type =>
        {
            if (fullSame.Count == 0)
            {
                type.VisitMethods(methodInfo =>
                {
                    if (fullSame.Count == 0)
                    {
                        if (methodReference.Equals(methodInfo.AsReference()))
                        {
                            fullSame.Add(methodInfo);
                        }

                        if (DeclaringTypesAreSameAndNameIsSame(methodReference, methodInfo.AsReference()))
                        {
                            sameNames.Add(methodInfo);
                        }
                    }
                });
            }
        });

        if (fullSame.Count == 1)
        {
            return fullSame[0];
        }

        if (sameNames.Count == 1)
        {
            return sameNames[0];
        }

        return null;

        static bool DeclaringTypesAreSameAndNameIsSame(MethodReference a, MethodReference b)
        {
            if (a.DeclaringType is not null && b.DeclaringType is not null)
            {
                if (a.DeclaringType.Equals(b.DeclaringType))
                {
                    if (a.Name == b.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static void VisitMethods(this Type type, Action<MethodInfo> visitAction)
    {
        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        if (type.IsStaticClass())
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

        void visitType(Type type)
        {
            visitAction(type);

            foreach (var nestedType in type.GetNestedTypes())
            {
                visitType(nestedType);
            }
        }
    }

    static bool IsStaticClass(this Type type)
    {
        return type.IsClass && type.IsAbstract && type.IsSealed;
    }

    static IEnumerable<Type> TryGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(isValidForAnalyze);
        }
        catch (Exception)
        {
            return Enumerable.Empty<Type>().ToArray();
        }

        static bool isValidForAnalyze(Type type)
        {
            var skipTypeList = new []
            {
                "Microsoft.CodeAnalysis.EmbeddedAttribute",
                "System.Runtime.CompilerServices.RefSafetyRulesAttribute"
            };
            
            if (skipTypeList.Contains(type.FullName) )
            {
                return false;
            }

            return true;
        }
    }
}