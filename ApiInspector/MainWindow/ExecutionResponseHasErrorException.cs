using System;

namespace ApiInspector.MainWindow
{
    [Serializable]
    public class ExecutionResponseHasErrorException:Exception
    {
        public ExecutionResponseHasErrorException(string message):base(message)
        {
            
        }

        public override string ToString()
        {
            return Message;
        }
    }
}