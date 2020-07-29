using System;
using System.Diagnostics;
using System.IO;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The launcher
    /// </summary>
    class Launcher
    {
        #region Constants
        /// <summary>
        ///     The target directory path
        /// </summary>
        const string TargetDirectoryPath = @"d:\boa\server\bin\";
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            Console.WriteLine("Yükleniyor...");

            var synchronizer = new ApplicationFilesSynchronizer(TargetDirectoryPath, "ApiInspector");

            synchronizer.Synchronize();

            StartProcess();

            Console.WriteLine("Process is started.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Starts the process.
        /// </summary>
        static void StartProcess()
        {
            Process.Start(Path.Combine(TargetDirectoryPath, "ApiInspector.exe"));
        }
        #endregion
    }
}