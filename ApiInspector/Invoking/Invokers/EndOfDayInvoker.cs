using System;
using BOA.EOD.Base;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The end of day invoker
    /// </summary>
    class EndOfDayInvoker
    {
        #region Public Methods
        /// <summary>
        ///     Invokes the specified eod type.
        /// </summary>
        public void Invoke(Type eodType)
        {
            var eodInstance = (EODBase) Activator.CreateInstance(eodType);

            EODTestHelper.InitializeContextPropertyOfEOD(eodInstance, DateTime.Today);

            var invoke = fun((string methodName) => EODTestHelper.InvokeMethod(eodInstance, methodName));

            invoke("InitializeParameters");
            invoke("BeforeProcess");
            invoke("Process");
            invoke("AfterProcess");
        }
        #endregion
    }
}