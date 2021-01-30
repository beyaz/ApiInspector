using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace ApiInspector
{
    static partial class _
    {
        #region NormalizeInvokedMethodReturnValue
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
        #endregion


        #region IsVoidMethod
        static readonly List<Func<MethodDefinition, bool>> isVoidMethodPipe = new List<Func<MethodDefinition, bool>>();

        public static void AddToIsVoidMethodPipe(Func<MethodDefinition, bool> func)
        {
            isVoidMethodPipe.Add(func);
        }

        public static bool IsVoidMethod(MethodDefinition methodDefinition)
        {
            foreach (var func in isVoidMethodPipe)
            {
                var isVoid = func(methodDefinition);
                if (isVoid)
                {
                    return true;
                }
            }

            return false;
        } 
        #endregion
    }
}