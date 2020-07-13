using System;
using System.Collections.Generic;
using ApiInspector.Models;

namespace ApiInspector.Components
{
    [Serializable]
    public class InvocationInfoEditorModel
    {
        public string AssemblyDirectory { get; set; }

        public InvocationInfo InvocationInfo { get; set; }

        public IReadOnlyList<string> AssemblyNames { get; set; }

        public IReadOnlyList<string> ClassNames { get; set; }

        public IReadOnlyList<string> MethodNames { get; set; }
    }
}