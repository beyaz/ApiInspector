using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

 using static ApiInspector.InvocationInfoEditor.ViewControllerKeys;

namespace ApiInspector.InvocationInfoEditor
{
    class ViewControllerKeys
    {
        public static DataKey<Action<string>> TraceKey = new DataKey<Action<string>>(nameof(TraceKey));


        public static DataKey<ItemSourceList> ItemSourceListKey = new DataKey<ItemSourceList>(nameof(ItemSourceList));

        /// <summary>
        ///     The selected invocation information key
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfoKey = new DataKey<InvocationInfo>(nameof(InvocationInfo));



        public static DataKey<MethodDefinition> MethodDefinitionKey = new DataKey<MethodDefinition>(nameof(MethodDefinition));

        public static DataKey<TypeDefinition>   TypeDefinitionKey   = new DataKey<TypeDefinition>(nameof(TypeDefinition));
    }

    /// <summary>
    ///     The view controller
    /// </summary>
    class ViewController
    {
        public ViewController()
        {
            
        }
        


        public DataContext context;

        InvocationInfo InvocationInfo => SelectedInvocationInfoKey[context];
        ItemSourceList ItemSourceList => ItemSourceListKey[context];


        #region Public Methods
        /// <summary>
        ///     Called when [assembly name changed].
        /// </summary>
        public void OnAssemblyNameChanged()
        {
            var log = TraceKey[context];

            var assemblyFilePath = GetAssemblyFilePath(InvocationInfo);

            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = InvocationInfo.AssemblySearchDirectory;

            
            var typeVisitor = new TypeVisitor(log, new List<string> {assemblySearchDirectory});

            ItemSourceList.ClassNameList = typeVisitor.GeTypeDefinitions(assemblyFilePath).Select(x => x.FullName).ToList();
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

            if (assemblySearchDirectory == AssemblySearchDirectories.clientBin)
            {
                ItemSourceList.AssemblyNameList = ItemSourceList.AssemblyNameList.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("BOA.EOD.")).ToList();
            }
        }

        /// <summary>
        ///     Called when [class name changed].
        /// </summary>
        public void OnClassNameChanged()
        {
            var log = TraceKey[context];
            var invocationInfo = InvocationInfo;

            var assemblyFilePath = GetAssemblyFilePath(invocationInfo);
            if (!File.Exists(assemblyFilePath))
            {
                log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            var assemblySearchDirectory = InvocationInfo.AssemblySearchDirectory;

            var typeVisitor = new TypeVisitor(log, new List<string> {assemblySearchDirectory});

            var typeDefinition =  typeVisitor.FindType(assemblyFilePath, invocationInfo.ClassName);
            context.Update(TypeDefinitionKey,typeDefinition);
            if (typeDefinition == null)
            {
                log($"Type not exists. File:{assemblyFilePath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            ItemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();

            if (assemblySearchDirectory == AssemblySearchDirectories.clientBin)
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
            
            context.Update(MethodDefinitionKey,TypeDefinitionKey[context].Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName));
            
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