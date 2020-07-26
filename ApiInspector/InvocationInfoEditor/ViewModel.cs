using System;
using ApiInspector.Models;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view model
    /// </summary>
    class ViewModel
    {
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
        ///     Gets or sets the method definition.
        /// </summary>
        public MethodDefinition MethodDefinition { get; set; }

        /// <summary>
        ///     Gets or sets the trace.
        /// </summary>
        public Action<string> Trace { get; set; }

        /// <summary>
        ///     Gets or sets the type definition.
        /// </summary>
        public TypeDefinition TypeDefinition { get; set; }
        #endregion
    }
}