using Newtonsoft.Json.Linq;
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

public readonly struct Result(bool success, Exception exception)
{
    public bool Success { get;  } = success;
    
    public Exception Exception { get;  } = exception;

    public static Result<T> From<T>(Func<T> func)
    {
        try
        {
            return new Result<T>(success: true, value: func(), exception: null);
        }
        catch (Exception ex)
        {
            return new Result<T>(success: false, value: default, exception: ex);
        }
    }
    
    public static Result<T> From<T>(Func<Result<T>> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return new Result<T>(success: false, value: default, exception: ex);
        }
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

    // L i n q   q u e r y   s y n t a x   s u p p o r t
    // First failed result short circuits the query.

    public Result<TResult> Select<TResult>(Func<TValue, TResult> selector)
    {
        if (!Success)
        {
            return Exception;
        }

        var value = selector(Value);
        
        return new Result<TResult>(success: true, value, exception: null);
    }
    
    public Result<TResult> Select<TResult>(Func<TValue, Result<TResult>> selector)
    {
        if (!Success)
        {
            return Exception;
        }

        var result =  selector(Value);
        if (result.Success)
        {
            return result.Value;
        }
        return result.Exception;
    }

    public Result<TResult> SelectMany<TResult>(Func<TValue, Result<TResult>> selector)
    {
        if (!Success)
        {
            return Exception;
        }

        var result =  selector(Value);
        if (result.Success)
        {
            return result.Value;
        }
        return result.Exception;
    }
    
    public Result<TResult> SelectMany<TResult>(Func<TValue, TResult> selector)
    {
        if (!Success)
        {
            return Exception;
        }

        return new Result<TResult>(success: true, value: selector(Value), exception: null);
    }
    
    

    public Result<TResult> SelectMany<TIntermediate, TResult>(Func<TValue, Result<TIntermediate>> selector, Func<TValue, TIntermediate, TResult> resultSelector)
    {
        if (!Success)
        {
            return Exception;
        }

        var intermediate = selector(Value);
        if (!intermediate.Success)
        {
            return intermediate.Exception;
        }

        return resultSelector(Value, intermediate.Value);
    }
    
    public Result<TResult> SelectMany<TIntermediate, TResult>(Func<TValue, Func<Result<TIntermediate>>> selector, Func<TValue, TIntermediate, TResult> resultSelector)
    {
        if (!Success)
        {
            return Exception;
        }

        var intermediate = selector(Value)();
        if (!intermediate.Success)
        {
            return intermediate.Exception;
        }

        return resultSelector(Value, intermediate.Value);
        
    }
    

    public Result<TValue> Where(Func<TValue, bool> predicate)    {
        if (!Success)
        {
            return this;
        }

        if (!predicate(Value))
        {
            return new Exception($"Predicate not satisfied. @value: {Value}");
        }

        return this;
    }

    /// <summary>
    ///     Runs given action for success value then returns same result.
    /// </summary>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (Success)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>
    ///     Runs given action for exception then returns same result.
    /// </summary>
    public Result<TValue> TapError(Action<Exception> action)
    {
        if (!Success)
        {
            action(Exception);
        }

        return this;
    }
    
    public static Result<TValue> operator | (Result<TValue> source, Action action)
    {
        action();
        return source;
    }
    
    public static Result<TValue> operator | (Result<TValue> source, Action<TValue> action)
    {
        action(source.Value);
        return source;
    }
}



public static class ResultExtensions
{
    /// <summary>
    ///     Flattens a nested Result&lt;Result&lt;T&gt;&gt; into Result&lt;T&gt;.
    ///     Useful when an implicit conversion accidentally wrapped a Result inside another Result.
    /// </summary>
    public static Result<T> Flatten<T>(this Result<Result<T>> nested)
    {
        if (!nested.Success)
        {
            return nested.Exception;
        }

        return nested.Value;
    }
}

public static class PipeExtensions
{
    extension<T, TResult>(T)
    {
        public static TResult operator |(T source, Func<T, TResult> func) => func(source);

        public static Result<TResult> operator | (T source, Func<T, Result<TResult>> func) => func(source);


    }
}

public readonly struct PipeData<TValue>(TValue value)
{
    public TValue Value { get; } = value;
}