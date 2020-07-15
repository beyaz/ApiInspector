using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.Models;
using BOA.Base;
using BOA.DataFlow;
using Mono.Cecil;
using Newtonsoft.Json;

namespace ApiInspector.InvocationInfoEditor
{
    class ParameterPanelIntegration
    {
        #region Public Methods
        public static void Connect(DataContext context)
        {
            var invocationInfo   = context.Get(Data.InvocationInfo);
            var methodDefinition = context.Get(Data.MethodDefinition);
            var panel            = context.Get(Data.ParametersPanel);

            var invocationParameters = invocationInfo.Parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            panel.Children.Clear();

            var len = methodDefinition.Parameters.Count;

            // arrange invocationParameters
            while (invocationParameters.Count > len)
            {
                invocationParameters.RemoveAt(invocationParameters.Count - 1);
            }

            while (invocationParameters.Count < len)
            {
                invocationParameters.Add(new InvocationMethodParameterInfo());
            }

            for (var i = 0; i < len; i++)
            {
                var parameterDefinition = methodDefinition.Parameters[i];

                var parameterInfo = invocationInfo.Parameters[i];
                var item          = Create(parameterDefinition, parameterInfo);

                panel.Children.Add(item);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the specified definition.
        /// </summary>
        static StackPanel Create(ParameterDefinition definition, InvocationMethodParameterInfo parameterInfo)
        {
            var sp = new StackPanel();

            var label = new Label
            {
                Content    = $"{definition.Name} ({definition.ParameterType.Name})",
                FontWeight = FontWeights.Bold
            };

            var editor = new TextBox();

            var primitiveTypeNames = new[]
            {
                typeof(string).FullName,
                typeof(int).FullName
            };
            var index = Array.IndexOf(primitiveTypeNames, definition.ParameterType.FullName);
            if (index >= 0)
            {
                var targetPrimitiveType = Type.GetType(primitiveTypeNames[index]);
                if (parameterInfo.Value == null && parameterInfo.ValueAsJson != null)
                {
                    Utility.TryRun(() => parameterInfo.Value = JsonConvert.DeserializeObject(parameterInfo.ValueAsJson, targetPrimitiveType));
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
                    Utility.TryRun(() => parameterInfo.Value = JsonConvert.DeserializeObject(parameterInfo.ValueAsJson, typeof(string)));
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
        #endregion
    }
}