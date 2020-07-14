using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            var assemblyNames = Directory.GetFiles(invocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();

            context.Update(Data.AssemblyNames, assemblyNames);
        }

        public static void OnAssemblyNameChanged(DataContext context)
        {
            var assemblySearchDirectory = context.Get(DataKeys.AssemblySearchDirectory);
            var assemblyName            = context.Get(DataKeys.AssemblyName);

            context.Update(DataKeys.AssemblyFilePath, Path.Combine(assemblySearchDirectory, assemblyName));

            ClassNamesInAssembly.Load(context);
        }

        public static void OnMethodNameSelected(DataContext context)
        {
            var className        = context.Get(DataKeys.ClassName);
            var methodName       = context.Get(DataKeys.MethodName);
            var assemblyFilePath = context.Get(DataKeys.AssemblyFilePath);

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, className);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(DataKeys.MethodDefinition, methodDefinition);

            UpdateUI(context);
        }

        public static void OnClassNameChanged(DataContext context)
        {
            MethodNamesInAssembly.Load(context);
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
            var panel = context.Get(DataKeys.ParametersPanel);

            panel.Children.Clear();

            var methodDefinition = context.Get(DataKeys.MethodDefinition);

            foreach (var parameterDefinition in methodDefinition.Parameters)
            {
                var item = Create(parameterDefinition);

                panel.Children.Add(item);
            }
        }
        #endregion
    }
}