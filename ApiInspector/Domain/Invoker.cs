using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using BOA.Base;
using BOA.Base.Data;
using BOA.DataFlow;
using Newtonsoft.Json;
using static ApiInspector.Domain.Data;

namespace ApiInspector.Domain
{
    /// <summary>
    ///     The invoker
    /// </summary>
    static class Invoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static void Invoke(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var assemblyName = invocationInfo.AssemblyName;
            var methodName   = invocationInfo.MethodName;
            var className    = invocationInfo.ClassName;

            Type targetType = null;

            if (!Utility.IsSuccess(() => Type.GetType($"{className},{Path.GetFileNameWithoutExtension(assemblyName)}", true), ref targetType))
            {
                var assemblyPath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);
                var assembly     = Assembly.LoadFile(assemblyPath);

                if (!Utility.IsSuccess(() => assembly.GetType(className, true), ref targetType))
                {
                    throw new TypeLoadException(className);
                }
            }

            var instance = CreateInstance(context, targetType);

            var methodInfo = targetType.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var methodParameters = parameters.ConvertAll(ConvertToMethodInvocationParameter).ToArray();

            var response = methodInfo.Invoke(instance, methodParameters);

            context.Update(ExecutionResponse, response);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Converts to method invocation parameter.
        /// </summary>
        static object ConvertToMethodInvocationParameter(InvocationMethodParameterInfo info)
        {
            return JsonConvert.DeserializeObject(info.ValueAsJson, info.Type);
        }

        /// <summary>
        ///     Creates the instance.
        /// </summary>
        static object CreateInstance(DataContext context, Type targetType)
        {
            // constructor with ExecutionDataContext
            {
                var constructorInfo = targetType.GetConstructor(new[]
                {
                    typeof(ExecutionDataContext)
                });
                if (constructorInfo != null)
                {
                    var instance = constructorInfo.Invoke(new object[]
                    {
                        context.Get(BOAExecutionContext)
                    });
                    return instance;
                }
            }

            // simple constructor
            {
                var instance     = Activator.CreateInstance(targetType);
                var objectHelper = instance as ObjectHelper;
                if (objectHelper != null)
                {
                    objectHelper.Context = context.Get(BOAExecutionContext);
                }

                return instance;
            }
        }
        #endregion
    }
}