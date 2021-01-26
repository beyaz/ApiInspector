using System;
using System.Collections.Generic;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The scenario information
    /// </summary>
    [Serializable]
    public sealed class ScenarioInfo
    {
        #region Constructors
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the assertions.
        /// </summary>
        public List<AssertionInfo> Assertions { get; set; }

        /// <summary>
        ///     Gets or sets the method parameters.
        /// </summary>
        public List<InvocationMethodParameterInfo> MethodParameters { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the response output file path.
        /// </summary>
        public string ResponseOutputFilePath { get; set; }
        #endregion
    }
}