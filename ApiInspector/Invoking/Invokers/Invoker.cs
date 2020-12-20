using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InstanceCreators;
using ApiInspector.Models;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Types;
using static ApiInspector.Application.App;
using static ApiInspector.Keys;
using static ApiInspector.Utility;
using static FunctionalPrograming.Extensions;

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
        public Invoker(ITracer tracer,
                       Serializer serializer,
                       BOAContext boaContext
        )
        {
            this.tracer     = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.boaContext = boaContext ?? throw new ArgumentNullException(nameof(boaContext));
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
            return Invoke(boaContext, serializer, tracer, invocationInfo);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        InvokeOutput Invoke(BOAContext boaContext, Serializer serializer, ITracer tracer, InvocationInfo invocationInfo)
        {
            var fail = fun((Exception exception) =>
            {
                boaContext.Dispose();

                return new InvokeOutput(exception, exception, serializer.SerializeToJson(exception));
            });

            var Success = fun((object response) => new InvokeOutput(response));

            var trace = fun((string message) => { tracer.Trace(message); });

            

            trace($"Started to search class: {invocationInfo.ClassName}");

            // INITIALIZE TargetType
            Type targetType = null;
            try
            {
                ApplicationScope.Update(InvocationSearchDirectory, invocationInfo.AssemblySearchDirectory);

                targetType = GetTargetType(invocationInfo);
            }
            catch (Exception e)
            {
                return fail(e);
            }

            // TRY CALL AS EOD
            {
                var tryToInvokeAsEndOfDay = fun(() =>
                {
                    var methodName = invocationInfo.MethodName;

                    if (methodName != EndOfDay.MethodAccessText)
                    {
                        return null;
                    }

                    try
                    {
                        boaContext.Authenticate(ChannelContract.EOD);

                        new EndOfDayInvoker().Invoke(targetType);

                        return new InvokeOutput(null, null, null);
                    }
                    catch (Exception exception)
                    {
                        return fail(exception);
                    }
                });
                var output = tryToInvokeAsEndOfDay();
                if (output != null)
                {
                    return output;
                }
            }

            trace($"Started to search method: {invocationInfo.MethodName}");

            // INITIALIZE METHOD INFO
            MethodInfo methodInfo = null;
            {
                try
                {
                    methodInfo = targetType.GetMethod(invocationInfo.MethodName, AllBindings);
                }
                catch (Exception e)
                {
                    return fail(e);
                }

                if (methodInfo == null)
                {
                    return fail(new Exception("Method not found."));
                }
            }

            if (invocationInfo.AssemblyName.StartsWith("BOA.") && invocationInfo.AssemblyName != "BOA.OneDesigner.dll")
            {
                tracer.Trace("Authentication is started. Because assembly name starts with BOA prefix.");
                try
                {
                    boaContext.Authenticate();
                }
                catch (Exception exception)
                {
                    return fail(exception);
                }
            }

            trace("Preparing invocation parameters");

            // PREPARE PARAMETERS
            IReadOnlyList<object> invocationParameters = null;
            {
                var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

                try
                {
                    invocationParameters = InvocationParameterPreparer.Prepare(parameters, methodInfo, boaContext, tracer.Trace);
                }
                catch (Exception exception)
                {
                    return fail(exception);
                }
            }

            trace("Invoke started. Response waiting...");

            var invokeMethod = fun(() =>
            {
                var tryInvokeStaticMethod = fun(() =>
                {
                    if (!methodInfo.IsStatic)
                    {
                        return null;
                    }

                    var responseStatInvoke = methodInfo.Invoke(null, invocationParameters.ToArray());

                    return Success(responseStatInvoke);
                });

                var tryInvokeAsCardServiceMethod = fun(() =>
                {
                    var methodName = invocationInfo.MethodName;

                    if (targetType.Namespace?.StartsWith("BOA.Card.Services.", StringComparison.OrdinalIgnoreCase) != true)
                    {
                        return null;
                    }

                    var cardServiceMethodInvokerInput = new CardServiceMethodInvokerInput(targetType, methodName, invocationParameters);

                    var responseCardServiceInvoke = CardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput, tracer.Trace, boaContext);

                    return Success(responseCardServiceInvoke);
                });

                var tryInvokeNonStaticMethod = fun(() =>
                {
                    if (methodInfo.IsStatic)
                    {
                        return null;
                    }

                    var instance = InstanceCreator.Create(targetType, boaContext);

                    var responseNonStaticInvoke = methodInfo.Invoke(instance, invocationParameters.ToArray());

                    return Success(responseNonStaticInvoke);
                });

                var invokeOutput = tryInvokeStaticMethod();
                if (invokeOutput != null)
                {
                    return invokeOutput.ExecutionResponse;
                }

                invokeOutput = tryInvokeAsCardServiceMethod();
                if (invokeOutput != null)
                {
                    return invokeOutput.ExecutionResponse;
                }

                invokeOutput = tryInvokeNonStaticMethod();
                if (invokeOutput != null)
                {
                    return invokeOutput.ExecutionResponse;
                }

                throw new InvalidOperationException("Unknown invocation type.");
            });
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var response = invokeMethod();

                stopwatch.Stop();

                trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                boaContext.Dispose();

                return new InvokeOutput(null, response, serializer.SerializeToJsonDoNotIgnoreDefaultValues(response));
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
        #endregion
    }
}