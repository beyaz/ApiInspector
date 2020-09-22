using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WpfControls;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The boa user intellisense text box
    /// </summary>
    public sealed class BoaUserIntellisenseTextBox : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoaUserIntellisenseTextBox" /> class.
        /// </summary>
        public BoaUserIntellisenseTextBox()
        {
            Provider = this;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        public Func<IReadOnlyList<string>> Suggestions { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Sets the value.
        /// </summary>
        public void SetValue(string value)
        {
            Editor.Text  = value;
            Popup.IsOpen = false;
        }
        #endregion

        #region Explicit Interface Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        IEnumerable ISuggestionProvider.GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            if (Suggestions == null)
            {
                return null;
            }

            return Suggestions()?.Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion
    }
}