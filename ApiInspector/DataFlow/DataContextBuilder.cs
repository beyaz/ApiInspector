using System.Collections.Generic;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;
using static ApiInspector.DataFlow.ServiceKeys;

namespace ApiInspector.DataFlow
{
    class DataContextBuilder
    {
        public DataContext Build()
        {
            var context = new DataContext();

            var traceMessages = new List<string>();

            var itemSourceList = new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string> {AssemblySearchDirectories.serverBin, AssemblySearchDirectories.clientBin},
                EnvironmentNameList         = new List<string> {"dev", "test"}
            };

            MainWindowViewModelKey[context] = new MainWindowViewModel
            {
                InvocationEditor = new InvocationEditorViewModel
                {
                    InvocationInfo = new InvocationInfo
                    {
                        AssemblySearchDirectory = AssemblySearchDirectories.serverBin
                    },
                    ItemSourceList = itemSourceList,
                },
                TraceMessages = traceMessages
            };

            TraceKey[context] = traceMessages.Add;
            HistoryServiceKey[context] = new DataSource();
            ItemSourceListKey[context] = itemSourceList;
            
            
            return context;
        }
    }
}