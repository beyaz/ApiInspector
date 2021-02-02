using System;
using System.Collections.Generic;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invoke output
    /// </summary>
    [Serializable]
    class InvokeOutput
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvokeOutput" /> class.
        /// </summary>
        public InvokeOutput(Exception exception)
        {
            Error = exception;
        }

       

        #region Public Properties
        /// <summary>
        ///     Gets the error.
        /// </summary>
        public Exception Error { set; get; }

        /// <summary>
        ///     The execution response as json
        /// </summary>
        public string ExecutionResponseAsJson { set; get; }


        public IReadOnlyList<string> InvocationParameters { get; set; }

        public InvokeOutput()
        {
            
        }


        #endregion
    }
}