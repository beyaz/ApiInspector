namespace ApiInspector.Application
{
    /// <summary>
    ///     The connection string
    /// </summary>
    class ConnectionString
    {
        #region Public Properties
        /// <summary>
        ///     Gets the current connection string.
        /// </summary>
        public string CurrentConnectionString { get; } = "server=srvdev\\ATLAS;database =BOA;integrated security=true";
        #endregion
    }
}