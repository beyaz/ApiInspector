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

            List<string> traceMessages = new List<string>();

            var itemSourceList = new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string> {AssemblySearchDirectories.serverBin, AssemblySearchDirectories.clientBin},
                EnvironmentNameList         = new List<string> {"dev", "test"}
            };

            MainWindowViewModelKey[context] = new MainWindowViewModel
            {
                 TraceMessages = traceMessages
            };

            TraceKey[context] = traceMessages.Add;
            HistoryServiceKey[context] = new DataSource();
            ItemSourceListKey[context] = itemSourceList;
            TraceQueueKey[context] = new Tracer();
            
            
            return context;
        }
    }

    class Tracer
    {
        readonly List<string> traceMessages = new List<string>();

        public void AddMessage(string message)
        {
            traceMessages.Add(message);
        }

        public void ClearQueue()
        {
            traceMessages.Clear();
        }


        public IReadOnlyList<string> GetAllMessagesInQueue()
        {
            return traceMessages;
        }

    }
}