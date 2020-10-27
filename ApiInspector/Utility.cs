using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ApiInspector
{
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
        ///     Cleans the path.
        /// </summary>
        public static string CleanPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r           = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, string.Empty);
        }

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
        ///     Sets the text.
        /// </summary>
        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
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
        ///     Tries the run.
        /// </summary>
        public static T TryRun<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        ///     Writes all text.
        /// </summary>
        public static void WriteToFile(string filePath, string content)
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