using System.Reflection;

namespace ApiInspector;

static class AssemblyModelHelper
{
    public static MethodReference AsMethodReference(this MethodInfo methodInfo)
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

            DeclaringType = asTypeReference(methodInfo.DeclaringType),
            Parameters    = methodInfo.GetParameters().Select(asParameterReference).ToList()
        };

        static ParameterReference asParameterReference(ParameterInfo parameterInfo)
        {
            return new()
            {
                Name          = parameterInfo.Name,
                ParameterType = asTypeReference(parameterInfo.ParameterType)
            };
        }

        static TypeReference asTypeReference(Type x)
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

        return methods.FirstOrDefault(m => methodReference.Equals(AsMethodReference(m)));
    }
}