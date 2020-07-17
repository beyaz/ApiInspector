using System.Collections.Generic;
using System.Windows.Controls;
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
        /// <summary>
        ///     The method definition
        /// </summary>
        public MethodDefinition MethodDefinition;

        /// <summary>
        ///     The type definition
        /// </summary>
        public TypeDefinition TypeDefinition;
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

        /// <summary>
        ///     Gets or sets the logs.
        /// </summary>
        public List<string> Logs { get; set; }

        /// <summary>
        ///     Gets or sets the parameters panel.
        /// </summary>
        public StackPanel ParametersPanel { get; set; }
        #endregion
    }
}