using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.Base;
using BOA.Base.Data;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector.Domain
{
    static class Invoker
    {
        #region Public Methods
        public static void Execute(DataContext context)
        {
            var invocationInfo = context.Get(DataKeys.InvocationInfo);

            var assemblyName = invocationInfo.AssemblyName;
            var methodName   = invocationInfo.MethodName;
            var className    = invocationInfo.ClassName;

            var targetType = Type.GetType($"{className},{Path.GetFileNameWithoutExtension(assemblyName)}", true);

            var instance = CreateInstance(context, targetType);

            var methodInfo = targetType.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            var methodParameters = parameters.ConvertAll(ConvertToMethodInvocationParameter).ToArray();

            var response = methodInfo.Invoke(instance, methodParameters);

            context.Update(DataKeys.ExecutionResponse, response);
        }
        #endregion

        #region Methods
        static object ConvertToMethodInvocationParameter(InvocationMethodParameterInfo info)
        {
            return JsonConvert.DeserializeObject(info.ValueAsJson, info.Type);
        }

        static object CreateInstance(DataContext context, Type targetType)
        {
            // constructor with ExecutionDataContext
            {
                var constructorInfo = targetType.GetConstructor(new[]
                {
                    typeof(ExecutionDataContext)
                });
                if (constructorInfo != null)
                {
                    var instance = constructorInfo.Invoke(new object[]
                    {
                        context.Get(DataKeys.ExecutionDataContext)
                    });
                    return instance;
                }
            }

            // simple constructor
            {
                var instance     = Activator.CreateInstance(targetType);
                var objectHelper = instance as ObjectHelper;
                if (objectHelper != null)
                {
                    objectHelper.Context = context.Get(DataKeys.ExecutionDataContext);
                }

                return instance;
            }
        }
        #endregion
    }
}