using System;
using Dapper.Contrib.Extensions;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The file model
    /// </summary>
    [Table("[WHT].[File]")]
    class FileModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        [ExplicitKey]
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the content.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        [ExplicitKey]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the last modification.
        /// </summary>
        public DateTime LastModification { get; set; }
        #endregion
    }
}