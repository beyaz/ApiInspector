using System;
using System.IO;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    class Logger
    {
        public static readonly DataKey<Logger> Key = new DataKey<Logger>(nameof(Logger));

        static string FilePath => Path.GetDirectoryName(typeof(Logger).Assembly.Location) + Path.DirectorySeparatorChar + "Log.txt";

        public void Push(Exception exception)
        {
            Log(exception.ToString());
        }

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
    }
}