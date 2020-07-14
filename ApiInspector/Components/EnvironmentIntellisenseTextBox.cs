using System.Collections.Generic;
using System.Linq;

namespace ApiInspector.Components
{
    class EnvironmentIntellisenseTextBox : IntellisenseTextBoxBase
    {
        static readonly List<string> items = new List<string>{"Dev","Test"};

        #region Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        protected override IEnumerable<string> GetSuggestions()
        {
            return items.Select(x=>x);
        }
        #endregion
    }
}