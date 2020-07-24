using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BOA.Common.Extensions;
using BOA.DataFlow;

namespace ApiInspector.Invoking
{
    class CardServiceMethodInvoker
    {
        static string FrameworkPath
        {
            get { return Path.GetDirectoryName(typeof(string).Assembly.Location) + Path.DirectorySeparatorChar; }
        }
        public static bool Invoke(DataContext context)
        {
            var invocationParameters = context.Get(InvocationContextKeys.InvocationParameters);
            var boaContext           = context.Get(InvocationContextKeys.BOAContext);

            string SourceCode = @"

using BOA.Base;
using BOA.Proxy.Kernel.Card;

namespace ApiInspector.Invoking.Dynamic
{
    class ServiceWrapper
    {
        public static object Wrap(ObjectHelper objectHelper, BOA.Card.Contracts.CreditCard.Limit.GetCardAvailableLimitRequest request)
        {
            return CardLocalProxy.Call(objectHelper, (BOA.Card.Contracts.CreditCard.Limit.ICRDLimitService service) => service.GetCardAvailableLimit(request));
        }
    }
}

";
            const string location = @"d:\boa\server\bin\";

            var referencedAssemblies = new List<string>()
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

            var results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(compilerParams, SourceCode);
            if (results.Errors.Count > 0)
            {
                throw new ArgumentException(ConvertToString(results.Errors));
            }

            var assembly = results.CompiledAssembly;

            var methodInfo = assembly.GetType("ApiInspector.Invoking.Dynamic.ServiceWrapper").GetMethod("Wrap");
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            

            var wrapMethodParameters = new List<object>
            {
                boaContext.GetObjectHelper()
            };
            wrapMethodParameters.AddRange(invocationParameters);

            var response = methodInfo.Invoke(null, wrapMethodParameters.ToArray());

                context.Add(InvocationContextKeys.Response,response);

            return true;
        }

        static string ConvertToString(CompilerErrorCollection errors)
        {
            var errorMessage = new StringBuilder();
            foreach (CompilerError compilerError in errors)
            {
                errorMessage.AppendLine(compilerError.ToString());
            }

            return errorMessage.ToString();
        }
    }
}