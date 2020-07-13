using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfControls;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The intellisense text box
    /// </summary>
    class IntellisenseTextBox : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyIntellisenseTextBox" /> class.
        /// </summary>
        public IntellisenseTextBox()
        {
            Provider = this;
        }
        #endregion

        #region IReadOnlyList<string> Suggestions

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register(
                                                        "Suggestions", typeof(IReadOnlyList<string>), typeof(IntellisenseTextBox), new PropertyMetadata(default(IReadOnlyList<string>)));

        public IReadOnlyList<string> Suggestions
        {
            get { return (IReadOnlyList<string>) GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
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

            return Suggestions.Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion
    }
}