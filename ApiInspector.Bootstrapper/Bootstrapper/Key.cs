using System.Collections.Generic;
using BOA.DataFlow;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The key
    /// </summary>
    class Key
    {
        #region Static Fields
        /// <summary>
        ///     The files
        /// </summary>
        public static DataKey<IReadOnlyList<FileModel>> Files = new DataKey<IReadOnlyList<FileModel>>(nameof(Files));

        /// <summary>
        ///     The target directory path
        /// </summary>
        public static DataKey<string> TargetDirectoryPath = new DataKey<string>(nameof(TargetDirectoryPath));
        #endregion
    }
}