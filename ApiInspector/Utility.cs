using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;






























































namespace ApiInspector
{
    static class Utility
    {
        #region Public Properties
        public static BindingFlags AllBindings
        {
            get { return BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; }
        }
        #endregion

        #region Public Methods
        

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