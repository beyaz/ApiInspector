using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.Models;
using BOA.Base;
using BOA.Base.Data;
using Newtonsoft.Json;

namespace ApiInspector.Domain
{
    /// <summary>
    ///     The invoker
    /// </summary>
    class Invoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public object Invoke(InvocationInfo invocationInfo, ExecutionDataContext executionDataContext)
        {
            var assemblyName = invocationInfo.AssemblyName;
            var methodName   = invocationInfo.MethodName;
            var className    = invocationInfo.ClassName;

            var targetType = Type.GetType($"{className},{Path.GetFileNameWithoutExtension(assemblyName)}", true);

            var instance = CreateInstance(targetType, executionDataContext);

            var methodInfo = targetType.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var methodParameters = parameters.ConvertAll(ConvertToMethodInvocationParameter).ToArray();

            return methodInfo.Invoke(instance, methodParameters);
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
        static object CreateInstance(Type targetType, ExecutionDataContext executionDataContext)
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
                        executionDataContext
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
                    objectHelper.Context = executionDataContext;
                }

                return instance;
            }
        }
        #endregion
    }
}