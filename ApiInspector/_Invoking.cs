using System;
using System.Collections.Generic;

namespace ApiInspector
{
    static partial class _
    {
        static readonly List<Func<object, object>> invokedMethodReturnValuePipe = new List<Func<object, object>>();

        public static void AddToInvokedMethodReturnValuePipe(Func<object, object> func)
        {
            invokedMethodReturnValuePipe.Add(func);
        }

        public static object NormalizeInvokedMethodReturnValue(object value)
        {
            foreach (var func in invokedMethodReturnValuePipe)
            {
                value = func(value);
            }

            return value;
        }
    }
}