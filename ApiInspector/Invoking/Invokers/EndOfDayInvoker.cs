using System;
using System.Collections.Generic;
using System.Linq;
using BOA.Common.Helpers;
using BOA.Common.Types;
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
        public static string Invoke(Type eodType)
        {
            var eodInstance = (EODBase) Activator.CreateInstance(eodType);

            EODTestHelper.InitializeContextPropertyOfEOD(eodInstance, DateTime.Today);

            var invoke = Fun((string methodName) => EODTestHelper.InvokeMethod(eodInstance, methodName));

            invoke("InitializeParameters");
            invoke("BeforeProcess");
            invoke("Process");
            invoke("AfterProcess");

            var context = eodInstance.GetContext();
            if (context.Success)
            {
                return null;
            }

            var results = new List<Result>();
            foreach (var list in context.Results.Select(x => x.Results))
            {
               results.AddRange(list);
            }

            return StringHelper.ResultToDetailedString(results);
        }
        #endregion
    }
}