using System.Windows;
using System.Windows.Controls;
using static ApiInspector.Components.EditorProp;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The label creator
    /// </summary>
    static class LabelCreator
    {
        #region Public Methods
        /// <summary>
        ///     Creates the label.
        /// </summary>
        public static Label CreateLabel(Scope scope)
        {
            return InitializeLabel(scope, new Label());
        }

        /// <summary>
        ///     Initializes the label.
        /// </summary>
        public static Label InitializeLabel(Scope scope, Label label)
        {
            var labelText = scope.Get(LabelText);

            label.Content    = labelText;
            label.FontWeight = FontWeights.Bold;

            return label;
        }
        #endregion
    }
}