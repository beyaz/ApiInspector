using System.Collections.Generic;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The method name intellisense text box
    /// </summary>
    class MethodNameIntellisenseTextBox : IntellisenseTextBoxBase
    {
        #region Static Fields
        /// <summary>
        ///     The class names
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