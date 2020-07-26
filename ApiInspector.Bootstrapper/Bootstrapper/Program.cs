using System.Diagnostics;
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
            foreach (var process in Process.GetProcessesByName("ApiInspector"))
            {
                process.Kill();
            }

            ModuleLoader.Load();

            new Launcher().Start();
        }
        #endregion
    }
}