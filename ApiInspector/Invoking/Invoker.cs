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
    ///     The invoker input
    /// </summary>
    class InvokerInput
    {
        #region Fields
        /// <summary>
        ///     The invocation information
        /// </summary>
        public InvocationInfo InvocationInfo;

        /// <summary>
        ///     The trace
        /// </summary>
        public Action<string> Trace;
        #endregion
    }

    /// <summary>
    ///     The invoker output
    /// </summary>
    class InvokerOutput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvokerOutput" /> class.
        /// </summary>
        public InvokerOutput(Exception error, object executionResponse, string executionResponseAsJson)
        {
            Error                   = error;
            ExecutionResponse       = executionResponse;
            ExecutionResponseAsJson = executionResponseAsJson;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the error.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        ///     Gets the execution response.
        /// </summary>
        public object ExecutionResponse { get; }

        /// <summary>
        ///     The execution response as json
        /// </summary>
        public string ExecutionResponseAsJson { get; }
        #endregion
    }

    /// <summary>
    ///     The invoker
    /// </summary>
    class Invoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static InvokerOutput Invoke(DataContext context, InvokerInput invokerInput)
        {
            var trace          = invokerInput.Trace;
            var invocationInfo = invokerInput.InvocationInfo;

            var boaContext = new BOAContext(invocationInfo.Environment);

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

            var instance = CreateInstance(targetType, boaContext);

            trace($"Started to search method: {methodName}");
            var methodInfo = targetType.GetMethod(methodName, AllBindings);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            trace("Preparing invocation parameters");
            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var invocationParameters     = new List<object>();
            var methodParametersInDotNet = methodInfo.GetParameters();
            for (var i = 0; i < methodParametersInDotNet.Length; i++)
            {
                var value               = parameters[i].Value;
                var targetParameterType = methodParametersInDotNet[i].ParameterType;

                if (targetParameterType == typeof(ObjectHelper))
                {
                    value = new ObjectHelper {Context = boaContext.GetObjectHelper().Context};
                }

                if (targetParameterType.FullName != typeof(string).FullName && targetParameterType.IsClass && value is string)
                {
                    value = JsonConvert.DeserializeObject((string) value, targetParameterType);
                }

                value = Convert.ChangeType(value, targetParameterType);
                invocationParameters.Add(value);
            }

            try
            {
                trace("Invoke started. Response waiting...");
                var response = methodInfo.Invoke(instance, invocationParameters.ToArray());
                trace("Successfully invoked.");

                boaContext.Dispose();

                return new InvokerOutput(null, response, SerializeToJson(response, false));
            }
            catch (Exception e)
            {
                trace($"FAIL:{e}");

                boaContext.Dispose();
                return new InvokerOutput(e, e, SerializeToJson(e));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the instance.
        /// </summary>
        static object CreateInstance(Type targetType, BOAContext boaContext)
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
                        boaContext.GetObjectHelper().Context
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
                    objectHelper.Context = boaContext.GetObjectHelper().Context;
                }

                return instance;
            }
        }
        #endregion
    }
}