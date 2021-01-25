using System;

namespace ApiInspector.MainWindow
{
    [Serializable]
    public sealed class ExecutionResponseHasErrorException : Exception
    {
        #region Constructors
        public ExecutionResponseHasErrorException()
        {
        }

        public ExecutionResponseHasErrorException(string message) : base(message)
        {
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return Message;
        }
        #endregion
    }
}