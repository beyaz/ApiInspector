using System;
using System.Collections.Generic;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The item source list
    /// </summary>
    [Serializable]
    public class ItemSourceList
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemSourceList" /> class.
        /// </summary>
        public ItemSourceList()
        {
            AssemblySearchDirectoryList = new List<string>
            {
                AssemblySearchDirectories.serverBin,
                AssemblySearchDirectories.clientBin
            };
            EnvironmentNameList = new List<string> {"dev", "test"};
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the assembly name list.
        /// </summary>
        public IReadOnlyList<string> AssemblyNameList { get; set; }

        /// <summary>
        ///     Gets or sets the assembly search directory list.
        /// </summary>
        public IReadOnlyList<string> AssemblySearchDirectoryList { get; }

        /// <summary>
        ///     Gets or sets the class name list.
        /// </summary>
        public IReadOnlyList<string> ClassNameList { get; set; }

        /// <summary>
        ///     Gets or sets the environment name list.
        /// </summary>
        public IReadOnlyList<string> EnvironmentNameList { get; }

        /// <summary>
        ///     Gets or sets the method name list.
        /// </summary>
        public IReadOnlyList<string> MethodNameList { get; set; }
        #endregion
    }
}