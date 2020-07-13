using System.Collections.Generic;
using System.Linq;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.Components
{
    class ClassNameIntellisenseTextBox : IntellisenseTextBoxBase
    {
        #region Static Fields
        public static DataKey<IReadOnlyList<TypeDefinition>> Names = new DataKey<IReadOnlyList<TypeDefinition>>(nameof(Names));
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        protected override IEnumerable<string> GetSuggestions()
        {
            return Context.Get(Names).Select(x=>x.FullName);
        }
        #endregion
    }
}