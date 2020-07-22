using System;
using System.Collections.Generic;
using System.Reflection;
using BOA.Common.TaskScheduler.Types;
using BOA.EOD.Base;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The eod test helper
    /// </summary>
    public static class EODTestHelper
    {
        #region Enums
        /// <summary>
        ///     The method name
        /// </summary>
        enum MethodName
        {
            /// <summary>
            ///     The process
            /// </summary>
            Process
        }

        /// <summary>
        ///     The property name
        /// </summary>
        enum PropertyName
        {
            /// <summary>
            ///     The context
            /// </summary>
            Context
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the context.
        /// </summary>
        public static EODExecutionContext GetContext<T>(this T eodInstance) where T : EODBase
        {
            var methodInfo = eodInstance.GetType().GetProperty(PropertyName.Context.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new MissingMemberException(PropertyName.Context.ToString());
            }

            return (EODExecutionContext) methodInfo.GetMethod.Invoke(eodInstance, null);
        }

        /// <summary>
        ///     Initialize new instance of <see cref="T:EODBaseInheritedType" /> with given <paramref name="businessDate" />.
        /// </summary>
        public static EODBaseInheritedType GetInstanceOfEOD<EODBaseInheritedType>(DateTime businessDate) where EODBaseInheritedType : EODBase, new()
        {
            var eodInstance = new EODBaseInheritedType();
            InitializeContextPropertyOfEOD(eodInstance, businessDate);
            return eodInstance;
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
        ///     Invokes the process.
        /// </summary>
        public static void InvokeProcess<EODBaseInheritedType>(EODBaseInheritedType eodInstance) where EODBaseInheritedType : EODBase
        {
            var methodInfo = eodInstance.GetType().GetMethod(MethodName.Process.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new MissingMemberException(MethodName.Process.ToString());
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

namespace ApiInspector.Invoking
{
    class EndOfDayInvoker
    {
        #region Public Methods
        public void Invoke(Type eodType)
        {
            var eodInstance = (EODBase) Activator.CreateInstance(eodType);
            EODTestHelper.InitializeContextPropertyOfEOD(eodInstance, DateTime.Today);
            EODTestHelper.InvokeProcess(eodInstance);
        }
        #endregion
    }
}