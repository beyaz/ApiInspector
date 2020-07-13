using System;
using System.IO;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using BOA.DataFlow;

namespace ApiInspector.Application
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        ///     The context
        /// </summary>
        public static readonly  DataContext Context = new DataContext();

        static App()
        {
            Context.ForwardKey(AssemblyIntellisenseTextBox.AssemblyNames,AssemblyNamesAll.Key);
            Context.ForwardKey(ClassNameIntellisenseTextBox.ClassNames,ClassNamesInAssembly.Key);
            Context.Add(Logger.Key,new Logger());
        }
    }


    class Logger
    {
        public static readonly DataKey<Logger> Key = new DataKey<Logger>(nameof(Logger));

        static string FilePath => Path.GetDirectoryName(typeof(Logger).Assembly.Location) + Path.DirectorySeparatorChar + "Log.txt";

        public  void Push(Exception exception)
        {
            Log(exception.ToString());
        }

        public  void Log(string message)
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
