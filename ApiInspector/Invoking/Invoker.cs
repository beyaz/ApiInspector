using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.Base;
using BOA.Base.Data;
using Newtonsoft.Json;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoke output
    /// </summary>
    class InvokeOutput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvokeOutput" /> class.
        /// </summary>
        public InvokeOutput(Exception error, object executionResponse, string executionResponseAsJson)
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
        #region Fields
        /// <summary>
        ///     The serializer
        /// </summary>
        readonly Serializer serializer = new Serializer();

        /// <summary>
        ///     The trace
        /// </summary>
        readonly Action<string> trace;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Invoker" /> class.
        /// </summary>
        public Invoker(Action<string> trace)
        {
            this.trace = trace;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public InvokeOutput Invoke(InvocationInfo invocationInfo)
        {
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

                object response = null;
                if (methodInfo.IsStatic)
                {
                    response = methodInfo.Invoke(null, invocationParameters.ToArray());
                }
                else
                {
                    var instance = CreateInstance(targetType, boaContext);

                    response = methodInfo.Invoke(instance, invocationParameters.ToArray());
                }

                trace("Successfully invoked.");

                boaContext.Dispose();

                return new InvokeOutput(null, response, serializer.SerializeToJsonDoNotIgnoreDefaultValues(response));
            }
            catch (Exception e)
            {
                trace($"FAIL:{e}");

                boaContext.Dispose();
                return new InvokeOutput(e, e, serializer.SerializeToJson(e));
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