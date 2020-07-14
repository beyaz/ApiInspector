using System;
using System.Collections.Generic;

namespace ApiInspector.InvocationInfoEditor
{
    [Serializable]
    public class ItemSourceList
    {
        public IReadOnlyList<string> AssemblySearchDirectoryList { get; set; }
        public IReadOnlyList<string> AssemblyNameList { get; set; }
        public IReadOnlyList<string> ClassNameList { get; set; }
        public IReadOnlyList<string> MethodNameList { get; set; }
        public IReadOnlyList<string> EnvironmentNameList { get; set; }
    }
}