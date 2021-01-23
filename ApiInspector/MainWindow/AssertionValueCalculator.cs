using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.Common.Types;
using Mono.Cecil;
using static ApiInspector.MainWindow.Mixin;

namespace ApiInspector.MainWindow
{
    partial class AssertionValueCalculator
    {
        #region Public Methods
        public static string RunAssertion(object actual, object expected, string operatorName)
        {
            string shouldBeEqual(string actualJson, string expectedJson)
            {
                if (actualJson != expectedJson)
                {
                    return $"Actual value: {actualJson} [SHOULD BE EQUAL TO] expected value: {expectedJson}";
                }

                return null;
            }

            string shouldBeGreaterThan(decimal actualValue, decimal expectedValue)
            {
                if (actualValue > expectedValue)
                {
                    return null;
                }

                return $"Actual value: {actualValue} [SHOULD BE GREATOR THAN] expected value: {expectedValue}";
            }

            string shouldBeGreaterThanOrEqual(decimal actualValue, decimal expectedValue)
            {
                if (actualValue >= expectedValue)
                {
                    return null;
                }

                return $"Actual value: {actualValue} [SHOULD BE GREATOR THAN OR EQUAL TO] expected value: {expectedValue}";
            }

            string shouldBeLessThan(decimal actualValue, decimal expectedValue)
            {
                if (actualValue < expectedValue)
                {
                    return null;
                }

                return $"Actual value: {actualValue} [SHOULD BE LESS THAN] expected value: {expectedValue}";
            }

            string shouldBeLessThanOrEqual(decimal actualValue, decimal expectedValue)
            {
                if (actualValue <= expectedValue)
                {
                    return null;
                }

                return $"Actual value: {actualValue} [SHOULD BE LESS THAN OR EQUAL TO] expected value: {expectedValue}";
            }

            string doOperationAsNumber(Func<decimal, decimal, string> operationFunc)
            {
                var actualValue = decimal.MaxValue;
                if (!decimal.TryParse(actual + string.Empty, out actualValue))
                {
                    return $"actualValue:{actual} can not be converted to number.";
                }

                var expectedValue = decimal.MaxValue;
                if (!decimal.TryParse(expected + string.Empty, out expectedValue))
                {
                    return $"expectedValue:{expected} can not be converted to number.";
                }

                return operationFunc(actualValue, expectedValue);
            }

            string shouldNotBeEqual(string actualJson, string expectedJson)
            {
                if (actualJson == expectedJson)
                {
                    return $"Actual value: {actualJson} [SHOULD NOT BE EQUAL TO] expected value: {expectedJson}";
                }

                return null;
            }

            string doEqualityOperation(Func<string, string, string> operatorFunc)
            {
                if (expected is string && actual != null && actual.GetType() != typeof(string))
                {
                    var exception = SafeRun(() => { expected = Serializer.Deserialize((string) expected, actual.GetType()); });
                    if (exception != null)
                    {
                        return exception.ToString();
                    }
                }

                var actualJson   = Serializer.SerializeToJson(actual);
                var expectedJson = Serializer.SerializeToJson(expected);

                return operatorFunc(actualJson, expectedJson);
            }

            if (operatorName == AssertionOperatorNames.IsEquals)
            {
                return doEqualityOperation(shouldBeEqual);
            }

            if (operatorName == AssertionOperatorNames.IsNotEquals)
            {
                return doEqualityOperation(shouldNotBeEqual);
            }

            if (operatorName == AssertionOperatorNames.GreaterThan)
            {
                return doOperationAsNumber(shouldBeGreaterThan);
            }

            if (operatorName == AssertionOperatorNames.GreaterThanOrEquals)
            {
                return doOperationAsNumber(shouldBeGreaterThanOrEqual);
            }

            if (operatorName == AssertionOperatorNames.LessThan)
            {
                return doOperationAsNumber(shouldBeLessThan);
            }

            if (operatorName == AssertionOperatorNames.LessThanOrEquals)
            {
                return doOperationAsNumber(shouldBeLessThanOrEqual);
            }

            return "NotImplemented operator: " + operatorName;
        }
        #endregion

        #region Methods
        internal static object CalculateFrom(ValueAccessInfo valueAccessInfo, MethodDefinition methodDefinition, InvokeOutput invocationOutput, EnvironmentInfo environmentInfo)
        {
            var input = new CompileSQLOperationInput
            {
                MethodDefinition        = methodDefinition,
                MethodParametersInJson  = invocationOutput.InvocationParameters,
                MethodReturnValueInJson = invocationOutput.ExecutionResponseAsJson,
                SQL                     = valueAccessInfo.Text.Trim()
            };
            var sqlOperationOutput = CompileSQLOperation(input);

            if (valueAccessInfo.FetchFromDatabase == false)
            {
                if (sqlOperationOutput.SqlParameters.Count == 1)
                {
                    if (input.SQL != sqlOperationOutput.SQL)
                    {
                        return sqlOperationOutput.SqlParameters[0].Value;
                    }

                    if (input.SQL == "@output")
                    {
                        return sqlOperationOutput.SqlParameters[0].Value;
                    }
                }

                return valueAccessInfo.Text;
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
        #endregion
    }
}