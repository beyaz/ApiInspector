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
        #region Fields
        /// <summary>
        ///     The application name
        /// </summary>
        readonly string applicationName;

        /// <summary>
        ///     The target directory path
        /// </summary>
        readonly string targetDirectoryPath;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Launcher" /> class.
        /// </summary>
        public Launcher(string targetDirectoryPath, string applicationName)
        {
            this.targetDirectoryPath = targetDirectoryPath ?? throw new ArgumentNullException(nameof(targetDirectoryPath));
            this.applicationName     = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            Console.WriteLine("Yükleniyor...");

            var synchronizer = new ApplicationFilesSynchronizer(targetDirectoryPath, applicationName);

            synchronizer.Synchronize();

            StartProcess();

            Console.WriteLine("Process is started.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Starts the process.
        /// </summary>
        void StartProcess()
        {
            Process.Start(Path.Combine(targetDirectoryPath, $"{applicationName}.exe"));
        }
        #endregion
    }
}