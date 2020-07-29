using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;

namespace ApiInspector.Bootstrapper
{
    /// <summary>
    ///     The application files synchronizer
    /// </summary>
    class ApplicationFilesSynchronizer
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
        ///     Initializes a new instance of the <see cref="ApplicationFilesSynchronizer" /> class.
        /// </summary>
        public ApplicationFilesSynchronizer(string targetDirectoryPath, string applicationName)
        {
            this.targetDirectoryPath = targetDirectoryPath ?? throw new ArgumentNullException(nameof(targetDirectoryPath));
            this.applicationName     = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Synchronizes this instance.
        /// </summary>
        public void Synchronize()
        {
            var files = FetchFiles();

            ExportFiles(files);
        }
        #endregion

        #region Methods
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

        /// <summary>
        ///     Downloads the files.
        /// </summary>
        void ExportFiles(IReadOnlyCollection<FileModel> files)
        {
            foreach (var file in files)
            {
                var path = Path.Combine(targetDirectoryPath, file.Name);

                WriteAllBytes(path, file.Content);
            }
        }

        /// <summary>
        ///     Fetches the files.
        /// </summary>
        IReadOnlyCollection<FileModel> FetchFiles()
        {
            const string ConnectionString = "server=srvdev\\ATLAS;database =BOA;integrated security=true";

            var connection = new SqlConnection(ConnectionString);

            var files = connection.Query<FileModel>($"SELECT {nameof(FileModel.Name)}, {nameof(FileModel.LastModification)} FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = @{nameof(applicationName)}", new {applicationName}).ToList().AsReadOnly();

            var requiredFiles = files.Where(x => !IsFileUpToDate(Path.Combine(targetDirectoryPath, x.Name), x.LastModification)).Select(x => x.Name).ToList();

            var sql = $"SELECT * FROM  [WHT].[File] WITH(NOLOCK) WHERE ApplicationName = @{nameof(applicationName)} AND {nameof(FileModel.Name)} IN ({"'" + string.Join("','", requiredFiles) + "'"})";

            return connection.Query<FileModel>(sql, new {applicationName}).ToList().AsReadOnly();
        }
        #endregion
    }
}