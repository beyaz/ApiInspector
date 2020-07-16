using System;
using System.Diagnostics;
using System.IO;
using ApiInspector.Application;
using BOA.DataFlow;

namespace ApiInspector.CardSystemOldAndNewApiCall
{
    /// <summary>
    ///     The external code compare program starter
    /// </summary>
    class ExternalCodeCompareProgramStarter
    {
        #region Static Fields
        public static DataKey<string> NewCardSystemResult = new DataKey<string>(nameof(NewCardSystemResult));
        public static DataKey<string> OldCardSystemResult = new DataKey<string>(nameof(OldCardSystemResult));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public static void Start(DataContext context)
        {
            var oldCardSystemResult = context.Get(OldCardSystemResult);
            var newCardSystemResult = context.Get(NewCardSystemResult);

            var externalProgramPaths = GetProgramPath();
            if (externalProgramPaths == null)
            {
                context.Get(Logger.Key).Push("Code compare tool not found");
                return;
            }

            var outputFolderPath = System.IO.Path.GetTempPath();

            var oldCardSystemResponseFilePath = Path.Combine(outputFolderPath, "OldCardSystemResponse.json");
            var newCardSystemResponseFilePath = Path.Combine(outputFolderPath, "NewCardSystemResponse.json");

            File.WriteAllText(oldCardSystemResponseFilePath, oldCardSystemResult);
            File.WriteAllText(newCardSystemResponseFilePath, newCardSystemResult);

            Process.Start(new ProcessStartInfo(externalProgramPaths)
            {
                Arguments = $"{oldCardSystemResponseFilePath} {newCardSystemResponseFilePath}"
            });
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the program path.
        /// </summary>
        static string GetProgramPath()
        {
            var path = @"C:\Program Files\Devart\Code Compare\CodeCompare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = @"D:\Program Files\Devart\Code Compare\CodeCompare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            return null;
        }
        #endregion
    }
}