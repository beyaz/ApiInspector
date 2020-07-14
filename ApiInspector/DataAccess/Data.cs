using System.Collections.Generic;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    static class Data
    {
        public static readonly DataKey<InvocationInfo> InvocationInfo = new DataKey<InvocationInfo>(nameof(InvocationInfo));

        public static DataKey<IReadOnlyList<string>> AssemblySearchDirectoryList = new DataKey<IReadOnlyList<string>>(nameof(AssemblySearchDirectoryList));

        public static  DataKey<string> AssemblySearchDirectory = new DataKey<string>(nameof(AssemblySearchDirectory));
        public static  DataKey<IReadOnlyList<string>> AssemblyNames = new DataKey<IReadOnlyList<string>>(nameof(AssemblyNames));
    }
}