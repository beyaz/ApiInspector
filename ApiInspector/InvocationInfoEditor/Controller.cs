using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using BOA.Base;
using BOA.Base.Data;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    static class Controller
    {
        #region Public Methods

        public static void OnAssemblySearchDirectoryChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            itemSourceList.AssemblyNameList = Directory.GetFiles(invocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();

        }
        public static void OnClassNameChanged(DataContext context)
        {

            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);


        

            var logger            = context.Get(Logger.Key);
         

            var assemblyPath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var typeDefinition = CecilHelper.FindType(context, assemblyPath, invocationInfo.ClassName);
            if (typeDefinition == null)
            {
                logger.Log($"Type not exists. File:{assemblyPath}, fullClassName:{invocationInfo.ClassName}");
                return;
            }

            itemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();
        }
        public static void OnAssemblyNameChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            var assemblyName = invocationInfo.AssemblyName;

            var logger            = context.Get(Logger.Key);
            var assemblyDirectory = invocationInfo.AssemblySearchDirectory;

            var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                logger.Log($"File not exists. File:{assemblyPath}");
                return;
            }

            var items = new List<TypeDefinition>();

            CecilHelper.VisitAllTypes(context, assemblyPath, typeDefinition => { items.Add(typeDefinition); });

            itemSourceList.ClassNameList = items.Select(x => x.FullName).ToList();


        }

        public static void OnMethodNameSelected(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var className = invocationInfo.ClassName;
            var methodName = invocationInfo.MethodName;
            var assemblyFilePath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, className);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(Data.MethodDefinition, methodDefinition);

            UpdateUI(context);
        }

       

        
        #endregion

        #region Methods
        static StackPanel Create(ParameterDefinition definition)
        {
            var sp = new StackPanel();

            var label = new Label
            {
                Content    = definition.Name,
                FontWeight = FontWeights.Bold
            };

            var editor = new TextBox();

            if (definition.ParameterType.FullName == typeof(ObjectHelper).FullName)
            {
                editor.IsEnabled = false;
            }

            sp.Children.Add(label);
            sp.Children.Add(editor);

            return sp;
        }

        static object CreateInstance(Type targetType, ExecutionDataContext executionDataContext)
        {
            // constructor with ExecutionDataContext
            {
                var constructorInfo = targetType.GetConstructor(new[]
                {
                    typeof(ExecutionDataContext)
                });
                if (constructorInfo != null)
                {
                    var instance = constructorInfo.Invoke(new object[]
                    {
                        executionDataContext
                    });
                    return instance;
                }
            }

            // simple constructor
            {
                var instance     = Activator.CreateInstance(targetType);
                var objectHelper = instance as ObjectHelper;
                if (objectHelper != null)
                {
                    objectHelper.Context = executionDataContext;
                }

                return instance;
            }
        }

        static void UpdateUI(DataContext context)
        {
            var panel = context.Get(Data.ParametersPanel);

            panel.Children.Clear();

            var methodDefinition = context.Get(Data.MethodDefinition);

            foreach (var parameterDefinition in methodDefinition.Parameters)
            {
                var item = Create(parameterDefinition);

                panel.Children.Add(item);
            }
        }
        #endregion
    }
}