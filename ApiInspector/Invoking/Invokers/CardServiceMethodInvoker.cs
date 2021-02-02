using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ApiInspector.DataAccess;
using ApiInspector.Invoking.BoaSystem;
using static ApiInspector._;
using static FunctionalPrograming.FPExtensions;
using static ApiInspector.Utility;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The card service method invoker
    /// </summary>
    class CardServiceMethodInvoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified context.
        /// </summary>
        public static object Invoke(CardServiceMethodInvokerInput input, Action<string> trace, string environment)
        {
            var invocationParameters = input.InvocationParameters;
            var targetType           = input.TargetType;

            trace("Searching service interface...");

            var serviceInterface = targetType.GetInterfaces().FirstOrDefault(i => i.Name.EndsWith("Service"));
            if (serviceInterface == null)
            {
                throw new ArgumentNullException(nameof(serviceInterface));
            }

            var serviceProjectName = Path.GetFileNameWithoutExtension(targetType.Assembly.CodeBase);

            
            var configFilePath = GetWebConfigFilePath(serviceInterface.AssemblyQualifiedName,environment);
            
            ChangeAppConfig(configFilePath);


            trace("Searching method in service...");
            var method = targetType.GetMethods(AllBindings).FirstOrDefault(m=>m.GetMethodNameWithSignature() == input.MethodName);
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            trace("Accessing service method parameter type.");
            var parameterType = method.GetParameters()[0].ParameterType;

            var methodName = input.MethodName.Substring(0,input.MethodName.IndexOf("(", StringComparison.Ordinal));


            string getCSharpCode()
            {
                if (method.ReturnType.FullName == "System.Void")
                {
                    return   @"

using BOA.Card.Core.ServiceBus;

namespace ApiInspector.Invoking.Dynamic
{
    class ServiceWrapper
    {
        public static void Wrap(" + parameterType.FullName + @" request)
        {
            EverestContext.Current.Build();

            using (BOA.Card.Core.ServiceBus.EverestContext.Current.BeginScope())
            {
                var service = EverestContext.Current.GetService<"+ serviceInterface.FullName +@">();

                service."+ methodName +@"(request);
            }
        }
    }
}

";
                }

                return @"

using BOA.Card.Core.ServiceBus;

namespace ApiInspector.Invoking.Dynamic
{
    class ServiceWrapper
    {
        public static object Wrap(" + parameterType.FullName + @" request)
        {
            EverestContext.Current.Build();

            using (BOA.Card.Core.ServiceBus.EverestContext.Current.BeginScope())
            {
                var service = EverestContext.Current.GetService<"+ serviceInterface.FullName +@">();

                return service."+ methodName +@"(request);
            }
        }
    }
}

";
            }

            const string location   = @"d:\boa\server\bin\";

            var referencedAssemblies = new List<string>
            {
                "System.Core.dll",
                "mscorlib.dll",
                "System.dll",

                $"{location}BOA.Card.Core.dll",
                $"{location}BOA.Card.Contracts.dll",
                $"{location}BOA.Card.Contracts.Online.dll",
                $"{location}BOA.Card.Definitions.dll",
                $"{location}SimpleInjector.dll"
            };

            var compilerParams = new CompilerParameters(referencedAssemblies.ToArray())
            {
                CompilerOptions         = "/target:library /optimize",
                GenerateExecutable      = false,
                GenerateInMemory        = true,
                IncludeDebugInformation = false
            };

            trace("Started to compile...");
            var results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(compilerParams, getCSharpCode());
            if (results.Errors.Count > 0)
            {
                trace("Compile fail.");

                var convertErrorsToException = fun(() =>
                {
                    var errorMessage = new StringBuilder();
                    foreach (CompilerError compilerError in results.Errors)
                    {
                        errorMessage.AppendLine(compilerError.ToString());
                    }

                    return new ArgumentException(errorMessage.ToString());
                });

                throw convertErrorsToException();
            }

            trace("Compile success.");
            var assembly = results.CompiledAssembly;

            var methodInfo = assembly.GetType("ApiInspector.Invoking.Dynamic.ServiceWrapper").GetMethod("Wrap");
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            trace("Service invocation is started. Waiting response...");

            var response = methodInfo.Invoke(null, invocationParameters.ToArray());

            trace("Service invocation is success.");

            return response;
        }
        #endregion
    }
}