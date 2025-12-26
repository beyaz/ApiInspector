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

  


}



sealed record Info
{
    public string Code { get; init; }

    public string Message { get; set; }

    public bool IsError { get; init; }

    public Exception Exception { get; init; }

    public static implicit operator Info(Exception exception)
    {
        return new Info { Exception = exception, IsError = true };
    }

    public override string ToString()
    {
        var items = new List<string>();

        if (Code is not null)
        {
            items.Add($"{nameof(Code)}:{Code}");
        }

        if (Message is not null)
        {
            items.Add($"{nameof(Message)}:{Message}");
        }

        if (Exception is not null)
        {
            items.Add($"{nameof(Exception)}:{Exception}");
        }

        return string.Join(", ", items);
    }
}

sealed record InfoCollection
{
    ImmutableList<Info> records = ImmutableList<Info>.Empty;

    public static implicit operator InfoCollection(Info info)
    {
        return new InfoCollection { records = ImmutableList<Info>.Empty.Add(info) };
    }

    public bool Success => !records.Any(x => x.IsError);

    public override string ToString()
    {
        return string.Join(Environment.NewLine, records);
    }

    public static implicit operator InfoCollection(Info[] infoArray)
    {
        return new InfoCollection { records = ImmutableList<Info>.Empty.AddRange(infoArray) };
    }

    public static implicit operator string(InfoCollection infoCollection)
    {
        return infoCollection?.ToString();
    }
}

sealed record Result_old<T>
{
    public T Value { get; init; }

    public InfoCollection InfoCollection { get; init; }

    public bool Fail => !Success;
    
    public bool Success
    {
        get
        {
            if (InfoCollection is not null)
            {
                return InfoCollection.Success;
            }

            return true;
        }
    }

    public static implicit operator Result_old<T>(T value)
    {
        return new() { Value = value };
    }

    public static implicit operator Result_old<T>(Exception exception)
    {
        Info info = exception;

        return new Result_old<T> { InfoCollection = info };
    }

    public static implicit operator Result_old<T>(InfoCollection infoCollection)
    {
        return new Result_old<T> { InfoCollection = infoCollection };
    }

    public static implicit operator Result_old<T>(Info[] infoArray)
    {
        return new Result_old<T> { InfoCollection = infoArray };
    }

    public static implicit operator Result_old<T>((T value, Exception exception) tuple)
    {
        if (tuple.exception is not null)
        {
            Info info = tuple.exception;

            return new Result_old<T>
            {
                Value          = tuple.value,
                InfoCollection = info
            };
        }

        return tuple.value;
    }


    public Result_old<A> Then<A>(Func<T, A> next)
    {
        if (Success)
        {
            return next(Value);
        }

        return InfoCollection;
    }

    public void Then(Action<T> ok, Action<InfoCollection> nok)
    {
        if (Success)
        {
            ok(Value);
            return;
        }

        nok(InfoCollection);
    }
}