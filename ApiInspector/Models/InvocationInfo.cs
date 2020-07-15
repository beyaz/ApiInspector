using System;
using System.Collections.Generic;

namespace ApiInspector.Models
{
    [Serializable]
    public class InvocationInfo
    {
        #region Public Properties
        public string AssemblyName            { get; set; }
        public string AssemblySearchDirectory { get; set; }
        public string ClassName               { get; set; }
        public string Environment             { get; set; }
        public string MethodName              { get; set; }

        public List<InvocationMethodParameterInfo> Parameters { get; set; } = new List<InvocationMethodParameterInfo>();
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return $"{ClassName}:{MethodName}";
        }
        #endregion
    }
}