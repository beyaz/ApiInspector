using System;
using System.Runtime.Serialization;

namespace ApiInspector.MainWindow
{
    [Serializable]
    public sealed class ExecutionResponseHasErrorException : Exception
    {
        public ExecutionResponseHasErrorException()
        {
        }
        public ExecutionResponseHasErrorException(string message) : base(message)
        {
        }
        public ExecutionResponseHasErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public ExecutionResponseHasErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string ToString()
        {
            return Message;
        }
    }
}