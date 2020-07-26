using System;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoke output
    /// </summary>
    class InvokeOutput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvokeOutput" /> class.
        /// </summary>
        public InvokeOutput(Exception error, object executionResponse, string executionResponseAsJson)
        {
            Error                   = error;
            ExecutionResponse       = executionResponse;
            ExecutionResponseAsJson = executionResponseAsJson;
        }

        public InvokeOutput(object response)
        {
            ExecutionResponse = response;
        }
        #endregion

        public bool IsSuccess => Error != null;

        #region Public Properties
        /// <summary>
        ///     Gets the error.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        ///     Gets the execution response.
        /// </summary>
        public object ExecutionResponse { get; }

        /// <summary>
        ///     The execution response as json
        /// </summary>
        public string ExecutionResponseAsJson { get; }
        #endregion
    }
}