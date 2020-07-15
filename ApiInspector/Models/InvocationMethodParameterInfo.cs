using System;

namespace ApiInspector.Models
{
    [Serializable]
    public class InvocationMethodParameterInfo
    {
        #region Public Properties
        public string ValueAsJson { get; set; }
        public object Value { get; set; }

        #endregion
    }
}