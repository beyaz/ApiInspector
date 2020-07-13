using System.Collections.Generic;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The assembly intellisense text box
    /// </summary>
    class AssemblyIntellisenseTextBox : IntellisenseTextBoxBase
    {
        #region Static Fields
        /// <summary>
        ///     The assembly names
        /// </summary>
        public static DataKey<IReadOnlyList<string>> Names = new DataKey<IReadOnlyList<string>>(nameof(Names));
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        protected override IEnumerable<string> GetSuggestions()
        {
            return Context.Get(Names);
        }
        #endregion
    }
}