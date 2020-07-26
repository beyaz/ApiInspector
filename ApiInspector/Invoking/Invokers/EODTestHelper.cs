using System;
using System.Collections.Generic;
using System.Reflection;
using BOA.Common.TaskScheduler.Types;
using BOA.EOD.Base;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The eod test helper
    /// </summary>
    public static class EODTestHelper
    {
        #region Public Methods
        /// <summary>
        ///     Gets the context.
        /// </summary>
        public static EODExecutionContext GetContext<T>(this T eodInstance) where T : EODBase
        {
            const string propertyName = "Context";

            var methodInfo = eodInstance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new MissingMemberException(propertyName);
            }

            return (EODExecutionContext) methodInfo.GetMethod.Invoke(eodInstance, null);
        }

        /// <summary>
        ///     Initialize new instance of <see cref="T:EODBaseInheritedType" /> with @BusinessDate: DateTime.Today
        /// </summary>
        public static EODBaseInheritedType GetInstanceOfEOD<EODBaseInheritedType>() where EODBaseInheritedType : EODBase, new()
        {
            var eodInstance = new EODBaseInheritedType();
            InitializeContextPropertyOfEOD(eodInstance, DateTime.Today);
            return eodInstance;
        }

        /// <summary>
        ///     Invokes the method.
        /// </summary>
        public static void InvokeMethod<EODBaseInheritedType>(EODBaseInheritedType eodInstance, string methodName) where EODBaseInheritedType : EODBase
        {
            var methodInfo = eodInstance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new MissingMemberException(methodName);
            }

            methodInfo.Invoke(eodInstance, null);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Initializes the context property of eod.
        /// </summary>
        internal static void InitializeContextPropertyOfEOD(object eodInstance, DateTime businessDate)
        {
            const string FieldNameContext = "context";
            const string fieldName        = "execcontext";

            SetEODField(eodInstance, FieldNameContext, new EODExecutionContext(1, new Dictionary<string, object>(), businessDate, 1, null));
            SetEODField(eodInstance, fieldName, new ExecutionContext());
        }

        /// <summary>
        ///     Sets the eod field.
        /// </summary>
        static void SetEODField(object eodInstance, string fieldName, object fieldValue)
        {
            var field = typeof(EODBase).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new MissingFieldException(fieldName);
            }

            field.SetValue(eodInstance, fieldValue);
        }
        #endregion
    }
}