using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.Application;
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
        public static DataKey<string> AssemblyFilePath = new DataKey<string>(nameof(AssemblyFilePath));
        public static DataKey<IReadOnlyList<TypeDefinition>> TypesInAssembly = new DataKey<IReadOnlyList<TypeDefinition>>(nameof(TypesInAssembly));
        public static DataKey<TypeDefinition> TypeDefinitionRelatedClassName = new DataKey<TypeDefinition>(nameof(TypeDefinitionRelatedClassName));
       

        #region Public Methods
        public static void OnAssemblyNameChanged(DataContext context)
        {
            var itemSourceList = context.Get(Data.ItemSourceList);
            var logger            = context.Get(Logger.Key);

            var assemblyFilePath = context.Get(AssemblyFilePath);

            if (!File.Exists(assemblyFilePath))
            {
                logger.Log($"File not exists. File:{assemblyFilePath}");
                return;
            }

            itemSourceList.ClassNameList = context.Get(TypesInAssembly).Select(x => x.FullName).ToList();
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
            var itemSourceList = context.Get(Data.ItemSourceList);

            var typeDefinition = context.Get(TypeDefinitionRelatedClassName);
            if (typeDefinition == null)
            {
                return;
            }

            itemSourceList.MethodNameList = typeDefinition.Methods.Select(x => x.Name).ToList();
        }

        public static void OnMethodNameSelected(DataContext context)
        {
            var invocationInfo = context.Get(Data.InvocationInfo);


            var typeDefinition = context.Get(TypeDefinitionRelatedClassName);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == invocationInfo.MethodName);

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