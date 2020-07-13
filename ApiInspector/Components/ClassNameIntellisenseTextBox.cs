using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ApiInspector.Application;
using BOA.DataFlow;
using WpfControls;

namespace ApiInspector.Components
{
 
    class ClassNameIntellisenseTextBox : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Static Fields
        
        public static DataKey<IReadOnlyList<string>> ClassNames = new DataKey<IReadOnlyList<string>>(nameof(ClassNames));
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyIntellisenseTextBox" /> class.
        /// </summary>
        public ClassNameIntellisenseTextBox()
        {
            Provider = this;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        public DataContext Context { get; set; } = App.Context;
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

            var assemblyNames = Context.Get(ClassNames);

            return assemblyNames.Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion
    }
}