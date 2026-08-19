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
                string.Join(", ", methodInfo.GetParameters().Select(parameterInfo => GetTypeName(parameterInfo.ParameterType) + " " + parameterInfo.Name)),
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
                Name          = GetTypeName(x),
                NamespaceName = x.Namespace,
                Assembly      = asReference(x.Assembly)
            };


            static AssemblyReference asReference(Assembly assembly)
            {
                var assemblyName = assembly.GetName();

                var name = assemblyName.Name;
                
                if (name.EndsWith(".dll"))
                {
                    name = name.RemoveFromEnd(".dll");
                }
                
                return new() { Name = assemblyName.Name };
            }
        }

        static string GetTypeName(Type type)
        {
            if (type.IsNested)
            {
                return GetTypeName(type.DeclaringType) + "+" + type.Name;
            }

            if (type.Name == "Nullable`1")
            {
                return GetTypeName(type.GenericTypeArguments[0]) + "?";
            }

            return type.Name;
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
    
    public static Result<Type> TryLoadType(this Assembly assembly, TypeReference typeReference)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (typeReference == null)
        {
            throw new ArgumentNullException(nameof(typeReference));
        }

        var type = assembly.GetType(typeReference.FullName, throwOnError: false, ignoreCase: true);
        if (type == null)
        {
            return new Exception($"Type '{typeReference.FullName}' not found in assembly '{assembly.FullName}'");
        }

        return type;
    }
    
    public static Result<MethodInfo> TryLoadMethod(this Assembly assembly, MethodReference methodReference)
    {
        if (assembly == null)
        {
            return new ArgumentNullException(nameof(assembly));
        }

        if (methodReference == null)
        {
            return new ArgumentNullException(nameof(methodReference));
        }

        var type = assembly.GetType(methodReference.DeclaringType.FullName, throwOnError: false, ignoreCase: true);
        if (type == null)
        {
            return new Exception($"Type '{methodReference.DeclaringType.FullName}' not found in assembly '{assembly.FullName}'");
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        methods = methods.Where(m => m.Name == methodReference.Name).ToArray();
        if (methods.Length == 1)
        {
            return methods[0];
        }

        var methodInfo = methods.FirstOrDefault(m => methodReference.Equals(AsMethodReference(m)));
        if (methodInfo == null)
        {
            return new Exception($"Method '{methodReference.Name}' not found in type '{methodReference.DeclaringType.FullName}'");
        }

        return methodInfo;
    }

    /// <summary>
    ///     Removes value from end of str
    /// </summary>
     static string RemoveFromEnd(this string data, string value)
    {
        return RemoveFromEnd(data, value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Removes from end.
    /// </summary>
     static string RemoveFromEnd(this string data, string value, StringComparison comparison)
    {
        if (data.EndsWith(value, comparison))
        {
            return data.Substring(0, data.Length - value.Length);
        }

        return data;
    }
}


public readonly struct Result<TValue>(bool success, TValue value, Exception exception)
{
    public bool Success { get;  } = success;

    public TValue Value { get; } = value;

    public Exception Exception { get;  } = exception;

    public static implicit operator Result<TValue>(TValue value) => new(true, value, null);

    public static implicit operator Result<TValue>(Exception exception) => new(false, default, exception);

    public static implicit operator Result<TValue>((bool Success, TValue Value, Exception Exception) tuple) => new(tuple.Success, tuple.Value, tuple.Exception);
   

    public override string ToString()
    {
        if (Success && Value != null)
        {
            return Value.ToString();
        }
        
        return $"Success: {Success}, Value: {Value}, Exception: {Exception}";
    }

    public void Deconstruct(out bool success, out TValue value, out Exception exception)
    {
        success   = this.Success;
        value     = this.Value;
        exception = this.Exception;
    }
}