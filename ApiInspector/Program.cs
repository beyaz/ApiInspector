using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

internal class Program
{
    public static void Main(string[] args)
    {
        //args = new[] { @"GetMetadataNodes|d:\boa\server\bin\BOA.Types.ERP.PersonRelation.dll" };
        try
        {
            if (args == null)
            {
                throw new Exception("CommandLine arguments cannot be null.");
            }

            if (args.Length == 0)
            {
                throw new Exception("CommandLine arguments cannot be empty.");
            }

            
            var methodName = args[0];
            

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new Exception($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
            }

            var parameters = new object[] {  };
            if (methodInfo.GetParameters().Length == 1)
            {
                parameters = new []{ FileHelper.ReadInput(methodInfo.GetParameters()[0].ParameterType) };
            }
            
            var response = methodInfo.Invoke(null, parameters);

            var responseAsJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                Formatting                 = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            FileHelper.WriteSuccessResponse(responseAsJson);

            Environment.Exit(1);
        }
        catch (Exception exception)
        {
            FileHelper.WriteFail(exception);
            Environment.Exit(0);
        }
    }

    internal static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            var fileNameWithoutExtension = new AssemblyName(e.Name).Name;

            if (File.Exists(@"d:\boa\server\bin\" + fileNameWithoutExtension + ".dll"))
            {
                return Assembly.LoadFile(@"d:\boa\server\bin\" + fileNameWithoutExtension + ".dll");
            }

            return null;
        };

        return MetadataHelper.GetMetadataNodes(assemblyFilePath);
    }
}