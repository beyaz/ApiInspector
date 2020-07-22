using ApiInspector.Infrastructure;

namespace ApiInspector.Bootstrapper
{
    class Program
    {
        #region Public Methods
        public static void Main(string[] args)
        {
            ModuleLoader.Load();

            new Launcher().Start();
        }
        #endregion
    }
}