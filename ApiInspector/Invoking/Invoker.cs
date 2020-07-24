using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.DataFlow;
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


        public static void InitializeTargetType(DataContext context)
        {
            var invocationInfo = context.Get(InvocationContextKeys.InvocationInfo);

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

            context.Add(InvocationContextKeys.TargetType,targetType);
        }

        /// <summary>
        ///     Invokes the specified invocation information.
        /// </summary>
        public InvokeOutput Invoke(InvocationInfo invocationInfo)
        {
            var boaContext = new BOAContext(invocationInfo.Environment);


            var dataContext = new DataContext
            {
                {InvocationContextKeys.BOAContext, boaContext},
                {InvocationContextKeys.InvocationInfo, invocationInfo},
                {InvocationContextKeys.Trace, trace},
            };

            var methodName   = invocationInfo.MethodName;
            var className    = invocationInfo.ClassName;

            trace($"Started to search class: {className}");

            InitializeTargetType(dataContext);

            Type targetType = dataContext.Get(InvocationContextKeys.TargetType);

            if (methodName == EndOfDay.MethodAccessText)
            {
                try
                {
                    BOAContext.CreateTestContext(invocationInfo.Environment).AuthenticateUser();

                    new EndOfDayInvoker().Invoke(targetType);

                    return new InvokeOutput(null, null, null);
                }
                catch (Exception exception)
                {
                    return Fail(exception, boaContext);
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
                dataContext.Add(InvocationContextKeys.MethodInfo, methodInfo);
        dataContext.Add(InvocationContextKeys.InvocationParameters, invocationParameters);


                trace("Invoke started. Response waiting...");
                var stopwatch = Stopwatch.StartNew();

                var invokeSuccess = TryInvokeStaticMethod(dataContext);
                if (invokeSuccess == false)
                {
                    invokeSuccess = TryInvokeNonStaticMethod(dataContext);
                    if (invokeSuccess == false)
                    {
                        throw new InvalidOperationException("Unknown invocation type.");
                    }
                }

                var response = dataContext.Get(InvocationContextKeys.Response);

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
            var instance = InstanceCreatorForObjectHelperDerivedClasses.TryCreate(targetType, boaContext);
            if (instance != null)
            {
                return instance;
            }

            return InstanceCreatorDefault.TryCreate(targetType, boaContext);
        }

        static bool TryInvokeNonStaticMethod(DataContext context)
        {
            var targetType           = context.Get(InvocationContextKeys.TargetType);
            var boaContext           = context.Get(InvocationContextKeys.BOAContext);
            var methodInfo           = context.Get(InvocationContextKeys.MethodInfo);
            var invocationParameters = context.Get(InvocationContextKeys.InvocationParameters);

            if (methodInfo.IsStatic)
            {
                return false;
            }

            var instance = CreateInstance(targetType, boaContext);

            var response = methodInfo.Invoke(instance, invocationParameters.ToArray());

            context.Add(InvocationContextKeys.Response, response);

            return true;
        }

        static bool TryInvokeStaticMethod(DataContext context)
        {
            var methodInfo           = context.Get(InvocationContextKeys.MethodInfo);
            var invocationParameters = context.Get(InvocationContextKeys.InvocationParameters);

            if (!methodInfo.IsStatic)
            {
                return false;
            }

            var response = methodInfo.Invoke(null, invocationParameters.ToArray());

            context.Add(InvocationContextKeys.Response, response);

            return true;
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

    static class InvocationContextKeys
    {
        #region Static Fields
        public static DataKey<BOAContext>     BOAContext           = new DataKey<BOAContext>(nameof(BOAContext));
        public static DataKey<InvocationInfo> InvocationInfo       = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        public static DataKey<List<object>>   InvocationParameters = new DataKey<List<object>>(nameof(InvocationParameters));
        public static DataKey<MethodInfo>     MethodInfo           = new DataKey<MethodInfo>(nameof(MethodInfo));
        public static DataKey<object>         Response             = new DataKey<object>(nameof(Response));

        public static DataKey<Type> TargetType = new DataKey<Type>(nameof(TargetType));

        public static DataKey<Action<string>> Trace = new DataKey<Action<string>>(nameof(Trace));
        #endregion
    }
}