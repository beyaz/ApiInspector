using System;

namespace ApiInspector.Components.KeyValueEditor
{
    /// <summary>
    ///     The code with description contract
    /// </summary>
    [Serializable]
    public sealed class CodeWithDescriptionContract
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the display text.
        /// </summary>
        public string DisplayText { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            if (DisplayText != null)
            {
                return DisplayText;
            }

            return $"{Code} - {Description}";
        }
        #endregion
    }
}