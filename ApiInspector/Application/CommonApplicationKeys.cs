using System;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The common application keys
    /// </summary>
    static class CommonApplicationKeys
    {
        #region Static Fields
        /// <summary>
        ///     The assembly search directories
        /// </summary>
        public static readonly StringList AssemblySearchDirectories = new StringList(typeof(AssemblyFinder), nameof(AssemblySearchDirectories));

        /// <summary>
        ///     The trace
        /// </summary>
        public static readonly DataKey<Action<string>> Trace = new DataKey<Action<string>>(typeof(CommonApplicationKeys), nameof(Trace));
        #endregion
    }
}