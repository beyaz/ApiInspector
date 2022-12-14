using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ApiInspector.DataAccess;
using BOA.Common.Extensions;
using Mono.Cecil;
using static ApiInspector._;
using static ApiInspector.MainWindow.Mixin;

namespace ApiInspector.MainWindow
{
    class CompileSQLOperationInput
    {
        public string SQL { get; set; }

        public IReadOnlyList<string> MethodParametersInJson { get; set; }

        public string MethodReturnValueInJson { get; set; }

        public MethodDefinition MethodDefinition { get; set; }

        public Dictionary<string, string> VariablesMap{ get; set; }
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

            var allSuggestions = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);

            

            var text = input.SQL;

            var sqlParameters = new Dictionary<string, DbParameterInfo>();

            SqlDbType getSqlDbType(object value)
            {
                if (value == null)
                {
                    return SqlDbType.VarChar;
                }

                var typeMap = new Dictionary<Type, SqlDbType>
                {
                    [typeof(string)]         = SqlDbType.NVarChar,
                    [typeof(char[])]         = SqlDbType.NVarChar,
                    [typeof(byte)]           = SqlDbType.TinyInt,
                    [typeof(short)]          = SqlDbType.SmallInt,
                    [typeof(int)]            = SqlDbType.Int,
                    [typeof(long)]           = SqlDbType.BigInt,
                    [typeof(byte[])]         = SqlDbType.Image,
                    [typeof(bool)]           = SqlDbType.Bit,
                    [typeof(DateTime)]       = SqlDbType.DateTime2,
                    [typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset,
                    [typeof(decimal)]        = SqlDbType.Money,
                    [typeof(float)]          = SqlDbType.Real,
                    [typeof(double)]         = SqlDbType.Float,
                    [typeof(TimeSpan)]       = SqlDbType.Time
                };

                var type = value.GetType();

                // Allow nullable types to be handled
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (typeMap.ContainsKey(type))
                {
                    return typeMap[type];
                }

                return SqlDbType.Structured;
            }

            void processSimpleSuggestion(string name, Func<object> getValue)
            {
                var prefix = IntellisensePrefix + name;

                if (!text.Contains(prefix))
                {
                    return;
                }

                var isComplex = text.Contains(IntellisensePrefix + name + ".");
                if (isComplex)
                {
                    return;
                }

                var key = name;

                if (sqlParameters.ContainsKey(key))
                {
                    return;
                }

                var value = getValue();
                var parameter = new DbParameterInfo
                {
                    Name      = key,
                    Value     = value,
                    SqlDbType = getSqlDbType(value)
                };

                sqlParameters.Add(key, parameter);
            }

            void processComplexSuggestion(string name, Func<object> getValue)
            {
                var prefix = IntellisensePrefix + name + ".";

                var suggestions = from x in allSuggestions
                                  where x.StartsWith(prefix)
                                  orderby x.Length descending
                                  select x;

               

                void processSuggestion(string suggestion)
                {
                    if (!text.Contains(suggestion))
                    {
                       return;
                    }

                    var propertyPath = suggestion.RemoveIfStartsWith(prefix);

                    var key = suggestion.Replace(".", "_");

                    text = text.Replace(suggestion, key);

                    var value = getValue();

                    var sqlValue = ReflectionUtil.ReadPropertyPath(value, propertyPath);

                    var parameter = new DbParameterInfo
                    {
                        Name      = key,
                        Value     = sqlValue,
                        SqlDbType = getSqlDbType(sqlValue)
                    };

                    sqlParameters.Add(key, parameter);
                }

                foreach (var suggestion in suggestions)
                {
                    processSuggestion(suggestion);
                }
            }

            foreach (var parameterDefinition in methodDefinition.Parameters)
            {
                var parameterName  = parameterDefinition.Name;

                object getParameterValue()
                {
                    var json = input.MethodParametersInJson[parameterDefinition.Index];
                    if (json == null)
                    {
                        return null;
                    }

                    return DeserializeForMethodParameter(json, parameterDefinition.ParameterType.GetDotNetType());    
                }

                processSimpleSuggestion(parameterName, getParameterValue);
                processComplexSuggestion(parameterName, getParameterValue);
            }

            if (!IsVoidMethod(methodDefinition))
            {
                object getReturnValue()
                {
                    return DeserializeForMethodParameter(input.MethodReturnValueInJson, GetTypeReference(methodDefinition.ReturnType).GetDotNetType());
                }

                processSimpleSuggestion("output", getReturnValue);
                processComplexSuggestion("output", getReturnValue);    
            }
            
            foreach (var pair in input.VariablesMap)
            {
                processSimpleSuggestion(pair.Key.RemoveFromStart(IntellisensePrefix), ()=>pair.Value);
            }


            return new CompileSQLOperationOutput
            {
                SQL           = text,
                SqlParameters = sqlParameters.Values.ToList()
            };
        }
    }
}