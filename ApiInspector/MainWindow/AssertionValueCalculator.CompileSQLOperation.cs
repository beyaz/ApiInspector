using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Serialization;
using BOA.Common.Extensions;
using Mono.Cecil;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    class CompileSQLOperationInput
    {
        public string SQL { get; set; }

        public IReadOnlyList<string> MethodParametersInJson { get; set; }

        public string MethodReturnValueInJson { get; set; }

        public MethodDefinition MethodDefinition{ get; set; }
    }

    class CompileSQLOperationOutput
    {
        public string SQL { get; set; }

        public IReadOnlyList<DbParameterInfo> SqlParameters { get; set; }
    }

    class DbParameterInfo
    {
        public string Name { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public object Value { get; set; }
    }

    partial class AssertionValueCalculator
    {
        static CompileSQLOperationOutput CompileSQLOperation(CompileSQLOperationInput input)
        {
            var methodDefinition = input.MethodDefinition;

            var suggestions      = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);

            var deserializeParameterAt = fun((int index) =>
            {
                var json = input.MethodParametersInJson[index];

                var targetType = methodDefinition.Parameters[index].ParameterType.GetDotNetType();
                if (targetType == typeof(string))
                {
                    return json;
                }

                return Serializer.Deserialize(json, targetType);
            });

            var text = input.SQL;

            var sqlParameters = new Dictionary<string, DbParameterInfo>();

            foreach (var suggestion in suggestions)
            {
                if (text.Contains(suggestion))
                {
                    foreach (var parameterDefinition in methodDefinition.Parameters)
                    {
                        void processComplexSuggestion()
                        {
                            var prefix = CecilHelper.PrefixCharacter + parameterDefinition.Name + ".";

                            if (text.Contains(prefix))
                            {
                                var propertyPath = suggestion.RemoveIfStartsWith(prefix);

                                var key = suggestion.Replace(".", "_");

                                text = text.Replace(suggestion, key);

                                var value = ReflectionUtil.ReadPropertyPath(deserializeParameterAt(parameterDefinition.Index), propertyPath);

                                var parameter = new DbParameterInfo
                                {
                                    Name  = key,
                                    Value = value
                                };

                                if (value is string)
                                {
                                    parameter.SqlDbType = SqlDbType.VarChar;
                                }
                                else
                                {
                                    throw new NotImplementedException(value + string.Empty);
                                }

                                sqlParameters.Add(key, parameter);
                            }
                        }

                        void processSimpleSuggestion ()
                        {
                            var prefix = CecilHelper.PrefixCharacter + parameterDefinition.Name;

                            if (text.Contains(prefix))
                            {
                                var key = parameterDefinition.Name;

                                if (sqlParameters.ContainsKey(key))
                                {
                                    return;
                                }

                                var value = deserializeParameterAt(parameterDefinition.Index);

                                var parameter = new DbParameterInfo
                                {
                                    Name  = key,
                                    Value = value
                                };

                                if (value is string)
                                {
                                    parameter.SqlDbType = SqlDbType.VarChar;
                                }
                                else if (value is int)
                                {
                                    parameter.SqlDbType = SqlDbType.Int;
                                }
                                else
                                {
                                    throw new NotImplementedException(value + string.Empty);
                                }

                                sqlParameters.Add(key, parameter);
                            }
                        }

                        if (suggestion.Contains("."))
                        {
                            processComplexSuggestion();
                        }
                        else
                        {
                            processSimpleSuggestion();
                        }
                    }
                }
            }

            return new CompileSQLOperationOutput
            {
                SQL           = text,
                SqlParameters = sqlParameters.Values.ToList()
            };
        }
    }
}