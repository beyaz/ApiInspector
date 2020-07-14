using System.Collections.Generic;
using System.Windows.Controls;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    static class Data
    {
        public static readonly DataKey<InvocationInfo> InvocationInfo = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        public static readonly DataKey<ItemSourceList> ItemSourceList = new DataKey<ItemSourceList>(nameof(ItemSourceList));
        public static readonly DataKey<MethodDefinition> MethodDefinition = new DataKey<MethodDefinition>(nameof(MethodDefinition));
        public static readonly DataKey<StackPanel> ParametersPanel = new DataKey<StackPanel>(nameof(ParametersPanel));
    }
}