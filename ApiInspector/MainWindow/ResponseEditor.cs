using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;

namespace ApiInspector.MainWindow
{
    class ResponseEditor : Grid
    {
        #region Fields
        readonly JsonTextEditor editor = new JsonTextEditor();
        #endregion

        #region Constructors
        public ResponseEditor()
        {
            RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});

            var label = new Label {FontWeight = FontWeights.Bold, Content = "Response"};
            label.SetValue(RowProperty, 0);

            editor.SetValue(RowProperty, 1);

            Children.Add(label);
            Children.Add(editor);
        }
        #endregion

        #region Public Properties
        public string Text
        {
            set => editor.Text = value;
        }
        #endregion
    }
}