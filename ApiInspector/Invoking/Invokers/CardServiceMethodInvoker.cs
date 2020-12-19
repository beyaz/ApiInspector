using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Tracing;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The card service method invoker
    /// </summary>
    class CardServiceMethodInvoker
    {
        #region Fields
        /// <summary>
        ///     The boa context
        /// </summary>
        readonly BOAContext boaContext;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="CardServiceMethodInvoker" /> class.
        /// </summary>
        public CardServiceMethodInvoker(ITracer tracer, BOAContext boaContext)
        {
            this.tracer     = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.boaContext = boaContext ?? throw new ArgumentNullException(nameof(boaContext));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Invokes the specified context.
        /// </summary>
        public object Invoke(CardServiceMethodInvokerInput input)
        {
            var invocationParameters = input.InvocationParameters;
            var targetType           = input.TargetType;
            var methodName           = input.MethodName;

            trace("Searching service interface...");

            var serviceInterface = targetType.GetInterfaces().FirstOrDefault(i => i.Name.EndsWith("Service"));
            if (serviceInterface == null)
            {
                throw new ArgumentNullException(nameof(serviceInterface));
            }

            trace("Searching method in service...");
            var method = targetType.GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            trace("Accessing service method parameter type.");
            var parameterType = method.GetParameters()[0].ParameterType;

            var SourceCode = @"

using BOA.Base;
using BOA.Proxy.Kernel.Card;

namespace ApiInspector.Invoking.Dynamic
{
    class ServiceWrapper
    {
        public static object Wrap(ObjectHelper objectHelper, " + parameterType.FullName + @" request)
        {
            return CardLocalProxy.Call(objectHelper, (" + serviceInterface.FullName + @" service) => service." + methodName + @"(request));
        }
    }
}

";
            const string location = @"d:\boa\server\bin\";

            var referencedAssemblies = new List<string>
            {
                "System.Core.dll",
                "mscorlib.dll",
                "System.dll",

                $"{location}BOA.Proxy.Kernel.Card.dll",
                $"{location}BOA.Common.dll",
                $"{location}BOA.Base.dll",
                $"{location}BOA.Card.Contracts.dll",
                $"{location}BOA.Card.Definitions.dll"
            };

            var compilerParams = new CompilerParameters(referencedAssemblies.ToArray())
            {
                CompilerOptions         = "/target:library /optimize",
                GenerateExecutable      = false,
                GenerateInMemory        = true,
                IncludeDebugInformation = false
            };

            trace("Started to compile...");
            var results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(compilerParams, SourceCode);
            if (results.Errors.Count > 0)
            {
                trace("Compile fail.");
                throw new ArgumentException(ConvertToString(results.Errors));
            }

            trace("Compile success.");
            var assembly = results.CompiledAssembly;

            var methodInfo = assembly.GetType("ApiInspector.Invoking.Dynamic.ServiceWrapper").GetMethod("Wrap");
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            trace("Creating ObjectHelper...");
            var wrapMethodParameters = new List<object>
            {
                boaContext.GetObjectHelper()
            };
            wrapMethodParameters.AddRange(invocationParameters);

            trace("Service invocation is started. Waiting response...");

            var response = methodInfo.Invoke(null, wrapMethodParameters.ToArray());

            trace("Service invocation is success.");

            return response;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Converts to string.
        /// </summary>
        static string ConvertToString(CompilerErrorCollection errors)
        {
            var errorMessage = new StringBuilder();
            foreach (CompilerError compilerError in errors)
            {
                errorMessage.AppendLine(compilerError.ToString());
            }

            return errorMessage.ToString();
        }

        /// <summary>
        ///     Traces the specified message.
        /// </summary>
        void trace(string message)
        {
            tracer.Trace(message);
        }
        #endregion
    }
}