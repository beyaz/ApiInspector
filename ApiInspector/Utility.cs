using System;
using System.Collections.Generic;
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

        public static void IfFileNotExistsThen(string filePath, Action body)
        {
            if (!File.Exists(filePath))
            {
                body();
            }
        }

        public static void IfNot(Func<bool> funcCondition, Action body)
        {
            var condition = funcCondition();
            if (!condition)
            {
                body();
            }
        }

        public static void IfNot(bool condition, Action body)
        {
            if (!condition)
            {
                body();
            }
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

        public static List<T> ListOf<T>(params T[] arr)
        {
            return new List<T>(arr);
        }

        /// <summary>
        ///     Removes from end.
        /// </summary>
        public static string RemoveFromEnd(this string value, string remove)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(remove))
            {
                return value;
            }

            if (value.EndsWith(remove, StringComparison.OrdinalIgnoreCase))
            {
                return value.Substring(0, value.Length - remove.Length);
            }

            return value;
        }

        /// <summary>
        ///     Removes value from start of str
        /// </summary>
        public static string RemoveFromStart(this string data, string value)
        {
            return RemoveFromStart(data, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Removes value from start of str
        /// </summary>
        public static string RemoveFromStart(this string data, string value, StringComparison comparison)
        {
            if (data == null)
            {
                return null;
            }

            if (data.StartsWith(value, comparison))
            {
                return data.Substring(value.Length, data.Length - value.Length);
            }

            return data;
        }

        /// <summary>
        ///     Sets the text.
        /// </summary>
        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        public static IReadOnlyList<TDestination> ToList<TSource, TDestination>(this IEnumerable<TSource> items, Func<TSource, int, TDestination> converter)
        {
            var resultList = new List<TDestination>();

            var i = 0;

            foreach (var item in items)
            {
                resultList.Add(converter(item, i++));
            }

            return resultList;
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