using System.Windows;
using System.Windows.Controls;
using BOA.Base;
using Mono.Cecil;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The parameter panel refresher
    /// </summary>
    class ParameterPanelRefresher
    {
        #region Public Methods
        /// <summary>
        ///     Updates the UI.
        /// </summary>
        public void UpdateUI(StackPanel panel, MethodDefinition methodDefinition)
        {
            panel.Children.Clear();

            foreach (var parameterDefinition in methodDefinition.Parameters)
            {
                var item = Create(parameterDefinition);

                panel.Children.Add(item);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the specified definition.
        /// </summary>
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
        #endregion
    }
}