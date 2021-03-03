using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiInspector.DataAccess;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InstanceCreators;
using ApiInspector.Models;
using ApiInspector.Plugins;
using BOA.Common.Types;
using static ApiInspector._;
using static ApiInspector.Invoking.__;
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
        [Serializable]
        sealed class AssemblyResolver
        {
            public string AssemblySearchDirectory;

            public Assembly Resolve(object sender, ResolveEventArgs args)
            {
                return ResolveBoaSystemAssembly(sender, args, AssemblySearchDirectory);
            }
        }

        /// <summary>
        ///     The listen messages
        /// </summary>
        static bool listenMessages;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Calls the in isolated domain.
        /// </summary>
        public static TOutput CallInIsolatedDomain<T, TOutput>(Func<T, TOutput> action, string assemblySearchDirectory, Action<string> trace, Func<Exception, TOutput> onFail)
        {
            var setup = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ApplicationBase = Path.GetDirectoryName(typeof(AppDomainHelper).Assembly.Location)
            };

            var domain = AppDomain.CreateDomain("AppDomainIsolation:" + Guid.NewGuid(), null, setup);
            
            domain.AssemblyResolve += new AssemblyResolver {AssemblySearchDirectory = assemblySearchDirectory}.Resolve;

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
        public static InvokeOutput Invoke(EnvironmentInfo environmentInfo, Action<string> trace, InvocationInfo invocationInfo, IReadOnlyList<InvocationMethodParameterInfo> parameters)
        {
            var invokeOutputAsJson = AppDomainHelper.CallInIsolatedDomain<InvokeExternal,string>(instance => instance.Invoke(environmentInfo, invocationInfo, parameters), invocationInfo.AssemblySearchDirectory, trace, ToInvokeOutputAsJson);

            return Deserialize<InvokeOutput>(invokeOutputAsJson);
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public static InvokeOutput Invoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo, IReadOnlyList<InvocationMethodParameterInfo> parameters)
        {
            var fail = fun((Exception exception) =>
            {
                boaContext.Dispose();

                return ToInvokeOutput(exception);
            });

            try
            {
                return UnsafeInvoke(boaContext, trace, invocationInfo, parameters);
            }
            catch (InvocationFailedException exception)
            {
                return fail(exception.InnerException);
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

        static bool IsBoaAuthenticationRequired(string assemblyName)
        {
            if (assemblyName == "BOA.OneDesigner.dll")
            {
                return false;
            }

            if (IsCardServiceAssembly(assemblyName))
            {
                return false;
            }

            if (assemblyName.StartsWith("BOA.Types."))
            {
                return false;
            }

            if (assemblyName.StartsWith("BOA."))
            {
                return true;
            }

            return false;
        }

        static InvokeOutput ToInvokeOutput(Exception exception)
        {
            return new InvokeOutput(exception);
        }

        static string ToInvokeOutputAsJson(Exception exception)
        {
            return SerializeToJson(new InvokeOutput(exception));
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        static InvokeOutput UnsafeInvoke(BOAContext boaContext, Action<string> trace, InvocationInfo invocationInfo, IReadOnlyList<InvocationMethodParameterInfo> parameters)
        {
            var success = fun((object responseOfInvokeMethod) => new InvokeOutput {ExecutionResponseAsJson = SerializeToJsonDoNotIgnoreDefaultValues(responseOfInvokeMethod)});

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

                var errorMessage = EndOfDayInvoker.Invoke(targetType);
                if (errorMessage == null)
                {
                    return EODSuccess;
                }

                return new InvokeOutput(new Exception(errorMessage));
            });
            var eodOutput = tryToInvokeAsEndOfDay();
            if (eodOutput != null)
            {
                return eodOutput;
            }

            trace($"Started to search method: {invocationInfo.MethodName}");

            var methodInfo = targetType.GetMethods(AllBindings).FirstOrDefault(m => m.GetMethodNameWithSignature() == invocationInfo.MethodName);
            if (methodInfo == null)
            {
                throw new Exception("Method not found.");
            }

            // TRY BOA Authenticate
            if (IsBoaAuthenticationRequired(invocationInfo.AssemblyName))
            {
                trace("Authentication is started. Because assembly name starts with BOA prefix.");

                boaContext.Authenticate();
            }

            trace("Preparing invocation parameters");

            var invocationParameters = InvocationParameterPreparer.Prepare(parameters ?? new List<InvocationMethodParameterInfo>(), methodInfo, boaContext, trace);

            trace("Invoke started. Response waiting...");

            var invokeMethod = fun(() =>
            {
                var tryInvokeStaticMethod = fun(() =>
                {
                    if (!methodInfo.IsStatic)
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    return new TryMethodInvokeResponse(InvokeStaticMethod(methodInfo, invocationParameters.ToArray()));
                });

                var tryInvokeAsCardServiceMethod = fun(() =>
                {
                    var methodName = invocationInfo.MethodName;

                    if (!IsCardServiceAssembly(targetType.Namespace))
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    var cardServiceMethodInvokerInput = new CardServiceMethodInvokerInput(targetType, methodName, invocationParameters);

                    return new TryMethodInvokeResponse(CardServiceMethodInvoker.Invoke(cardServiceMethodInvokerInput, trace, invocationInfo.Environment));
                });

                var tryInvokeNonStaticMethod = fun(() =>
                {
                    if (methodInfo.IsStatic)
                    {
                        return TryMethodInvokeResponse.NotInvoked;
                    }

                    var instance = InstanceCreator.Create(targetType, boaContext);

                    return new TryMethodInvokeResponse(InvokeNonStaticMethod(methodInfo, instance, invocationParameters.ToArray()));
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

            responseInvokeMethod = NormalizeInvokedMethodReturnValue(responseInvokeMethod);

            stopwatch.Stop();

            trace($"Successfully invoked in {stopwatch.Elapsed.Milliseconds} milliseconds.");

            boaContext.Dispose();

            var output = success(responseInvokeMethod);

            var serializeParameter = fun((object instance) =>
            {
                if (instance == null)
                {
                    return null;
                }

                return SerializeToJson(instance);
            });

            output.InvocationParameters = invocationParameters.Select(serializeParameter).ToList();

            return output;
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
            public string Invoke(EnvironmentInfo environmentInfo, InvocationInfo invocationInfo, IReadOnlyList<InvocationMethodParameterInfo> parameters)
            {
                PluginLoader.AttachPlugins();

                InvokeOutput output = null;

                var trace = fun((string message) => { AppDomain.CurrentDomain.SetData("trace", message); });

                using (var boaContext = new BOAContext(environmentInfo, trace))
                {
                    output = Invoker.Invoke(boaContext, trace, invocationInfo, parameters);
                }

                return SerializeToJson(output);
            }
            #endregion
        }

        sealed class TryMethodInvokeResponse
        {
            #region Static Fields
            public static readonly TryMethodInvokeResponse NotInvoked = new TryMethodInvokeResponse();
            #endregion

            #region Fields
            public readonly object MethodReturnValue;
            #endregion

            #region Constructors
            public TryMethodInvokeResponse(object methodReturnValue)
            {
                MethodReturnValue = methodReturnValue;
            }

            TryMethodInvokeResponse()
            {
            }
            #endregion
        }
    }
}