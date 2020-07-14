using System.Windows.Controls;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    static class DataKeys
    {
        #region Static Fields
        public static readonly DataKey<string> AssemblyFilePath = new DataKey<string>(nameof(AssemblyFilePath));
        public static readonly DataKey<string> AssemblyName     = new DataKey<string>(nameof(AssemblyName));

        public static readonly DataKey<string> AssemblySearchDirectory = new DataKey<string>(nameof(AssemblySearchDirectory));
        public static readonly DataKey<string> ClassName               = new DataKey<string>(nameof(ClassName));

        public static readonly DataKey<object>         ExecutionResponse = new DataKey<object>(nameof(ExecutionResponse));
        public static readonly DataKey<InvocationInfo> InvocationInfo    = new DataKey<InvocationInfo>(nameof(InvocationInfo));

        public static readonly DataKey<MethodDefinition> MethodDefinition = new DataKey<MethodDefinition>(nameof(MethodDefinition));
        public static readonly DataKey<string>           MethodName       = new DataKey<string>(nameof(MethodName));

        public static readonly DataKey<StackPanel> ParametersPanel = new DataKey<StackPanel>(nameof(ParametersPanel));

        public static readonly DataKey<string> TargetEnvironment = new DataKey<string>(nameof(TargetEnvironment));
        #endregion
    }
}