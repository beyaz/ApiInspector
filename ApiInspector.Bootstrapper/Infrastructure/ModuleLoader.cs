namespace ApiInspector.Infrastructure
{
    static class ModuleLoader
    {
        #region Public Methods
        public static void Load()
        {
            EmbeddedCompressedAssemblyReferencesResolver.Resolve("EmbeddedReferences.zip");
        }
        #endregion
    }
}