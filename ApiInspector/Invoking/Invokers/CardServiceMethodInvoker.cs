using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                // find service implementation and find interface by using it
                serviceInterface = targetType.Assembly.GetTypes().FirstOrDefault(i => i.Name.EndsWith("Service"));
                if (serviceInterface == null)
                {
                    throw new ArgumentNullException(nameof(serviceInterface));
                }

                serviceInterface = serviceInterface.GetInterfaces().FirstOrDefault(i => i.Name.EndsWith("Service"));
                if (serviceInterface == null)
                {
                    throw new ArgumentNullException(nameof(serviceInterface));
                }
            }

            var requestedType = serviceInterface.FullName;

            var partAfterSetupCompleted = @"

            EverestContext.Current.AfterSetupCompleted(() =>
            {
                var bindings = BindingFlags.NonPublic | BindingFlags.Instance;
                
                var container = (Container)EverestContext.Current.GetType().GetField(" + "\"_container\"" + @", bindings).GetValue(EverestContext.Current);

                container.Options.AllowOverridingRegistrations = true;

                container.RegisterLifetimeScope<" + targetType.FullName + @"," + targetType.FullName + @">();
                container.RegisterLifetimeScope<" + serviceInterface.FullName + @">(() => container.GetInstance<" + targetType.FullName + @">());
            });
";

            if (targetType.IsGenericType) // maybe calling job definition
            {
                requestedType = $"{targetType.FullName.RemoveFromEnd("`1")}<{requestedType}>";

                partAfterSetupCompleted = string.Empty;
            }

            var configFilePath = GetWebConfigFilePath(serviceInterface.AssemblyQualifiedName, environment);

            ChangeAppConfig(configFilePath);

            trace("Searching method in service...");
            var method = targetType.GetMethods(AllBindings).FirstOrDefault(m => m.GetMethodNameWithSignature() == input.MethodName);
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            // interface has method
            if (!targetType.IsGenericType && serviceInterface.GetMethods(AllBindings).All(m => m.GetMethodNameWithSignature() != input.MethodName))
            {
                requestedType = targetType.FullName;
            }

            trace("Accessing service method parameter type.");

            var methodName = input.MethodName.Substring(0, input.MethodName.IndexOf("(", StringComparison.Ordinal));

            var parameterDefinitionPart = string.Empty;
            var parameterCallPart       = string.Empty;

            var parameterInfoList = method.GetParameters();
            if (parameterInfoList.Length > 0)
            {
                parameterDefinitionPart = string.Join(" ,", parameterInfoList.Select(p => $"{p.ParameterType.FullName} {p.Name}"));
                parameterCallPart       = string.Join(" ,", parameterInfoList.Select(p => $"{p.Name}"));
            }

            MethodInfo methodInfo = null;
            {
                const string location = @"d:\boa\server\bin\";

                var referencedAssemblies = new List<string>
                {
                    "System.Core.dll",
                    "mscorlib.dll",
                    "System.dll",

                    $"{location}BOA.Card.Core.dll",
                    $"{location}BOA.Card.Contracts.dll",
                    $"{location}BOA.Card.Contracts.Online.dll",
                    $"{location}BOA.Card.Definitions.dll",
                    $"{location}SimpleInjector.dll",
                    $"{location}SimpleInjector.Extensions.LifetimeScoping.dll",
                    $"{location}BOA.Card.Services.Batch.dll",
                    $"{location}{targetType.Assembly.GetName().Name}.dll"
                };

                var sourceCode = @"

using BOA.Card.Core.ServiceBus;
using System.Reflection;
using SimpleInjector;

namespace ApiInspector.Invoking.Dynamic
{
    class ServiceWrapper
    {
        public static object Wrap(" + parameterDefinitionPart + @")
        {
            var methodName = " + "\"" + methodName + "\"" + @";

            " + partAfterSetupCompleted + @"
    
            EverestContext.Current.Build();

            EverestContext.Current.ContextHeader = EverestContext.Current.ContextHeader ??  new BOA.Card.Core.ServiceBus.ContextHeader{ Environment = " + '"' + EnvironmentInfo.Parse(environment) + '"' + @" };

            using (BOA.Card.Core.ServiceBus.EverestContext.Current.BeginScope())
            {
                var serviceInstance = EverestContext.Current.GetService<" + requestedType + @">();

                var bindings = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static|BindingFlags.InvokeMethod;
                
                var realTypeOfService = typeof(" + requestedType + @");

                var output = realTypeOfService.InvokeMember(methodName, bindings, null, serviceInstance, new object[] {
                    " + parameterCallPart + @"
                });

                var transactionContext = EverestContext.Current.GetService<TransactionContext>();

                if (transactionContext.Exception != null)
                {
                    throw transactionContext.Exception;
                }
            
                return output;
            }
        }
    }
}

";

                var compilerParams = new CompilerParameters(referencedAssemblies.ToArray())
                {
                    CompilerOptions         = "/target:library /optimize",
                    GenerateExecutable      = false,
                    GenerateInMemory        = true,
                    IncludeDebugInformation = false
                };

                trace("Started to compile...");
                var results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(compilerParams, sourceCode);
                if (results.Errors.Count > 0)
                {
                    trace("Compile fail.");

                    var convertErrorsToException = Fun(() =>
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

                methodInfo = assembly.GetType("ApiInspector.Invoking.Dynamic.ServiceWrapper").GetMethod("Wrap");
                if (methodInfo == null)
                {
                    throw new ArgumentNullException(nameof(methodInfo));
                }
            }

            // invoke method
            {
                trace("Service invocation is started. Waiting response...");

                var response = InvokeStaticMethod(methodInfo, invocationParameters.ToArray());

                trace("Service invocation is success.");

                return response;
            }
        }
        #endregion
    }
}