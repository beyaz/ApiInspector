using System;
using System.Diagnostics;
using System.IO;
using ApiInspector.Infrastructure;

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

        readonly Tracer tracer;

        /// <summary>
        ///     The target directory path
        /// </summary>
        readonly string targetDirectoryPath;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Launcher" /> class.
        /// </summary>
        public Launcher(string targetDirectoryPath, string applicationName, Tracer tracer)
        {
            this.targetDirectoryPath = targetDirectoryPath ?? throw new ArgumentNullException(nameof(targetDirectoryPath));
            this.applicationName     = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            this.tracer              = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            tracer.Trace("Yükleniyor...");

            var synchronizer = new ApplicationFilesSynchronizer(targetDirectoryPath, applicationName,tracer);

            synchronizer.Synchronize();

            StartProcess();

            tracer.Trace("Process is started.");
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