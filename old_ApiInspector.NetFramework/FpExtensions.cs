global using static ApiInspector.FpExtensions;

namespace ApiInspector;

static class FpExtensions
{
    internal static C ExecUntilNotNull<A,B,C>(A a,B b, Func<A,B, C>[] methods)
    {
        foreach (var func in methods)
        {
            var result = func(a,b);
            if (result is not null)
            {
                return result;
            }
        }

        return default;
    }
    
    internal static string ExecUntilNotNull<T>(T value, Func<T, string>[] methods)
    {
        foreach (var func in methods)
        {
            var result = func(value);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
    
    internal const string Tab = "    ";
    
    public static (T value, Exception exception) SafeInvoke<T>(Func<T> func)
    {
        try
        {
            return (func(), null);
        }
        catch (Exception exception)
        {
            return (default, exception);
        }
    }

    public static void Then<T>(this (T value, Exception exception) response, Action<T> onSuccess, Action<Exception> onFail)
    {
        if (response.exception is null)
        {
            onSuccess(response.value);
        }
        else
        {
            onFail(response.exception);
        }
    }


    public static (T value, Exception exception) Then<T>(this (T value, Exception exception) response, Action<T> onSuccess)
    {
        if (response.exception is null)
        {
            onSuccess(response.value);
        }

        return response;


    }

    public static (T value, Exception exception) TraceError<T>(this (T value, Exception exception) response, Action<Exception> onFail)
    {
        if (response.exception is not null)
        {
            onFail(response.exception);
        }

        return response;
    }

    public static T Unwrap<T>(this (T value, Exception exception) response)
    {
        if (response.exception is not null)
        {
            throw response.exception;
        }

        return response.value;
    }
}