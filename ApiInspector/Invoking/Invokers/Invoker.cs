using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiInspector.Application;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InstanceCreators;
using ApiInspector.Models;
using BOA.Base;
using BOA.Common.Types;
using static ApiInspector.Serialization.Serializer;
using static ApiInspector.Utility;
using static FunctionalPrograming.FPExtensions;
using Task = System.Threading.Tasks.Task;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The application domain helper
    /// </summary>
    class AppDomainHelper
    {
        #region Static Fields
        /// <summary>
        ///     The listen messages
        /// </summary>
        static bool listenMessages;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Calls the in isolated domain.
        /// </summary>
        public static TOutput CallInIsolatedDomain<T, TOutput>(Func<T, TOutput> action, Action<string> trace, Func<Exception, TOutput> onFail)
        {
            var setup = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ApplicationBase = Path.GetDirectoryName(typeof(AppDomainHelper).Assembly.Location)
            };

            var domain = AppDomain.CreateDomain("AppDomainIsolation:" + Guid.NewGuid(), null, setup);
            domain.AssemblyResolve += BoaAssemblyResolver.DomainAssemblyResolve;

            var type = typeof(T);

            var instance = (T) domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName ?? throw new InvalidOperationException());

            listenMessages = true;

            var listen = fun(() =>
            {
                while (listenMessages)
                {
                    Thread.Sleep(5);
                    if (domain.GetData("trace") is string message)
                    {
                        trace(message);
                    }
                }
            });

            Task.Run(listen);

            var output = default(TOutput);
            try
            {
                output = action(instance);

                AppDomain.Unload(domain);
            }
            catch (Exception exception)
            {
                output = onFail(exception);
            }

            listenMessages = false;

            return output;
        }
        #endregion
    }

    /// <summary>
    ///     The invoker
    /// </summary>
    static class Invoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified environment information.
        /// </summary>
        public static InvokeOutput Invoke(EnvironmentInfo environmentInfo, Action<string> trace, InvocationInfo invocationInfo, int scenarioIndex)
        {
            return AppDomainHelper.CallInIsolatedDomain((InvokeExternal instance) => instance.Invoke(environmentInfo, invocationInfo, scenarioIndex), trace,ToOutput);
        }

        static InvokeOutput ToOutput(Exception exception)
        {
            return new InvokeOutput(exception);
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static InvokeOutput Invoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo, int scenarioIndex)
        {
            var fail = fun((Exception exception) =>
            {
                boaContext.Dispose();

                return ToOutput(exception);
            });

            try
            {
                return UnsafeInvoke(boaContext, trace, invocationInfo, scenarioIndex);
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
                var assembly     = Assembly.Load(File.ReadAllBytes(assemblyPath));

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
        static InvokeOutput UnsafeInvoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo, int scenarioIndex)
        {

            var success = fun((object responseOfInvokeMethod) => new InvokeOutput(SerializeToJsonDoNotIgnoreDefaultValues(responseOfInvokeMethod)));

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

                return InvokeOutput.EODSuccess;
            });
            var eodOutput = tryToInvokeAsEndOfDay();
            if (eodOutput != null)
            {
                return eodOutput;
            }

            trace($"Started to search method: {invocationInfo.MethodName}");

            var findMethod = fun(() =>
            {

                var mi = targetType.GetMethods(AllBindings).FirstOrDefault(m=>m.ToString().Split(' ')[1] == invocationInfo.MethodName);

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
                var parameters = invocationInfo.Scenarios[scenarioIndex].MethodParameters ?? new List<InvocationMethodParameterInfo>();

                return InvocationParameterPreparer.Prepare(parameters, methodInfo, boaContext, trace);
            });

            var invocationParameters = prepareParameters();



            trace("Invoke started. Response waiting...");

            var invokeMethod = fun(() =>
            {
                var tryInvokeStaticMethod = fun(() =>
                {
                    if (!methodInfo.IsStatic)
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    return  new TryMethodInvokeResponse(methodInfo.Invoke(null, invocationParameters.ToArray()));
                });

                var tryInvokeAsCardServiceMethod = fun(() =>
                {
                    var methodName = invocationInfo.MethodName;

                    if (targetType.Namespace?.StartsWith("BOA.Card.Services.", StringComparison.OrdinalIgnoreCase) != true)
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    var cardServiceMethodInvokerInput = new CardServiceMethodInvokerInput(targetType, methodName, invocationParameters);

                    return new TryMethodInvokeResponse(CardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput, trace, boaContext));

                });

                var tryInvokeNonStaticMethod = fun(() =>
                {
                    if (methodInfo.IsStatic)
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    var instance = InstanceCreator.Create(targetType, boaContext);

                    return new TryMethodInvokeResponse(methodInfo.Invoke(instance, invocationParameters.ToArray()));
                });

                var tryMethodInvokeResponse = tryInvokeStaticMethod();
                if (tryMethodInvokeResponse != TryMethodInvokeResponse.NotInvoked)
                {
                    return tryMethodInvokeResponse.MethodReturnValue;
                }

                tryMethodInvokeResponse = tryInvokeAsCardServiceMethod();
                if (tryMethodInvokeResponse != TryMethodInvokeResponse.NotInvoked)
                {
                    return tryMethodInvokeResponse.MethodReturnValue;
                }

                tryMethodInvokeResponse = tryInvokeNonStaticMethod();
                if (tryMethodInvokeResponse != TryMethodInvokeResponse.NotInvoked)
                {
                    return tryMethodInvokeResponse.MethodReturnValue;
                }

                throw new InvalidOperationException("Unknown invocation type.");
            });

            var stopwatch = Stopwatch.StartNew();

            var responseInvokeMethod = invokeMethod();

            

            stopwatch.Stop();

            trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

            boaContext.Dispose();

            

            var output = success(responseInvokeMethod);

            var serializeParameter = fun((object instance) =>
            {
                if (instance == null || instance is ObjectHelper)
                {
                    return null;
                }

                return SerializeToJson(instance);
            });

            output.InvocationParameters = invocationParameters.Select(serializeParameter).ToList();

            return output;

        }

        sealed class TryMethodInvokeResponse
        {
            public readonly object MethodReturnValue;

            public  TryMethodInvokeResponse(object methodReturnValue)
            {
                MethodReturnValue = methodReturnValue;
            }

            
            TryMethodInvokeResponse()
            {
                
            }

            public  static readonly TryMethodInvokeResponse NotInvoked = new TryMethodInvokeResponse();

        }

        #endregion

        /// <summary>
        ///     The invoke external
        /// </summary>
        class InvokeExternal : MarshalByRefObject
        {
            #region Public Methods
            /// <summary>
            ///     Invokes the specified environment information.
            /// </summary>
            public InvokeOutput Invoke(EnvironmentInfo environmentInfo, InvocationInfo invocationInfo, int scenarioIndex)
            {
                var trace = fun((string message) => { AppDomain.CurrentDomain.SetData("trace", message); });

                using (var boaContext = new BOAContext(environmentInfo, trace))
                {
                    return Invoker.Invoke(boaContext, trace, invocationInfo, scenarioIndex);
                }
            }
            #endregion
        }
    }
}