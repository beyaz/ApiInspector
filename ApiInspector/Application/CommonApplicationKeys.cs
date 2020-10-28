using System;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The common application keys
    /// </summary>
    static class CommonApplicationKeys
    {
        #region Static Fields
        /// <summary>
        ///     The trace
        /// </summary>
        public static readonly DataKey<Action<string>> Trace = new DataKey<Action<string>>(typeof(CommonApplicationKeys), nameof(Trace));
        #endregion
    }
}