using System;
using System.IO;
using System.Reflection;

namespace ApiInspector
{
    static class AssemblySearchDirectories
    {
        public const string serverBin = @"d:\boa\server\bin\";
        public const string clientBin = @"d:\boa\client\bin\";
    }

    static class EndOfDay
    {
        public const string MethodAccessText = "InitializeParameters -> BeforeProcess -> Process -> AfterProcess";
    }

    /// <summary>
    ///     The utility
    /// </summary>
    static class Utility
    {
        #region Public Properties
        /// <summary>
        ///     Gets all bindings.
        /// </summary>
        public static BindingFlags AllBindings
        {
            get { return BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Determines whether the specified action is success.
        /// </summary>
        public static bool IsSuccess<T>(Func<T> action, ref T target)
        {
            try
            {
                target = action();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Tries the run.
        /// </summary>
        public static Exception TryRun(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        ///     Writes all text.
        /// </summary>
        public static void WriteAllText(string filePath, string content)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null)
            {
                throw new ArgumentNullException(nameof(directoryName));
            }

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(filePath, content);
        }
        #endregion
    }
}