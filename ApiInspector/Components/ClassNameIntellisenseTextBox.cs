using System.Collections.Generic;
using BOA.DataFlow;

namespace ApiInspector.Components
{
    class ClassNameIntellisenseTextBox : IntellisenseTextBoxBase
    {
        #region Static Fields
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