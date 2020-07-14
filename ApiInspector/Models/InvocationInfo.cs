using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiInspector.Models
{
    [Serializable]
    public class InvocationInfo
    {
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public List<InvocationMethodParameterInfo> Parameters { get; set; }

    }
}
