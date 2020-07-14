using System;
using System.Collections.Generic;

namespace ApiInspector.Models
{
    [Serializable]
    public class InvocationInfo
    {
        #region Public Properties
        public string AssemblyName { get; set; }
        public string ClassName    { get; set; }
        public string MethodName   { get; set; }

        public List<InvocationMethodParameterInfo> Parameters { get; set; }
        #endregion
    }
}