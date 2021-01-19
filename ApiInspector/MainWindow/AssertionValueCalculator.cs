using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.Common.Extensions;
using BOA.Common.Types;
using Mono.Cecil;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    partial class AssertionValueCalculator
    {
        internal static object CalculateFrom(ValueAccessInfo valueAccessInfo, MethodDefinition methodDefinition, InvokeOutput invocationOutput, EnvironmentInfo environmentInfo)
        {
            var suggestions = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);

            var deserializeParameterAt = fun((int index) =>
            {
                var json = invocationOutput.InvocationParameters[index];

                return Serializer.Deserialize(json, methodDefinition.Parameters[index].ParameterType.GetDotNetType());
            });

            var text = valueAccessInfo.Text.Trim();

            if (valueAccessInfo.FetchFromDatabase == false)
            {
                if (suggestions.Contains(text))
                {
                    if (text.StartsWith(CecilHelper.OutputPrefix))
                    {
                        var propertyPath = text.RemoveIfStartsWith(CecilHelper.OutputPrefix + ".");

                        propertyPath = text.RemoveIfStartsWith(CecilHelper.OutputPrefix);

                        var methodReturnValue = Serializer.Deserialize(invocationOutput.ExecutionResponseAsJson, methodDefinition.ReturnType.GetDotNetType());

                        return ReflectionUtil.ReadPropertyPath(methodReturnValue, propertyPath);
                    }

                    foreach (var parameterDefinition in methodDefinition.Parameters)
                    {
                        var prefix = CecilHelper.PrefixCharacter + parameterDefinition.Name;
                        if (text.StartsWith(prefix))
                        {
                            text = text.RemoveIfStartsWith(prefix);
                            text = text.RemoveIfStartsWith(".");

                            return ReflectionUtil.ReadPropertyPath(deserializeParameterAt(parameterDefinition.Index), text);
                        }
                    }
                }

                return text;
            }

            if (string.IsNullOrWhiteSpace(valueAccessInfo.DatabaseName))
            {
                return "Hata: DatabaseName is empty";
            }

            var database = Databases.Boa;

            if (!Enum.TryParse(valueAccessInfo.DatabaseName, true, out database))
            {
                return "Hata: DatabaseName is not recognized." + valueAccessInfo.DatabaseName;
            }

            DataTable sqlToDataTable()
            {
                var boaContext = new BOAContext(environmentInfo, Console.Write);

                var input = new CompileSQLOperationInput
                {
                    MethodDefinition        = methodDefinition,
                    MethodParametersInJson  = invocationOutput.InvocationParameters,
                    MethodReturnValueInJson = invocationOutput.ExecutionResponseAsJson,
                    SQL                     = text
                };
                var sqlOperationOutput = CompileSQLOperation(input);

                var command = boaContext.Context.DBLayer.GetDBCommand(database, sqlOperationOutput.SQL, new SqlParameter[0], CommandType.Text);
                foreach (var item in sqlOperationOutput.SqlParameters)
                {
                    boaContext.Context.DBLayer.AddInParameter(command, item.Name, item.SqlDbType, item.Value);
                }

                var reader = command.ExecuteReader();
                var dt     = new DataTable();
                dt.Load(reader);
                reader.Close();

                boaContext.Dispose();

                return dt;
            }

            var dataTable = sqlToDataTable();

            if (dataTable.Columns.Count == 1)
            {
                var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(dataTable.Columns[0].DataType));

                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(row[0]);
                }

                if (list.Count == 1)
                {
                    return list[0];
                }

                return list;
            }

            return dataTable;
        }

        public static string RunAssertion(object actual, object expected, string operatorName)
        {
            if (operatorName == AssertionOperatorNames.IsEquals)
            {
                if (expected is string && actual != null && actual.GetType() != typeof(string))
                {
                    expected = Serializer.Deserialize((string) expected, actual.GetType());
                }

                var actualJson   = Serializer.SerializeToJson(actual);
                var expectedJson = Serializer.SerializeToJson(expected);
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