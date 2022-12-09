using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiInspector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
            { 
                Console.WriteLine("CommandLine arguments cannot be null.");
                Console.Read();
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("CommandLine arguments cannot be empty.");
                Console.Read();
                return;
            }

            var arr = (args[0] + string.Empty).Split(':');
            if (arr.Length != 2)
            {
                Console.WriteLine($"CommandLine argument invalid. @argument: {arr[0]}");
                Console.Read();
                return;
            }

            var methodName = arr[0];
            var parameter = arr[1];

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static| BindingFlags.Public| BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                Console.WriteLine($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
                Console.Read();
                return;
            }

            var response = methodInfo.Invoke(null, new object[] { parameter });

            var responseAsJson = JsonConvert.SerializeObject(response,new JsonSerializerSettings{Formatting = Formatting.Indented});
            
            File.WriteAllText(@"c:\ApiInspector.Response.json", responseAsJson);

        }

        static string GetAssemblyModel(string assemblyFileFullPath)
        {
            return "HelloWorld: " + assemblyFileFullPath;
        }
        
        
    }
}
