using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.Base;
using BOA.Base.Data;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoker
    /// </summary>
    class Invoker
    {
        #region Fields
        /// <summary>
        ///     The parameter adapters
        /// </summary>
        readonly IParameterAdapter[] parameterAdapters =
        {
            new ParameterAdapterForObjectType(),
            new ParameterAdapterForStringType(),
            new ParameterAdapterForObjectHelperType(),
            new ParameterAdapterForSerializableTypes(),
            new ParameterAdapterForConvertibleTypes()
        };

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

            MethodInfo methodInfo = null;
            try
            {
                methodInfo = targetType.GetMethod(methodName, AllBindings);
            }
            catch (Exception e)
            {
                trace($"Failed when accessing method. {e}");

                boaContext.Dispose();

                return new InvokeOutput(e, e, serializer.SerializeToJson(e));
            }

            if (methodInfo == null)
            {
                return Fail(new Exception("Method not found."), boaContext);
            }

            trace("Preparing invocation parameters");
            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var invocationParameters = new List<object>();

            var methodParametersInDotNet = methodInfo.GetParameters();

            var parameterAdapterInputs = new List<ParameterAdapterInput>();

            for (var i = 0; i < methodParametersInDotNet.Length; i++)
            {
                var parameterAdapterInput = new ParameterAdapterInput
                {
                    InvocationValue = parameters[i].Value,
                    ParameterInfo   = methodParametersInDotNet[i],
                    boaContext      = boaContext
                };
                parameterAdapterInputs.Add(parameterAdapterInput);
            }

            foreach (var parameterAdapterInput in parameterAdapterInputs)
            {
                var stopwatch = Stopwatch.StartNew();

                var isAdapted = false;
                foreach (var parameterAdapter in parameterAdapters)
                {
                    try
                    {
                        isAdapted = parameterAdapter.TryAdapt(parameterAdapterInput);
                    }
                    catch (Exception exception)
                    {
                        return Fail(exception, boaContext);
                    }

                    if (isAdapted)
                    {
                        invocationParameters.Add(parameterAdapterInput.InvocationValue);
                        break;
                    }
                }

                if (isAdapted)
                {
                    stopwatch.Stop();
                    trace($"Parameter: {parameterAdapterInput.ParameterInfo.Name} calculated in {stopwatch.Elapsed.Milliseconds} milliseconds.");
                    continue;
                }

                return Fail(new Exception($"Parameter not adapted. Value: {parameterAdapterInput.InvocationValue}, target parameter type: {parameterAdapterInput.ParameterInfo.ParameterType}"), boaContext);
            }

            try
            {
                trace("Invoke started. Response waiting...");
                var stopwatch = Stopwatch.StartNew();

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

                stopwatch.Stop();
                trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                boaContext.Dispose();

                return new InvokeOutput(null, response, serializer.SerializeToJsonDoNotIgnoreDefaultValues(response));
            }
            catch (Exception exception)
            {
                trace($"Failed when invoking method. {exception}");

                return Fail(exception, boaContext);
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

        /// <summary>
        ///     Fails the specified exception.
        /// </summary>
        InvokeOutput Fail(Exception exception, BOAContext boaContext)
        {
            boaContext.Dispose();

            return new InvokeOutput(exception, exception, serializer.SerializeToJson(exception));
        }
        #endregion
    }
}