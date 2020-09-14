using ApiInspector.Infrastructure;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The program
    /// </summary>
    class Program
    {
        #region Public Methods
        /// <summary>
        ///     Mains the specified arguments.
        /// </summary>
        public static void Main(string[] args)
        {
            const string applicationName     = "ApiInspector";
            const string TargetDirectoryPath = @"d:\boa\server\bin\";

            ProcessKiller.KillAll(applicationName);

            ModuleLoader.Load();

            new Launcher(TargetDirectoryPath,applicationName,new Tracer()).Start();
        }
        #endregion
    }
}