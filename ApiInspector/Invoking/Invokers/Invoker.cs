using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InstanceCreators;
using ApiInspector.Models;
using BOA.Common.Types;
using static ApiInspector.Serialization.Serializer;
using static ApiInspector.Utility;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The invoker
    /// </summary>
    static class Invoker
    {
        #region Public Methods
        public static InvokeOutput Invoke(EnvironmentInfo environmentInfo, Action<string> trace, InvocationInfo invocationInfo)
        {
            using (var boaContext = new BOAContext(environmentInfo, trace))
            {
                return Invoke(boaContext, trace, invocationInfo);
            }
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static InvokeOutput Invoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo)
        {
            var fail = fun((Exception exception) =>
            {
                boaContext.Dispose();

                return new InvokeOutput(exception, exception, SerializeToJson(exception));
            });

            try
            {
                return UnsafeInvoke(boaContext, trace, invocationInfo);
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the type of the target.
        /// </summary>
        static Type GetTargetType(InvocationInfo invocationInfo)
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
        static InvokeOutput UnsafeInvoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo)
        {
            var findTargetType = fun(() =>
            {
                trace($"Started to search class: {invocationInfo.ClassName}");

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
                trace("Authentication is started. Because assembly name starts with BOA prefix.");

                boaContext.Authenticate();
            }

            trace("Preparing invocation parameters");

            var prepareParameters = fun(() =>
            {
                var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

                return InvocationParameterPreparer.Prepare(parameters, methodInfo, boaContext, trace);
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

                    var responseCardServiceInvoke = CardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput, trace, boaContext);

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

            return new InvokeOutput(null, responseInvokeMethod, SerializeToJsonDoNotIgnoreDefaultValues(responseInvokeMethod));
        }
        #endregion
    }
}