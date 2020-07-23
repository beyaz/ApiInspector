using System.Diagnostics;
using ApiInspector.Infrastructure;

namespace ApiInspector.Bootstrapper
{
    class Program
    {
        #region Public Methods
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