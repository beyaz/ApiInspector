using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Models;
using ApiInspector.Serialization;
using BOA.Base;
using Mono.Cecil;
using static ApiInspector.InvocationInfoEditor.TypeFinder;
using static ApiInspector.WPFExtensions;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The parameter panel integration
    /// </summary>
    class ParameterPanelIntegration
    {
        #region Public Methods
        /// <summary>
        ///     Connects the specified invocation information.
        /// </summary>
        public void Connect(InvocationInfo invocationInfo, StackPanel panel, MethodDefinition methodDefinition)
        {
            var invocationParameters = invocationInfo.Parameters = invocationInfo.Parameters ?? new List<InvocationMethodParameterInfo>();

            panel.Children.Clear();

            if (methodDefinition == null)
            {
                return;
            }

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
            var isNullableDateTime = definition.ParameterType.FullName == "System.Nullable`1<System.DateTime>";
            var isDateTime         = definition.ParameterType.FullName == "System.DateTime";

            var getLabel = fun(() =>
            {
                if (isNullableDateTime)
                {
                    return "DateTime? [Sample: 23/02/2020 19:48:59]";
                }

                if (isDateTime)
                {
                    return "DateTime [Sample: 23/02/2020 19:48:59]";
                }

                return definition.ParameterType.Name;
            });

            var label = new Label
            {
                Content    = $"{definition.Name} : {getLabel()}",
                FontWeight = FontWeights.Bold
            };

            var editor = new TextBox
            {
                TextWrapping                = TextWrapping.Wrap,
                AcceptsReturn               = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var canPresentSimpleTextBox = fun(() =>
            {
                var types = new[]
                {
                    typeof(string),
                    typeof(DateTime),
                    typeof(DateTime?),

                    // numbers
                    typeof(byte),
                    typeof(short),
                    typeof(int),
                    typeof(long),

                    // nullable numbers
                    typeof(byte?),
                    typeof(short?),
                    typeof(int?),
                    typeof(long?),

                    // unsigned numbers
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),

                    // unsigned nullable numbers
                    typeof(ushort?),
                    typeof(uint?),
                    typeof(ulong?)
                };

                var getFullName = fun((Type t) =>
                {
                    var genericArguments = t.GetGenericArguments();
                    if (genericArguments.Length == 1)
                    {
                        if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var genericArgument = genericArguments[0];

                            return $"{typeof(Nullable<>).FullName}<{genericArgument.FullName}>";
                        }
                    }

                    return t.FullName;
                });

                return types.Any(t => getFullName(t) == definition.ParameterType.FullName);
            });

            if (canPresentSimpleTextBox())
            {
                Bind(editor, TextBox.TextProperty, parameterInfo, nameof(parameterInfo.Value));
            }
            else if (definition.ParameterType.FullName == typeof(ObjectHelper).FullName)
            {
                editor.IsEnabled = false;
                editor.Text      = "objectHelper";
            }
            else
            {
                // complex items should be as json input
                editor.TextWrapping                = TextWrapping.Wrap;
                editor.MaxLines                    = 10;
                editor.AcceptsReturn               = true;
                editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                if (parameterInfo.Value != null && !(parameterInfo.Value is string))
                {
                    parameterInfo.Value = Serializer.SerializeToJson(parameterInfo.Value);
                }

                if (string.IsNullOrWhiteSpace(parameterInfo.Value + string.Empty))
                {
                    var parameterType = FindType(definition.ParameterType.FullName);
                    if (parameterType != null)
                    {
                        parameterInfo.Value = Serializer.SerializeToJsonDoNotIgnoreDefaultValues(Activator.CreateInstance(parameterType));
                    }
                }

                Bind(editor, TextBox.TextProperty, parameterInfo, nameof(parameterInfo.Value));
            }

            return NewStackPanel(label, editor);
        }
        #endregion
    }
}