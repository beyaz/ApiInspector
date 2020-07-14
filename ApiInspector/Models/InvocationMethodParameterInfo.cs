using System;

namespace ApiInspector.Models
{
    [Serializable]
    public class InvocationMethodParameterInfo
    {
        public string ValueAsJson { get; set; }
        public Type   Type        { get; set; }
    }
}