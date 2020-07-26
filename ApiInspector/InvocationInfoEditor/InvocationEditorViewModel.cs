using System.Collections.Generic;
using ApiInspector.Models;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The invocation editor view model
    /// </summary>
    public class InvocationEditorViewModel
    {
        #region Fields
        
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the invocation information.
        /// </summary>
        public InvocationInfo InvocationInfo { get; set; }

        /// <summary>
        ///     Gets or sets the item source list.
        /// </summary>
        public ItemSourceList ItemSourceList { get; set; }

        #endregion
    }
}