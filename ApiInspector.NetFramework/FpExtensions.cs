namespace ApiInspector;

static class FpExtensions
{
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