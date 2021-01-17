using System;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.Common.Extensions;
using Mono.Cecil;

namespace ApiInspector.MainWindow
{
    class AssertionValueCalculator
    {
        internal static object CalculateFrom(ValueAccessInfo valueAccessInfo, MethodDefinition methodDefinition,object invocationOutput)
        {
            var suggestions = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);

            if (valueAccessInfo.FetchFromDatabase == false)
            {
                if (suggestions.Contains(valueAccessInfo.Text.Trim()))
                {
                    if (valueAccessInfo.Text.StartsWith("@output"))
                    {
                        var propertyPath = valueAccessInfo.Text.RemoveIfStartsWith("@output");
                        propertyPath = valueAccessInfo.Text.RemoveIfStartsWith("@output.");

                        return ReflectionUtil.ReadPropertyPath(invocationOutput, propertyPath);
                    }
                }
            }

            return null;
        }

        public static string RunAssertion(object actual, object expected, string operatorName)
        {
            if (operatorName == AssertionOperatorNames.IsEquals)
            {
                var actualJson = Serialization.Serializer.SerializeToJson(actual);
                var expectedJson = Serialization.Serializer.SerializeToJson(expected);
                if (actualJson != expectedJson)
                {
                    return $"Actual value: {actualJson} is not equals to expected value: {expectedJson}";
                }
            }

            throw new NotImplementedException(operatorName);
        }
    }
}