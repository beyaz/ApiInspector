using System;
using BOA.EOD.Base;

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

            EODTestHelper.InvokeMethod(eodInstance, "InitializeParameters");
            EODTestHelper.InvokeMethod(eodInstance, "BeforeProcess");
            EODTestHelper.InvokeMethod(eodInstance, "Process");
            EODTestHelper.InvokeMethod(eodInstance, "AfterProcess");
        }
        #endregion
    }
}