using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BOA.DataFlow;
using Dapper;
using Dapper.Contrib.Extensions;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The file model
    /// </summary>
    [Table("[WHT].[File]")]
    class FileModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        [ExplicitKey]
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the content.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        [ExplicitKey]
        public string FullName { get; set; }
        #endregion
    }

    /// <summary>
    ///     The key
    /// </summary>
    class Key
    {
        #region Static Fields
        /// <summary>
        ///     The files
        /// </summary>
        public static DataKey<IReadOnlyList<FileModel>> Files = new DataKey<IReadOnlyList<FileModel>>(nameof(Files));

        /// <summary>
        ///     The target directory path
        /// </summary>
        public static DataKey<string> TargetDirectoryPath = new DataKey<string>(nameof(TargetDirectoryPath));
        #endregion
    }

    /// <summary>
    ///     The launcher
    /// </summary>
    class Launcher
    {
        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            Console.WriteLine("Yükleniyor...");

            var context = new DataContext
            {
                {Key.TargetDirectoryPath, @"d:\boa\server\bin\"}
            };

            FetchFiles(context);

            ExportFiles(context);

            StartProcess(context);

            Console.WriteLine("Process is started.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Downloads the files.
        /// </summary>
        static void ExportFiles(DataContext context)
        {
            var targetDir = context.Get(Key.TargetDirectoryPath);

            foreach (var file in context.Get(Key.Files))
            {
                var path = Path.Combine(targetDir, file.FullName);

                File.WriteAllBytes(path, file.Content);
            }
        }

        /// <summary>
        ///     Fetches the files.
        /// </summary>
        static void FetchFiles(DataContext context)
        {
            const string ConnectionString = "server=srvdev\\ATLAS;database =BOA;integrated security=true";

            var connection = new SqlConnection(ConnectionString);

            context.Add(Key.Files, connection.Query<FileModel>("SELECT * FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = 'ApiInspector'").ToList().AsReadOnly());
        }

        /// <summary>
        ///     Starts the process.
        /// </summary>
        static void StartProcess(DataContext context)
        {
            var targetDir = context.Get(Key.TargetDirectoryPath);

            Process.Start(Path.Combine(targetDir, "Run.bat"));
        }
        #endregion
    }
}