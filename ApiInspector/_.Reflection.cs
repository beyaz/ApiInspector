using System;
using System.Linq;

namespace ApiInspector
{
    static partial class _
    {
        public static string GetFullName(Type type)
        {
            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length>0 && !type.IsGenericTypeDefinition )
            {
                return GetFullName(type.GetGenericTypeDefinition()) + "<" + string.Join(",", genericArguments.Select(GetFullName)) + ">";
            }

            return type.FullName;
        }

        public static bool CanPresentSimpleTextBox(string fullTypeName)
        {
            var types = new[]
            {
                typeof(string),
                typeof(DateTime),
                typeof(DateTime?),

                // numbers
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),

                // nullable numbers
                typeof(byte?),
                typeof(short?),
                typeof(int?),
                typeof(long?),

                // unsigned numbers
                typeof(ushort),
                typeof(uint),
                typeof(ulong),

                // unsigned nullable numbers
                typeof(ushort?),
                typeof(uint?),
                typeof(ulong?)
            };

            string getFullName(Type t)
            {
                var genericArguments = t.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var genericArgument = genericArguments[0];

                        return $"{typeof(Nullable<>).FullName}<{genericArgument.FullName}>";
                    }
                }

                return t.FullName;
            }

            return types.Any(t => getFullName(t) == fullTypeName);
        }
    }
}