namespace ApiInspector.WebUI;

partial class Extensions
{
    public static void IgnoreException(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static bool IsNullOrWhiteSpaceOrEmptyJsonObject(this string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText) ||
            string.IsNullOrWhiteSpace(jsonText.Replace("{", string.Empty).Replace("}", string.Empty)))
        {
            return true;
        }

        return false;
    }

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

    public static B Then<T,B>(this (T value, Exception exception) response, Func<T,B> onSuccess, Func<Exception,B> onFail)
    {
        if (response.exception is null)
        {
            return onSuccess(response.value);
        }

        return onFail(response.exception);
    }

    public static C Flow<A,B,C>(A value, Func<A,(B, Exception)> nextFunc, Func<B,C> onSuccess)
    {
        var (b, exception) = nextFunc(value);
        if (exception is null)
        {
            return onSuccess(b);
        }

        return default;
    }
}