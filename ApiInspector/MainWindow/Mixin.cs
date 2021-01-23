using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The mixin
    /// </summary>
    static partial class Mixin
    {
        #region Public Properties
        /// <summary>
        ///     Gets the show error notification key.
        /// </summary>
        public static DataKey<Action<string>> ShowErrorNotificationKey => CreateKey<Action<string>>(typeof(Mixin));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates the key.
        /// </summary>
        public static DataKey<T> CreateKey<T>(Type locatedType, [CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(locatedType, callerMemberName);
        }

        /// <summary>
        ///     Creates the new invocation information.
        /// </summary>
        public static InvocationInfo CreateNewInvocationInfo()
        {
            return new InvocationInfo
            {
                Scenarios = new List<ScenarioInfo>()
            };
        }

        public static Exception SafeRun(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        ///     Creates the new scenario information.
        /// </summary>
        public static ScenarioInfo CreateNewScenarioInfo()
        {
            return new ScenarioInfo
            {
                Assertions       = new List<AssertionInfo>(),
                MethodParameters = new List<InvocationMethodParameterInfo>()
            };
        }

        /// <summary>
        ///     Determines whether [is end of day method] [the specified invocation information].
        /// </summary>
        public static bool IsEndOfDayMethod(InvocationInfo invocationInfo)
        {
            return invocationInfo.MethodName == EndOfDay.MethodAccessText;
        }

        /// <summary>
        ///     Shows the error notification.
        /// </summary>
        public static void ShowErrorNotification(this Scope scope, string errorMessage)
        {
            scope.Get(ShowErrorNotificationKey)(errorMessage);
        }
        #endregion
    }
}