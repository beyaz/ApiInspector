using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using BOA.Base;
using BOA.Base.Data;
using BOA.DataFlow;
using Newtonsoft.Json;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoker
    /// </summary>
    static class Invoker
    {
        #region Static Fields
        /// <summary>
        ///     The boa execution context
        /// </summary>
        public static DataKey<ObjectHelper> BOAExecutionContext = new DataKey<ObjectHelper>(nameof(BOAExecutionContext));

        /// <summary>
        ///     The execution response
        /// </summary>
        public static DataKey<object> ExecutionResponse = new DataKey<object>(nameof(ExecutionResponse));

        /// <summary>
        ///     The execution response as json
        /// </summary>
        public static DataKey<string> ExecutionResponseAsJson = new DataKey<string>(nameof(ExecutionResponseAsJson));

        /// <summary>
        ///     The invocation information
        /// </summary>
        public static DataKey<InvocationInfo> InvocationInfo = new DataKey<InvocationInfo>(nameof(InvocationInfo));

        public static DataKey<Action<string>> Trace = new DataKey<Action<string>>(nameof(Trace));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static void Invoke(DataContext context)
        {
            var trace = context.Get(Trace);

            var invocationInfo = context.Get(InvocationInfo);

            var assemblyName = invocationInfo.AssemblyName;
            var methodName   = invocationInfo.MethodName;
            var className    = invocationInfo.ClassName;

            Type targetType = null;

            trace($"Started to search class: {className}");

            if (!IsSuccess(() => Type.GetType($"{className},{Path.GetFileNameWithoutExtension(assemblyName)}", true), ref targetType))
            {
                var assemblyPath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);
                var assembly     = Assembly.LoadFile(assemblyPath);

                if (!IsSuccess(() => assembly.GetType(className, true), ref targetType))
                {
                    throw new TypeLoadException(className);
                }
            }

            var instance = CreateInstance(context, targetType);

            trace($"Started to search method: {methodName}");
            var methodInfo = targetType.GetMethod(methodName, AllBindings);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            trace($"Preparing invocation parameters");
            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var invocationParameters     = new List<object>();
            var methodParametersInDotNet = methodInfo.GetParameters();
            for (var i = 0; i < methodParametersInDotNet.Length; i++)
            {
                var value               = parameters[i].Value;
                var targetParameterType = methodParametersInDotNet[i].ParameterType;

                if (targetParameterType == typeof(ObjectHelper))
                {
                    value = new ObjectHelper {Context = context.Get(BOAExecutionContext).Context};
                }

                if (targetParameterType.FullName != typeof(string).FullName && targetParameterType.IsClass && value is string)
                {
                    value = JsonConvert.DeserializeObject((string) value, targetParameterType);
                }

                value = Convert.ChangeType(value, targetParameterType);
                invocationParameters.Add(value);
            }

            trace("Invoke started. Response waiting...");
            var response = methodInfo.Invoke(instance, invocationParameters.ToArray());
            trace($"Successfully invoked.");

            context.Update(ExecutionResponse, response);
            context.Update(ExecutionResponseAsJson, SerializeToJson(response));
        }
        #endregion

        #region Methods
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
                    objectHelper.Context = context.Get(BOAExecutionContext).Context;
                }

                return instance;
            }
        }
        #endregion
    }
}