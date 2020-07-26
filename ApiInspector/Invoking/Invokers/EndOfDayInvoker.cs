using System;
using BOA.EOD.Base;

namespace ApiInspector.Invoking.Invokers
{
    class EndOfDayInvoker
    {
        #region Public Methods
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