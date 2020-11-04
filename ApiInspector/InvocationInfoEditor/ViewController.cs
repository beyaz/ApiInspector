using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using static ApiInspector.Keys;
using static ApiInspector.DataAccess.TypeVisitor;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
    {
        #region Fields
        /// <summary>
        ///     The model
        /// </summary>
        readonly ViewModel model;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewController" /> class.
        /// </summary>
        public ViewController(ViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => model.InvocationInfo;

        /// <summary>
        ///     Gets the item source list.
        /// </summary>
        ItemSourceList ItemSourceList => model.ItemSourceList;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Called when [assembly name changed].
        /// </summary>
        public void OnAssemblyNameChanged()
        {
            var log = model.Trace;

            var assemblyFilePath = GetAssemblyFilePath(InvocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = InvocationInfo.AssemblySearchDirectory;

            var scope = new Scope
            {
                {AssemblySearchDirectories,new List<string> {assemblySearchDirectory}},
                {Trace,model.Trace},
                {AssemblyPath,assemblyFilePath}
            };

            ItemSourceList.ClassNameList = GeTypeDefinitions(scope).Select(x => x.FullName).ToList();
        }

        /// <summary>
        ///     Called when [assembly search directory changed].
        /// </summary>
        public void OnAssemblySearchDirectoryChanged()
        {
            var assemblySearchDirectory = InvocationInfo.AssemblySearchDirectory;
            if (!Directory.Exists(assemblySearchDirectory))
            {
                return;
            }

            ItemSourceList.AssemblyNameList = Directory.GetFiles(assemblySearchDirectory).Select(Path.GetFileName).ToList();

            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                ItemSourceList.AssemblyNameList = ItemSourceList.AssemblyNameList.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("BOA.EOD.")).ToList();
            }
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public void OnClassNameChanged()
        {
            var log            = model.Trace;
            var invocationInfo = InvocationInfo;

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);
            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = InvocationInfo.AssemblySearchDirectory;

            var scope = new Scope
            {
                {AssemblySearchDirectories, new List<string> {assemblySearchDirectory}},
                {Trace, model.Trace},
                {AssemblyPath,assemblyFilePath}
            };
            var typeDefinition = model.TypeDefinition = FindTypeDefinition(scope, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            ItemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();

            if (assemblySearchDirectory == CommonAssemblySearchDirectories.clientBin)
            {
                ItemSourceList.MethodNameList = new List<string> {EndOfDay.MethodAccessText};
            }
        }

        /// <summary>
        ///     Called when [method name selected].
        /// </summary>
        public void OnMethodNameSelected()
        {
            var invocationInfo = InvocationInfo;

            model.MethodDefinition = model.TypeDefinition?.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the assembly file path.
        /// </summary>
        static string GetAssemblyFilePath(InvocationInfo invocationInfo)
        {
            var assemblyName      = invocationInfo.AssemblyName;
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;
            var assemblyPath      = Path.Combine(assemblyDirectory, assemblyName);

            return assemblyPath;
        }
        #endregion
    }
}