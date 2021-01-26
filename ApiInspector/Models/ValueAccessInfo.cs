using System;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The value access information
    /// </summary>
    [Serializable]
    public sealed class ValueAccessInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [fetch from database].
        /// </summary>
        public bool FetchFromDatabase { get; set; }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text { get; set; }
        #endregion
    }
}