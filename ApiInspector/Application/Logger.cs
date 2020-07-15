using System;
using System.IO;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The logger
    /// </summary>
    class Logger
    {
        #region Static Fields
        /// <summary>
        ///     The key
        /// </summary>
        public static readonly DataKey<Logger> Key = new DataKey<Logger>(nameof(Logger));
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the file path.
        /// </summary>
        static string FilePath => Path.GetDirectoryName(typeof(Logger).Assembly.Location) + Path.DirectorySeparatorChar + "Log.txt";
        #endregion

        #region Public Methods
        /// <summary>
        ///     Logs the specified message.
        /// </summary>
        public void Log(string message)
        {
            try
            {
                var fs = new FileStream(FilePath, FileMode.Append);

                var sw = new StreamWriter(fs);
                sw.Write(message);
                sw.Write(Environment.NewLine);
                sw.Close();
                fs.Close();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     Pushes the specified exception.
        /// </summary>
        public void Push(Exception exception)
        {
            Log(exception.ToString());
        }
        #endregion
    }
}