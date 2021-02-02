using System;
using System.Reflection;

namespace ApiInspector
{
    class InvocationFailedException:Exception
    {
        public InvocationFailedException(Exception exception):base(null,exception)
        {
        }
    }

    static partial class _
    {
        public static object InvokeStaticMethod(MethodInfo methodInfo, object[] parameters)
        {
            try
            {
                return methodInfo.Invoke(null, parameters);
            }
            catch (Exception e)
            {
                throw new InvocationFailedException(e.InnerException ?? e);
            }
        }

        public static object InvokeNonStaticMethod(MethodInfo methodInfo, object instance, object[] parameters)
        {
            try
            {
                return methodInfo.Invoke(instance, parameters);
            }
            catch (Exception e)
            {
                throw new InvocationFailedException(e.InnerException ?? e);
            }
        }
    }
}