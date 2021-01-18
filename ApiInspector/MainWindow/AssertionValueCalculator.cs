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
using BOA.Common.Configuration.Sections;
using BOA.Common.Extensions;
using BOA.Common.Types;
using Mono.Cecil;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    class AssertionValueCalculator
    {
        internal static object CalculateFrom(ValueAccessInfo valueAccessInfo, MethodDefinition methodDefinition, InvokeOutput invocationOutput, EnvironmentInfo environmentInfo)
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


            if (string.IsNullOrWhiteSpace(valueAccessInfo.DatabaseName))
            {
                return "Hata: DatabaseName is empty";
            }

            var database = Databases.Boa;

            if (!Enum.TryParse(valueAccessInfo.DatabaseName, true, out database))
            {
                return "Hata: DatabaseName is not recognized";
            }

            var sqlToDataTable = fun(() =>
            {
                var boaContext = new BOAContext(environmentInfo, Console.Write);

                Dictionary<string, object> sqlParameters = new Dictionary<string, object>();

                foreach (var suggestion in suggestions)
                {
                    if (text.Contains(suggestion))
                    {
                        foreach (var parameterDefinition in methodDefinition.Parameters)
                        {
                            var prefix = CecilHelper.PrefixCharacter + parameterDefinition.Name+".";

                            if (text.Contains(prefix))
                            {
                                var propertyPath = suggestion.RemoveIfStartsWith(prefix);

                                var key = suggestion.Replace(".", "_");

                                text = text.Replace(suggestion, key);
                                
                                var value = ReflectionUtil.ReadPropertyPath(deserializeParameterAt(parameterDefinition.Index), propertyPath);

                                sqlParameters.Add( key, value);
                            }
                        }
                    }
                }

               


                var command    = boaContext.Context.DBLayer.GetDBCommand(database, text, new SqlParameter[0], CommandType.Text);
                foreach (var pair in sqlParameters)
                {
                    boaContext.Context.DBLayer.AddInParameter(command,pair.Key,SqlDbType.Char,pair.Value);
                }

                var reader     = command.ExecuteReader();
                var dt  = new DataTable();
                dt.Load(reader);
                reader.Close();
            
                boaContext.Dispose();

                return dt;
            });

            var dataTable = sqlToDataTable();

            
            if (dataTable.Columns.Count ==1)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dataTable.Columns[0].DataType));

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