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
    static class Invoker
    {
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
        public static InvokeOutput Invoke(BOAContext boaContext, Serializer serializer, ITracer tracer, InvocationInfo invocationInfo)
        {
            var fail = fun((Exception exception) =>
            {
                boaContext.Dispose();

                return new InvokeOutput(exception, exception, serializer.SerializeToJson(exception));
            });

            try
            {
                return UnsafeInvoke(boaContext, serializer, tracer, invocationInfo);
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        static InvokeOutput UnsafeInvoke(BOAContext boaContext, Serializer serializer, ITracer tracer, InvocationInfo invocationInfo)
        {
            

            var trace = fun((string message) => { tracer.Trace(message); });

            var findTargetType = fun(() =>
            {
                trace($"Started to search class: {invocationInfo.ClassName}");

                ApplicationScope.Update(InvocationSearchDirectory, invocationInfo.AssemblySearchDirectory);

                return GetTargetType(invocationInfo);
            });

            var targetType = findTargetType();

            var tryToInvokeAsEndOfDay = fun(() =>
            {
                var methodName = invocationInfo.MethodName;

                if (methodName != EndOfDay.MethodAccessText)
                {
                    return null;
                }

                boaContext.Authenticate(ChannelContract.EOD);

                new EndOfDayInvoker().Invoke(targetType);

                return new InvokeOutput(null, null, null);
            });
            var eodOutput = tryToInvokeAsEndOfDay();
            if (eodOutput != null)
            {
                return eodOutput;
            }

            trace($"Started to search method: {invocationInfo.MethodName}");

            var findMethod = fun(() =>
            {
                var mi = targetType.GetMethod(invocationInfo.MethodName, AllBindings);

                if (mi == null)
                {
                    throw new Exception("Method not found.");
                }

                return mi;
            });

            var methodInfo = findMethod();

            // TRY BOA Authenticate
            if (invocationInfo.AssemblyName.StartsWith("BOA.") && invocationInfo.AssemblyName != "BOA.OneDesigner.dll")
            {
                tracer.Trace("Authentication is started. Because assembly name starts with BOA prefix.");

                boaContext.Authenticate();
            }

            trace("Preparing invocation parameters");

            var prepareParameters = fun(() =>
            {
                var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

                return InvocationParameterPreparer.Prepare(parameters, methodInfo, boaContext, tracer.Trace);
            });

            var invocationParameters = prepareParameters();

            trace("Invoke started. Response waiting...");

            var invokeMethod = fun(() =>
            {
                var success = fun((object r) => new InvokeOutput(r));

                var tryInvokeStaticMethod = fun(() =>
                {
                    if (!methodInfo.IsStatic)
                    {
                        return null;
                    }

                    var responseStatInvoke = methodInfo.Invoke(null, invocationParameters.ToArray());

                    return success(responseStatInvoke);
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

                    return success(responseCardServiceInvoke);
                });

                var tryInvokeNonStaticMethod = fun(() =>
                {
                    if (methodInfo.IsStatic)
                    {
                        return null;
                    }

                    var instance = InstanceCreator.Create(targetType, boaContext);

                    var responseNonStaticInvoke = methodInfo.Invoke(instance, invocationParameters.ToArray());

                    return success(responseNonStaticInvoke);
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

            var stopwatch = Stopwatch.StartNew();

            var responseInvokeMethod = invokeMethod();

            stopwatch.Stop();

            trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

            boaContext.Dispose();

            return new InvokeOutput(null, responseInvokeMethod, serializer.SerializeToJsonDoNotIgnoreDefaultValues(responseInvokeMethod));
        }
        #endregion
    }
}