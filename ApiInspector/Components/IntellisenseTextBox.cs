using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WpfControls;

namespace ApiInspector.Components
{
    public sealed class IntellisenseTextBox : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        
        public IntellisenseTextBox()
        {
            Provider = this;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        public IReadOnlyList<string> Suggestions { get; set; }
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

            return Suggestions?.Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion

      
    }
}