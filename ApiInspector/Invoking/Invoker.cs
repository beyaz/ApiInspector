using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using ApiInspector.Serialization;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoker
    /// </summary>
    class Invoker
    {
        #region Fields
        readonly InstanceCreator InstanceCreator = new InstanceCreator();

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
        public static Type InitializeTargetType(InvocationInfo invocationInfo)
        {
            var assemblyName = invocationInfo.AssemblyName;
            var className    = invocationInfo.ClassName;

            Type targetType = null;
            if (!IsSuccess(() => Type.GetType($"{className},{Path.GetFileNameWithoutExtension(assemblyName)}", true), ref targetType))
            {
                var assemblyPath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);
                var assembly     = Assembly.LoadFile(assemblyPath);

                if (!IsSuccess(() => assembly.GetType(className, true), ref targetType))
                {
                    throw new TypeLoadException(className);
                }
            }

            return targetType;
        }

        public InvokeOutput Invoke(InvocationInfo invocationInfo)
        {
            var boaContext = new BOAContext(invocationInfo.Environment);

            var input = new InvokerInput(invocationInfo, trace, boaContext);

            return Invoke(input);
        }

        InvokeOutput TryToInvokeAsEndOfDay(InvokerInput input)
        {
            var invocationInfo = input.InvocationInfo;

            var methodName = invocationInfo.MethodName;

            if (methodName != EndOfDay.MethodAccessText)
            {
                return null;
            }

            try
            {
                BOAContext.CreateTestContext(invocationInfo.Environment).AuthenticateUser();

                new EndOfDayInvoker().Invoke(input.TargetType);

                return new InvokeOutput(null, null, null);
            }
            catch (Exception exception)
            {
                return Fail(exception, input.BoaContext);
            }
        }
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public InvokeOutput Invoke(InvokerInput input)
        {
            var boaContext = input.BoaContext;

            var invocationInfo = input.InvocationInfo;

            var methodName = invocationInfo.MethodName;
            var className  = invocationInfo.ClassName;

            trace($"Started to search class: {className}");

            var targetType = input.TargetType = InitializeTargetType(invocationInfo);
            
            // E O D 
            {
                var output = TryToInvokeAsEndOfDay(input);
                if (output != null)
                {
                    return output;
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
                return Fail(e, boaContext);
            }

            if (methodInfo == null)
            {
                return Fail(new Exception("Method not found."), boaContext);
            }

            input.MethodInfo = methodInfo;

            trace("Preparing invocation parameters");
            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var invocationParameters = new List<object>();

            var methodParametersInDotNet =  input.MethodInfo.GetParameters();

            var parameterAdapterInputs = new List<ParameterAdapterInput>();

            for (var i = 0; i < methodParametersInDotNet.Length; i++)
            {
                var parameterAdapterInput = new ParameterAdapterInput
                {
                    InvocationValue = parameters[i].Value,
                    ParameterInfo   = methodParametersInDotNet[i],
                    BoaContext      = boaContext
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

            input.InvocationParameters = invocationParameters;
            try
            {
                trace("Invoke started. Response waiting...");

                var stopwatch = Stopwatch.StartNew();

                var response = InvokeMethod(input);

                stopwatch.Stop();

                trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                boaContext.Dispose();

                return new InvokeOutput(null, response, serializer.SerializeToJsonDoNotIgnoreDefaultValues(response));
            }
            catch (Exception exception)
            {
                return Fail(exception, boaContext);
            }
        }
        #endregion

        #region Methods
        static InvokeOutput Success(object response)
        {
            return new InvokeOutput(response);
        }

        static InvokeOutput TryInvokeAsCardServiceMethod(InvokerInput input)
        {
            var targetType           = input.TargetType;
            var invocationParameters = input.InvocationParameters;
            var boaContext           = input.BoaContext;
            var methodName           = input.InvocationInfo.MethodName;
            var trace                = input.Trace;

            if (targetType.Namespace?.StartsWith("BOA.Card.Services.", StringComparison.OrdinalIgnoreCase) != true)
            {
                return null;
            }

            var cardServiceMethodInvokerInput = new CardServiceMethodInvokerInput(targetType, methodName, invocationParameters, trace, boaContext);

            var cardServiceMethodInvoker = new CardServiceMethodInvoker();

            var response = cardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput);

            return Success(response);
        }

        static InvokeOutput TryInvokeStaticMethod(InvokerInput input)
        {
            var methodInfo           = input.MethodInfo;
            var invocationParameters = input.InvocationParameters;

            if (!methodInfo.IsStatic)
            {
                return null;
            }

            var response = methodInfo.Invoke(null, invocationParameters.ToArray());

            return Success(response);
        }

        /// <summary>
        ///     Fails the specified exception.
        /// </summary>
        InvokeOutput Fail(Exception exception, BOAContext boaContext)
        {
            boaContext.Dispose();

            return new InvokeOutput(exception, exception, serializer.SerializeToJson(exception));
        }

        object InvokeMethod(InvokerInput input)
        {
            var invokeOutput = TryInvokeStaticMethod(input);
            if (invokeOutput == null)
            {
                invokeOutput = TryInvokeAsCardServiceMethod(input);
                if (invokeOutput == null)
                {
                    invokeOutput = TryInvokeNonStaticMethod(input);
                    if (invokeOutput == null)
                    {
                        throw new InvalidOperationException("Unknown invocation type.");
                    }
                }
            }

            var response = invokeOutput.ExecutionResponse;
            return response;
        }

        InvokeOutput TryInvokeNonStaticMethod(InvokerInput input)
        {
            var targetType           = input.TargetType;
            var invocationParameters = input.InvocationParameters;
            var boaContext           = input.BoaContext;
            var methodInfo           = input.MethodInfo;

            if (methodInfo.IsStatic)
            {
                return null;
            }

            var instance = InstanceCreator.Create(targetType, boaContext);

            var response = methodInfo.Invoke(instance, invocationParameters.ToArray());

            return Success(response);
        }
        #endregion
    }
}