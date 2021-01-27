using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ApiInspector.Models;

namespace ApiInspector
{
    static partial class _
    {
        public const string IntellisensePrefix = "@";

        public static DataKey<Dictionary<string, string>> VariablesMap => CreateKey<Dictionary<string, string>>();

        static DataKey<T> CreateKey<T>([CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(typeof(_), callerMemberName);
        }

        public static bool IsAssignToVariableOperator(string operatorName)
        {
            return operatorName == AssertionOperatorNames.AssignToVariable;
        }

        public static bool IsAssertionInfoShouldProcessBeforeExecutionStart(AssertionInfo assertionInfo)
        {
            return assertionInfo.OperatorName == AssertionOperatorNames.AssignTo || 
                   assertionInfo.OperatorName == AssertionOperatorNames.AssignToVariable;
        }
    }
}