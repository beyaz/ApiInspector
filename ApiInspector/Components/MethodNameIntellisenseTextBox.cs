using System.Collections.Generic;
using System.Linq;
using BOA.DataFlow;
using Mono.Cecil;

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
        public static DataKey<IReadOnlyList<MethodDefinition>> Names = new DataKey<IReadOnlyList<MethodDefinition>>(nameof(Names));
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        protected override IEnumerable<string> GetSuggestions()
        {
            return Context.Get(Names).Select(x=>x.Name);
        }
        #endregion
    }
}