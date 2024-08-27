using System.Collections.Immutable;

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

    public static (T value, Exception exception) Try<T>(Func<T> func)
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

sealed class ExecutionException : Exception
{
    public IReadOnlyList<Info> InfoList { get; init; }
}

sealed record Info
{
    public int Code{ get; init; }

    public string Message { get; set; }

    public bool IsError { get; init; }

    public Exception Exception { get; init; }
    
    public static implicit operator Info(Exception exception)
    {
        return new Info { Exception = exception, IsError = true};
    }
}

sealed record Result<T>
{
    public T Value { get; init; }

    public ImmutableList<Info> InfoList { get; init; }

    public bool Success
    {
        get
        {
            if (InfoList is not null)
            {
                if (InfoList.Any(x=>x.IsError))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static implicit operator Result<T>(T value)
    {
        return new() { Value = value };
    }
    public static implicit operator Result<T>(Exception exception)
    {
        Info info = exception;

        return new Result<T> { InfoList = new[] { info }.ToImmutableList() };
    }

    public static implicit operator Result<T>((T value, Exception exception) tuple)
    {
        if (tuple.exception is not null)
        {
            Info info = tuple.exception;

            return new Result<T>
            {
                Value = tuple.value,
                InfoList = new[] { info }.ToImmutableList()
            };
        }

        return tuple.value;
    }

    public T Unwrap()
    {
        if (Success)
        {
            return Value;
        }

        throw new ExecutionException
        {
            InfoList = InfoList
        };
    }
}