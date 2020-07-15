using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.Application;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.Base;
using BOA.DataFlow;
using Mono.Cecil;
using Newtonsoft.Json;

namespace ApiInspector.InvocationInfoEditor
{

    class ParameterPanelIntegration
    {
        public static void Connect(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var methodDefinition = context.Get(Data.MethodDefinition);
            var panel = context.Get(Data.ParametersPanel);


            var invocationParameters = invocationInfo.Parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            

            panel.Children.Clear();

            
            var len = methodDefinition.Parameters.Count;

            // arrange invocationParameters
            while (invocationParameters.Count>len)
            {
                invocationParameters.RemoveAt(invocationParameters.Count-1);
            }

            while (invocationParameters.Count<len)
            {
                invocationParameters.Add(new InvocationMethodParameterInfo());
            }



            for (var i = 0; i < len; i++)
            {
                var parameterDefinition = methodDefinition.Parameters[i];


                var parameterInfo = invocationInfo.Parameters[i];
                var item                = Create(parameterDefinition,parameterInfo);

                panel.Children.Add(item);
            }
        }

      

        /// <summary>
        ///     Creates the specified definition.
        /// </summary>
        static StackPanel Create(ParameterDefinition definition, InvocationMethodParameterInfo parameterInfo)
        {
            var sp = new StackPanel();

            var label = new Label
            {
                Content    = $"{definition.Name} ({definition.ParameterType.Name})" ,
                FontWeight = FontWeights.Bold
            };

            var editor = new TextBox();


            var primitiveTypeNames = new []
            {
                typeof(string).FullName,
                typeof(int).FullName
            };
            var index = Array.IndexOf(primitiveTypeNames,definition.ParameterType.FullName);
            if (index>=0)
            {
                var targetPrimitiveType = Type.GetType(primitiveTypeNames[index]);
                if (parameterInfo.Value == null && parameterInfo.ValueAsJson != null)
                {
                    Utility.TryRun(()=>parameterInfo.Value = JsonConvert.DeserializeObject(parameterInfo.ValueAsJson, targetPrimitiveType));
                }

                BindingOperations.SetBinding(editor, TextBox.TextProperty, new Binding
                {
                    Source              = parameterInfo,
                    Path                = new PropertyPath(nameof(parameterInfo.Value)),
                    Mode                = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }

            if (definition.ParameterType.FullName == typeof(string).FullName)
            {
                if (parameterInfo.Value == null && parameterInfo.ValueAsJson != null)
                {
                    Utility.TryRun(()=>parameterInfo.Value = JsonConvert.DeserializeObject(parameterInfo.ValueAsJson, typeof(string)));
                }

                BindingOperations.SetBinding(editor, TextBox.TextProperty, new Binding
                {
                    Source              = parameterInfo,
                    Path                = new PropertyPath(nameof(parameterInfo.Value)),
                    Mode                = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }


            if (definition.ParameterType.FullName == typeof(ObjectHelper).FullName)
            {
                editor.IsEnabled = false;
            }

            sp.Children.Add(label);
            sp.Children.Add(editor);

            return sp;
        }
    }

    static class Controller_old
    {
        #region Public Methods
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

        public static void OnAssemblySearchDirectoryChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            if (!Directory.Exists(invocationInfo.AssemblySearchDirectory))
            {
                return;
            }

            itemSourceList.AssemblyNameList = Directory.GetFiles(invocationInfo.AssemblySearchDirectory).Select(Path.GetFileName).ToList();
        }

        public static void OnClassNameChanged(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            var logger = context.Get(Logger.Key);

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

        public static void OnMethodNameSelected(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);

            var className        = invocationInfo.ClassName;
            var methodName       = invocationInfo.MethodName;
            var assemblyFilePath = Path.Combine(invocationInfo.AssemblySearchDirectory, invocationInfo.AssemblyName);

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, className);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(Data.MethodDefinition, methodDefinition);

            ParameterPanelIntegration.Connect(context);

        }

        #endregion
    }
}