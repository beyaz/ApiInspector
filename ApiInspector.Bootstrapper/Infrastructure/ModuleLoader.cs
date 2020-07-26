namespace ApiInspector.Infrastructure
{
    /// <summary>
    ///     The module loader
    /// </summary>
    static class ModuleLoader
    {
        #region Public Methods
        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public static void Load()
        {
            EmbeddedCompressedAssemblyReferencesResolver.Resolve("EmbeddedReferences.zip");
        }
        #endregion
    }
}