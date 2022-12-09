﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

internal class Program
{
    static void Main(string[] args)
    {
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

            var arr = (args[0] + string.Empty).Split(':');
            if (arr.Length != 2)
            {
                throw new Exception($"CommandLine argument invalid. @argument: {arr[0]}");
               
            }

            var methodName = arr[0];
            var parameter  = arr[1];

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static| BindingFlags.Public| BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new Exception($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
                
            }

        
            var response = methodInfo.Invoke(null, new object[] { parameter });

            var responseAsJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });

            File.WriteAllText(@"c:\ApiInspector.Response.json", responseAsJson);

            Environment.Exit(1);
        }
        catch (Exception exception)
        {
            File.WriteAllText(@"c:\ApiInspector.Response.Error.json", exception.ToString());
            Environment.Exit(0);
        }
    }

    public static (IReadOnlyList<TypeReference> types, IReadOnlyList<MethodReference> methods) GetAssemblyModel(string assemblyFileFullPath)
    {
        if (assemblyFileFullPath == null)
        {
            throw new ArgumentNullException(nameof(assemblyFileFullPath));
        }

        if (!File.Exists(assemblyFileFullPath))
        {
            throw new FileNotFoundException(assemblyFileFullPath);
        }

        var assembly = Assembly.LoadFile(assemblyFileFullPath);

        var types   = new List<TypeReference>();
        var methods = new List<MethodReference>();

        assembly.VisitTypes(t=>
        {
            types.Add(t.AsReference());
            
            t.VisitMethods(m=>methods.Add(m.AsReference()));
        });

        
        
        

        return (types,methods);
    }

}