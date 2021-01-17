using System;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.Common.Extensions;
using Mono.Cecil;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    class AssertionValueCalculator
    {
        internal static object CalculateFrom(ValueAccessInfo valueAccessInfo, MethodDefinition methodDefinition,  InvokeOutput invocationOutput)
        {
            var suggestions = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);


            var deserializeParameterAt = fun((int index) =>
            {
                var json = invocationOutput.InvocationParameters[index];

                return Serialization.Serializer.Deserialize(json, methodDefinition.Parameters[index].ParameterType.GetDotNetType());
            });

            var text = valueAccessInfo.Text.Trim();

            if (valueAccessInfo.FetchFromDatabase == false)
            {
                if (suggestions.Contains(text))
                {
                    if (text.StartsWith(CecilHelper.OutputPrefix))
                    {
                        var propertyPath = text.RemoveIfStartsWith(CecilHelper.OutputPrefix+".");

                        propertyPath = text.RemoveIfStartsWith(CecilHelper.OutputPrefix);

                        var methodReturnValue = Serialization.Serializer.Deserialize(invocationOutput.ExecutionResponseAsJson, methodDefinition.ReturnType.GetDotNetType());
                        
                        return ReflectionUtil.ReadPropertyPath(methodReturnValue, propertyPath);
                    }

                    foreach (var parameterDefinition in methodDefinition.Parameters)
                    {
                        var prefix = CecilHelper.PrefixCharacter + parameterDefinition.Name;
                        if ( text.StartsWith(prefix))
                        {
                            text = text.RemoveIfStartsWith(prefix);
                            text = text.RemoveIfStartsWith(".");

                            return ReflectionUtil.ReadPropertyPath(deserializeParameterAt(parameterDefinition.Index), text);
                        }
                    }
                }

                return text;
            }

            return null;
        }
        
        

        public static string RunAssertion(object actual, object expected, string operatorName)
        {
            if (operatorName == AssertionOperatorNames.IsEquals)
            {
                if (expected is string && actual != null && actual.GetType() != typeof(string))
                {
                    expected= Serialization.Serializer.Deserialize((string) expected, actual.GetType());
                }
                var actualJson = Serialization.Serializer.SerializeToJson(actual);
                var expectedJson = Serialization.Serializer.SerializeToJson(expected);
                if (actualJson != expectedJson)
                {
                    return $"Actual value: {actualJson} is not equals to expected value: {expectedJson}";
                }

                return null;
            }

            return "NotImplemented operator: " + operatorName;
        }
    }
}