using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InstanceCreators;
using ApiInspector.Models;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Types;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The invoker
    /// </summary>
    class Invoker
    {
        #region Fields
        /// <summary>
        ///     The boa context
        /// </summary>
        readonly BOAContext boaContext;

        /// <summary>
        ///     The card service method invoker
        /// </summary>
        readonly CardServiceMethodInvoker cardServiceMethodInvoker;

        /// <summary>
        ///     The instance creator
        /// </summary>
        readonly InstanceCreator instanceCreator;

        /// <summary>
        ///     The invocation parameter preparer
        /// </summary>
        readonly InvocationParameterPreparer invocationParameterPreparer;

        /// <summary>
        ///     The serializer
        /// </summary>
        readonly Serializer serializer;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Invoker" /> class.
        /// </summary>
        public Invoker(ITracer                     tracer,
                       Serializer                  serializer,
                       InstanceCreator             instanceCreator,
                       BOAContext                  boaContext,
                       CardServiceMethodInvoker    cardServiceMethodInvoker,
                       InvocationParameterPreparer invocationParameterPreparer
        )
        {
            this.tracer                      = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.serializer                  = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.instanceCreator             = instanceCreator ?? throw new ArgumentNullException(nameof(instanceCreator));
            this.boaContext                  = boaContext ?? throw new ArgumentNullException(nameof(boaContext));
            this.cardServiceMethodInvoker    = cardServiceMethodInvoker ?? throw new ArgumentNullException(nameof(cardServiceMethodInvoker));
            this.invocationParameterPreparer = invocationParameterPreparer ?? throw new ArgumentNullException(nameof(invocationParameterPreparer));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the type of the target.
        /// </summary>
        public static Type GetTargetType(InvocationInfo invocationInfo)
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

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public InvokeOutput Invoke(InvocationInfo invocationInfo)
        {
            var input = new InvokerInput(invocationInfo);

            return Invoke(input);
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public InvokeOutput Invoke(InvokerInput input)
        {
            Func<Exception, InvokeOutput> fail = Fail;

            var invocationInfo = input.InvocationInfo;

            Trace($"Started to search class: {invocationInfo.ClassName}");

            // INITIALIZE TargetType
            {
                input.TargetType = GetTargetType(invocationInfo);
            }

            // TRY CALL AS EOD
            {
                var output = TryToInvokeAsEndOfDay(input);
                if (output != null)
                {
                    return output;
                }
            }

            Trace($"Started to search method: {invocationInfo.MethodName}");

            // INITIALIZE METHOD INFO
            {
                MethodInfo methodInfo = null;
                try
                {
                    methodInfo = input.TargetType.GetMethod(input.InvocationInfo.MethodName, AllBindings);
                }
                catch (Exception e)
                {
                    return fail(e);
                }

                if (methodInfo == null)
                {
                    return fail(new Exception("Method not found."));
                }

                input.MethodInfo = methodInfo;
            }

            if (invocationInfo.AssemblyName.StartsWith("BOA."))
            {
                tracer.Trace("Authentication is started. Because assembly name starts with BOA prefix.");
                boaContext.Authenticate();
            }

            Trace("Preparing invocation parameters");

            // PREPARE PARAMETERS
            {
                var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

                try
                {
                    input.InvocationParameters = invocationParameterPreparer.Prepare(parameters, input.MethodInfo);
                }
                catch (Exception exception)
                {
                    return fail(exception);
                }
            }

            Trace("Invoke started. Response waiting...");

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var response = InvokeMethod(input);

                stopwatch.Stop();

                Trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                boaContext.Dispose();

                return new InvokeOutput(null, response, serializer.SerializeToJsonDoNotIgnoreDefaultValues(response));
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Successes the specified response.
        /// </summary>
        static InvokeOutput Success(object response)
        {
            return new InvokeOutput(response);
        }

        /// <summary>
        ///     Tries the invoke static method.
        /// </summary>
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
        InvokeOutput Fail(Exception exception)
        {
            boaContext.Dispose();

            return new InvokeOutput(exception, exception, serializer.SerializeToJson(exception));
        }

        /// <summary>
        ///     Invokes the method.
        /// </summary>
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

        /// <summary>
        ///     The trace
        /// </summary>
        void Trace(string message)
        {
            tracer.Trace(message);
        }

        /// <summary>
        ///     Tries the invoke as card service method.
        /// </summary>
        InvokeOutput TryInvokeAsCardServiceMethod(InvokerInput input)
        {
            var targetType           = input.TargetType;
            var invocationParameters = input.InvocationParameters;
            var methodName           = input.InvocationInfo.MethodName;

            if (targetType.Namespace?.StartsWith("BOA.Card.Services.", StringComparison.OrdinalIgnoreCase) != true)
            {
                return null;
            }

            var cardServiceMethodInvokerInput = new CardServiceMethodInvokerInput(targetType, methodName, invocationParameters);

            var response = cardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput);

            return Success(response);
        }

        /// <summary>
        ///     Tries the invoke non static method.
        /// </summary>
        InvokeOutput TryInvokeNonStaticMethod(InvokerInput input)
        {
            var targetType           = input.TargetType;
            var invocationParameters = input.InvocationParameters;
            var methodInfo           = input.MethodInfo;

            if (methodInfo.IsStatic)
            {
                return null;
            }

            var instance = instanceCreator.Create(targetType, boaContext);

            var response = methodInfo.Invoke(instance, invocationParameters.ToArray());

            return Success(response);
        }

        /// <summary>
        ///     Tries to invoke as end of day.
        /// </summary>
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
                boaContext.Authenticate(ChannelContract.EOD);

                new EndOfDayInvoker().Invoke(input.TargetType);

                return new InvokeOutput(null, null, null);
            }
            catch (Exception exception)
            {
                return Fail(exception);
            }
        }
        #endregion
    }
}