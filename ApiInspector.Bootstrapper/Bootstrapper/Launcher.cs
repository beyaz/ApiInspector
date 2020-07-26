using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BOA.DataFlow;
using Dapper;
using static ApiInspector.Bootstrapper.Key;

namespace ApiInspector.Bootstrapper
{
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
                {TargetDirectoryPath, @"d:\boa\server\bin\"}
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
            var targetDirectoryPath = context.Get(TargetDirectoryPath);

            foreach (var file in context.Get(Files))
            {
                var path = Path.Combine(targetDirectoryPath, file.Name);

                File.Delete(path);
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

            var files = connection.Query<FileModel>($"SELECT {nameof(FileModel.Name)}, {nameof(FileModel.LastModification)} FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = 'ApiInspector'").ToList().AsReadOnly();

            var targetDirectoryPath = context.Get(TargetDirectoryPath);

            var requiredFiles = files.Where(x => !IsFileUpToDate(Path.Combine(targetDirectoryPath, x.Name), x.LastModification)).Select(x => x.Name).ToList();

            var sql = $"SELECT * FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = 'ApiInspector' AND {nameof(FileModel.Name)} IN ({"'" + string.Join("','", requiredFiles) + "'"})";

            context.Add(Files, connection.Query<FileModel>(sql).ToList().AsReadOnly());
        }

        /// <summary>
        ///     Determines whether [is file up to date] [the specified path].
        /// </summary>
        static bool IsFileUpToDate(string path, DateTime lastModification)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            return new FileInfo(path).CreationTime > lastModification;
        }

        /// <summary>
        ///     Starts the process.
        /// </summary>
        static void StartProcess(DataContext context)
        {
            var targetDirectoryPath = context.Get(TargetDirectoryPath);

            Process.Start(Path.Combine(targetDirectoryPath, "ApiInspector.Run.bat"));
        }
        #endregion
    }
}