using System;
using System.Collections;
using System.Linq;
using ApiInspector.Application;
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

            var source = new BoaUserDataSource(new ConnectionString());

           
            
            return source.GetUsers(filter)?.Where(x => x.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                       x.UserCode.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion
    }
}