using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dapper;

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

            var files = FetchFiles();

            ExportFiles(files);

            StartProcess();

            Console.WriteLine("Process is started.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Downloads the files.
        /// </summary>
        static void ExportFiles(IReadOnlyCollection<FileModel> files)
        {
            foreach (var file in files)
            {
                var path = Path.Combine(TargetDirectoryPath, file.Name);

                WriteAllBytes(path, file.Content);
            }
        }

        /// <summary>
        ///     Fetches the files.
        /// </summary>
        static IReadOnlyCollection<FileModel> FetchFiles()
        {
            const string ConnectionString = "server=srvdev\\ATLAS;database =BOA;integrated security=true";

            var connection = new SqlConnection(ConnectionString);

            var files = connection.Query<FileModel>($"SELECT {nameof(FileModel.Name)}, {nameof(FileModel.LastModification)} FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = 'ApiInspector'").ToList().AsReadOnly();

            var requiredFiles = files.Where(x => !IsFileUpToDate(Path.Combine(TargetDirectoryPath, x.Name), x.LastModification)).Select(x => x.Name).ToList();

            var sql = $"SELECT * FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = 'ApiInspector' AND {nameof(FileModel.Name)} IN ({"'" + string.Join("','", requiredFiles) + "'"})";

            return connection.Query<FileModel>(sql).ToList().AsReadOnly();
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
        static void StartProcess()
        {
            Process.Start(Path.Combine(TargetDirectoryPath, "ApiInspector.Run.bat"));
        }

        /// <summary>
        ///     Writes all bytes.
        /// </summary>
        static void WriteAllBytes(string path, byte[] content)
        {
            var directoryName = Path.GetDirectoryName(path);

            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
            }

            File.Delete(path);
            File.WriteAllBytes(path, content);
        }
        #endregion
    }
}