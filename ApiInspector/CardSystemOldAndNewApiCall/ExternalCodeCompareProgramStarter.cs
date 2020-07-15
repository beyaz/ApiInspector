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
        public static DataKey<string> NewCardSystemResultOutputFilePath = new DataKey<string>(nameof(NewCardSystemResultOutputFilePath));
        public static DataKey<string> OldCardSystemResultOutputFilePath = new DataKey<string>(nameof(OldCardSystemResultOutputFilePath));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public static void Start(DataContext context)
        {
            var source = context.Get(OldCardSystemResultOutputFilePath);
            var target = context.Get(NewCardSystemResultOutputFilePath);

            var externalProgramPaths = GetProgramPath();
            if (externalProgramPaths == null)
            {
                context.Get(Logger.Key).Push("Code compare tool not found");
                return;
            }

            Process.Start(new ProcessStartInfo(externalProgramPaths)
            {
                Arguments = $"{source} {target}"
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