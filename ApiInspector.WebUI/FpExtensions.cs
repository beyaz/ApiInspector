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
}