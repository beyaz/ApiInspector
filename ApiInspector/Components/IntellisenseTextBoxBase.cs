using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BOA.DataFlow;
using WpfControls;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The intellisense text box
    /// </summary>
    public abstract class IntellisenseTextBoxBase : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="IntellisenseTextBoxBase" /> class.
        /// </summary>
        protected IntellisenseTextBoxBase()
        {
            Provider = this;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        public DataContext Context { get; set; }
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

            return GetSuggestions().Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        protected abstract IEnumerable<string> GetSuggestions();
        #endregion
    }
}