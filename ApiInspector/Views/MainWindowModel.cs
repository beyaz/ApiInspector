using System;
using ApiInspector.Components;

namespace ApiInspector.Views
{
    [Serializable]
    public class MainWindowModel
    {
        public InvocationInfoEditorModel CurrentInvocationInfoEditorModel { get; set; }
    }
}