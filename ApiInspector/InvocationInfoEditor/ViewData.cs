using System.Collections.Generic;
using System.Windows.Controls;
using ApiInspector.Models;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    public class InvocationEditorViewModel
    {
        #region Public Properties
        public InvocationInfo InvocationInfo  { get; set; }
        public ItemSourceList ItemSourceList  { get; set; }
        public List<string>   Logs            { get; set; }
        public StackPanel     ParametersPanel { get; set; }

        public TypeDefinition   TypeDefinition;
        public MethodDefinition MethodDefinition;
        #endregion
    }
}