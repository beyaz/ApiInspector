using System.Collections.Generic;
using ApiInspector.InvocationInfoEditor;

namespace ApiInspector.MainWindow
{
    class MainWindowViewModel
    {
        public InvocationEditorViewModel InvocationEditor { get; set; }
        public List<string> TraceMessages { get; set; }
    }
}