using System;
using System.Collections.Generic;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The invocation information
    /// </summary>
    [Serializable]
    public class InvocationInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the assembly.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        ///     Gets or sets the assembly search directory.
        /// </summary>
        public string AssemblySearchDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the name of the class.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///     Gets or sets the environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        ///     Gets or sets the name of the method.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        public List<InvocationMethodParameterInfo> Parameters { get; set; } = new List<InvocationMethodParameterInfo>();

        /// <summary>
        ///     Gets or sets the response output file path.
        /// </summary>
        public string ResponseOutputFilePath { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{ClassName}:{MethodName}";
        }
        #endregion
    }
}